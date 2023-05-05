﻿#region "copyright"

/*
    Copyright © 2016 - 2020 Stefan Berg <isbeorn86+NINA@googlemail.com>

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

using NINA.ViewModel;
using NINA.ViewModel.Equipment.Camera;
using NINA.ViewModel.Equipment.FilterWheel;
using NINA.ViewModel.Imaging;
using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.AvalonDock.Layout;

namespace NINA.Utility.AvalonDock {

    public class PaneTemplateSelector : DataTemplateSelector {

        public PaneTemplateSelector() {
        }

        public DataTemplate CameraTemplate { get; set; }

        public DataTemplate TelescopeTemplate { get; set; }

        public DataTemplate ImageControlTemplate { get; set; }

        public DataTemplate PlatesolveTemplate { get; set; }

        public DataTemplate PolarAlignmentTemplate { get; set; }

        public DataTemplate GuiderTemplate { get; set; }

        public DataTemplate FilterWheelTemplate { get; set; }

        public DataTemplate ImagingTemplate { get; set; }

        public DataTemplate ImageHistoryTemplate { get; set; }

        public DataTemplate ImageStatisticsTemplate { get; set; }

        public DataTemplate RotatorTemplate { get; set; }

        public DataTemplate SequenceTemplate { get; set; }

        public DataTemplate WeatherDataTemplate { get; set; }


        public DataTemplate AutoFocusTemplate { get; set; }

        public DataTemplate ThumbnailTemplate { get; set; }

        public DataTemplate FocusTargetsTemplate { get; set; }

        public DataTemplate SwitchTemplate { get; set; }
        public DataTemplate ExposureCalculatorTemplate { get; set; }

        public override System.Windows.DataTemplate SelectTemplate(object item, System.Windows.DependencyObject container) {
            var itemAsLayoutContent = item as LayoutContent;

            if (item is CameraVM) {
                return CameraTemplate;
            }

            //zhang081
            //if (item is AnchorablePlateSolverVM) {
            //    return PlatesolveTemplate;
            //}

            //zhang085
            //if (item is PolarAlignmentVM) {
            //    return PolarAlignmentTemplate;
            //}

            //zhang057
            //if (item is GuiderVM) {
            //    return GuiderTemplate;
            //}

            if (item is FilterWheelVM) {
                return FilterWheelTemplate;
            }

            if (item is AnchorableSnapshotVM) {
                return ImagingTemplate;
            }

            if (item is ImageHistoryVM) {
                return ImageHistoryTemplate;
            }

            if (item is ImageStatisticsVM) {
                return ImageStatisticsTemplate;
            }

            if (item is ImageControlVM) {
                return ImageControlTemplate;
            }

            if (item is SequenceVM) {
                return SequenceTemplate;
            }
            
            //zhang040
            //if (item is WeatherDataVM) {
            //    return WeatherDataTemplate;
            //}


            //zhang101
            //if (item is AutoFocusVM) {
            //    return AutoFocusTemplate;
            //}

            if (item is ThumbnailVM) {
                return ThumbnailTemplate;
            }

            //zhang029
            //if (item is RotatorVM) {
            //    return RotatorTemplate;
            //}

            //zhang099
            //if (item is FocusTargetsVM) {
            //    return FocusTargetsTemplate;
            //}

            //zhang017
            //if (item is SwitchVM) {
            //    return SwitchTemplate;
            //}

            //zhang089
            //if (item is ExposureCalculatorVM) {
            //    return ExposureCalculatorTemplate;
            //}

            return base.SelectTemplate(item, container);
        }
    }
}