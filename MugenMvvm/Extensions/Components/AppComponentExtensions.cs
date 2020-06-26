﻿using MugenMvvm.Enums;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.App.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Extensions.Components
{
    public static class AppComponentExtensions
    {
        #region Methods

        public static void OnLifecycleChanged<TState>(this IApplicationLifecycleDispatcherComponent[] components, IMugenApplication application, ApplicationLifecycleState lifecycleState, in TState state,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(application, nameof(application));
            Should.NotBeNull(lifecycleState, nameof(lifecycleState));
            for (var i = 0; i < components.Length; i++)
                components[i].OnLifecycleChanged(application, lifecycleState, state, metadata);
        }

        #endregion
    }
}