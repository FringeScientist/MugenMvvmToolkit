﻿using System;
using Android.Views;
using MugenMvvm.Android.Interfaces;
using MugenMvvm.Android.Members;
using MugenMvvm.Binding.Build;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Members;

namespace MugenMvvm.Android.Binding
{
    public sealed class MenuTemplate : IMenuTemplate
    {
        #region Properties

        public Action<IMenu, object>? ApplyHandler { get; set; }

        public Action<IMenu>? ClearHandler { get; set; }

        public string? Bind { get; set; }

        public IMenuItemTemplate? ItemTemplate { get; set; }

        #endregion

        #region Implementation of interfaces

        public void Apply(IMenu menu, object owner)
        {
            menu.BindableMembers().SetParent(owner);
            if (ItemTemplate != null)
                menu.BindableMembers().SetItemTemplate(ItemTemplate);
            if (Bind != null)
                menu.BindWithoutResult(Bind);
            ApplyHandler?.Invoke(menu, owner);
        }

        public void Clear(IMenu menu)
        {
            ClearHandler?.Invoke(menu);
            ClearMenu(menu);
        }

        #endregion

        #region Methods

        public static void ClearMenu(IMenu? menu)
        {
            if (menu == null)
                return;
            var size = menu.Size();
            for (var i = 0; i < size; i++)
                MenuItemTemplate.ClearMenuItem(menu.GetItem(i));
            menu.Clear();
            MugenBindingExtensions.ClearBindings(menu, true);
        }

        #endregion
    }
}