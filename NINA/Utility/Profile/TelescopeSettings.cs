﻿using NINA.Utility.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace NINA.Utility.Profile {
    [Serializable()]
    [XmlRoot(nameof(TelescopeSettings))]
    public class TelescopeSettings {

        private string id = "No_Device";
        [XmlElement(nameof(Id))]
        public string Id {
            get {
                return id;
            }
            set {
                id = value;
                Mediator.Mediator.Instance.Request(new SaveProfilesMessage());
            }
        }

        private int focalLength = 800;
        [XmlElement(nameof(FocalLength))]
        public int FocalLength {
            get {
                return focalLength;
            }
            set {
                focalLength = value;
                Mediator.Mediator.Instance.Request(new SaveProfilesMessage());
            }
        }

        private string snapPortStart = ":SNAP1,1#";
        [XmlElement(nameof(SnapPortStart))]
        public string SnapPortStart {
            get {
                return snapPortStart;
            }
            set {
                snapPortStart = value;
                Mediator.Mediator.Instance.Request(new SaveProfilesMessage());
            }
        }

        private string snapPortStop = "SNAP1,0#";
        [XmlElement(nameof(SnapPortStop))]
        public string SnapPortStop {
            get {
                return snapPortStop;
            }
            set {
                snapPortStop = value;
                Mediator.Mediator.Instance.Request(new SaveProfilesMessage());
            }
        }
    }
}
