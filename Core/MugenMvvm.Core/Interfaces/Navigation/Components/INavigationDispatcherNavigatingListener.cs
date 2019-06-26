﻿using System.Threading.Tasks;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface INavigationDispatcherNavigatingListener : IComponent<INavigationDispatcher>
    {
        Task<bool>? OnNavigatingAsync(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext);
    }
}