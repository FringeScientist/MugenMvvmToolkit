﻿using MugenMvvm.Binding.Observers;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Core.Components
{
    public interface ITargetValueSetterBindingComponent : IComponent<IBinding>
    {
        bool TrySetTargetValue(in MemberPathLastMember targetMembers, object? newValue, IReadOnlyMetadataContext metadata);
    }
}