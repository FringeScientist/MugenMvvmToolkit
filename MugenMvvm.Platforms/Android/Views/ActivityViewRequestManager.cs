﻿using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Android.Enums;
using MugenMvvm.Android.Interfaces;
using MugenMvvm.Android.Native.Interfaces.Views;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;

namespace MugenMvvm.Android.Views
{
    public sealed class ActivityViewRequestManager : ComponentDecoratorBase<IViewManager, IViewManagerComponent>, IViewManagerComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = ViewComponentPriority.ExecutionModeDecorator + 1;

        #endregion

        #region Implementation of interfaces

        public async ValueTask<IView?> TryInitializeAsync(IViewManager viewManager, IViewMapping mapping, object request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            if (!(request is IActivityViewRequest activityRequest))
            {
                if (MugenExtensions.TryGetViewModelView(request, out IActivityView? activityView) != null && activityView != null)
                    viewManager.OnLifecycleChanged(activityView, AndroidViewLifecycleState.PendingInitialization, request, metadata);

                return await Components.TryInitializeAsync(viewManager, mapping, request, cancellationToken, metadata).ConfigureAwait(false);
            }

            var handler = new PendingActivityHandler(activityRequest.Mapping, cancellationToken);
            viewManager.AddComponent(handler);
            activityRequest.StartActivity();

            activityRequest.View = await handler.Task.ConfigureAwait(false);
            return await Components.TryInitializeAsync(viewManager, mapping, activityRequest, default, metadata).ConfigureAwait(false);
        }

        public Task<bool> TryCleanupAsync(IViewManager viewManager, IView view, object? state, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
            => Components.TryCleanupAsync(viewManager, view, state, cancellationToken, metadata);

        #endregion

        #region Nested types

        private sealed class PendingActivityHandler : TaskCompletionSource<object>, IViewLifecycleDispatcherComponent, IHasPriority
        {
            #region Fields

            private readonly CancellationToken _cancellationToken;
            private readonly IViewMapping _mapping;

            #endregion

            #region Constructors

            public PendingActivityHandler(IViewMapping mapping, CancellationToken cancellationToken)
            {
                _mapping = mapping;
                _cancellationToken = cancellationToken;
            }

            #endregion

            #region Properties

            public int Priority => ViewComponentPriority.PreInitializer;

            #endregion

            #region Implementation of interfaces

            public void OnLifecycleChanged(IViewManager viewManager, object view, ViewLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata)
            {
                if (lifecycleState != AndroidViewLifecycleState.Created)
                    return;
                view = MugenExtensions.GetUnderlyingView(view);
                if (!_mapping.ViewType.IsInstanceOfType(view))
                    return;

                viewManager.RemoveComponent(this);
                if (_cancellationToken.IsCancellationRequested)
                {
                    if (view is IActivityView activityView)
                        activityView.Finish();
                    TrySetCanceled(_cancellationToken);
                }
                else
                {
                    viewManager.OnLifecycleChanged(view, AndroidViewLifecycleState.PendingInitialization, state, metadata);
                    TrySetResult(view);
                }
            }

            #endregion
        }

        #endregion
    }
}