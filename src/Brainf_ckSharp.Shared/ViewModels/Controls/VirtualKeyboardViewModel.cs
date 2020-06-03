﻿using System.Windows.Input;
using Brainf_ckSharp.Services;
using Brainf_ckSharp.Shared.Messages.InputPanel;
using Brainf_ckSharp.Shared.ViewModels.Controls.SubPages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Toolkit.Mvvm.Messaging.Messages;

namespace Brainf_ckSharp.Shared.ViewModels.Controls
{
    public sealed class VirtualKeyboardViewModel : ViewModelBase
    {
        /// <summary>
        /// Creates a new <see cref="VirtualKeyboardViewModel"/> instance
        /// </summary>
        public VirtualKeyboardViewModel()
        {
            _IsPBrainModeEnabled = Ioc.Default.GetRequiredService<ISettingsService>().GetValue<bool>(SettingsKeys.ShowPBrainButtons);

            InsertOperatorCommand = new RelayCommand<char>(InsertOperator);

            Messenger.Register<PropertyChangedMessage<bool>>(this, UpdateIsPBrainModeEnabled);
        }

        /// <summary>
        /// Gets the <see cref="ICommand"/> instance responsible for inserting a new Brainf*ck/PBrain operator
        /// </summary>
        public ICommand InsertOperatorCommand { get; }

        private bool _IsPBrainModeEnabled;

        /// <summary>
        /// Gets whether or not the PBrain mode is currently enabled
        /// </summary>
        public bool IsPBrainModeEnabled
        {
            get => _IsPBrainModeEnabled;
            private set => Set(ref _IsPBrainModeEnabled, value);
        }

        /// <summary>
        /// Signals the insertion of a new operator
        /// </summary>
        /// <param name="op">The input Brainf*ck/PBrain operator</param>
        private void InsertOperator(char op)
        {
            Messenger.Send(new OperatorKeyPressedNotificationMessage(op));
        }

        /// <summary>
        /// Updates <see cref="IsPBrainModeEnabled"/> if the relative setting is changed
        /// </summary>
        /// <param name="message">The <see cref="PropertyChangedMessage{T}"/> instance to receive</param>
        private void UpdateIsPBrainModeEnabled(PropertyChangedMessage<bool> message)
        {
            if (message.Sender.GetType() == typeof(SettingsSubPageViewModel) &&
                message.PropertyName == nameof(SettingsKeys.ShowPBrainButtons))
            {
                IsPBrainModeEnabled = message.NewValue;
            }
        }
    }
}
