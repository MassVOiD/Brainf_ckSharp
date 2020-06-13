﻿using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Brainf_ckSharp.Enums;
using Brainf_ckSharp.Models;
using Brainf_ckSharp.Services;
using Brainf_ckSharp.Shared.Enums;
using Brainf_ckSharp.Shared.Extensions.Microsoft.Toolkit.Collections;
using Brainf_ckSharp.Shared.Messages.InputPanel;
using Brainf_ckSharp.Shared.Models.Ide.Views;
using Brainf_ckSharp.Shared.ViewModels.Abstract;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Collections;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Nito.AsyncEx;

#nullable enable

namespace Brainf_ckSharp.Shared.ViewModels.Controls.SubPages
{
    public sealed class IdeResultSubPageViewModel : ViewModelBase<ObservableGroupedCollection<IdeResultSection, IdeResultWithSectionInfo>>
    {
        /// <summary>
        /// The <see cref="ISettingsService"/> instance currently in use
        /// </summary>
        private readonly ISettingsService SettingsService = Ioc.Default.GetRequiredService<ISettingsService>();

        /// <summary>
        /// A mutex to avoid race conditions when handling executions and tokens
        /// </summary>
        private readonly AsyncLock LoadingMutex = new AsyncLock();

        /// <summary>
        /// The <see cref="CancellationTokenSource"/> to handle executions within <see cref="LoadDataAsync"/>
        /// </summary>
        private CancellationTokenSource? _ExecutionTokenSource;

        /// <summary>
        /// The <see cref="CancellationTokenSource"/> to handle debug breakpoints within <see cref="LoadDataAsync"/>
        /// </summary>
        private CancellationTokenSource? _DebugTokenSource;

        /// <summary>
        /// The <see cref="InterpreterSession"/> to use, when in DEBUG mode
        /// </summary>
        private InterpreterSession? _DebugSession;

        /// <summary>
        /// Creates a new <see cref="IdeResultSubPageViewModel"/> instance
        /// </summary>
        public IdeResultSubPageViewModel()
        {
            LoadDataCommand = new AsyncRelayCommand(LoadDataAsync);
            ContinueCommand = new AsyncRelayCommand(ContinueAsync);
            SkipCommand = new AsyncRelayCommand(SkipAsync);
        }

        /// <summary>
        /// Gets or sets the script to execute
        /// </summary>
        public string? Script { get; set; }

        /// <summary>
        /// Gets or sets the breakpoints to use
        /// </summary>
        /// <remarks>If <see langword="null"/>, the RELEASE mode is used to run the code</remarks>
        public IMemoryOwner<int>? Breakpoints { get; set; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> instance responsible for loading the available source codes
        /// </summary>
        public ICommand LoadDataCommand { get; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> instance responsible for continuing a DEBUG session
        /// </summary>
        public ICommand ContinueCommand { get; }

        /// <summary>
        /// Gets the <see cref="ICommand"/> instance responsible for completing a DEBUG session
        /// </summary>
        public ICommand SkipCommand { get; }

        /// <summary>
        /// Gets whether or not the debugger is currently stopped at a breakpoint
        /// </summary>
        public bool IsAtBreakpoint => _DebugSession?.Current.ExitCode.HasFlag(ExitCode.BreakpointReached) == true;

        private bool _IsRunning;

        /// <summary>
        /// Gets whether or not a script is currently being executed
        /// </summary>
        public bool IsRunning
        {
            get => _IsRunning;
            private set => Set(ref _IsRunning, value);
        }

        /// <inheritdoc/>
        protected override async void OnDeactivated()
        {
            _ExecutionTokenSource?.Cancel();
            _DebugTokenSource?.Cancel();

            using (await LoadingMutex.LockAsync())
            {
                _DebugSession?.Dispose();
            }
        }

        /// <summary>
        /// Loads the currently available code samples and recently used files
        /// </summary>
        private async Task LoadDataAsync()
        {
            Guard.IsNotNull(Source, nameof(Source));

            using (await LoadingMutex.LockAsync())
            {
                IsRunning = true;

                _ExecutionTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));

                // Execution arguments and options
                string stdin = Messenger.Send(new StdinRequestMessage(false));
                int memorySize = SettingsService.GetValue<int>(SettingsKeys.MemorySize);
                OverflowMode overflowMode = SettingsService.GetValue<OverflowMode>(SettingsKeys.OverflowMode);

                // Run in RELEASE mode
                if (Breakpoints is null)
                {
                    InterpreterResult result = await Task.Run(() =>
                    {
                        return Brainf_ckInterpreter
                            .CreateReleaseConfiguration()
                            .WithSource(Script!)
                            .WithStdin(stdin)
                            .WithMemorySize(memorySize)
                            .WithOverflowMode(overflowMode)
                            .WithExecutionToken(_ExecutionTokenSource.Token)
                            .TryRun()
                            .Value!;
                    });

                    LoadResults(result);
                }
                else
                {
                    _DebugTokenSource = new CancellationTokenSource();

                    // Run in DEBUG mode
                    _DebugSession = await Task.Run(() =>
                    {
                        var session = Brainf_ckInterpreter
                            .CreateDebugConfiguration()
                            .WithSource(Script!)
                            .WithStdin(stdin)
                            .WithBreakpoints(Breakpoints.Memory)
                            .WithMemorySize(memorySize)
                            .WithOverflowMode(overflowMode)
                            .WithExecutionToken(_ExecutionTokenSource.Token)
                            .WithDebugToken(_DebugTokenSource.Token)
                            .TryRun()
                            .Value!;

                        session.MoveNext();

                        return session;
                    });

                    LoadResults(_DebugSession.Current);

                    OnPropertyChanged(nameof(IsAtBreakpoint));
                }

                IsRunning = false;
            }
        }

        /// <summary>
        /// Continues the DEBUG session and moves ahead by one step
        /// </summary>
        private async Task ContinueAsync()
        {
            using (await LoadingMutex.LockAsync())
            {
                IsRunning = true;

                await Task.Run(() => _DebugSession!.MoveNext());

                LoadResults(_DebugSession!.Current);

                OnPropertyChanged(nameof(IsAtBreakpoint));

                IsRunning = false;
            }
        }

        /// <summary>
        /// Runs the current DEBUG session to completion, skipping remaining breakpoints
        /// </summary>
        private async Task SkipAsync()
        {
            using (await LoadingMutex.LockAsync())
            {
                IsRunning = true;

                _DebugTokenSource!.Cancel();

                await Task.Run(() => _DebugSession!.MoveNext());

                LoadResults(_DebugSession!.Current);

                OnPropertyChanged(nameof(IsAtBreakpoint));

                IsRunning = false;
            }
        }

        /// <summary>
        /// Displays the result for an execution run
        /// </summary>
        /// <param name="result">The <see cref="InterpreterResult"/> instance to display the results for</param>
        private void LoadResults(InterpreterResult result)
        {
            Source.Clear();

            // A function used to quickly add a specific section to the current collection
            void AddToSource(IdeResultSection section)
            {
                var model = new IdeResultWithSectionInfo(section, result);

                Source.Add(section, model);
            }

            // The order of items in the result view is as follows:
            // - (optional) Exception type
            // - (optional) Stdout buffer
            // - (optional) Error location
            // - (optional) Breakpoint location
            // - (optional) Stack trace
            // - Source code
            // - (optional) Function definitions
            // - Memory state
            // - Statistics
            //
            // Each group stores the type of section it represents, so that
            // a template selector can be used in the view. The value of each
            // group is the the whole session result, as it contains all the
            // available info for the current script execution.
            // Each template is responsible for extracting info from it
            // and display according to its own function and section type.
            if (!result.ExitCode.HasFlag(ExitCode.Success)) AddToSource(IdeResultSection.ExceptionType);
            if (result.Stdout.Length > 0) AddToSource(IdeResultSection.Stdout);

            if (result.ExitCode.HasFlag(ExitCode.ExceptionThrown)) AddToSource(IdeResultSection.ErrorLocation);
            else if (result.ExitCode.HasFlag(ExitCode.BreakpointReached)) AddToSource(IdeResultSection.BreakpointReached);

            if (result.ExitCode.HasFlag(ExitCode.ExceptionThrown) ||
                result.ExitCode.HasFlag(ExitCode.ThresholdExceeded) ||
                result.ExitCode.HasFlag(ExitCode.BreakpointReached))
            {
                AddToSource(IdeResultSection.StackTrace);
            }

            AddToSource(IdeResultSection.SourceCode);

            if (result.Functions.Count > 0) AddToSource(IdeResultSection.FunctionDefinitions);

            AddToSource(IdeResultSection.MemoryState);
            AddToSource(IdeResultSection.Statistics);
        }
    }
}
