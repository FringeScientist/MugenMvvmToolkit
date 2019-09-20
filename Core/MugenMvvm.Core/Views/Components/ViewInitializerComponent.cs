﻿using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;
using MugenMvvm.Metadata;

namespace MugenMvvm.Views.Components
{
    public sealed class ViewInitializerComponent : AttachableComponentBase<IViewManager>, IViewInitializerComponent, IViewInfoProviderComponent, IHasPriority
    {
        #region Fields

        private readonly IMetadataContextProvider? _metadataContextProvider;

        private static readonly IMetadataContextKey<Dictionary<string, IViewInfo>> ViewsMetadataKey =
            MetadataContextKey.FromMember<Dictionary<string, IViewInfo>>(typeof(ViewInitializerComponent), nameof(ViewsMetadataKey));

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ViewInitializerComponent(IMetadataContextProvider? metadataContextProvider = null)
        {
            _metadataContextProvider = metadataContextProvider;
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        public IReadOnlyList<IViewInfo> GetViews(IViewModelBase viewModel, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            var views = viewModel.Metadata.Get(ViewsMetadataKey);
            if (views == null)
                return Default.EmptyArray<IViewInfo>();
            lock (views)
            {
                return views.Values.ToArray();
            }
        }

        public IViewInitializerResult? TryInitialize(IViewInitializer initializer, IViewModelBase viewModel, object view, IMetadataContext metadata)
        {
            Should.NotBeNull(initializer, nameof(initializer));
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(view, nameof(view));
            Should.NotBeNull(metadata, nameof(metadata));
            var views = viewModel.Metadata.GetOrAdd(ViewsMetadataKey, (object?)null, (object?)null, (context, vm, _) => new Dictionary<string, IViewInfo>());
            ViewInfo viewInfo;
            lock (views)
            {
                if (views.TryGetValue(initializer.Id, out var oldView))
                {
                    if (ReferenceEquals(oldView.View, view))
                        return new ViewInitializerResult(oldView, viewModel, metadata);

                    OnViewCleared(oldView, viewModel, metadata);
                }

                viewInfo = new ViewInfo(initializer, view, null, _metadataContextProvider);
                views[initializer.Id] = viewInfo;
            }

            OnViewInitialized(viewInfo, viewModel, metadata);
            return new ViewInitializerResult(viewInfo, viewModel, metadata);
        }

        public IReadOnlyMetadataContext? TryCleanup(IViewInitializer initializer, IViewInfo viewInfo, IViewModelBase viewModel, IMetadataContext metadata)
        {
            bool removed = false;
            var views = viewModel.Metadata.Get(ViewsMetadataKey);
            if (views != null)
            {
                lock (views)
                {
                    removed = views.Remove(initializer.Id);
                }
            }

            if (removed)
                OnViewCleared(viewInfo, viewModel, metadata);
            return Default.Metadata;
        }

        #endregion

        #region Methods

        private void OnViewInitialized(IViewInfo viewInfo, IViewModelBase viewModel, IMetadataContext metadata)
        {
            var components = Owner.GetComponents();
            for (var i = 0; i < components.Length; i++)
                (components[i] as IViewManagerListener)?.OnViewInitialized(Owner, viewInfo, viewModel, metadata);
        }

        private void OnViewCleared(IViewInfo viewInfo, IViewModelBase viewModel, IMetadataContext metadata)
        {
            var components = Owner.GetComponents();
            for (var i = 0; i < components.Length; i++)
                (components[i] as IViewManagerListener)?.OnViewCleared(Owner, viewInfo, viewModel, metadata);
        }

        #endregion
    }
}