﻿#region Copyright

// ****************************************************************************
// <copyright file="PlatformDataBindingModule.cs">
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
using System.Reflection;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Modules;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Xamarin.Forms.Binding.Converters;
using MugenMvvmToolkit.Xamarin.Forms.Binding.Infrastructure;
using MugenMvvmToolkit.Xamarin.Forms.Binding.Models;
using Xamarin.Forms;

namespace MugenMvvmToolkit.Xamarin.Forms.Binding.Modules
{
    public class PlatformDataBindingModule : DataBindingModule
    {
        #region Methods

        private static void Register(IBindingMemberProvider memberProvider)
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember<Entry>(nameof(Entry.Text));
            BindingBuilderExtensions.RegisterDefaultBindingMember<Label>(nameof(Label.Text));
            BindingBuilderExtensions.RegisterDefaultBindingMember<Button>(nameof(Button.Clicked));
            BindingBuilderExtensions.RegisterDefaultBindingMember<ToolbarItem>(nameof(ToolbarItem.Clicked));
            BindingBuilderExtensions.RegisterDefaultBindingMember<ListView>(nameof(ListView.ItemsSource));
            BindingBuilderExtensions.RegisterDefaultBindingMember<ProgressBar>(nameof(ProgressBar.Progress));

            //Element
            memberProvider.Register(AttachedBindingMember
                .CreateMember<Element, object>(AttachedMemberConstants.Parent, GetParentValue, SetParentValue, ObserveParentMember));
            memberProvider.Register(typeof(Element), nameof(Element.BindingContext), BindingMemberProvider.BindingContextMember, true);

            //VisualElement
            var visibleMember = memberProvider.GetBindingMember(typeof(VisualElement), nameof(VisualElement.IsVisible), true, false);
            if (visibleMember != null)
            {
                memberProvider.Register(typeof(VisualElement), AttachedMembers.VisualElement.Visible, visibleMember, true);
                memberProvider.Register(AttachedBindingMember.CreateMember(AttachedMembers.VisualElement.Hidden,
                    (info, element) => !element.IsVisible, (info, element, arg3) => element.IsVisible = !arg3,
                    (info, element, arg3) => visibleMember.TryObserve(element, arg3)));
            }
            memberProvider.Register(AttachedBindingMember
                .CreateMember<VisualElement, object>(AttachedMemberConstants.FindByNameMethod, FindByNameMemberImpl));

            memberProvider.Register(AttachedBindingMember.CreateMember<VisualElement, bool>(AttachedMemberConstants.Focused, (info, element) => element.IsFocused,
                (info, element, arg3) =>
                {
                    if (arg3)
                        element.Focus();
                    else
                        element.Unfocus();
                }, (info, element, arg3) => BindingServiceProvider.WeakEventManager.Subscribe(element, nameof(VisualElement.IsFocused), arg3)));

            var enabledMember = memberProvider.GetBindingMember(typeof(VisualElement), nameof(VisualElement.IsEnabled), true, false);
            if (enabledMember != null)
                memberProvider.Register(typeof(VisualElement), AttachedMemberConstants.Enabled, enabledMember, true);

            //Toolbar item               
            enabledMember = memberProvider.GetBindingMember(typeof(ToolbarItem), "IsEnabled", true, false);
            if (enabledMember != null)
                memberProvider.Register(typeof(ToolbarItem), AttachedMemberConstants.Enabled, enabledMember, true);
        }

        private static object FindByNameMemberImpl(IBindingMemberInfo bindingMemberInfo, VisualElement target, object[] arg3)
        {
            var name = (string)arg3[0];
            return target.FindByName<object>(name);
        }

        private static object GetParentValue(IBindingMemberInfo bindingMemberInfo, Element target)
        {
            return ParentObserver.GetOrAdd(target).Parent;
        }

        private static void SetParentValue(IBindingMemberInfo bindingMemberInfo, Element element, object arg3)
        {
            ParentObserver.GetOrAdd(element).Parent = arg3;
        }

        private static IDisposable ObserveParentMember(IBindingMemberInfo bindingMemberInfo, Element o, IEventListener arg3)
        {
            return ParentObserver.GetOrAdd(o).AddWithUnsubscriber(arg3);
        }

        #endregion

        #region Overrides of DataBindingModule

        protected override void OnLoaded(IModuleContext context)
        {
            base.OnLoaded(context);
            Register(BindingServiceProvider.MemberProvider);
        }

        protected override void RegisterType(Type type)
        {
            base.RegisterType(type);

            if (BindingServiceProvider.DisableConverterAutoRegistration)
                return;
            var typeInfo = type.GetTypeInfo();
            if (!typeof(IValueConverter).GetTypeInfo().IsAssignableFrom(typeInfo) || !type.IsPublicNonAbstractClass())
                return;
            var constructor = type.GetConstructor(Empty.Array<Type>());
            if (constructor == null || !constructor.IsPublic)
                return;
            var converter = (IValueConverter)constructor.Invoke(Empty.Array<object>());
            BindingServiceProvider.ResourceResolver.AddConverter(new ValueConverterWrapper(converter), type, true);
            ServiceProvider.BootstrapCodeBuilder?.Append(nameof(DataBindingModule), $"{typeof(BindingExtensions).FullName}.AddConverter(resolver, new {typeof(ValueConverterWrapper).FullName}(new {type.GetPrettyName()}()), typeof({type.GetPrettyName()}, true);");
            if (Tracer.TraceInformation)
                Tracer.Info("The {0} converter is registered.", type);
        }

        protected override IBindingContextManager GetBindingContextManager(IModuleContext context)
        {
            return new BindingContextManagerEx();
        }

        protected override IBindingResourceResolver GetBindingResourceResolver(IModuleContext context)
        {
            var resolver = BindingServiceProvider.ResourceResolver as BindingResourceResolver;
            return resolver == null
                ? new BindingResourceResolverEx()
                : new BindingResourceResolverEx(resolver);
        }

        #endregion
    }
}
