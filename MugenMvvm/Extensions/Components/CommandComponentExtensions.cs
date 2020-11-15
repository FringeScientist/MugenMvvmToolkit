﻿using System;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Extensions.Components
{
    public static class CommandComponentExtensions
    {
        #region Methods

        public static ICompositeCommand? TryGetCommand<TParameter>(this ICommandProviderComponent[] components, ICommandManager commandManager, object request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(commandManager, nameof(commandManager));
            Should.NotBeNull(request, nameof(request));
            for (var i = 0; i < components.Length; i++)
            {
                var result = components[i].TryGetCommand<TParameter>(commandManager, request, metadata);
                if (result != null)
                    return result;
            }

            return null;
        }

        public static void OnCommandCreated<TParameter>(this ICommandManagerListener[] listeners, ICommandManager commandManager, ICompositeCommand command, object request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(commandManager, nameof(commandManager));
            Should.NotBeNull(command, nameof(command));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnCommandCreated<TParameter>(commandManager, command, request, metadata);
        }

        public static bool HasCanExecute(this IConditionCommandComponent[] components, ICompositeCommand command, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(command, nameof(command));
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].HasCanExecute(command, metadata))
                    return true;
            }

            return false;
        }

        public static bool CanExecute(this IConditionCommandComponent[] components, ICompositeCommand command, object? parameter, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(command, nameof(command));
            for (var i = 0; i < components.Length; i++)
            {
                if (!components[i].CanExecute(command, parameter, metadata))
                    return false;
            }

            return true;
        }

        public static void AddCanExecuteChanged(this IConditionEventCommandComponent[] components, ICompositeCommand command, EventHandler? handler, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(command, nameof(command));
            Should.NotBeNull(handler, nameof(handler));
            for (var i = 0; i < components.Length; i++)
                components[i].AddCanExecuteChanged(command, handler, metadata);
        }

        public static void RemoveCanExecuteChanged(this IConditionEventCommandComponent[] components, ICompositeCommand command, EventHandler? handler, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(command, nameof(command));
            Should.NotBeNull(handler, nameof(handler));
            for (var i = 0; i < components.Length; i++)
                components[i].RemoveCanExecuteChanged(command, handler, metadata);
        }

        public static void RaiseCanExecuteChanged(this IConditionEventCommandComponent[] components, ICompositeCommand command, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(command, nameof(command));
            for (var i = 0; i < components.Length; i++)
                components[i].RaiseCanExecuteChanged(command, metadata);
        }

        public static Task ExecuteAsync(this IExecutorCommandComponent[] components, ICompositeCommand command, object? parameter, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(command, nameof(command));
            return components.InvokeAllAsync((command, parameter, metadata), (component, s, c) => component.ExecuteAsync(s.command, s.parameter, s.metadata), default);
        }

        #endregion
    }
}