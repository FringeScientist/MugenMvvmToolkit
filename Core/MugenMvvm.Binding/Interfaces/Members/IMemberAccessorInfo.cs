﻿using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Members
{
    public interface IMemberAccessorInfo : IObservableMemberInfo
    {
        bool CanRead { get; }

        bool CanWrite { get; }

        object? GetValue(object? target, IReadOnlyMetadataContext? metadata = null);

        void SetValue(object? target, object? value, IReadOnlyMetadataContext? metadata = null);
    }
}