﻿using System;
using System.Collections.Generic;
using Brainf_ck_sharp_UWP.Helpers.Settings;
using Brainf_ck_sharp_UWP.Messages.UI;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using JetBrains.Annotations;
using UICompositionAnimations.Helpers;

namespace Brainf_ck_sharp_UWP.ViewModels.FlyoutsViewModels
{
    public class SettingsPanelFlyoutViewModel : ViewModelBase
    {
        /// <summary>
        /// Gets the collection of the available blur modes
        /// </summary>
        [NotNull]
        public IReadOnlyCollection<String> BlurModeOptions { get; } = new[]
        {
            "Background blur",
            "In-app blur"
        };

        private int _BlurModeSelectedIndex = AppSettingsManager.Instance.GetValue<int>(nameof(AppSettingsKeys.InAppBlurMode));

        /// <summary>
        /// Gets or sets the selected index for the custom blur mode
        /// </summary>
        public int BlurModeSelectedIndex
        {
            get => _BlurModeSelectedIndex;
            set
            {
                if (Set(ref _BlurModeSelectedIndex, value))
                {
                    AppSettingsManager.Instance.SetValue(nameof(AppSettingsKeys.InAppBlurMode), value, SettingSaveMode.OverwriteIfExisting);
                    Messenger.Default.Send(new BlurModeChangedMessage(value));
                }
            }
        }

        private bool _AutoindentBrackets = AppSettingsManager.Instance.GetValue<bool>(nameof(AppSettingsKeys.AutoIndentBrackets));

        /// <summary>
        /// Gets or sets whether or not the IDE should automatically indents new brackets
        /// </summary>
        public bool AutoindentBrackets
        {
            get => _AutoindentBrackets;
            set
            {
                if (Set(ref _AutoindentBrackets, value))
                {
                    AppSettingsManager.Instance.SetValue(nameof(AppSettingsKeys.AutoIndentBrackets), value, SettingSaveMode.OverwriteIfExisting);
                }
            }
        }

        /// <summary>
        /// Gets the collection of the available brackets styles
        /// </summary>
        [NotNull]
        public IReadOnlyCollection<String> BracketsStyleOptions { get; } = new[]
        {
            "New line",
            "Same line"
        };

        private int _BracketsStyleSelectedIndex = AppSettingsManager.Instance.GetValue<int>(nameof(AppSettingsKeys.BracketsStyle));

        /// <summary>
        /// Gets or sets the selected index for the custom brackets style
        /// </summary>
        public int BracketsStyleSelectedIndex
        {
            get => _BracketsStyleSelectedIndex;
            set
            {
                if (Set(ref _BracketsStyleSelectedIndex, value))
                {
                    AppSettingsManager.Instance.SetValue(nameof(AppSettingsKeys.BracketsStyle), value, SettingSaveMode.OverwriteIfExisting);
                }
            }
        }

        /// <summary>
        /// Gets whether or not the current device is not a mobile phone
        /// </summary>
        public bool HostBlurOptionSupported => !ApiInformationHelper.IsMobileDevice;
    }
}
