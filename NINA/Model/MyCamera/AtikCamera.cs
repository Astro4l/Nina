﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ASCOM.DeviceInterface;
using NINA.Utility;
using NINA.Utility.AtikSDK;
using NINA.Utility.Notification;

namespace NINA.Model.MyCamera {

    internal class AtikCamera : BaseINPC, ICamera {

        public AtikCamera(int id) {
            _cameraId = id;
        }

        private int _cameraId;
        private IntPtr _cameraP;

        private AtikCameraDll.ArtemisPropertiesStruct? _info;

        private AtikCameraDll.ArtemisPropertiesStruct Info {
            // info is cached only while camera is open
            get {
                return _info ?? AtikCameraDll.GetCameraProperties(_cameraId);
            }
        }

        public bool HasShutter {
            get {
                var bitNumber = 5;
                var bit = (Info.cameraflags & (1 << bitNumber - 1)) != 0;
                return bit;
            }
        }

        public bool Connected {
            get {
                return AtikCameraDll.IsConnected(_cameraP);
            }
        }

        private double _ccdTemperature;

        public double CCDTemperature {
            get {
                return AtikCameraDll.GetTemperature(_cameraP);
            }
        }

        public double SetCCDTemperature {
            get {
                if (CanSetCCDTemperature) {
                    return _ccdTemperature;
                } else {
                    return double.NaN;
                }
            }

            set {
                if (CanSetCCDTemperature) {
                    _ccdTemperature = value;
                    AtikCameraDll.SetCooling(_cameraP, _ccdTemperature);
                    RaisePropertyChanged();
                }
            }
        }

        private bool _coolerOn;

        public bool CoolerOn {
            get {
                return _coolerOn;
            }
            set {
                try {
                    if (Connected) {
                        if (_coolerOn != value) {
                            _coolerOn = value;
                            if (_coolerOn == false) {
                                AtikCameraDll.SetWarmup(_cameraP);
                            }
                        }
                        RaisePropertyChanged();
                    }
                } catch (Exception) {
                    //_hasCooler = false;
                    _coolerOn = false;
                }
            }
        }

        public short BinX {
            get {
                AtikCameraDll.GetBinning(_cameraP, out var x, out var y);
                return (short)x;
            }

            set {
                if (value < MaxBinX) {
                    AtikCameraDll.SetBinning(_cameraP, value, value);
                    RaisePropertyChanged();
                }
            }
        }

        public short BinY {
            get {
                AtikCameraDll.GetBinning(_cameraP, out var x, out var y);
                return (short)y;
            }

            set {
                if (value < MaxBinY) {
                    AtikCameraDll.SetBinning(_cameraP, value, value);
                    RaisePropertyChanged();
                }
            }
        }

        public string Description {
            get {
                return CleanedUpString(Info.Manufacturer) + " " + CleanedUpString(Info.Description);
            }
        }

        public string DriverInfo {
            get {
                return string.Empty;
            }
        }

        public string DriverVersion {
            get {
                return string.Empty;
            }
        }

        public string SensorName {
            get {
                return string.Empty;
            }
        }

        public SensorType SensorType {
            get {
                return Info.ccdflags == 1 ? SensorType.RGGB : SensorType.Monochrome;
            }
        }

        public int CameraXSize {
            get {
                return Info.nPixelsX;
            }
        }

        public int CameraYSize {
            get {
                return Info.nPixelsY;
            }
        }

        public double ExposureMin {
            get {
                return 0;
            }
        }

        public double ExposureMax {
            get {
                return double.MaxValue;
            }
        }

        public short MaxBinX {
            get {
                AtikCameraDll.GetMaxBinning(_cameraP, out var x, out var y);
                return (short)x;
            }
        }

        public short MaxBinY {
            get {
                AtikCameraDll.GetMaxBinning(_cameraP, out var x, out var y);
                return (short)y;
            }
        }

        public double PixelSizeX {
            get {
                return Info.PixelMicronsX;
            }
        }

        public double PixelSizeY {
            get {
                return Info.PixelMicronsY;
            }
        }

        public bool CanSetCCDTemperature {
            get {
                return AtikCameraDll.HasCooler(_cameraP);
            }
        }

        /*public bool CoolerOn {
            get {
                return false;
            }

            set {
            }
        }*/

        public double CoolerPower {
            get {
                if (CanSetCCDTemperature) {
                    return AtikCameraDll.CoolerPower(_cameraP);
                } else {
                    return double.NaN;
                }
            }
        }

        public string CameraState {
            get {
                return AtikCameraDll.CameraState(_cameraP).ToString();
            }
        }

        public bool CanSetOffset {
            get {
                return false;
            }
        }

        public bool CanSetUSBLimit {
            get {
                return false;
            }
        }

        public int Offset {
            get {
                return -1;
            }
            set {
            }
        }

        public int USBLimit {
            get {
                return -1;
            }
            set {
            }
        }

        public bool CanGetGain {
            get {
                return false;
            }
        }

        public bool CanSetGain {
            get {
                return false;
            }
        }

        public short GainMax {
            get {
                return -1;
            }
        }

        public short GainMin {
            get {
                return -1;
            }
        }

        public short Gain {
            get {
                return -1;
            }

            set {
            }
        }

        public ArrayList Gains {
            get {
                return new ArrayList();
            }
        }

        private AsyncObservableCollection<BinningMode> _binningModes;
        private bool _hasCooler;

        public AsyncObservableCollection<BinningMode> BinningModes {
            get {
                if (_binningModes == null) {
                    _binningModes = new AsyncObservableCollection<BinningMode>();
                    for (short i = 1; i <= MaxBinX; i++) {
                        _binningModes.Add(new BinningMode(i, i));
                    }
                }
                return _binningModes;
            }
        }

        public bool HasSetupDialog {
            get {
                return false;
            }
        }

        public string Id {
            get {
                return CleanedUpString(Info.Description);
            }
        }

        public string Name {
            get {
                return CleanedUpString(Info.Description);
            }
        }

        private string CleanedUpString(char[] values) {
            return string.Join("", values.Take(Array.IndexOf(values, '\0')));
        }

        public void AbortExposure() {
            AtikCameraDll.StopExposure(_cameraP);
        }

        public async Task<bool> Connect(CancellationToken token) {
            return await Task.Run(() => {
                var success = false;
                try {
                    _cameraP = AtikCameraDll.Connect(_cameraId);
                    _info = AtikCameraDll.GetCameraProperties(_cameraP);
                    RaisePropertyChanged(nameof(BinningModes));
                    RaisePropertyChanged(nameof(Connected));
                    success = true;
                } catch (Exception e) {
                    Logger.Error(e);
                    Notification.ShowError(e.Message);
                }

                return success;
            });
        }

        public void Disconnect() {
            AtikCameraDll.Disconnect(_cameraP);
            _info = null;
            _binningModes = null;
            RaisePropertyChanged(nameof(Connected));
        }

        public async Task<ImageArray> DownloadExposure(CancellationToken token) {
            using (MyStopWatch.Measure("ATIK Download")) {
                return await Task.Run<ImageArray>(async () => {
                    try {
                        do {
                            await Task.Delay(100, token);
                        } while (!AtikCameraDll.ImageReady(_cameraP));

                        return await AtikCameraDll.DownloadExposure(_cameraP, SensorType != SensorType.Monochrome);
                    } catch (OperationCanceledException) {
                    } catch (Exception ex) {
                        Notification.ShowError(ex.Message);
                    }
                    return null;
                });
            }
        }

        public void SetBinning(short x, short y) {
            AtikCameraDll.SetBinning(_cameraP, x, y);
        }

        public void SetupDialog() {
        }

        public void StartExposure(double exposureTime, bool isLightFrame) {
            do {
                System.Threading.Thread.Sleep(100);
            } while (AtikCameraDll.CameraState(_cameraP) != AtikCameraDll.ArtemisCameraStateEnum.CAMERA_IDLE);
            AtikCameraDll.StartExposure(_cameraP, exposureTime);
        }

        public void StopExposure() {
            AtikCameraDll.StopExposure(_cameraP);
        }

        public void UpdateValues() {
            RaisePropertyChanged(nameof(CCDTemperature));
            RaisePropertyChanged(nameof(CoolerPower));
            RaisePropertyChanged(nameof(CoolerOn));
            RaisePropertyChanged(nameof(SetCCDTemperature));
            RaisePropertyChanged(nameof(CameraState));
        }
    }
}