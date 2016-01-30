﻿#region Copyright

// ****************************************************************************
// <copyright file="MenuTemplate.cs">
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
using System.Collections.Generic;
using System.Xml.Serialization;
using Android.Content;
using Android.Views;
using MugenMvvmToolkit.Android.Binding.Infrastructure;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Builders;

namespace MugenMvvmToolkit.Android.Binding.Models
{
    [XmlRoot("MENU", Namespace = "")]
    public sealed class MenuTemplate
    {
        #region Properties

        [XmlAttribute("DATACONTEXT")]
        public string DataContext { get; set; }

        [XmlAttribute("BIND")]
        public string Bind { get; set; }

        [XmlAttribute("ISENABLED")]
        public string IsEnabled { get; set; }

        [XmlAttribute("ISVISIBLE")]
        public string IsVisible { get; set; }

        [XmlAttribute("ITEMSSOURCE")]
        public string ItemsSource { get; set; }

        [XmlElement("ITEMTEMPLATE")]
        public MenuItemTemplate ItemTemplate { get; set; }

        [XmlElement("MENUITEM")]
        public List<MenuItemTemplate> Items { get; set; }

        #endregion

        #region Methods

        public void Apply(IMenu menu, Context context, object parent)
        {
            PlatformExtensions.ValidateTemplate(ItemsSource, Items);
            var setter = new XmlPropertySetter<MenuTemplate, IMenu>(menu, context, new BindingSet());
            menu.SetBindingMemberValue(AttachedMembers.Object.Parent, parent);
            setter.SetBinding(nameof(DataContext), DataContext, false);
            setter.SetBoolProperty(nameof(IsVisible), IsVisible);
            setter.SetBoolProperty(nameof(IsEnabled), IsEnabled);
            if (!string.IsNullOrEmpty(Bind))
                setter.BindingSet.BindFromExpression(menu, Bind);
            if (string.IsNullOrEmpty(ItemsSource))
            {
                if (Items != null)
                {
                    for (int index = 0; index < Items.Count; index++)
                        Items[index].Apply(menu, context, index, index);
                }
            }
            else
            {
                menu.SetBindingMemberValue(AttachedMembers.Menu.ItemsSourceGenerator, new MenuItemsSourceGenerator(menu, context, ItemTemplate));
                setter.SetBinding(nameof(ItemsSource), ItemsSource, true);
            }
            setter.Apply();
        }

        public static void Clear(IMenu menu)
        {
            try
            {
                ClearInternal(menu);
            }
            catch (Exception e)
            {
                Tracer.Error(e.Flatten(true));
            }
        }

        internal static void ClearInternal(IMenu menu)
        {
            if (menu == null)
                return;
            int size = menu.Size();
            for (int i = 0; i < size; i++)
                MenuItemTemplate.ClearInternal(menu.GetItem(i));
            menu.Clear();
            menu.ClearBindings(true, true);
        }

        #endregion
    }
}
