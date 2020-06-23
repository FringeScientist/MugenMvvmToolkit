﻿using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Entities;
using MugenMvvm.Interfaces.Entities.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Entities.Components
{
    public sealed class EntityStateTransitionManager : IEntityStateChangingListener, IHasPriority
    {
        #region Fields

        public static readonly EntityStateTransitionManager Instance = new EntityStateTransitionManager();

        #endregion

        #region Constructors

        private EntityStateTransitionManager()
        {
        }

        #endregion

        #region Properties

        public static int Priority { get; set; } = EntityComponentPriority.StateTransitionManager;

        int IHasPriority.Priority => Priority;

        #endregion

        #region Implementation of interfaces

        public EntityState OnEntityStateChanging(IEntityTrackingCollection collection, object entity, EntityState from, EntityState to, IReadOnlyMetadataContext? metadata)
        {
            if (from == EntityState.Added)
            {
                if (to == EntityState.Deleted)
                    return EntityState.Detached;
                if (to == EntityState.Modified)
                    return EntityState.Added;
            }
            else if (from == EntityState.Deleted && to == EntityState.Added)
                return EntityState.Modified;

            return to;
        }

        #endregion
    }
}