﻿using System;
using System.Windows.Input;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Brainf_ckSharp.Services;
using Brainf_ckSharp.Shared.Constants;
using Brainf_ckSharp.Shared.Enums.Settings;
using Brainf_ckSharp.Shared.ViewModels;
using Brainf_ckSharp.Uwp.Controls.SubPages.Shell;
using Brainf_ckSharp.Uwp.Controls.SubPages.Shell.Settings;
using Brainf_ckSharp.Uwp.Controls.SubPages.Shell.UserGuide;
using Brainf_ckSharp.Uwp.Controls.SubPages.Views;
using Brainf_ckSharp.Uwp.Controls.SubPages.Views.UnicodeCharactersMap;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Toolkit.Mvvm.Messaging.Messages;
using Microsoft.Toolkit.Uwp.Helpers;

#nullable enable

namespace Brainf_ckSharp.Uwp.Controls.Host
{
    /// <summary>
    /// The shell that acts as root visual element for the application
    /// </summary>
    public sealed partial class Shell : UserControl
    {
        public Shell()
        {
            this.InitializeComponent();
            this.DataContext = App.Current.Services.GetRequiredService<ShellViewModel>();

            ViewModel.UserGuideRequested += ViewModel_OnUserGuideRequested;
            ViewModel.UnicodeMapRequested += ViewModel_OnUnicodeMapRequested;
            ViewModel.SettingsRequested += ViewModel_OnSettingsRequested;
            ViewModel.AboutInfoRequested += ViewModel_OnAboutInfoRequested;
            ViewModel.CodeLibraryRequested += ViewModel_OnCodeLibraryRequested;

            // Override the starting view if there is a file request pending
            if (App.Current.IsFileRequestPending)
            {
                ViewModel.SelectedView = ViewType.Ide;
            }
        }

        /// <summary>
        /// Gets the <see cref="ShellViewModel"/> instance currently in use
        /// </summary>
        public ShellViewModel ViewModel => (ShellViewModel)DataContext;

        /// <summary>
        /// Displays the review prompt, if needed
        /// </summary>
        private void Shell_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (SystemInformation.Instance.LaunchCount == 4)
            {
                App.Current.SubPageHost.DisplaySubFramePage(new ReviewPromptSubPage());
            }
        }

        /// <summary>
        /// Gets an <see cref="ICommand"/> instance responsible for requesting to move within the code editor
        /// </summary>
        public ICommand MoveCommand { get; } = new RelayCommand<VirtualKey>(key => App.Current.Services.GetRequiredService<IMessenger>().Send(new ValueChangedMessage<VirtualKey>(key)));

        /// <summary>
        /// Brings the IDE into view, if necessary
        /// </summary>
        public void BringIdeIntoView()
        {
            ViewModel.SelectedView = ViewType.Ide;
        }

        /// <summary>
        /// Shows the user guide
        /// </summary>
        private void ViewModel_OnUserGuideRequested(object sender, EventArgs e)
        {
            App.Current.SubPageHost.DisplaySubFramePage(new UserGuideSubPage());
        }

        /// <summary>
        /// Shows the unicode map
        /// </summary>
        private void ViewModel_OnUnicodeMapRequested(object sender, EventArgs e)
        {
            App.Current.SubPageHost.DisplaySubFramePage(new UnicodeCharactersMapSubPage());
        }

        /// <summary>
        /// Shows the app settings
        /// </summary>
        private void ViewModel_OnSettingsRequested(object sender, EventArgs e)
        {
            App.Current.SubPageHost.DisplaySubFramePage(new SettingsSubPage());
        }

        /// <summary>
        /// Shows the info about the current app
        /// </summary>
        private void ViewModel_OnAboutInfoRequested(object sender, EventArgs e)
        {
            App.Current.SubPageHost.DisplaySubFramePage(new AboutSubPage());
        }

        /// <summary>
        /// Shows the code library
        /// </summary>
        private void ViewModel_OnCodeLibraryRequested(object sender, EventArgs e)
        {
            App.Current.SubPageHost.DisplaySubFramePage(new CodeLibrarySubPage());
        }

        /// <summary>
        /// Logs when the compact memory viewer is opened
        /// </summary>
        private void Pivot_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((Pivot)sender).SelectedIndex == 1)
            {
                App.Current.Services.GetRequiredService<IAnalyticsService>().Log(EventNames.CompactMemoryViewerOpened);
            }
        }
    }
}
