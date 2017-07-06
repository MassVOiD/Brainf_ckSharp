﻿using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Brainf_ck_sharp.Enums;
using Brainf_ck_sharp_UWP.Helpers;
using Brainf_ck_sharp_UWP.Helpers.Extensions;
using Brainf_ck_sharp_UWP.Helpers.Settings;
using Brainf_ck_sharp_UWP.Messages;
using Brainf_ck_sharp_UWP.PopupService;
using GalaSoft.MvvmLight.Messaging;
using UICompositionAnimations;
using UICompositionAnimations.Enums;

namespace Brainf_ck_sharp_UWP.UserControls.VirtualKeyboard.StdinHeader
{
    public sealed partial class KeyboardHeaderControl : UserControl
    {
        public KeyboardHeaderControl()
        {
            this.InitializeComponent();
            AppSettingsManager.Instance.TryGetValue(nameof(AppSettingsKeys.ByteOverflowModeEnabled), out bool overflow);
            OverflowSwitchButton.IsChecked = overflow;
            OverflowSwitchButton.ManageControlPointerStates((_, value) =>
            {
                LightBorder.StartXAMLTransformFadeAnimation(null, value ? 0 : 1, 200, null, EasingFunctionNames.Linear);
            });
        }

        public void SelectKeyboard() => SelectedHeaderIndex = 0;

        public void SelectMemoryView() => SelectedHeaderIndex = 1;

        /// <summary>
        /// Gets or sets the currently selected index for the header control
        /// </summary>
        public int SelectedHeaderIndex
        {
            get => (int)GetValue(SelectedHeaderIndexProperty);
            set => SetValue(SelectedHeaderIndexProperty, value);
        }

        public static readonly DependencyProperty SelectedHeaderIndexProperty = DependencyProperty.Register(
            nameof(SelectedHeaderIndex), typeof(int), typeof(KeyboardHeaderControl), new PropertyMetadata(default(int), OnSelectedHeaderIndexPropertyChanged));

        private static void OnSelectedHeaderIndexPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            KeyboardHeaderControl @this = d.To<KeyboardHeaderControl>();
            int index = e.NewValue.To<int>();
            @this.KeyboardButton.IsSelected = index == 0;
            @this.MemoryMapButton.IsSelected = index == 1;
        }

        /// <summary>
        /// Gets the current text in the Stdin buffer
        /// </summary>
        public String StdinBuffer => StdinBox.Text;

        /// <summary>
        /// Resets the current Stdin buffer
        /// </summary>
        public void ResetStdin() => StdinBox.Text = String.Empty;

        // Toggles the overflow mode currently selected
        private void ToggleOverflowMode()
        {
            // Update the local setting
            bool overflow = OverflowSwitchButton.IsChecked == true;
            AppSettingsManager.Instance.SetValue(nameof(AppSettingsKeys.ByteOverflowModeEnabled), overflow, SettingSaveMode.OverwriteIfExisting);
            Messenger.Default.Send(new OverflowModeChangedMessage(overflow ? OverflowMode.ByteOverflow : OverflowMode.ShortNoOverflow));

            // Show the info message if needed
            if (overflow && AppSettingsManager.Instance.TryGetValue(nameof(AppSettingsKeys.OverflowToggleMessageShown), out bool shown) && !shown)
            {
                AppSettingsManager.Instance.SetValue(nameof(AppSettingsKeys.OverflowToggleMessageShown), true, SettingSaveMode.OverwriteIfExisting);
                FlyoutManager.Instance.Show(LocalizationManager.GetResource("OverflowMode"), LocalizationManager.GetResource("OverflowModeBody"));
            }
        }
    }
}
