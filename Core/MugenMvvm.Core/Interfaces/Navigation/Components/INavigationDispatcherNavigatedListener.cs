﻿using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface INavigationDispatcherNavigatedListener : IComponent<INavigationDispatcher>
    {
        void OnNavigated(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext);
    }
}