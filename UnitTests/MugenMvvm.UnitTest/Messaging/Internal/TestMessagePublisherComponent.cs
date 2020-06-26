﻿using System;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Messaging.Components;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTest.Messaging.Internal
{
    public class TestMessagePublisherComponent : IMessagePublisherComponent, IHasPriority
    {
        #region Properties

        public Func<IMessageContext, bool>? TryPublish { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        bool IMessagePublisherComponent.TryPublish(IMessageContext messageContext)
        {
            return TryPublish?.Invoke(messageContext) ?? false;
        }

        #endregion
    }
}