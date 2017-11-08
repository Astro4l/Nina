﻿using NINA.Model;
using NINA.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ToastNotifications;

namespace NINA.ViewModel {
    class ApplicationVM : BaseVM {

        public ApplicationVM() {
            ExitCommand = new RelayCommand(ExitApplication);
            MinimizeWindowCommand = new RelayCommand(MinimizeWindow);
            MaximizeWindowCommand = new RelayCommand(MaximizeWindow);
            
            RegisterMediatorMessages();

            InitAvalonDockLayout();

            MeridianFlipVM = new MeridianFlipVM();
        }

        public void InitAvalonDockLayout() {            
            DockManagerVM.Documents.Add(ImagingVM.ImageControl);
            DockManagerVM.Anchorables.Add(CameraVM);
            DockManagerVM.Anchorables.Add(TelescopeVM);
            DockManagerVM.Anchorables.Add(PlatesolveVM);            
            DockManagerVM.Anchorables.Add(PolarAlignVM);
            DockManagerVM.Anchorables.Add(WeatherDataVM);
            DockManagerVM.Anchorables.Add(GuiderVM);
            DockManagerVM.Anchorables.Add(SeqVM);
            DockManagerVM.Anchorables.Add(FilterWheelVM);
            DockManagerVM.Anchorables.Add(FocuserVM);
            DockManagerVM.Anchorables.Add(ImagingVM);
            DockManagerVM.Anchorables.Add(ImagingVM.ImageControl.ImgHistoryVM);
            DockManagerVM.Anchorables.Add(ImagingVM.ImageControl.ImgStatisticsVM);
            DockManagerVM.Anchorables.Add(AutoFocusVM);
        }

        

        private void RegisterMediatorMessages() {
            Mediator.Instance.Register((object o) => {
                Status = (string)o;
            }, MediatorMessages.StatusUpdate);
        }
        


        public string Version {
            get {               
                return "v. 1.2.2";
            }
        }

        private string _status;        
        public string Status {
            get {
                return _status;
            }
            set {
                _status = value;
                RaisePropertyChanged();
            }
        }


        private static void MaximizeWindow(object obj) {
            if (Application.Current.MainWindow.WindowState == WindowState.Maximized) {
                Application.Current.MainWindow.WindowState = WindowState.Normal;
            }
            else {
                Application.Current.MainWindow.WindowState = WindowState.Maximized;
            }
        }

        private void MinimizeWindow(object obj) {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        private void ExitApplication(object obj) {
            DockManagerVM.SaveAvalonDockLayout();
            if (CameraVM?.Cam?.Connected == true) {
                var diag = MyMessageBox.MyMessageBox.Show("Camera still connected. Exit anyway?", "", MessageBoxButton.OKCancel, MessageBoxResult.Cancel);                
                if(diag == MessageBoxResult.OK) {
                    DisconnectEquipment();
                    Application.Current.Shutdown();
                }
            } else {
                DisconnectEquipment();
                Application.Current.Shutdown();
            }
            
        }

        private void DisconnectEquipment() {
            CameraVM?.Disconnect();
            TelescopeVM?.Telescope?.Disconnect();
            FilterWheelVM?.FW?.Disconnect();
            FocuserVM?.Focuser?.Disconnect();
            GuiderVM?.Guider?.Disconnect();
        }

        private DockManagerVM _dockManagerVM;
        public DockManagerVM DockManagerVM {
            get {
                if(_dockManagerVM == null) {
                    _dockManagerVM = new DockManagerVM();
                }
                return _dockManagerVM;
            }
            set {
                _dockManagerVM = value;
                RaisePropertyChanged();
            }
        }

        private MeridianFlipVM _meridianFlipVM;
        public MeridianFlipVM MeridianFlipVM {
            get {
                return _meridianFlipVM;
            }
            set {
                _meridianFlipVM = value;
                RaisePropertyChanged();
            }
        }
        
        private CameraVM _cameraVM;
        public CameraVM CameraVM {
            get {
                if(_cameraVM == null) {
                    _cameraVM = new CameraVM();

                }
                return _cameraVM;
            }
            set {
                _cameraVM = value;
                RaisePropertyChanged();
            }
        }

        private FilterWheelVM _filterWheelVM;
        public FilterWheelVM FilterWheelVM {
            get {
                if (_filterWheelVM == null) {
                    _filterWheelVM = new FilterWheelVM();
                }
                return _filterWheelVM;
            }
            set {
                _filterWheelVM = value;
                RaisePropertyChanged();
            }
        }

        private FocuserVM _focuserVM;
        public FocuserVM FocuserVM {
            get {
                if (_focuserVM == null) {
                    _focuserVM = new FocuserVM();
                }
                return _focuserVM;
            }
            set {
                _focuserVM = value;
                RaisePropertyChanged();
            }
        }

        private WeatherDataVM _weatherDataVM;
        public WeatherDataVM WeatherDataVM {
            get {
                if (_weatherDataVM == null) {
                    _weatherDataVM = new WeatherDataVM();

                }
                return _weatherDataVM;
            }
            set {
                _weatherDataVM = value;
                RaisePropertyChanged();
            }
        }

        private SequenceVM _seqVM;
        public SequenceVM SeqVM {
            get {
                if(_seqVM == null) {
                    _seqVM = new SequenceVM();
                }
                return _seqVM;
            }
            set {
                _seqVM = value;
                RaisePropertyChanged();
            }
        }

        private ImagingVM _imagingVM;
        public ImagingVM ImagingVM {
            get {
                if (_imagingVM == null) {
                    _imagingVM = new ImagingVM();
                }
                return _imagingVM;
            }
            set {
                _imagingVM = value;
                RaisePropertyChanged();
            }
        }

        private PolarAlignmentVM _polarAlignVM;
        public PolarAlignmentVM PolarAlignVM {
            get {
                if (_polarAlignVM == null) {
                    _polarAlignVM = new PolarAlignmentVM();
                }
                return _polarAlignVM;
            } set {
                _polarAlignVM = value;
                RaisePropertyChanged();
            }
        }

        private PlatesolveVM _platesolveVM;
        public PlatesolveVM PlatesolveVM {
            get {
                if (_platesolveVM == null) {
                    _platesolveVM = new PlatesolveVM();
                }
                return _platesolveVM;
            }
            set {
                _platesolveVM = value;
                RaisePropertyChanged();
            }
        }

        private TelescopeVM _telescopeVM;
        public TelescopeVM TelescopeVM {
            get {
                if (_telescopeVM == null) {
                    _telescopeVM = new TelescopeVM();
                }
                return _telescopeVM;
            }
            set {
                _telescopeVM = value;
                RaisePropertyChanged();
            }
        }

        private GuiderVM _guiderVM;
        public GuiderVM GuiderVM {
            get {
                if (_guiderVM == null) {
                    _guiderVM = new GuiderVM();
                }
                return _guiderVM;
            }
            set {                
                _guiderVM = value;
                RaisePropertyChanged();
            }
        }

        private OptionsVM _optionsVM;
        public OptionsVM OptionsVM {
            get {
                if (_optionsVM == null) {
                    _optionsVM = new OptionsVM();
                }
                return _optionsVM;
            }
            set {
                _optionsVM = value;
                RaisePropertyChanged();
            }
        }

        private FrameFocusVM _frameFocusVM;
        public FrameFocusVM FrameFocusVM {
            get {
                if (_frameFocusVM == null) {
                    _frameFocusVM = new FrameFocusVM();
                }
                return _frameFocusVM;
            }
            set {
                _frameFocusVM = value;
                RaisePropertyChanged();
            }
        }

        private AutoFocusVM _autoFocusVM;
        public AutoFocusVM AutoFocusVM {
            get {
                if (_autoFocusVM == null) {
                    _autoFocusVM = new AutoFocusVM();
                }
                return _autoFocusVM;
            }
            set {
                _autoFocusVM = value;
                RaisePropertyChanged();
            }
        }

        private SkyAtlasVM _skyAtlasVM;
        public SkyAtlasVM SkyAtlasVM {
            get {
                if (_skyAtlasVM == null) {
                    _skyAtlasVM = new SkyAtlasVM();
                }
                return _skyAtlasVM;
            }
            set {
                _skyAtlasVM = value;
                RaisePropertyChanged();
            }
        }

        public ICommand MinimizeWindowCommand { get; private set; }

        public ICommand MaximizeWindowCommand { get; private set; }

        public ICommand ExitCommand { get; private set; }
                
        


    }
}
