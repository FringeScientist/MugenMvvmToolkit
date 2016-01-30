﻿#region Copyright

// ****************************************************************************
// <copyright file="NoneBindingMode.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion

using System;
using MugenMvvmToolkit.Binding.Interfaces;

namespace MugenMvvmToolkit.Binding.Behaviors
{
    public sealed class NoneBindingMode : IBindingBehavior
    {
        #region Fields

        public static readonly NoneBindingMode Instance;

        #endregion

        #region Constructors

        static NoneBindingMode()
        {
            Instance = new NoneBindingMode();
        }

        private NoneBindingMode()
        {
        }

        #endregion

        #region Implementation of IBindingBehavior

        public Guid Id => BindingModeBase.IdBindingMode;

        public int Priority => BindingModeBase.DefaultPriority;

        bool IBindingBehavior.Attach(IDataBinding binding)
        {
            return true;
        }

        void IBindingBehavior.Detach(IDataBinding binding)
        {
        }

        public IBindingBehavior Clone()
        {
            return this;
        }

        #endregion
    }
}
