﻿using System;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Messaging.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTest.Messaging
{
    public class TestMessageContextProviderComponent : IMessageContextProviderComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; }

        public Func<object?, object, IReadOnlyMetadataContext?, IMessageContext?>? TryGetMessageContext { get; set; }

        #endregion

        #region Implementation of interfaces

        IMessageContext? IMessageContextProviderComponent.TryGetMessageContext(object? sender, object message, IReadOnlyMetadataContext? metadata)
        {
            return TryGetMessageContext?.Invoke(sender, message, metadata);
        }

        #endregion
    }
}