﻿#region Copyright

// ****************************************************************************
// <copyright file="PlatformExtensionsCommon.cs">
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
using System.Collections.Generic;
using JetBrains.Annotations;
using MugenMvvmToolkit.Models;
using ObjCRuntime;
using UIKit;

#if XAMARIN_FORMS
using MugenMvvmToolkit.Xamarin.Forms.iOS.Interfaces;

namespace MugenMvvmToolkit.Xamarin.Forms.iOS
{
    public static partial class XamarinFormsTouchToolkitExtensions
#else
using MugenMvvmToolkit.iOS.Interfaces;

namespace MugenMvvmToolkit.iOS
{
    public static partial class TouchToolkitExtensions
#endif
    {
        #region Fields

        private static readonly List<WeakReference> OrientationChangeListeners = new List<WeakReference>();
        private static bool _hasOrientationChangeSubscriber;
        private static bool? _isOs7;
        private static bool? _isOs8;

        #endregion

        #region Properties

        public static bool IsOS7
        {
            get
            {
                if (_isOs7 == null)
                    _isOs7 = UIDevice.CurrentDevice.CheckSystemVersion(7, 0);
                return _isOs7.Value;
            }
        }

        public static bool IsOS8
        {
            get
            {
                if (_isOs8 == null)
                    _isOs8 = UIDevice.CurrentDevice.CheckSystemVersion(8, 0);
                return _isOs8.Value;
            }
        }

        #endregion

        #region Methods

        public static void AddOrientationChangeListener([NotNull] IOrientationChangeListener listener)
        {
            Should.NotBeNull(listener, nameof(listener));
            lock (OrientationChangeListeners)
            {
                if (!_hasOrientationChangeSubscriber)
                {
                    UIApplication.Notifications.ObserveDidChangeStatusBarOrientation(DidChangeStatusBarOrientation);
                    _hasOrientationChangeSubscriber = true;
                }
                OrientationChangeListeners.Add(ToolkitExtensions.GetWeakReference(listener));
            }
        }

        public static void RemoveOrientationChangeListener([NotNull]IOrientationChangeListener listener)
        {
            Should.NotBeNull(listener, nameof(listener));
            lock (OrientationChangeListeners)
            {
                for (int i = 0; i < OrientationChangeListeners.Count; i++)
                {
                    var target = OrientationChangeListeners[i].Target;
                    if (target == null)
                    {
                        OrientationChangeListeners.RemoveAt(i);
                        --i;
                        continue;
                    }
                    if (ReferenceEquals(target, listener))
                    {
                        OrientationChangeListeners.RemoveAt(i);
                        return;
                    }
                }
            }
        }

        internal static PlatformIdiom GetIdiom()
        {
            switch (UIDevice.CurrentDevice.UserInterfaceIdiom)
            {
                case UIUserInterfaceIdiom.Phone:
                    return PlatformIdiom.Phone;
                case UIUserInterfaceIdiom.Pad:
                    return PlatformIdiom.Tablet;
                case UIUserInterfaceIdiom.TV:
                    return PlatformIdiom.TV;
                case UIUserInterfaceIdiom.CarPlay:
                    return PlatformIdiom.Car;
                default:
                    return PlatformIdiom.Unknown;
            }
        }

        [AssertionMethod]
        internal static bool IsAlive([AssertionCondition(AssertionConditionType.IS_NOT_NULL)] this INativeObject item)
        {
            return item != null && item.Handle != IntPtr.Zero;
        }

        private static void DidChangeStatusBarOrientation(object sender, UIStatusBarOrientationChangeEventArgs orientation)
        {
            if (OrientationChangeListeners.Count == 0)
                return;
            var listeners = new List<IOrientationChangeListener>(OrientationChangeListeners.Count);
            lock (OrientationChangeListeners)
            {
                for (int i = 0; i < OrientationChangeListeners.Count; i++)
                {
                    var target = (IOrientationChangeListener)OrientationChangeListeners[i].Target;
                    if (target == null)
                    {
                        OrientationChangeListeners.RemoveAt(i);
                        --i;
                    }
                    else
                        listeners.Add(target);
                }
            }
            for (int index = 0; index < listeners.Count; index++)
                listeners[index].OnOrientationChanged();
        }

        #endregion
    }
}
