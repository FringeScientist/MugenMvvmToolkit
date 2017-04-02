﻿#region Copyright

// ****************************************************************************
// <copyright file="AttachedValueProvider.cs">
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

using System;
using System.Runtime.CompilerServices;
using MugenMvvmToolkit.Collections;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Models;
#if WINDOWS_UWP
using Windows.UI.Xaml;

namespace MugenMvvmToolkit.UWP.Infrastructure
#elif XAMARIN_FORMS
using Xamarin.Forms;

namespace MugenMvvmToolkit.Xamarin.Forms.Infrastructure
#elif TOUCH
using MugenMvvmToolkit.iOS.Models;
using Foundation;
using ObjCRuntime;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace MugenMvvmToolkit.iOS.Infrastructure
#elif ANDROID
using Android.OS;
using Android.Runtime;
using Android.Views;

namespace MugenMvvmToolkit.Android.Infrastructure
#elif WPF
using System.Windows;

namespace MugenMvvmToolkit.WPF.Infrastructure
#elif WINFORMS
using System.Windows;

namespace MugenMvvmToolkit.WinForms.Infrastructure
#endif
{
    public class AttachedValueProvider : AttachedValueProviderBase
    {
        #region Nested types

#if TOUCH
        private enum OBJC_ASSOCIATION_POLICY
        {
            OBJC_ASSOCIATION_RETAIN = 01401
        }

        private sealed class IntPtrComparer : IEqualityComparer<IntPtr>
        {
        #region Implementation of IEqualityComparer<in IntPtr>

            public bool Equals(IntPtr x, IntPtr y)
            {
                return x == y;
            }

            public int GetHashCode(IntPtr obj)
            {
                return obj.GetHashCode();
            }

        #endregion
        }

        [Register("NSObjectEx")]
        private abstract class NSObjectEx : NSObject
        {
        }

        [Register("AttachedValueHolder")]
        private sealed class AttachedValueHolder : NSObjectEx
        {
        #region Fields

            internal readonly NativeObjectWeakReference WeakReference;
            private AttachedValueDictionary _dictionary;

        #endregion

        #region Constructors

            public AttachedValueHolder(NSObject target)
            {
                objc_setAssociatedObject(target.Handle, AttachedValueKeyHandle, Handle, OBJC_ASSOCIATION_POLICY.OBJC_ASSOCIATION_RETAIN);
                WeakReference = new NativeObjectWeakReference(target);
            }

        #endregion

        #region Methods

            public AttachedValueDictionary GetOrCreateDictionary()
            {
                if (_dictionary == null)
                    Interlocked.CompareExchange(ref _dictionary, new AttachedValueDictionary(), null);
                return _dictionary;
            }

            [Export("dealloc")]
            private void Dealloc()
            {
                WeakReference.IsInvalid = true;
                lock (AttachedValueHolders)
                    AttachedValueHolders.Remove(WeakReference.Handle);
                if (TouchToolkitExtensions.AttachedValueProviderSuppressFinalize)
                    GC.SuppressFinalize(this);
                else
                    Dispose();
            }

        #endregion
        }

#elif ANDROID
        private sealed class AttachedValueDictionaryJava : Java.Lang.Object
        {
        #region Fields

            public readonly AttachedValueDictionary Dictionary;

        #endregion

        #region Constructors

            public AttachedValueDictionaryJava(IntPtr handle, JniHandleOwnership transfer)
                : base(handle, transfer)
            {
                Dictionary = new AttachedValueDictionary();
                Tracer.Error("The {0} is recreated", this);
            }

            public AttachedValueDictionaryJava()
            {
                Dictionary = new AttachedValueDictionary();
            }

        #endregion
        }
#endif
        #endregion

        #region Fields

#if WINDOWS_UWP || WPF || WINFORMS
        //NOTE ConditionalWeakTable incorrectly tracks WinRT objects https://connect.microsoft.com/VisualStudio/feedback/details/930200/conditionalweaktable-incorrectly-tracks-winrt-objects
        private static readonly DependencyProperty AttachedValueDictionaryProperty = DependencyProperty.RegisterAttached(
            "AttachedValueDictionary", typeof(object), typeof(DependencyObject), new PropertyMetadata(null));
#elif XAMARIN_FORMS
        private static readonly BindableProperty AttachedValueDictionaryProperty = BindableProperty
            .CreateAttached("AttachedValueDictionary", typeof(AttachedValueDictionary), typeof(AttachedValueProvider),
                null);
#elif TOUCH
        private static readonly IntPtr AttachedValueKeyHandle = new NSObject().Handle;
        private static readonly Dictionary<IntPtr, AttachedValueHolder> AttachedValueHolders = new Dictionary<IntPtr, AttachedValueHolder>(109, new IntPtrComparer());
#elif ANDROID
        //Prior to Android 4.0, the implementation of View.setTag(int, Object) would store the objects in a static map, where the values were strongly referenced.
        //This means that if the object contains any references pointing back to the context, the context (which points to pretty much everything else) will leak.
        internal static readonly bool SetTagSupported = Build.VERSION.SdkInt >= BuildVersionCodes.IceCreamSandwich;
#endif
        private static readonly ConditionalWeakTable<object, AttachedValueDictionary>.CreateValueCallback
            CreateDictionaryDelegate = o => new AttachedValueDictionary();

        private readonly ConditionalWeakTable<object, AttachedValueDictionary> _internalDictionary =
            new ConditionalWeakTable<object, AttachedValueDictionary>();

        #endregion

        #region Constructors

        [MugenMvvmToolkit.Attributes.Preserve(Conditional = true)]
        public AttachedValueProvider()
        {
        }

        #endregion

        #region Methods

#if TOUCH
        [DllImport(Constants.ObjectiveCLibrary)]
        private static extern void objc_setAssociatedObject(IntPtr target, IntPtr key, IntPtr value,
            OBJC_ASSOCIATION_POLICY policy);

        internal static WeakReference GetNativeObjectWeakReference(NSObject nsObject)
        {
            var handle = nsObject.Handle;
            if (handle == IntPtr.Zero)
                return Empty.WeakReference;
            lock (AttachedValueHolders)
            {
                AttachedValueHolder value;
                if (!AttachedValueHolders.TryGetValue(handle, out value))
                {
                    value = new AttachedValueHolder(nsObject);
                    AttachedValueHolders[handle] = value;
                }
                return value.WeakReference;
            }
        }
#endif
        #endregion

        #region Properties

        public event Func<object, bool> ClearHandler;

        public event Func<object, bool, LightDictionaryBase<string, object>> GetOrAddAttachedDictionaryHandler;

        #endregion

        #region Overrides of AttachedValueProviderBase<WeakKey,AttachedValueDictionary>

        protected override bool ClearInternal(object item)
        {
            var handler = ClearHandler?.GetInvocationList();
            if (handler != null)
            {
                for (int i = 0; i < handler.Length; i++)
                {
                    if (((Func<object, bool>)handler[i]).Invoke(item))
                        return true;
                }
            }
            var model = item as NotifyPropertyChangedBase;
            if (model != null)
            {
                ClearAttachedValues(model);
                return true;
            }
#if TOUCH
            var nsObject = item as NSObject;
            if (nsObject != null)
            {
                var handle = nsObject.Handle;
                if (handle == IntPtr.Zero)
                    return false;
                objc_setAssociatedObject(handle, AttachedValueKeyHandle, IntPtr.Zero, OBJC_ASSOCIATION_POLICY.OBJC_ASSOCIATION_RETAIN);

                AttachedValueHolder value;
                lock (AttachedValueHolders)
                {
                    if (AttachedValueHolders.TryGetValue(handle, out value))
                        AttachedValueHolders.Remove(handle);
                }
                if (value == null)
                    return false;
                value.Dispose();
                return true;
            }
#elif WINDOWS_UWP || WPF || WINFORMS
            var dependencyObject = item as DependencyObject;
            if (dependencyObject != null)
            {
                dependencyObject.ClearValue(AttachedValueDictionaryProperty);
                return true;
            }
#elif XAMARIN_FORMS
            var bindableObject = item as BindableObject;
            if (bindableObject != null)
            {
                bindableObject.ClearValue(AttachedValueDictionaryProperty);
                return true;
            }
#elif ANDROID
            if (SetTagSupported)
            {
                var view = item as View;
                if (view.IsAlive())
                {
                    view.SetTag(Resource.Id.AttachedProperties, null);
                    return true;
                }
            }
#endif
            return _internalDictionary.Remove(item);
        }

        protected override LightDictionaryBase<string, object> GetOrAddAttachedDictionary(object item, bool addNew)
        {
            var handler = GetOrAddAttachedDictionaryHandler?.GetInvocationList();
            if (handler != null)
            {
                for (int i = 0; i < handler.Length; i++)
                {
                    var result = ((Func<object, bool, LightDictionaryBase<string, object>>)handler[i]).Invoke(item, addNew);
                    if (result != null)
                        return result;
                }
            }
            var model = item as NotifyPropertyChangedBase;
            if (model != null)
                return GetOrAddAttachedValues(model, true);
#if TOUCH
            var nsObject = item as NSObject;
            if (nsObject != null)
            {
                var handle = nsObject.Handle;
                if (handle == IntPtr.Zero)
                {
                    if (addNew)
                    {
                        Tracer.Error("The object {0} is disposed the attached values cannot be obtained", item.GetType());
                        return new AttachedValueDictionary();
                    }
                    return null;
                }
                AttachedValueHolder holder;
                lock (AttachedValueHolders)
                {
                    if (!AttachedValueHolders.TryGetValue(handle, out holder))
                    {
                        if (!addNew)
                            return null;
                        holder = new AttachedValueHolder(nsObject);
                        AttachedValueHolders[handle] = holder;
                    }
                }
                return holder.GetOrCreateDictionary();
            }
#elif WINDOWS_UWP || WPF || WINFORMS
            //Synchronization is not necessary because accessing the DependencyObject is possible only from the main thread.
            var dependencyObject = item as DependencyObject;
            if (dependencyObject != null)
            {
                var dict = (AttachedValueDictionary)dependencyObject.GetValue(AttachedValueDictionaryProperty);
                if (dict == null && addNew)
                {
                    dict = new AttachedValueDictionary();
                    dependencyObject.SetValue(AttachedValueDictionaryProperty, dict);
                }
                return dict;
            }
#elif XAMARIN_FORMS
            var bindableObject = item as BindableObject;
            if (bindableObject != null)
            {
                var dict = (AttachedValueDictionary)bindableObject.GetValue(AttachedValueDictionaryProperty);
                if (dict == null && addNew)
                {
                    lock (_internalDictionary)
                    {
                        dict = (AttachedValueDictionary)bindableObject.GetValue(AttachedValueDictionaryProperty);
                        if (dict == null)
                        {
                            dict = new AttachedValueDictionary();
                            bindableObject.SetValue(AttachedValueDictionaryProperty, dict);
                        }
                    }
                }
                return dict;
            }
#elif ANDROID
            if (SetTagSupported)
            {
                var view = item as View;
                if (view.IsAlive())
                {
                    var dict = (AttachedValueDictionaryJava)view.GetTag(Resource.Id.AttachedProperties);
                    if (dict == null && addNew)
                    {
                        lock (_internalDictionary)
                        {
                            dict = (AttachedValueDictionaryJava)view.GetTag(Resource.Id.AttachedProperties);
                            if (dict == null)
                            {
                                dict = new AttachedValueDictionaryJava();
                                view.SetTag(Resource.Id.AttachedProperties, dict);
                            }
                        }
                    }
                    return dict?.Dictionary;
                }
            }
#endif
            if (addNew)
                return _internalDictionary.GetValue(item, CreateDictionaryDelegate);
            AttachedValueDictionary value;
            _internalDictionary.TryGetValue(item, out value);
            return value;
        }

        #endregion
    }
}
