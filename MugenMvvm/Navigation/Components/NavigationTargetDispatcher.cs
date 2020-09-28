﻿using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Navigation.Components
{
    public sealed class NavigationTargetDispatcher : INavigationConditionComponent, INavigationListener, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = NavigationComponentPriority.Condition;

        #endregion

        #region Implementation of interfaces

        public async Task<bool> CanNavigateAsync(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, CancellationToken cancellationToken)
        {
            var nextTarget = navigationDispatcher.GetNextNavigationTarget(navigationContext);
            var target = navigationContext.Target;
            if (navigationContext.NavigationMode.IsClose)
            {
                return await CanNavigateFromAsync(navigationDispatcher, target, nextTarget, navigationContext, cancellationToken).ConfigureAwait(false) &&
                       await CanNavigateToAsync(navigationDispatcher, nextTarget, target, navigationContext, cancellationToken).ConfigureAwait(false) &&
                       await ((navigationContext.Target as IHasCloseNavigationCondition)?.CanCloseAsync(navigationDispatcher, navigationContext, cancellationToken) ?? Default.TrueTask);
            }

            return await CanNavigateFromAsync(navigationDispatcher, nextTarget, target, navigationContext, cancellationToken).ConfigureAwait(false) &&
                   await CanNavigateToAsync(navigationDispatcher, target, nextTarget, navigationContext, cancellationToken).ConfigureAwait(false);
        }

        public void OnNavigating(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext)
        {
            var nextTarget = navigationDispatcher.GetNextNavigationTarget(navigationContext);
            if (navigationContext.NavigationMode.IsClose)
            {
                (navigationContext.Target as IHasNavigatingHandler)?.OnNavigatingFrom(navigationDispatcher, navigationContext, nextTarget);
                (nextTarget as IHasNavigatingHandler)?.OnNavigatingTo(navigationDispatcher, navigationContext, navigationContext.Target);
                (navigationContext.Target as IHasCloseNavigationHandler)?.OnClosing(navigationDispatcher, navigationContext);
            }
            else
            {
                (nextTarget as IHasNavigatingHandler)?.OnNavigatingFrom(navigationDispatcher, navigationContext, navigationContext.Target);
                (navigationContext.Target as IHasNavigatingHandler)?.OnNavigatingTo(navigationDispatcher, navigationContext, nextTarget);
            }
        }

        public void OnNavigated(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext)
        {
            var nextTarget = navigationDispatcher.GetNextNavigationTarget(navigationContext);
            if (navigationContext.NavigationMode.IsClose)
            {
                (navigationContext.Target as IHasNavigatedHandler)?.OnNavigatedFrom(navigationDispatcher, navigationContext, nextTarget);
                (nextTarget as IHasNavigatedHandler)?.OnNavigatedTo(navigationDispatcher, navigationContext, navigationContext.Target);
                (navigationContext.Target as IHasCloseNavigationHandler)?.OnClosed(navigationDispatcher, navigationContext);
            }
            else
            {
                (nextTarget as IHasNavigatedHandler)?.OnNavigatedFrom(navigationDispatcher, navigationContext, navigationContext.Target);
                (navigationContext.Target as IHasNavigatedHandler)?.OnNavigatedTo(navigationDispatcher, navigationContext, nextTarget);
            }
        }

        #endregion

        #region Methods

        private static Task<bool> CanNavigateFromAsync(INavigationDispatcher navigationDispatcher, object? target, object? toTarget, INavigationContext navigationContext, CancellationToken cancellationToken)
            => (target as IHasNavigationCondition)?.CanNavigateFromAsync(navigationDispatcher, navigationContext, toTarget, cancellationToken) ?? Default.TrueTask;

        private static Task<bool> CanNavigateToAsync(INavigationDispatcher navigationDispatcher, object? target, object? fromTarget, INavigationContext navigationContext, CancellationToken cancellationToken)
            => (target as IHasNavigationCondition)?.CanNavigateToAsync(navigationDispatcher, navigationContext, fromTarget, cancellationToken) ?? Default.TrueTask;

        #endregion
    }
}