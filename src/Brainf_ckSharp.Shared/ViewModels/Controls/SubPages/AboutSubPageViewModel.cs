﻿using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using GitHub.APIs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using User = GitHub.Models.User;

#nullable enable

namespace Brainf_ckSharp.Shared.ViewModels.Controls.SubPages
{
    /// <summary>
    /// A view model for the about page in the app
    /// </summary>
    public sealed class AboutSubPageViewModel : ObservableObject
    {
        /// <summary>
        /// The <see cref="IGitHubService"/> instance currently in use
        /// </summary>
        private readonly IGitHubService GitHubService = Ioc.Default.GetRequiredService<IGitHubService>();

        /// <summary>
        /// Creates a new <see cref="AboutSubPageViewModel"/> instance
        /// </summary>
        public AboutSubPageViewModel()
        {
            LoadDataCommand = new AsyncRelayCommand(LoadDataAsync);
        }

        /// <summary>
        /// Gets the <see cref="ICommand"/> instance responsible for loading the available source codes
        /// </summary>
        public ICommand LoadDataCommand { get; }

        /// <summary>
        /// Forwards the <see cref="ThisAssembly.Git.Branch"/> property
        /// </summary>
        public string GitBranch => ThisAssembly.Git.Branch;

        /// <summary>
        /// Forwards the <see cref=" ThisAssembly.Git.Commit"/> property
        /// </summary>
        public string GitCommit => ThisAssembly.Git.Commit;

        private static IEnumerable<User>? _Developers;

        /// <summary>
        /// Gets the list of lead developers to the Legere repository
        /// </summary>
        public IEnumerable<User>? Developers
        {
            get => _Developers;
            private set => SetProperty(ref _Developers, value);
        }

        private static IEnumerable<string>? _FeaturedLinks;

        /// <summary>
        /// Gets the list of featured links to use
        /// </summary>
        public IEnumerable<string>? FeaturedLinks
        {
            get => _FeaturedLinks;
            private set => SetProperty(ref _FeaturedLinks, value);
        }

        /// <summary>
        /// Loads all the necessary data for the view model
        /// </summary>
        public async Task LoadDataAsync()
        {
            if (Developers != null) return;

            try
            {
                Developers = new[] { await GitHubService.GetUserAsync(Constants.GitHubUsername) };
                FeaturedLinks = new[] { Constants.PayPalMeUrl };
            }
            catch
            {
                // Whoops!
            }
        }
    }
}

