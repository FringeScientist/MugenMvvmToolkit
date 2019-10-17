﻿using System;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Observers
{
    public interface IObserverProvider : IComponentOwner<IObserverProvider>, IComponent<IBindingManager>
    {
        MemberObserver TryGetMemberObserver<TMember>(Type type, in TMember member, IReadOnlyMetadataContext? metadata = null);

        IMemberPath GetMemberPath<TPath>(in TPath path, IReadOnlyMetadataContext? metadata = null);

        IMemberPathObserver GetMemberPathObserver<TRequest>(object source, in TRequest request, IReadOnlyMetadataContext? metadata = null);
    }
}