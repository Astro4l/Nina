﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NINA.Utility.Profile {
    [Serializable()]
    [XmlRoot(nameof(Profile))]
    class MeridianFlipSettings {

        private bool enabled = false;
        [XmlElement(nameof(Enabled))]
        public bool Enabled {
            get {
                return enabled;
            }
            set {
                enabled = value;
            }
        }

        private bool recenter = true;
        [XmlElement(nameof(Recenter))]
        public bool Recenter {
            get {
                return recenter;
            }
            set {
                recenter = value;
            }
        }

        private double minutesAfterMeridian = 1;
        [XmlElement(nameof(MinutesAfterMeridian))]
        public double MinutesAfterMeridian {
            get {
                return minutesAfterMeridian;
            }
            set {
                minutesAfterMeridian = value;
            }
        }

        private double settleTime = 5;
        [XmlElement(nameof(SettleTime))]
        public double SettleTime {
            get {
                return settleTime;
            }
            set {
                settleTime = value;
            }
        }

        private double pauseTimeBeforeMeridian = 1;
        [XmlElement(nameof(PauseTimeBeforeMeridian))]
        public double PauseTimeBeforeMeridian {
            get {
                return pauseTimeBeforeMeridian;
            }
            set {
                pauseTimeBeforeMeridian = value;
            }
        }        
    }
}
