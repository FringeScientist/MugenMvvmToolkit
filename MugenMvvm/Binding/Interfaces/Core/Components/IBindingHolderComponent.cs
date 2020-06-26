﻿using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Interfaces.Core.Components
{
    public interface IBindingHolderComponent : IComponent<IBindingManager>
    {
        ItemOrList<IBinding, IReadOnlyList<IBinding>> TryGetBindings(object target, string? path, IReadOnlyMetadataContext? metadata);

        bool TryRegister(object? target, IBinding binding, IReadOnlyMetadataContext? metadata);

        bool TryUnregister(object? target, IBinding binding, IReadOnlyMetadataContext? metadata);
    }
}