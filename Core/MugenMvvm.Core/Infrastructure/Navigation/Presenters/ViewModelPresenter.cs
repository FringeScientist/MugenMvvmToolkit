﻿using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation.Presenters;
using MugenMvvm.Metadata;

namespace MugenMvvm.Infrastructure.Navigation.Presenters
{
    public class ViewModelPresenter : IViewModelPresenter
    {
        #region Fields

        private IViewModelPresenterCallbackManager _callbackManager;

        private IComponentCollection<IViewModelPresenterListener>? _listeners;
        private IComponentCollection<IChildViewModelPresenter>? _presenters;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ViewModelPresenter(IViewModelPresenterCallbackManager callbackManager, IComponentCollectionProvider componentCollectionProvider)
        {
            Should.NotBeNull(callbackManager, nameof(callbackManager));
            Should.NotBeNull(componentCollectionProvider, nameof(componentCollectionProvider));
            ComponentCollectionProvider = componentCollectionProvider;
            CallbackManager = callbackManager;
        }

        #endregion

        #region Properties

        protected IComponentCollectionProvider ComponentCollectionProvider { get; }

        public IViewModelPresenterCallbackManager CallbackManager
        {
            get => _callbackManager;
            set
            {
                Should.NotBeNull(value, nameof(CallbackManager));
                _callbackManager?.OnDetached(this, Default.MetadataContext);
                _callbackManager = value;
                _callbackManager.OnAttached(this, Default.MetadataContext);
            }
        }

        public IComponentCollection<IChildViewModelPresenter> Presenters
        {
            get
            {
                if (_presenters == null)
                    ComponentCollectionProvider.LazyInitialize(ref _presenters, this);
                return _presenters;
            }
        }

        public IComponentCollection<IViewModelPresenterListener> Listeners
        {
            get
            {
                if (_listeners == null)
                    ComponentCollectionProvider.LazyInitialize(ref _listeners, this);
                return _listeners;
            }
        }

        #endregion

        #region Implementation of interfaces

        public IViewModelPresenterResult Show(IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            using (CallbackManager.BeginPresenterOperation(metadata))
            {
                var result = ShowInternal(metadata);
                if (result == null)
                    ExceptionManager.ThrowPresenterCannotShowRequest(metadata);

                return OnShownInternal(result!, metadata);
            }
        }

        public IReadOnlyList<IClosingViewModelPresenterResult> TryClose(IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            using (CallbackManager.BeginPresenterOperation(metadata))
            {
                var result = TryCloseInternal(metadata);
                return OnClosedInternal(result, metadata);
            }
        }

        public IRestorationViewModelPresenterResult TryRestore(IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            using (CallbackManager.BeginPresenterOperation(metadata))
            {
                var result = TryRestoreInternal(metadata);
                return OnRestoredInternal(result, metadata);
            }
        }

        #endregion

        #region Methods

        protected virtual IChildViewModelPresenterResult? ShowInternal(IReadOnlyMetadataContext metadata)
        {
            var presenters = Presenters.GetItems();
            for (var i = 0; i < presenters.Length; i++)
            {
                var presenter = presenters[i];
                if (!CanShow(presenter, metadata))
                    continue;

                var operation = presenter.TryShow(this, metadata);
                if (operation != null)
                    return operation;
            }

            return null;
        }

        protected virtual IViewModelPresenterResult OnShownInternal(IChildViewModelPresenterResult result, IReadOnlyMetadataContext metadata)
        {
            var r = result as IViewModelPresenterResult;
            if (r == null)
            {
                var viewModel = metadata.Get(NavigationMetadata.ViewModel, result.Metadata.Get(NavigationMetadata.ViewModel));
                if (viewModel == null)
                    ExceptionManager.ThrowPresenterInvalidRequest(metadata, result.Metadata);

                var showingCallback = CallbackManager.AddCallback<bool>(viewModel!, NavigationCallbackType.Showing, result, metadata);
                var closeCallback = CallbackManager.AddCallback<bool>(viewModel!, NavigationCallbackType.Close, result, metadata);

                r = new ViewModelPresenterResult(viewModel!, showingCallback, closeCallback, result);
            }

            var listeners = GetListeners();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnShown(this, r, metadata);

            return r;
        }

        protected virtual IReadOnlyList<IChildViewModelPresenterResult> TryCloseInternal(IReadOnlyMetadataContext metadata)
        {
            var results = new List<IChildViewModelPresenterResult>();
            var presenters = Presenters.GetItems();
            for (var i = 0; i < presenters.Length; i++)
            {
                var presenter = presenters[i];
                if (!CanClose(presenter, results, metadata))
                    continue;

                var operations = presenter.TryClose(this, metadata);
                if (operations != null)
                    results.AddRange(operations);
            }

            return results;
        }

        protected virtual IReadOnlyList<IClosingViewModelPresenterResult> OnClosedInternal(IReadOnlyList<IChildViewModelPresenterResult> results, IReadOnlyMetadataContext metadata)
        {
            var r = new List<IClosingViewModelPresenterResult>();
            for (var i = 0; i < results.Count; i++)
            {
                var result = results[i];

                if (result is IClosingViewModelPresenterResult closingViewModelPresenterResult)
                    r.Add(closingViewModelPresenterResult);
                else
                {
                    var viewModel = metadata.Get(NavigationMetadata.ViewModel, result.Metadata.Get(NavigationMetadata.ViewModel));
                    if (viewModel == null)
                        ExceptionManager.ThrowPresenterInvalidRequest(metadata, result.Metadata);

                    var callback = CallbackManager.AddCallback<bool>(viewModel!, NavigationCallbackType.Closing, result, metadata);
                    r.Add(new ClosingViewModelPresenterResult(callback, result));
                }
            }


            var listeners = GetListeners();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnClosed(this, r, metadata);

            return r;
        }

        protected virtual IChildViewModelPresenterResult? TryRestoreInternal(IReadOnlyMetadataContext metadata)
        {
            var presenters = Presenters.GetItems();
            for (var i = 0; i < presenters.Length; i++)
            {
                if (presenters[i] is IRestorableChildViewModelPresenter presenter && CanRestore(presenter, metadata))
                {
                    var result = presenter.TryRestore(this, metadata);
                    if (result != null)
                        return result;
                }
            }

            return null;
        }

        protected virtual IRestorationViewModelPresenterResult OnRestoredInternal(IChildViewModelPresenterResult? result, IReadOnlyMetadataContext metadata)
        {
            var r = result == null
                ? RestorationViewModelPresenterResult.Unrestored
                : result as IRestorationViewModelPresenterResult ?? new RestorationViewModelPresenterResult(true, result);
            var listeners = GetListeners();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i]?.OnRestored(this, r, metadata);

            return r;
        }

        protected virtual bool CanShow(IChildViewModelPresenter childPresenter, IReadOnlyMetadataContext metadata)
        {
            var listeners = GetListeners();
            for (var i = 0; i < listeners.Length; i++)
            {
                var canShow = (listeners[i] as IConditionViewModelPresenterListener)?.CanShow(this, childPresenter, metadata) ?? true;
                if (!canShow)
                    return false;
            }

            return true;
        }

        protected virtual bool CanClose(IChildViewModelPresenter childPresenter, IReadOnlyList<IChildViewModelPresenterResult> currentResults, IReadOnlyMetadataContext metadata)
        {
            var listeners = GetListeners();
            for (var i = 0; i < listeners.Length; i++)
            {
                var canClose = (listeners[i] as IConditionViewModelPresenterListener)?.CanClose(this, childPresenter, currentResults, metadata) ?? true;
                if (!canClose)
                    return false;
            }

            return true;
        }

        protected virtual bool CanRestore(IRestorableChildViewModelPresenter childPresenter, IReadOnlyMetadataContext metadata)
        {
            var listeners = GetListeners();
            for (var i = 0; i < listeners.Length; i++)
            {
                var canRestore = (listeners[i] as IConditionViewModelPresenterListener)?.CanRestore(this, childPresenter, metadata) ?? true;
                if (!canRestore)
                    return false;
            }

            return true;
        }

        protected IViewModelPresenterListener[] GetListeners()
        {
            return _listeners.GetItemsOrDefault();
        }

        #endregion
    }
}