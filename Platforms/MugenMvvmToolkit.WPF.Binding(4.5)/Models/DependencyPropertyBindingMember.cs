﻿#region Copyright

// ****************************************************************************
// <copyright file="DependencyPropertyBindingMember.cs">
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

#if WINDOWSCOMMON
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
#else
using System.Windows;
using System.Windows.Data;
#endif
using System;
using System.Reflection;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.DataConstants;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Models;

#if WINDOWSCOMMON
using BindingEx = Windows.UI.Xaml.Data.Binding;

namespace MugenMvvmToolkit.WinRT.Binding.Models
#elif WPF
using BindingEx = System.Windows.Data.Binding;

namespace MugenMvvmToolkit.WPF.Binding.Models
#elif SILVERLIGHT
using BindingEx = System.Windows.Data.Binding;

namespace MugenMvvmToolkit.Silverlight.Binding.Models
#elif WINDOWS_PHONE
using MugenMvvmToolkit.Binding.Infrastructure;
using BindingEx = System.Windows.Data.Binding;

namespace MugenMvvmToolkit.WinPhone.Binding.Models
#endif
{
    public sealed class DependencyPropertyBindingMember : IBindingMemberInfo
    {
        #region Nested types

        //BUG: WP doesn't update DataContextProperty on change in parent.
#if WINDOWS_PHONE
        private static class DataContextChangedHelper
        {
        #region Fields

            public static readonly DependencyProperty InternalDataContextProperty = DependencyProperty.RegisterAttached(
                "InternalDataContext", typeof(object), typeof(DataContextChangedHelper),
                new PropertyMetadata(null, OnDataContextChanged));

            public static readonly DependencyProperty DataContextListenerProperty = DependencyProperty.RegisterAttached(
                "DataContextListener", typeof(EventListenerList), typeof(DataContextChangedHelper), new PropertyMetadata(default(EventListenerList)));

        #endregion

        #region Methods

            private static void OnDataContextChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
            {
                var value = (EventListenerList)sender.GetValue(DataContextListenerProperty);
                if (value != null)
                    value.Raise(sender, e);
            }

            public static IDisposable Listen(FrameworkElement control, IEventListener listener)
            {
                var value = (EventListenerList)control.GetValue(DataContextListenerProperty);
                if (value == null)
                {
                    value = new EventListenerList();
                    control.SetValue(DataContextListenerProperty, value);
                    control.SetBinding(InternalDataContextProperty, new BindingEx
                    {
                        ValidatesOnDataErrors = false,
                        NotifyOnValidationError = false,
                        ValidatesOnNotifyDataErrors = false,
                        ValidatesOnExceptions = false
                    });
                }
                return value.AddWithUnsubscriber(listener);
            }

        #endregion
        }
#endif

#if !WINDOWS_UWP
        public sealed class DependencyPropertyListener : DependencyObject, IDisposable
        {
            #region Fields

            public static readonly DependencyProperty ValueProperty = DependencyProperty
                .Register("Value", typeof(object), typeof(DependencyPropertyListener), new PropertyMetadata(null, OnValueChanged));

#if !WINDOWSCOMMON
            private static readonly Action<DependencyPropertyListener> DisposeDelegate = DisposeInternal;
#endif
            private WeakEventListenerWrapper _listener;

            #endregion

            #region Constructors

            public DependencyPropertyListener(DependencyObject source, string propertyToBind, IEventListener listener)
            {
                _listener = listener.ToWeakWrapper();
                BindingOperations.SetBinding(this, ValueProperty,
                    new BindingEx
                    {
                        Path = new PropertyPath(propertyToBind),
                        Source = source,
                        Mode = BindingMode.OneWay,
#if !WINDOWS_PHONE
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
#endif

#if !WINDOWSCOMMON
#if !WPF || !NET4
                        ValidatesOnNotifyDataErrors = false,
#endif
                        ValidatesOnDataErrors = false,
                        ValidatesOnExceptions = false,
                        NotifyOnValidationError = false,

#endif

#if WPF
                        NotifyOnSourceUpdated = false,
                        NotifyOnTargetUpdated = false
#endif
                    });
            }
#if !WINDOWSCOMMON && !WINDOWS_PHONE
            public DependencyPropertyListener(DependencyObject source, DependencyProperty propertyToBind, IEventListener listener)
            {
                _listener = listener.ToWeakWrapper();
                BindingOperations.SetBinding(this, ValueProperty,
                    new System.Windows.Data.Binding
                    {
                        Path = new PropertyPath(propertyToBind),
                        Source = source,
                        Mode = BindingMode.OneWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
#if !WPF || !NET4
                        ValidatesOnNotifyDataErrors = false,
#endif
                        ValidatesOnDataErrors = false,
                        ValidatesOnExceptions = false,
                        NotifyOnValidationError = false,
#if WPF
                        NotifyOnSourceUpdated = false,
                        NotifyOnTargetUpdated = false
#endif
                    });
            }
#endif

            #endregion

            #region Properties

            public object Value
            {
                get { return GetValue(ValueProperty); }
                set { SetValue(ValueProperty, value); }
            }

            #endregion

            #region Methods

            private static void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
            {
                var listener = (DependencyPropertyListener)sender;
                if (!listener._listener.EventListener.TryHandle(sender, EventArgs.Empty))
                    DisposeInternal(listener);
            }

            private static void DisposeInternal(DependencyPropertyListener listener)
            {
                try
                {
                    if (listener._listener.IsEmpty)
                        return;
                    listener.ClearValue(ValueProperty);
                    listener._listener = WeakEventListenerWrapper.Empty;
                }
                catch (InvalidOperationException e)
                {
                    Tracer.Error(e.Flatten(true));
                }
            }

            #endregion

            #region Implementation of IDisposable

            public void Dispose()
            {
#if WINDOWSCOMMON
                if (Dispatcher.HasThreadAccess)
                    DisposeInternal(this);
                else
                    Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => DisposeInternal(this));
#else
                if (Dispatcher.CheckAccess())
                    DisposeInternal(this);
                else
                    Dispatcher.BeginInvoke(DisposeDelegate, this);
#endif
            }

            #endregion
        }
#endif


        #endregion

        #region Fields

        internal static readonly Func<object, bool> IsNamedObjectFunc;

        private readonly bool _canWrite;
        private readonly IBindingMemberInfo _changePropertyMember;
        private readonly DependencyProperty _dependencyProperty;
        private readonly MemberInfo _member;
        private readonly string _path;
        private readonly Type _type;

        #endregion

        #region Constructors

        static DependencyPropertyBindingMember()
        {
            Type type = null;
            bool isNamedObject = false;
            try
            {
                type = DependencyProperty.UnsetValue.GetType();
                isNamedObject = type.FullName.Equals("MS.Internal.NamedObject", StringComparison.Ordinal);
                var methodInfo = typeof(DependencyPropertyBindingMember).GetMethodEx(nameof(Is), MemberFlags.Static | MemberFlags.Public);
                if (methodInfo == null || !isNamedObject)
                    IsNamedObjectFunc = o => false;
                else
                    IsNamedObjectFunc = (Func<object, bool>)ServiceProvider.ReflectionManager.TryCreateDelegate(typeof(Func<object, bool>), null, methodInfo.MakeGenericMethod(type));
            }
            catch
            {
                if (isNamedObject)
                    IsNamedObjectFunc = o => o != null && o.GetType() == type;
                else
                    IsNamedObjectFunc = o => false;
            }
        }

        public DependencyPropertyBindingMember([NotNull] DependencyProperty dependencyProperty, [NotNull] string path,
            [NotNull] Type type, bool readOnly, [CanBeNull] MemberInfo member, [CanBeNull] IBindingMemberInfo changePropertyMember)
        {
            Should.NotBeNull(dependencyProperty, nameof(dependencyProperty));
            Should.NotBeNullOrEmpty(path, nameof(path));
            Should.NotBeNull(type, nameof(type));
            _dependencyProperty = dependencyProperty;
            _path = path;
#if WPF
            _type = dependencyProperty.PropertyType;
            _canWrite = !dependencyProperty.ReadOnly;
#else
            _type = type;
            _canWrite = !readOnly;
#endif
            _member = member;
            _changePropertyMember = changePropertyMember;
        }

        #endregion

        #region Methods

        public static bool Is<T>(object item)
        {
            return item is T;
        }

#if WINDOWS_UWP
        public static IDisposable ObserveProperty(DependencyObject src, DependencyProperty property, IEventListener listener)
        {
            listener = listener.ToWeakEventListener();
            var t = src.RegisterPropertyChangedCallback(property, listener.Handle);
            return WeakActionToken.Create(src, property, t, (dp, p, token) => dp.UnregisterPropertyChangedCallback(p, token));
        }
#endif
        #endregion

        #region Implementation of IBindingMemberInfo

        public string Path => _path;

        public Type Type => _type;

        public MemberInfo Member => _member;

        public BindingMemberType MemberType => BindingMemberType.DependencyProperty;

        public bool CanRead => true;

        public bool CanWrite => _canWrite;

        public bool CanObserve => true;

        public object GetValue(object source, object[] args)
        {
            object value = ((DependencyObject)source).GetValue(_dependencyProperty);
            if (ReferenceEquals(value, DependencyProperty.UnsetValue) || IsNamedObjectFunc(value))
                return BindingConstants.UnsetValue;
            return value;
        }

        public object SetValue(object source, object[] args)
        {
            return SetSingleValue(source, args[0]);
        }

        public object SetSingleValue(object source, object value)
        {
            if (ReferenceEquals(value, BindingConstants.UnsetValue))
                value = DependencyProperty.UnsetValue;
            ((DependencyObject)source).SetValue(_dependencyProperty, value);
            return null;
        }

        public IDisposable TryObserve(object source, IEventListener listener)
        {
#if WINDOWS_PHONE
            var frameworkElement = source as FrameworkElement;
            if (frameworkElement != null && _path == AttachedMemberConstants.DataContext)
                return DataContextChangedHelper.Listen(frameworkElement, listener);
#endif
            if (_changePropertyMember == null)
#if WINDOWS_UWP
                return ObserveProperty((DependencyObject)source, _dependencyProperty, listener);
#elif WINDOWSCOMMON || WINDOWS_PHONE
                return new DependencyPropertyListener((DependencyObject)source, _path, listener);
#else
                return new DependencyPropertyListener((DependencyObject)source, _dependencyProperty, listener);
#endif
            return _changePropertyMember.SetSingleValue(source, listener) as IDisposable;
        }

        #endregion
    }
}
