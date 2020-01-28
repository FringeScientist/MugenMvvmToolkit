﻿using System;
using MugenMvvm.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Internal
{
    public sealed class Tracer : ComponentOwnerBase<ITracer>, ITracer
    {
        #region Constructors

        public Tracer(IComponentCollectionProvider? componentCollectionProvider = null)
            : base(componentCollectionProvider)
        {
        }

        #endregion

        #region Implementation of interfaces

        public bool CanTrace(TraceLevel level, IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<ITracerComponent>().CanTrace(level, metadata);
        }

        public void Trace(TraceLevel level, string message, Exception? exception = null, IReadOnlyMetadataContext? metadata = null)
        {
            GetComponents<ITracerComponent>().Trace(level, message, exception, metadata);
        }

        #endregion

        #region Methods

        public static MugenExtensions.TracerWithLevel? Info(IReadOnlyMetadataContext? metadata = null)
        {
            return MugenService.Optional<ITracer>().Info(metadata);
        }

        public static MugenExtensions.TracerWithLevel? Warn(IReadOnlyMetadataContext? metadata = null)
        {
            return MugenService.Optional<ITracer>().Warn(metadata);
        }

        public static MugenExtensions.TracerWithLevel? Error(IReadOnlyMetadataContext? metadata = null)
        {
            return MugenService.Optional<ITracer>().Error(metadata);
        }

        #endregion
    }
}