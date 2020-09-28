﻿using Java.Lang;
using MugenMvvm.Android.Native.Interfaces.Views;
using MugenMvvm.Binding.Build;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Members.Descriptors;

namespace MugenMvvm.Android.Binding
{
    public sealed class BindViewCallback : Object, IBindViewCallback
    {
        #region Fields

        private static readonly BindablePropertyDescriptor<object, object?> ItemTemplateSelector = nameof(ItemTemplateSelector);

        #endregion

        #region Implementation of interfaces

        public void Bind(Object view, IViewAttributeAccessor accessor)
        {
            var template = accessor.ItemTemplate;
            if (template != 0)
                ItemTemplateSelector.SetValue(view, SingleResourceTemplateSelector.Get(template));
            view.Bind(accessor.Bind, includeResult: false);
        }

        public void OnSetView(Object owner, Object view) => view.BindableMembers().SetParent(owner);

        #endregion
    }
}