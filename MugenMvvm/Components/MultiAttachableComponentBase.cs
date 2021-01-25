﻿using System.Runtime.CompilerServices;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Components
{
    public abstract class MultiAttachableComponentBase<T> : IAttachableComponent, IDetachableComponent where T : class
    {
        private object? _owners;

        protected ItemOrArray<T> Owners
        {
            // ReSharper disable once InconsistentlySynchronizedField
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ItemOrArray.FromRawValue<T>(_owners);
        }

        protected virtual bool OnAttaching(T owner, IReadOnlyMetadataContext? metadata) => true;

        protected virtual void OnAttached(T owner, IReadOnlyMetadataContext? metadata)
        {
        }

        protected virtual bool OnDetaching(T owner, IReadOnlyMetadataContext? metadata) => true;

        protected virtual void OnDetached(T owner, IReadOnlyMetadataContext? metadata)
        {
        }

        bool IAttachableComponent.OnAttaching(object owner, IReadOnlyMetadataContext? metadata)
        {
            if (owner is T o)
                return OnAttaching(o, metadata);
            return true;
        }

        void IAttachableComponent.OnAttached(object owner, IReadOnlyMetadataContext? metadata)
        {
            if (owner is not T o)
                return;

            lock (this)
            {
                MugenExtensions.AddRaw(ref _owners, o);
            }

            OnAttached(o, metadata);
        }

        bool IDetachableComponent.OnDetaching(object owner, IReadOnlyMetadataContext? metadata)
        {
            if (owner is T o)
                return OnDetaching(o, metadata);
            return true;
        }

        void IDetachableComponent.OnDetached(object owner, IReadOnlyMetadataContext? metadata)
        {
            if (owner is not T o)
                return;

            OnDetached(o, metadata);
            lock (this)
            {
                MugenExtensions.RemoveRaw(ref _owners, o);
            }
        }
    }
}