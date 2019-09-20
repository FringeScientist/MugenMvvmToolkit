﻿using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;

namespace MugenMvvm.Views
{
    public class ViewManager : ComponentOwnerBase<IViewManager>, IViewManager
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public ViewManager(IComponentCollectionProvider? componentCollectionProvider = null)
            : base(componentCollectionProvider)
        {
        }

        #endregion

        #region Implementation of interfaces

        public IReadOnlyList<IViewInfo> GetViews(IViewModelBase viewModel, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            return GetViewsInternal(viewModel, metadata);
        }

        public IReadOnlyList<IViewInitializer> GetInitializersByView(object view, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(view, nameof(view));
            return GetInitializersByViewInternal(view, metadata);
        }

        public IReadOnlyList<IViewInitializer> GetInitializersByViewModel(IViewModelBase viewModel, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            return GetInitializersByViewModelInternal(viewModel, metadata);
        }

        #endregion

        #region Methods

        protected virtual IReadOnlyList<IViewInfo> GetViewsInternal(IViewModelBase viewModel, IReadOnlyMetadataContext? metadata)
        {
            var components = Components.GetItems();
            if (components.Length == 0)
                return Default.EmptyArray<IViewInfo>();

            var result = new List<IViewInfo>();
            for (var i = 0; i < components.Length; i++)
            {
                var views = (components[i] as IViewInfoProviderComponent)?.GetViews(viewModel, metadata);
                if (views != null && views.Count != 0)
                    result.AddRange(views);
            }

            return result;
        }

        protected virtual IReadOnlyList<IViewInitializer> GetInitializersByViewInternal(object view, IReadOnlyMetadataContext? metadata)
        {
            var components = Components.GetItems();
            if (components.Length == 0)
                return Default.EmptyArray<IViewInitializer>();

            var result = new List<IViewInitializer>();
            for (var i = 0; i < components.Length; i++)
            {
                var initializers = (components[i] as IViewInitializerProviderComponent)?.GetInitializersByView(view, metadata);
                if (initializers != null && initializers.Count != 0)
                    result.AddRange(initializers);
            }

            return result;
        }

        protected virtual IReadOnlyList<IViewInitializer> GetInitializersByViewModelInternal(IViewModelBase viewModel, IReadOnlyMetadataContext? metadata)
        {
            var components = Components.GetItems();
            if (components.Length == 0)
                return Default.EmptyArray<IViewInitializer>();

            var result = new List<IViewInitializer>();
            for (var i = 0; i < components.Length; i++)
            {
                var initializers = (components[i] as IViewInitializerProviderComponent)?.GetInitializersByViewModel(viewModel, metadata);
                if (initializers != null && initializers.Count != 0)
                    result.AddRange(initializers);
            }

            return result;
        }

        #endregion
    }
}