﻿using System;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Test.TestModels
{
    public class SpyHandler : IHandler<object>
    {
        #region Properties

        public Action<object, object> HandleDelegate { get; set; }

        public int HandleCount { get; set; }

        #endregion

        #region Implementation of IHandler<in object>

        void IHandler<object>.Handle(object sender, object message)
        {
            HandleCount++;
            if (HandleDelegate != null)
                HandleDelegate(sender, message);
        }

        #endregion
    }
}
