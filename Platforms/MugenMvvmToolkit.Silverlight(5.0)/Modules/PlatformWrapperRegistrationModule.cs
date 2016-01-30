﻿#region Copyright

// ****************************************************************************
// <copyright file="PlatformWrapperRegistrationModule.cs">
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
using System.ComponentModel;
using System.Windows.Controls;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Views;
using MugenMvvmToolkit.Modules;
using MugenMvvmToolkit.Silverlight.Interfaces.Views;

namespace MugenMvvmToolkit.Silverlight.Modules
{
    public class PlatformWrapperRegistrationModule : WrapperRegistrationModuleBase
    {
        #region Nested types

        private sealed class WindowViewWrapper : IWindowView, IViewWrapper
        {
            #region Fields

            private readonly ChildWindow _window;

            #endregion

            #region Constructors

            public WindowViewWrapper(ChildWindow window)
            {
                Should.NotBeNull(window, nameof(window));
                _window = window;
            }

            #endregion

            #region Implementation of IWindowView

            public object View => _window;

            public void Show()
            {
                _window.Show();
            }

            public void Close()
            {
                _window.Close();
            }

            public void Activate()
            {
            }

            event EventHandler<CancelEventArgs> IWindowView.Closing
            {
                add { _window.Closing += value; }
                remove { _window.Closing -= value; }
            }

            #endregion
        }

        #endregion

        #region Overrides of WrapperRegistrationModuleBase

        protected override void RegisterWrappers(IConfigurableWrapperManager wrapperManager)
        {
            wrapperManager.AddWrapper<IWindowView, WindowViewWrapper>(
                (type, context) => typeof(ChildWindow).IsAssignableFrom(type),
                (o, context) => new WindowViewWrapper((ChildWindow)o));
        }

        #endregion
    }
}
