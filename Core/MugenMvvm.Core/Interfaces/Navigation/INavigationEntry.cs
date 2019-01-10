﻿using System;
using System.Collections.Generic;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface INavigationEntry
    {
        DateTime NavigationDate { get; }

        NavigationType NavigationType { get; }

        object NavigationProvider { get; }

        IViewModel? ViewModel { get; }

        IReadOnlyList<INavigationCallback> GetCallbacks(NavigationCallbackType? callbackType, IReadOnlyMetadataContext metadata);
    }
}