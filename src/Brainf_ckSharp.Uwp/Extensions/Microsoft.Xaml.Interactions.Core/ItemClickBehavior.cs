﻿using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;

#nullable enable

namespace Microsoft.Xaml.Interactions.Core
{
    /// <summary>
    /// A behavior that listens for the <see cref="ListViewBase.ItemClick"/> event on its source and executes a specified command when that event is fired
    /// </summary>
    public sealed class ItemClickBehavior : Behavior
    {
        /// <summary>
        /// The currently targeted <see cref="ListViewBase"/> instance
        /// </summary>
        private ListViewBase? _ResolvedSource;

        /// <summary>
        /// Gets or sets the <see cref="ICommand"/> instance to invoke when the current behavior is triggered
        /// </summary>
        public ICommand? Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        /// <summary>
        /// Identifies the <seealso cref="Command"/> property
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            "Command",
            typeof(ICommand),
            typeof(ItemClickBehavior),
            new PropertyMetadata(default(ICommand)));

        /// <summary>
        /// Handles a clicked item and invokes the associated command
        /// </summary>
        /// <param name="sender">The current <see cref="ListViewBase"/> instance</param>
        /// <param name="e">The <see cref="ItemClickEventArgs"/> instance with the clicked item</param>
        private void HandleItemClick(object sender, ItemClickEventArgs e)
        {
            if (!(Command is ICommand command)) return;
            if (!command.CanExecute(e.ClickedItem)) return;

            command.Execute(e.ClickedItem);
        }

        /// <inheritdoc/>
        protected override void OnAttached()
        {
            base.OnAttached();

            SetResolvedSource(AssociatedObject);
        }

        /// <inheritdoc/>
        protected override void OnDetaching()
        {
            base.OnDetaching();

            SetResolvedSource(null);
        }

        /// <summary>
        /// Sets a new resolved source and wires up the target event if needed
        /// </summary>
        /// <param name="newSource">The new resolved source element</param>
        private void SetResolvedSource(object? newSource)
        {
            if (!(newSource is ListViewBase listView)) return;

            if (_ResolvedSource != null) _ResolvedSource.ItemClick -= HandleItemClick;

            _ResolvedSource = listView;

            if (_ResolvedSource != null) _ResolvedSource.ItemClick += HandleItemClick;
        }
    }
}