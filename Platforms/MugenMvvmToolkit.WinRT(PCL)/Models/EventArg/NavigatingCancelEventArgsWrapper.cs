﻿#region Copyright

// ****************************************************************************
// <copyright file="NavigatingCancelEventArgsWrapper.cs">
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

using Windows.UI.Xaml.Navigation;
using JetBrains.Annotations;
using MugenMvvmToolkit.Models.EventArg;
using NavigationMode = MugenMvvmToolkit.Models.NavigationMode;

namespace MugenMvvmToolkit.WinRT.Models.EventArg
{
    public sealed class NavigatingCancelEventArgsWrapper : NavigatingCancelEventArgsBase
    {
        #region Fields

        private readonly NavigatingCancelEventArgs _args;
        private readonly string _parameter;
        private readonly bool _bringToFront;

        #endregion

        #region Constructors

        public NavigatingCancelEventArgsWrapper([NotNull] NavigatingCancelEventArgs args, string parameter, bool bringToFront)
        {
            Should.NotBeNull(args, nameof(args));
            _args = args;
            _parameter = parameter;
            _bringToFront = bringToFront;
        }

        #endregion

        #region Properties

        public NavigatingCancelEventArgs Args => _args;

        public string Parameter => _parameter;

        #endregion

        #region Overrides of NavigatingCancelEventArgsBase

        public override bool Cancel
        {
            get { return _args.Cancel; }
            set { _args.Cancel = value; }
        }

        public override NavigationMode NavigationMode
        {
            get
            {
                var mode = _args.NavigationMode.ToNavigationMode();
                if (_bringToFront && mode == NavigationMode.New)
                    return NavigationMode.Refresh;
                return mode;
            }
        }

        public override bool IsCancelable => true;

        #endregion
    }
}
