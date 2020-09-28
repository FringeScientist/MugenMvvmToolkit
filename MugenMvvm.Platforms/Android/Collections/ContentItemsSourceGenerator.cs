﻿using System;
using Android.Views;
using MugenMvvm.Android.Constants;
using MugenMvvm.Android.Interfaces;
using MugenMvvm.Android.Native.Views;
using MugenMvvm.Android.Native.Views.Support;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Members;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using Object = Java.Lang.Object;

namespace MugenMvvm.Android.Collections
{
    public sealed class ContentItemsSourceGenerator : DiffableBindableCollectionAdapter
    {
        #region Constructors

        private ContentItemsSourceGenerator(View view, IContentTemplateSelector contentTemplateSelector)
        {
            View = view;
            ContentTemplateSelector = contentTemplateSelector;
            DiffableComparer = contentTemplateSelector as IDiffableEqualityComparer;
        }

        #endregion

        #region Properties

        public View View { get; }

        public IContentTemplateSelector ContentTemplateSelector { get; }

        protected override bool IsAlive => View.Handle != IntPtr.Zero;

        #endregion

        #region Methods

        public static ContentItemsSourceGenerator? TryGet(View view)
        {
            view.AttachedValues().TryGet(AndroidInternalConstant.ItemsSourceGenerator, out var provider);
            return provider as ContentItemsSourceGenerator;
        }

        public static ContentItemsSourceGenerator GetOrAdd(View view, IContentTemplateSelector selector)
        {
            Should.NotBeNull(view, nameof(view));
            Should.NotBeNull(selector, nameof(selector));
            return view.AttachedValues().GetOrAdd(AndroidInternalConstant.ItemsSourceGenerator, selector, (o, templateSelector) => new ContentItemsSourceGenerator((View) o, templateSelector));
        }

        protected override void OnAdded(object? item, int index, bool batchUpdate, int version)
        {
            base.OnAdded(item, index, batchUpdate, version);
            ViewGroupExtensions.Add(View, GetItem(item)!, index, false);
        }

        protected override void OnMoved(object? item, int oldIndex, int newIndex, bool batchUpdate, int version)
        {
            base.OnMoved(item, oldIndex, newIndex, batchUpdate, version);
            var selected = ViewGroupExtensions.GetSelectedIndex(View) == oldIndex;
            if (TabLayoutExtensions.IsSupported(View))
            {
                ViewGroupExtensions.Remove(View, oldIndex);
                ViewGroupExtensions.Add(View, GetItem(item)!, newIndex, selected);
            }
            else
            {
                var target = ViewGroupExtensions.Get(View, oldIndex);
                ViewGroupExtensions.Remove(View, oldIndex);
                ViewGroupExtensions.Add(View, target, newIndex, selected);
            }
        }

        protected override void OnRemoved(object? item, int index, bool batchUpdate, int version)
        {
            base.OnRemoved(item, index, batchUpdate, version);
            ViewGroupExtensions.Remove(View, index);
        }

        protected override void OnReplaced(object? oldItem, object? newItem, int index, bool batchUpdate, int version)
        {
            base.OnReplaced(oldItem, newItem, index, batchUpdate, version);
            ViewGroupExtensions.Remove(View, index);
            ViewGroupExtensions.Add(View, GetItem(newItem)!, index, false);
        }

        protected override void OnClear(bool batchUpdate, int version)
        {
            base.OnClear(batchUpdate, version);
            ViewGroupExtensions.Clear(View);
        }

        private Object? GetItem(object? item)
        {
            var template = (Object?) ContentTemplateSelector.SelectTemplate(View, item);
            if (template != null)
            {
                template.BindableMembers().SetDataContext(item);
                template.BindableMembers().SetParent(View);
            }

            return template;
        }

        #endregion
    }
}