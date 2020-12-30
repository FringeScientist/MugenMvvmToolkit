﻿using System;
using System.Diagnostics.CodeAnalysis;
using Android.OS;
using MugenMvvm.Android.Constants;
using MugenMvvm.Android.Enums;
using MugenMvvm.Android.Native.Interfaces.Views;
using MugenMvvm.Android.Native.Views;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Requests;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;
using MugenMvvm.Metadata;
using MugenMvvm.Requests;
using MugenMvvm.Views;

namespace MugenMvvm.Android.Views
{
    public sealed class ViewStateDispatcher : IViewLifecycleListener, IHasPriority
    {
        #region Fields

        private readonly IPresenter? _presenter;
        private readonly ISerializer? _serializer;
        private readonly IViewModelManager? _viewModelManager;

        #endregion

        #region Constructors

        public ViewStateDispatcher(IViewModelManager? viewModelManager = null, IPresenter? presenter = null, ISerializer? serializer = null)
        {
            _viewModelManager = viewModelManager;
            _presenter = presenter;
            _serializer = serializer;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ViewComponentPriority.StateManager;

        #endregion

        #region Implementation of interfaces

        public void OnLifecycleChanged(IViewManager viewManager, object view, ViewLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata)
        {
            if (lifecycleState == AndroidViewLifecycleState.SavingState && view is IView v && TryGetBundle(state, out var bundle))
                PreserveState(v, bundle, metadata);
            else if (lifecycleState == AndroidViewLifecycleState.Creating && TryGetBundle(state, out bundle))
                TryRestore(viewManager, view, bundle, state, metadata);
            else if (lifecycleState == AndroidViewLifecycleState.Created)
                TryRestore(viewManager, view, null, state, metadata);
        }

        #endregion

        #region Methods

        private void TryRestore(IViewManager viewManager, object view, Bundle? b, object? state, IReadOnlyMetadataContext? metadata)
        {
            if (viewManager.IsInState(view, AndroidViewLifecycleState.PendingInitialization))
                return;

            view = MugenExtensions.Unwrap(view);
            var request = TryRestoreState(view, b, metadata);
            if (b == null)
            {
                if (request != null)
                    viewManager.TryInitializeAsync(ViewMapping.Undefined, request, default, metadata);
                return;
            }

            if (request == null)
            {
                FragmentMugenExtensions.ClearFragmentState(b);
                if (view is IActivityView av)
                    Finish(av);
                else if (view is IFragmentView f)
                    FragmentMugenExtensions.Remove(f);
            }
            else if (_presenter.DefaultIfNull().TryShow(request, default, metadata).IsEmpty)
            {
                if (view is IActivityView activity)
                    activity.Finish();
                else
                    viewManager.TryInitializeAsync(ViewMapping.Undefined, request, default, metadata);
            }
            else
                viewManager.OnLifecycleChanged(view, AndroidViewLifecycleState.PendingInitialization, state, metadata);
        }

        private void Finish(IActivityView activityView)
        {
            if (!ActivityMugenExtensions.IsTaskRoot(activityView) || _presenter.DefaultIfNull().Show(activityView).IsEmpty)
                activityView.Finish();
        }

        private void PreserveState(IView view, Bundle bundle, IReadOnlyMetadataContext? metadata)
        {
            bundle.PutString(AndroidInternalConstant.BundleVmId, view.ViewModel.GetId());

            var serializer = _serializer.DefaultIfNull();
            if (!serializer.IsSupported(SerializationFormat.AppStateBytes))
            {
                bundle.Remove(AndroidInternalConstant.BundleViewState);
                return;
            }

            var state = ViewModelMetadata.ViewModel.ToContext(view.ViewModel);
            ReadOnlyMemory<byte> buffer = default;
            if (serializer.TrySerialize(SerializationFormat.AppStateBytes, state, ref buffer, metadata))
                bundle.PutByteArray(AndroidInternalConstant.BundleViewState, buffer.ToArray());
            else
                bundle.Remove(AndroidInternalConstant.BundleViewState);
        }

        private ViewModelViewRequest? TryRestoreState(object view, Bundle? bundle, IReadOnlyMetadataContext? metadata)
        {
            var id = bundle?.GetString(AndroidInternalConstant.BundleVmId) ?? GetViewModelId(view);
            if (string.IsNullOrEmpty(id))
                return null;

            var viewModel = _viewModelManager.DefaultIfNull().TryGetViewModel(id!, metadata);
            if (viewModel == null)
            {
                if (bundle == null)
                    return null;

                var serializer = _serializer.DefaultIfNull();
                if (!serializer.IsSupported(DeserializationFormat.AppStateBytes))
                    return null;

                var state = bundle.GetByteArray(AndroidInternalConstant.BundleViewState);
                if (state == null)
                    return null;

                IReadOnlyMetadataContext? restoredState = null;
                if (!serializer.TryDeserialize(DeserializationFormat.AppStateBytes, state, ref restoredState))
                    return null;

                viewModel = restoredState.Get(ViewModelMetadata.ViewModel);
                if (viewModel == null)
                    return null;
            }

            return new ViewModelViewRequest(viewModel, view);
        }

        private static bool TryGetBundle(object? state, [NotNullWhen(true)] out Bundle? bundle)
        {
            while (true)
            {
                if (state is Bundle b)
                {
                    bundle = b;
                    return true;
                }

                if (state is ICancelableRequest cancelableRequest)
                {
                    if (cancelableRequest.Cancel.GetValueOrDefault())
                    {
                        bundle = null;
                        return false;
                    }

                    state = cancelableRequest.State;
                    continue;
                }

                bundle = null;
                return false;
            }
        }

        private static string? GetViewModelId(object view) => view is IActivityView activityView ? ActivityMugenExtensions.GetViewModelId(activityView) : null;

        #endregion
    }
}