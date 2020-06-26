﻿using System;
using System.Threading.Tasks;
using MugenMvvm.Busy;
using MugenMvvm.Extensions.Internal;
using MugenMvvm.Interfaces.Busy;
using MugenMvvm.Interfaces.Busy.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        #region Methods

        public static IBusyToken BeginBusy<TRequest>(this IBusyManager provider, in TRequest request, IReadOnlyMetadataContext? metadata = null)
        {
            var token = provider.TryBeginBusy(request, metadata);
            if (token == null)
                ExceptionManager.ThrowObjectNotInitialized<IBusyManagerComponent>(provider);
            return token;
        }

        public static TTask WithBusyIndicator<TTask>(this TTask task, IHasService<IBusyManager> busyIndicatorProvider,
            object? message = null, int millisecondsDelay = 0, IReadOnlyMetadataContext? metadata = null)
            where TTask : Task
        {
            Should.NotBeNull(busyIndicatorProvider, nameof(busyIndicatorProvider));
            return task.WithBusyIndicator(busyIndicatorProvider.Service, message, millisecondsDelay, metadata);
        }

        public static TTask WithBusyIndicator<TTask>(this TTask task, IBusyManager busyManager, object? message = null, int millisecondsDelay = 0, IReadOnlyMetadataContext? metadata = null)
            where TTask : Task
        {
            Should.NotBeNull(task, nameof(task));
            Should.NotBeNull(busyManager, nameof(busyManager));
            if (task.IsCompleted)
                return task;
            if (millisecondsDelay == 0 && message is IHasBusyDelayMessage hasBusyDelay)
                millisecondsDelay = hasBusyDelay.Delay;
            var token = busyManager.BeginBusy(new DelayBusyRequest(message, millisecondsDelay), metadata);
            task.ContinueWith((t, o) => ((IDisposable)o).Dispose(), token, TaskContinuationOptions.ExecuteSynchronously);
            return task;
        }

        public static void ClearBusy(this IBusyManager provider)
        {
            Should.NotBeNull(provider, nameof(provider));
            var tokens = provider.GetTokens();
            for (var i = 0; i < tokens.Count(); i++)
                tokens.Get(i).Dispose();
        }

        #endregion
    }
}