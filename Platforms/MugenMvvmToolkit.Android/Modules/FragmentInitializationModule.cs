﻿#region Copyright

// ****************************************************************************
// <copyright file="FragmentInitializationModule.cs">
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

using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;
#if APPCOMPAT
using MugenMvvmToolkit.Android.AppCompat.Infrastructure.Presenters;

namespace MugenMvvmToolkit.Android.AppCompat.Modules
#else
using MugenMvvmToolkit.Android.Infrastructure.Presenters;

namespace MugenMvvmToolkit.Android.Modules
#endif
{
    public class FragmentInitializationModule : IModule
    {
        #region Implementation of IModule

        public int Priority => ApplicationSettings.ModulePriorityDefault;

        public bool Load(IModuleContext context)
        {
            if (context.IocContainer == null)
                return false;
            IViewModelPresenter service;
#if APPCOMPAT
            if (!context.IocContainer.TryGet(out service))
                return false;
#else
            if (!PlatformExtensions.IsApiGreaterThanOrEqualTo17 || !context.IocContainer.TryGet(out service))
                return false;
#endif
            var mediatorFactory = PlatformExtensions.MediatorFactory;
            PlatformExtensions.MediatorFactory = (o, dataContext, arg3) =>
            {
#if APPCOMPAT
                return FragmentExtensions.MvvmFragmentMediatorDefaultFactory(o, dataContext, arg3) ?? mediatorFactory?.Invoke(o, dataContext, arg3);
#else
                return PlatformExtensions.MvvmFragmentMediatorDefaultFactory(o, dataContext, arg3) ?? mediatorFactory?.Invoke(o, dataContext, arg3);
#endif
            };
            service.DynamicPresenters.Add(context.IocContainer.Get<DynamicViewModelWindowPresenter>());
            return true;
        }

        public void Unload(IModuleContext context)
        {
        }

        #endregion
    }
}
