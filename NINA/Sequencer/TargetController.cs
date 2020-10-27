﻿#region "copyright"

/*
    Copyright © 2016 - 2020 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using NINA.Model;
using NINA.Profile;
using NINA.Sequencer.Container;
using NINA.Sequencer.DragDrop;
using NINA.Sequencer.Serialization;
using NINA.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace NINA.Sequencer {

    public class TargetController {
        private readonly SequenceJsonConverter sequenceJsonConverter;
        private readonly IProfileService profileService;
        private string targetPath;

        public IList<TargetSequenceContainer> Targets { get; }

        private CollectionViewSource targetsView;
        private CollectionViewSource targetsMenuView;
        public ICollectionView TargetsView { get => targetsView.View; }
        public ICollectionView TargetsMenuView { get => targetsMenuView.View; }

        private string viewFilter = string.Empty;

        public string ViewFilter {
            get => viewFilter;
            set {
                viewFilter = value;
                TargetsView.Refresh();
            }
        }

        public TargetController(SequenceJsonConverter sequenceJsonConverter, IProfileService profileService) {
            this.sequenceJsonConverter = sequenceJsonConverter;
            this.profileService = profileService;

            Targets = new List<TargetSequenceContainer>();

            targetsView = new CollectionViewSource { Source = Targets };
            TargetsView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            TargetsView.Filter += new Predicate<object>(ApplyViewFilter);

            targetsMenuView = new CollectionViewSource { Source = Targets };
            TargetsMenuView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));

            LoadTargets();
        }

        private bool ApplyViewFilter(object obj) {
            return (obj as TargetSequenceContainer).Name.IndexOf(ViewFilter, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public void AddTarget(IDeepSkyObjectContainer deepSkyObjectContainer) {
            var jsonContainer = sequenceJsonConverter.Serialize(deepSkyObjectContainer);
            File.WriteAllText(Path.Combine(targetPath, deepSkyObjectContainer.Name + ".json"), jsonContainer);

            var existingTarget = Targets.FirstOrDefault(x => x.Name == deepSkyObjectContainer.Name);
            if (existingTarget != null) {
                Targets.Remove(existingTarget);
                Targets.Add(new TargetSequenceContainer(deepSkyObjectContainer));
            } else {
                Targets.Add(new TargetSequenceContainer(deepSkyObjectContainer));
            }

            RefreshFilters();
        }

        private void LoadTargets() {
            targetPath = profileService.ActiveProfile.SequenceSettings.SequencerTargetsFolder;

            if (!Directory.Exists(targetPath)) {
                Directory.CreateDirectory(targetPath);
            }

            foreach (var target in Targets) {
                Application.Current.Dispatcher.Invoke(() => Targets.Remove(target));
            }

            foreach (var file in Directory.GetFiles(targetPath, "*.json", SearchOption.AllDirectories)) {
                try {
                    var container = sequenceJsonConverter.Deserialize(File.ReadAllText(file));

                    var dsoContainer = container as IDeepSkyObjectContainer;
                    if (dsoContainer != null) {
                        Targets.Add(new TargetSequenceContainer(dsoContainer));
                    }
                } catch (Exception ex) {
                    Logger.Error($"Invalid target JSON {file}", ex);
                }
            }
            RefreshFilters();
        }

        private void RefreshFilters() {
            Application.Current.Dispatcher.Invoke(() => { TargetsView.Refresh(); TargetsMenuView.Refresh(); });
        }

        public void DeleteTarget(TargetSequenceContainer targetSequenceContainer) {
            var file = Path.Combine(targetPath, targetSequenceContainer.Name + ".json");
            if (File.Exists(file)) {
                File.Delete(file);
            }

            Targets.Remove(targetSequenceContainer);

            RefreshFilters();
        }
    }

    public class TargetSequenceContainer : IDroppable {

        public TargetSequenceContainer(IDeepSkyObjectContainer container) {
            Container = container;
        }

        public string Name { get => Container.Name; }

        public IDeepSkyObjectContainer Container { get; }

        public ISequenceContainer Parent => null;

        public ICommand DetachCommand => null;

        public ICommand MoveUpCommand => null;

        public ICommand MoveDownCommand => null;

        public void AfterParentChanged() {
        }

        public void AttachNewParent(ISequenceContainer newParent) {
        }

        public void Detach() {
        }

        public void MoveDown() {
        }

        public void MoveUp() {
        }

        public IDeepSkyObjectContainer Clone() {
            return Container.Clone() as IDeepSkyObjectContainer;
        }
    }
}