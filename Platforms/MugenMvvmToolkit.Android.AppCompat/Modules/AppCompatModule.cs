﻿#region Copyright

// ****************************************************************************
// <copyright file="AppCompatModule.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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

using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using MugenMvvmToolkit.Android.Binding;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Android.AppCompat.Modules
{
    public class AppCompatModule : IModule
    {
        #region Implementation of IModule

        public int Priority => ApplicationSettings.ModulePriorityInitialization - 2;

        public bool Load(IModuleContext context)
        {
            BindingResourceExtensions.ColorHandler = ContextCompat.GetColor;
            BindingResourceExtensions.DrawableHandler = ContextCompat.GetDrawable;
            var isActionBar = AndroidToolkitExtensions.IsActionBar;
            var isFragment = AndroidToolkitExtensions.IsFragment;
            AndroidToolkitExtensions.IsActionBar = o => isActionBar(o) || o is ActionBar;
            AndroidToolkitExtensions.IsFragment = o => isFragment(o) || o is Fragment;

            AttachedMembersRegistration.RegisterToolbarMembers();
            AttachedMembersRegistration.RegisterDrawerLayoutMembers();
            AttachedMembersRegistration.RegisterViewPagerMembers();
            AttachedMembersRegistration.RegisterPopupMenuMembers();
            AttachedMembersRegistration.RegisterMenuItemMembers();
            AttachedMembersRegistration.RegisterMenuItemActionViewMembers();
            AttachedMembersRegistration.RegisterMenuItemActionProviderMembers();
            AttachedMembersRegistration.RegisterSearchViewMembers();
            AttachedMembersRegistration.RegisterActionBarBaseMembers();
            AttachedMembersRegistration.RegisterActionBarMembers();
            AttachedMembersRegistration.RegisterActionBarTabMembers();

            return true;
        }

        public void Unload(IModuleContext context)
        {
        }

        #endregion
    }
}