﻿using NINA.Model.MyCamera;
using NINA.Model.MyFilterWheel;
using NINA.Model.MyTelescope;
using NINA.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ZWOptical.ASISDK;

namespace NINA.ViewModel {
    class EquipmentChooserVM : BaseVM {
        private EquipmentChooserVM(EquipmentType equipment) {
            if(equipment == EquipmentType.Camera) {
                GetCameras();
            } else if (equipment == EquipmentType.Telescope) {
                GetTelescopes();
            } else if (equipment == EquipmentType.FilterWheel) {
                GetFilterWheels();
            }

            SetupDialogCommand = new RelayCommand(OpenSetupDialog);

        }

        private bool? _dialogResult;
        public bool? DialogResult {
            get {
                return _dialogResult;
            }
            set {
                _dialogResult = value;
                RaisePropertyChanged();
            }
        }

        private void OpenSetupDialog(object o) {
            if(SelectedDevice != null && SelectedDevice.HasSetupDialog) {
                SelectedDevice.SetupDialog();
            }
        }

        private void GetFilterWheels() {
            var ascomDevices = new ASCOM.Utilities.Profile();

            foreach (ASCOM.Utilities.KeyValuePair device in ascomDevices.RegisteredDevices("FilterWheel")) {

                try {
                    AscomFilterWheel cam = new AscomFilterWheel(device.Key, device.Value);
                    Devices.Add(cam);
                } catch (Exception) {
                    //only add filter wheels which are supported. e.g. x86 drivers will not work in x64
                }
            }

            if (Devices.Count > 0) {
                var selected = (from device in Devices where device.Id == Settings.FilterWheelId select device).First();
                SelectedDevice = selected;
            }
        }

        private void GetTelescopes() {
            var ascomDevices = new ASCOM.Utilities.Profile();

            foreach (ASCOM.Utilities.KeyValuePair device in ascomDevices.RegisteredDevices("Telescope")) {

                try {
                    AscomTelescope cam = new AscomTelescope(device.Key, device.Value);
                    Devices.Add(cam);
                } catch (Exception) {
                    //only add telescopes which are supported. e.g. x86 drivers will not work in x64
                }
            }            

            if (Devices.Count > 0) {
                var selected = (from device in Devices where device.Id == Settings.TelescopeId select device).First();
                SelectedDevice = selected;
            }
        }

        public static Model.IDevice Show(EquipmentType equipment) {
            var chooser = new EquipmentChooserVM(equipment);


            System.Windows.Window win = new View.EquipmentChooserView {
                Title = "Choose Equipment",
                DataContext = chooser
            };
            win.ShowDialog();
            if (win.DialogResult.Value) {
                return chooser.SelectedDevice;
            } else {
                return null;
            }
            
        }

        private ObservableCollection<Model.IDevice> _devices;
        public ObservableCollection<Model.IDevice> Devices {
            get {
                if(_devices == null) {
                    _devices = new ObservableCollection<Model.IDevice>();
                }
                return _devices;
            }
            set {
                _devices = value;
            }
        }

        private Model.IDevice _selectedDevice;
        public Model.IDevice SelectedDevice {
            get {
                return _selectedDevice;
            }
            set {
                _selectedDevice = value;
                RaisePropertyChanged();
            }
        }

        private void GetCameras() {
            var ascomDevices = new ASCOM.Utilities.Profile();

            for (int i = 0; i < ASICameras.Count; i++) {
                var cam = ASICameras.GetCamera(i);
                if (cam.Name != "") {
                    Devices.Add(cam);
                }
            }

            foreach (ASCOM.Utilities.KeyValuePair device in ascomDevices.RegisteredDevices("Camera")) {
                
                try {
                    AscomCamera cam = new AscomCamera(device.Key, "ASCOM --- " + device.Value);
                    Devices.Add(cam);
                } catch (Exception) {
                    //only add cameras which are supported. e.g. x86 drivers will not work in x64
                }
            }
            
            

            if(Devices.Count > 0) {
                var items = (from device in Devices where device.Id == Settings.CameraId select device);
                if (items.Count() > 0) {
                    SelectedDevice = items.First();

                } else {
                    SelectedDevice = Devices.First();
                }
            }            
        }


        [TypeConverter(typeof(EnumDescriptionTypeConverter))]
        public enum EquipmentType {
            [Description("Camera")]
            Camera,
            [Description("Telescope")]
            Telescope,
            [Description("FilterWheel")]
            FilterWheel
        }

        ICommand _setupDialogCommand;
        public ICommand SetupDialogCommand {
            get {
                return _setupDialogCommand;
            }
            set {
                _setupDialogCommand = value;
            }
        }

    }
}
