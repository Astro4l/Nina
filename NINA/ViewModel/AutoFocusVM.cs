﻿#region "copyright"

/*
    Copyright © 2016 - 2019 Stefan Berg <isbeorn86+NINA@googlemail.com>

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    N.I.N.A. is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    N.I.N.A. is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with N.I.N.A..  If not, see <http://www.gnu.org/licenses/>.
*/

#endregion "copyright"

using NINA.Model;
using NINA.Model.MyCamera;
using NINA.Model.MyFilterWheel;
using NINA.Model.MyFocuser;
using NINA.Utility;
using NINA.Utility.Mediator.Interfaces;
using NINA.Utility.Notification;
using NINA.Profile;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NINA.ViewModel {

    internal class AutoFocusVM : DockableVM, ICameraConsumer, IFocuserConsumer, IFilterWheelConsumer {

        public AutoFocusVM(IProfileService profileService,
            IFocuserMediator focuserMediator,
            IGuiderMediator guiderMediator,
            IImagingMediator imagingMediator,
            IApplicationStatusMediator applicationStatusMediator) : this(profileService, null, null, focuserMediator, guiderMediator, imagingMediator, applicationStatusMediator) {
        }

        public AutoFocusVM(
                IProfileService profileService,
                ICameraMediator cameraMediator,
                IFilterWheelMediator filterWheelMediator,
                IFocuserMediator focuserMediator,
                IGuiderMediator guiderMediator,
                IImagingMediator imagingMediator,
                IApplicationStatusMediator applicationStatusMediator
        ) : base(profileService) {
            Title = "LblAutoFocus";
            ImageGeometry = (System.Windows.Media.GeometryGroup)System.Windows.Application.Current.Resources["AutoFocusSVG"];

            if (cameraMediator != null) {
                this.cameraMediator = cameraMediator;
                this.cameraMediator.RegisterConsumer(this);
            }

            if (filterWheelMediator != null) {
                this.filterWheelMediator = filterWheelMediator;
                this.filterWheelMediator.RegisterConsumer(this);
            }

            this.focuserMediator = focuserMediator;
            this.focuserMediator.RegisterConsumer(this);

            this.imagingMediator = imagingMediator;
            this.guiderMediator = guiderMediator;
            this.applicationStatusMediator = applicationStatusMediator;

            FocusPoints = new AsyncObservableCollection<DataPoint>();

            StartAutoFocusCommand = new AsyncCommand<bool>(
                () =>
                    Task.Run(

                        async () => {
                            _autoFocusCancelToken?.Dispose();
                            _autoFocusCancelToken = new CancellationTokenSource();
                            FilterInfo filter = null;
                            if (this.filterInfo?.SelectedFilter != null) {
                                filter = profileService.ActiveProfile.FilterWheelSettings.FilterWheelFilters.Where(x => x.Position == this.filterInfo.SelectedFilter.Position).FirstOrDefault();
                            }
                            return await StartAutoFocus(filter, _autoFocusCancelToken.Token, new Progress<ApplicationStatus>(p => Status = p));
                        }
                    ),
                (p) => { return focuserInfo?.Connected == true && cameraInfo?.Connected == true; }
            );
            CancelAutoFocusCommand = new RelayCommand(CancelAutoFocus);
        }

        private CancellationTokenSource _autoFocusCancelToken;
        private AsyncObservableCollection<DataPoint> _focusPoints;
        private ICameraMediator cameraMediator;
        private IImagingMediator imagingMediator;
        private IGuiderMediator guiderMediator;
        private IApplicationStatusMediator applicationStatusMediator;

        public AsyncObservableCollection<DataPoint> FocusPoints {
            get {
                return _focusPoints;
            }
            set {
                _focusPoints = value;
                RaisePropertyChanged();
            }
        }

        private DataPoint _minimum;

        private ApplicationStatus _status;

        public ApplicationStatus Status {
            get {
                return _status;
            }
            set {
                _status = value;
                _status.Source = Title;
                RaisePropertyChanged();

                this.applicationStatusMediator.StatusUpdate(_status);
            }
        }

        private TrendLine _leftTrend;

        public TrendLine LeftTrend {
            get {
                return _leftTrend;
            }
            set {
                _leftTrend = value;
                RaisePropertyChanged();
            }
        }

        private TrendLine _rightTrend;

        public TrendLine RightTrend {
            get {
                return _rightTrend;
            }
            set {
                _rightTrend = value;
                RaisePropertyChanged();
            }
        }

        private int _focusPosition;

        private async Task GetFocusPoints(FilterInfo filter, int nrOfSteps, IProgress<ApplicationStatus> progress, CancellationToken token, int offset = 0) {
            var stepSize = profileService.ActiveProfile.FocuserSettings.AutoFocusStepSize;

            if (offset != 0) {
                //Move to initial position
                _focusPosition = await focuserMediator.MoveFocuserRelative(offset * stepSize);
            }

            var comparer = new FocusPointComparer();

            var exposuresPerFocusPoint = profileService.ActiveProfile.FocuserSettings.AutoFocusNumberOfFramesPerPoint;

            for (int i = 0; i < nrOfSteps; i++) {
                token.ThrowIfCancellationRequested();

                double hfr = getAverageHFR(filter, token, progress).Result;

                token.ThrowIfCancellationRequested();

                FocusPoints.AddSorted(new DataPoint(_focusPosition, hfr), comparer);
                if (i < nrOfSteps - 1) {
                    Logger.Trace("Moving focuser to next autofocus position");
                    _focusPosition = await focuserMediator.MoveFocuserRelative(-stepSize);
                }

                token.ThrowIfCancellationRequested();
                CalculateTrends();
            }
        }

        private async Task<ImageArray> TakeExposure(FilterInfo filter, CancellationToken token, IProgress<ApplicationStatus> progress) {
            Logger.Trace("Starting Exposure for autofocus");
            double expTime = profileService.ActiveProfile.FocuserSettings.AutoFocusExposureTime;
            if (filter != null && filter.AutoFocusExposureTime > 0) {
                expTime = filter.AutoFocusExposureTime;
            }
            var seq = new CaptureSequence(expTime, CaptureSequence.ImageTypes.SNAP, filter, null, 1);

            return await imagingMediator.CaptureImage(seq, token, progress);
        }

        private async Task<double> EvaluateExposure(ImageArray iarr, CancellationToken token, IProgress<ApplicationStatus> progress) {
            Logger.Trace("Evaluating Expsoure");
            var source = ImageAnalysis.CreateSourceFromArray(iarr, System.Windows.Media.PixelFormats.Gray16);
            source = await ImageControlVM.StretchAsync(iarr, source, profileService.ActiveProfile.ImageSettings.AutoStretchFactor, profileService.ActiveProfile.ImageSettings.BlackClipping);
            var analysis = new ImageAnalysis(source, iarr);
            await analysis.DetectStarsAsync(progress, token);

            Logger.Debug(string.Format("Current Focus: Position: {0}, HRF: {1}", _focusPosition, analysis.AverageHFR));

            return analysis.AverageHFR;
        }

        private async Task<bool> ValidateCalculatedFocusPosition(DataPoint focusPoint, FilterInfo filter, CancellationToken token, IProgress<ApplicationStatus> progress, double initialHFR) {
            _focusPosition = await focuserMediator.MoveFocuser((int)focusPoint.X);

            double hfr = getAverageHFR(filter, token, progress).Result;

            if (hfr > (focusPoint.Y * 1.25)) {
                Notification.ShowWarning(string.Format(Locale.Loc.Instance["LblFocusPointValidationFailed"], focusPoint.X, focusPoint.Y, hfr));
            }

            if (hfr > (initialHFR * 1.15)) {
                Notification.ShowWarning(string.Format(Locale.Loc.Instance["LblAutoFocusNewWorseThanOriginal"], hfr, initialHFR));
                Logger.Warning(string.Format("New focus point HFR {0} is significantly worse than original HFR {1}", hfr, initialHFR));
                return false;
            }
            return true;
        }

        private void CalculateTrends() {
            _minimum = FocusPoints.Aggregate((l, r) => l.Y < r.Y ? l : r);
            IEnumerable<DataPoint> leftTrendPoints = FocusPoints.Where((x) => x.X < _minimum.X && x.Y > (_minimum.Y + 0.1));
            IEnumerable<DataPoint> rightTrendPoints = FocusPoints.Where((x) => x.X > _minimum.X && x.Y > (_minimum.Y + 0.1));
            LeftTrend = new TrendLine(leftTrendPoints);
            RightTrend = new TrendLine(rightTrendPoints);
        }

        private async Task<double> getAverageHFR(FilterInfo filter, CancellationToken token, IProgress<ApplicationStatus> progress) {
            var exposuresPerFocusPoint = profileService.ActiveProfile.FocuserSettings.AutoFocusNumberOfFramesPerPoint;
            
            //Average HFR  of multiple exposures (if configured this way)
            double sumHfr = 0;
            for (int i = 0; i < exposuresPerFocusPoint; i++) {
                var iarr = await TakeExposure(filter, token, progress);
                var partialHfr = await EvaluateExposure(iarr, token, progress);
                sumHfr = sumHfr + partialHfr;
                token.ThrowIfCancellationRequested();
            }

            return sumHfr / exposuresPerFocusPoint;
        }

        public async Task<bool> StartAutoFocus(FilterInfo filter, CancellationToken token, IProgress<ApplicationStatus> progress) {
            Logger.Trace("Starting Autofocus");
            FocusPoints.Clear();
            LeftTrend = null;
            RightTrend = null;
            _minimum = new DataPoint(0, 0);
            int numberOfAttempts = 0;
            int initialFocusPosition;
            double initialHFR = 0;

            try {
                await this.guiderMediator.StopGuiding(token);

                //Get initial position information, as average of multiple exposures, if configured this way
                initialHFR = getAverageHFR(filter, token, progress).Result;
                initialFocusPosition = focuserInfo.Position;

                bool reattempt = false;
                do {
                    numberOfAttempts = numberOfAttempts + 1;

                    var offsetSteps = profileService.ActiveProfile.FocuserSettings.AutoFocusInitialOffsetSteps;
                    var offset = offsetSteps;

                    var nrOfSteps = offsetSteps + 1;

                    await GetFocusPoints(filter, nrOfSteps, progress, token, offset);

                    var laststeps = offset;

                    int leftcount = LeftTrend.DataPoints.Count(), rightcount = RightTrend.DataPoints.Count();
                    //When datapoints are not sufficient analyze and take more
                    do {
                        if (leftcount == 0 && rightcount == 0) {
                            Notification.ShowWarning(Locale.Loc.Instance["LblAutoFocusNotEnoughtSpreadedPoints"]);
                            progress.Report(new ApplicationStatus() { Status = Locale.Loc.Instance["LblAutoFocusNotEnoughtSpreadedPoints"] });
                            //Reattempting in this situation is very likely meaningless - just move back to initial focus position and call it a day
                            await focuserMediator.MoveFocuser(initialFocusPosition);
                            return false;
                        }

                        var remainingSteps = Math.Min(Math.Abs(leftcount - rightcount), offsetSteps);
                        if (leftcount == rightcount && leftcount < offsetSteps) {
                            remainingSteps = offsetSteps - leftcount;
                        }

                        if ((LeftTrend.DataPoints.Count() < offsetSteps && leftcount < rightcount)
                                || (leftcount == rightcount && remainingSteps > 0)) {
                            Logger.Trace("More datapoints needed to the left of the minimum");
                            //More points needed to the left
                            laststeps += remainingSteps;
                            await GetFocusPoints(filter, remainingSteps, progress, token, -1);
                        } else if (RightTrend.DataPoints.Count() < offsetSteps && leftcount > rightcount) {
                            Logger.Trace("More datapoints needed to the right of the minimum");
                            //More points needed to the right
                            offset = laststeps + remainingSteps;  //todo
                            laststeps = remainingSteps - 1;
                            await GetFocusPoints(filter, remainingSteps, progress, token, offset);
                        }

                        leftcount = LeftTrend.DataPoints.Count();
                        rightcount = RightTrend.DataPoints.Count();

                        token.ThrowIfCancellationRequested();
                    } while (rightcount < offsetSteps || leftcount < offsetSteps);

                    token.ThrowIfCancellationRequested();

                    //Get Trendline Intersection
                    var p = LeftTrend.Intersect(RightTrend);

                    LastAutoFocusPoint = new AutoFocusPoint { Focuspoint = p, Temperature = focuserInfo.Temperature, Timestamp = DateTime.Now };

                    //Todo when data is too noisy for trend lines find something else

                    bool goodAutoFocus = await ValidateCalculatedFocusPosition(p, filter, token, progress, initialHFR);

                    if (!goodAutoFocus) {
                        if (numberOfAttempts < profileService.ActiveProfile.FocuserSettings.AutoFocusTotalNumberOfAttempts) {
                            Notification.ShowWarning(Locale.Loc.Instance["LblAutoFocusReattempting"]);
                            await focuserMediator.MoveFocuser(initialFocusPosition);
                            Logger.Warning("Potentially bad auto-focus. Reattempting.");
                            FocusPoints.Clear();
                            LeftTrend = null;
                            RightTrend = null;
                            _minimum = new DataPoint(0, 0);
                            reattempt = true;
                        } else {
                            Notification.ShowWarning(Locale.Loc.Instance["LblAutoFocusRestoringOriginalPosition"]);
                            Logger.Warning("Potentially bad auto-focus. Restoring original focus position.");
                            reattempt = false;
                            await focuserMediator.MoveFocuser(initialFocusPosition);
                            return false;
                        }
                    }
                } while (reattempt);
                //_focusPosition = await Mediator.Instance.RequestAsync(new MoveFocuserMessage() { Position = (int)p.X, Absolute = true, Token = token });
            } catch (OperationCanceledException) {
                FocusPoints.Clear();
            } catch (Exception ex) {
                Notification.ShowError(ex.Message);
                Logger.Error(ex);
            } finally {
                await this.guiderMediator.StartGuiding(token);
                progress.Report(new ApplicationStatus() { Status = string.Empty });
            }
            return true;
        }

        private AutoFocusPoint _lastAutoFocusPoint;
        private CameraInfo cameraInfo = DeviceInfo.CreateDefaultInstance<CameraInfo>();
        private FocuserInfo focuserInfo = DeviceInfo.CreateDefaultInstance<FocuserInfo>();
        private IFocuserMediator focuserMediator;
        private IFilterWheelMediator filterWheelMediator;
        private FilterWheelInfo filterInfo;

        public AutoFocusPoint LastAutoFocusPoint {
            get {
                return _lastAutoFocusPoint;
            }
            set {
                _lastAutoFocusPoint = value;
                RaisePropertyChanged();
            }
        }

        private void CancelAutoFocus(object obj) {
            _autoFocusCancelToken?.Cancel();
        }

        public void UpdateDeviceInfo(CameraInfo cameraInfo) {
            this.cameraInfo = cameraInfo;
        }

        public void UpdateDeviceInfo(FocuserInfo focuserInfo) {
            this.focuserInfo = focuserInfo;
        }

        public void UpdateDeviceInfo(FilterWheelInfo deviceInfo) {
            this.filterInfo = deviceInfo;
        }

        public ICommand StartAutoFocusCommand { get; private set; }
        public ICommand CancelAutoFocusCommand { get; private set; }
    }

    public class AutoFocusPoint {
        public DataPoint Focuspoint { get; set; }
        public DateTime Timestamp { get; set; }
        public double Temperature { get; set; }
    }

    public class FocusPointComparer : IComparer<DataPoint> {

        public int Compare(DataPoint x, DataPoint y) {
            if (x.X < y.X) {
                return -1;
            } else if (x.X > y.X) {
                return 1;
            } else {
                return 0;
            }
        }
    }

    public class TrendLine {

        public TrendLine(IEnumerable<DataPoint> l) {
            DataPoints = l;

            var n = DataPoints.Count();
            var sumXY = DataPoints.Sum((x) => x.X * x.Y);
            var sumX = DataPoints.Sum((x) => x.X);
            var sumY = DataPoints.Sum((x) => x.Y);
            var sumXsumY = sumX * sumY;
            var sumXsquared = DataPoints.Sum((x) => Math.Pow(x.X, 2));

            var alpha = (n * sumXY - sumXsumY) / (n * sumXsquared - Math.Pow(sumX, 2));

            var beta = (sumY - alpha * sumX) / n;
            var beta2 = (sumY * sumXsquared - sumX * sumXY) / (n * sumXsquared - Math.Pow(sumX, 2));

            Slope = alpha;
            Offset = beta;

            // y = alpha * x + beta
        }

        public double Slope { get; set; }
        public double Offset { get; set; }

        public IEnumerable<DataPoint> DataPoints { get; set; }

        public double GetY(double x) {
            return Slope * x + Offset;
        }

        public DataPoint Intersect(TrendLine line) {
            if (this.Slope == line.Slope) {
                //Lines are parallel
                return new DataPoint(0, 0);
            }
            var x = (line.Offset - this.Offset) / (this.Slope - line.Slope);
            var y = this.Slope * x + this.Offset;

            return new DataPoint((int)Math.Round(x), y);
        }
    }
}