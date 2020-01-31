﻿using System.Linq;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;
using MugenMvvm.Metadata;

namespace MugenMvvm.Navigation.Components
{
    public sealed class NavigationEntryFinderComponent : AttachableComponentBase<INavigationDispatcher>, INavigationEntryFinderComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = NavigationComponentPriority.EntryFinder;

        #endregion

        #region Implementation of interfaces

        public INavigationEntry? TryGetPreviousNavigationEntry(INavigationEntry navigationEntry, IReadOnlyMetadataContext? metadata)
        {
            if (navigationEntry.NavigationType.IsUndefined)
                return null;

            if (navigationEntry.NavigationType.IsSystemNavigation)
                return GetLastRootEntry(metadata);

            var entries = Owner.GetNavigationEntries(navigationEntry.NavigationType, metadata);
            if (entries.Count == 0 && !navigationEntry.NavigationType.IsNestedNavigation)
                return GetLastRootEntry(metadata);

            var result = entries
                .Where(entry => entry.NavigationProvider.Id == navigationEntry.NavigationProvider.Id && entry.NavigationType == navigationEntry.NavigationType)
                .OrderByDescending(entry => entry.Metadata.Get(NavigationMetadata.NavigationDate))
                .FirstOrDefault();
            if (result == null && navigationEntry.NavigationType.IsRootNavigation)
                return GetLastRootEntry(metadata);
            return result;
        }

        #endregion

        #region Methods

        private INavigationEntry? GetLastRootEntry(IReadOnlyMetadataContext? metadata)
        {
            return Owner.GetNavigationEntries(null, metadata)
                .Where(entry => entry.NavigationType.IsRootNavigation)
                .OrderByDescending(entry => entry.Metadata.Get(NavigationMetadata.NavigationDate))
                .FirstOrDefault();
        }

        #endregion
    }
}