﻿using System;
using System.Collections.Generic;
using MugenMvvm.Entities;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Interfaces.Entities
{
    public interface IEntityTrackingCollection : IComponentOwner<IEntityTrackingCollection>, IReadOnlyCollection<TrackingEntity>
    {
        bool HasChanges { get; }

        ItemOrList<TrackingEntity, IReadOnlyList<TrackingEntity>> GetChanges<TState>(TState state, Func<TrackingEntity, TState, bool> predicate);

        EntityState GetState(object entity, IReadOnlyMetadataContext? metadata = null);

        void SetState(object entity, EntityState state, IReadOnlyMetadataContext? metadata = null);

        void Clear(IReadOnlyMetadataContext? metadata = null);
    }
}