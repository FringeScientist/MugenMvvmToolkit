﻿using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presentation;
using MugenMvvm.Interfaces.Presentation.Components;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;

namespace MugenMvvm.Presentation
{
    public abstract class ViewPresenterMediatorBase<TView> : IViewPresenterMediatorProviderComponent, IViewPresenterMediator, IHasPriority where TView : class
    {
        public abstract NavigationType NavigationType { get; }

        public int Priority { get; set; }

        protected abstract Task ActivateAsync(IViewModelPresenterMediator mediator, TView view, INavigationContext navigationContext);

        protected abstract Task ShowAsync(IViewModelPresenterMediator mediator, TView view, INavigationContext navigationContext);

        protected abstract Task CloseAsync(IViewModelPresenterMediator mediator, TView view, INavigationContext navigationContext);

        protected virtual bool CanPresent(IPresenter presenter, IViewModelBase viewModel, IViewMapping mapping, IReadOnlyMetadataContext? metadata) =>
            typeof(TView).IsAssignableFrom(mapping.ViewType);

        protected virtual object? TryGetViewRequest(IViewModelPresenterMediator mediator, TView? view, INavigationContext navigationContext) => null;

        protected virtual void Initialize(IViewModelPresenterMediator mediator, TView view, INavigationContext navigationContext)
        {
        }

        protected virtual void Cleanup(IViewModelPresenterMediator mediator, TView view, INavigationContext navigationContext)
        {
        }

        object? IViewPresenterMediator.TryGetViewRequest(IViewModelPresenterMediator mediator, object? view, INavigationContext navigationContext)
            => TryGetViewRequest(mediator, (TView?) view, navigationContext);

        void IViewPresenterMediator.Initialize(IViewModelPresenterMediator mediator, object view, INavigationContext navigationContext) =>
            Initialize(mediator, (TView) view, navigationContext);

        void IViewPresenterMediator.Cleanup(IViewModelPresenterMediator mediator, object view, INavigationContext navigationContext) => Cleanup(mediator, (TView) view, navigationContext);

        Task IViewPresenterMediator.ActivateAsync(IViewModelPresenterMediator mediator, object view, INavigationContext navigationContext) =>
            ActivateAsync(mediator, (TView) view, navigationContext);

        Task IViewPresenterMediator.ShowAsync(IViewModelPresenterMediator mediator, object view, INavigationContext navigationContext) =>
            ShowAsync(mediator, (TView) view, navigationContext);

        Task IViewPresenterMediator.CloseAsync(IViewModelPresenterMediator mediator, object view, INavigationContext navigationContext) =>
            CloseAsync(mediator, (TView) view, navigationContext);

        IViewPresenterMediator? IViewPresenterMediatorProviderComponent.TryGetViewPresenter(IPresenter presenter, IViewModelBase viewModel, IViewMapping mapping,
            IReadOnlyMetadataContext? metadata) =>
            CanPresent(presenter, viewModel, mapping, metadata) ? this : null;
    }
}