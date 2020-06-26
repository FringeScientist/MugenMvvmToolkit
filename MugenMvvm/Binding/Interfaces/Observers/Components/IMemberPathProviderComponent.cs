﻿using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Observers.Components
{
    public interface IMemberPathProviderComponent : IComponent<IObserverProvider>
    {
        IMemberPath? TryGetMemberPath<TPath>([DisallowNull]in TPath path, IReadOnlyMetadataContext? metadata);
    }
}