﻿#region Copyright

// ****************************************************************************
// <copyright file="ViewResult.cs">
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

using Android.Views;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Android.Models
{
    public struct ViewResult
    {
        #region Fields

        private readonly IDataContext _dataContext;
        private readonly View _view;

        #endregion

        #region Constructors

        public ViewResult([NotNull] View view, IDataContext dataContext)
        {
            Should.NotBeNull(view, nameof(view));
            _view = view;
            _dataContext = dataContext ?? MugenMvvmToolkit.Models.DataContext.Empty;
        }

        #endregion

        #region Properties

        public bool IsEmpty => _view == null;

        [NotNull]
        public View View => _view;

        [NotNull]
        public IDataContext DataContext => _dataContext;

        #endregion
    }
}
