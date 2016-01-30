﻿#region Copyright

// ****************************************************************************
// <copyright file="ValidatesOnExceptionsBehavior.cs">
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
using MugenMvvmToolkit.Binding.DataConstants;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Behaviors
{
    public sealed class ValidatesOnExceptionsBehavior : IBindingBehavior
    {
        #region Fields

        private const string Key = "@@#bexc.";
        public static readonly Guid IdValidatesOnExceptionsBehavior;
        public static readonly ValidatesOnExceptionsBehavior Instance;
        private static readonly EventHandler<IDataBinding, BindingEventArgs> BindingUpdatedDelegate;

        #endregion

        #region Constructors

        static ValidatesOnExceptionsBehavior()
        {
            IdValidatesOnExceptionsBehavior = new Guid("046EC76A-0DC9-4024-B893-7E2AF9E4F636");
            ShowOriginalException = true;
            Instance = new ValidatesOnExceptionsBehavior();
            BindingUpdatedDelegate = OnBindingUpdated;
        }

        private ValidatesOnExceptionsBehavior()
        {
        }

        #endregion

        #region Properties

        public static bool ShowOriginalException { get; set; }

        #endregion

        #region Implementation of IBindingBehavior

        public Guid Id => IdValidatesOnExceptionsBehavior;

        public int Priority => 0;

        public bool Attach(IDataBinding binding)
        {
            if (BindingServiceProvider.ErrorProvider == null)
                return false;
            binding.BindingUpdated += BindingUpdatedDelegate;
            return true;
        }

        public void Detach(IDataBinding binding)
        {
            binding.BindingUpdated -= BindingUpdatedDelegate;
            var errorProvider = BindingServiceProvider.ErrorProvider;
            if (errorProvider == null)
                return;

            var context = new DataContext(binding.Context);
            context.AddOrUpdate(BindingConstants.ClearErrors, true);
            SetErrors(errorProvider, binding, Empty.Array<object>(), context);
        }

        public IBindingBehavior Clone()
        {
            return this;
        }

        #endregion

        #region Methods

        private static void OnBindingUpdated(IDataBinding sender, BindingEventArgs args)
        {
            if (args.Exception == null)
            {
                IBindingErrorProvider errorProvider = BindingServiceProvider.ErrorProvider;
                if (errorProvider != null)
                    SetErrors(errorProvider, sender, Empty.Array<object>(), null);
            }
            else
            {
                IBindingErrorProvider errorProvider = BindingServiceProvider.ErrorProvider;
                if (errorProvider != null)
                    SetErrors(errorProvider, sender,
                        new object[] { ShowOriginalException ? args.OriginalException.Message : args.Exception.Message }, null);
            }
        }

        private static void SetErrors(IBindingErrorProvider errorProvider, IDataBinding sender, object[] errors, IDataContext context)
        {
            IBindingPathMembers pathMembers = sender.TargetAccessor.Source.GetPathMembers(false);
            object target = pathMembers.PenultimateValue;
            if (target != null && !target.IsUnsetValue())
                errorProvider.SetErrors(target, Key + pathMembers.Path.Path, errors, context ?? sender.Context);
        }

        #endregion
    }
}
