﻿#region Copyright

// ****************************************************************************
// <copyright file="ViewMappingItem.cs">
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
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;

namespace MugenMvvmToolkit.Models
{
    public class ViewMappingItem : IViewMappingItem
    {
        #region Fields

        internal static readonly Uri Empty;

        private readonly string _name;
        private readonly Uri _uri;
        private readonly Type _viewModelType;
        private readonly Type _viewType;

        #endregion

        #region Constructor

        static ViewMappingItem()
        {
            Empty = new Uri("app://empty/", UriKind.Absolute);
        }

        public ViewMappingItem(Type viewModelType, Type viewType, string name, Uri uri)
        {
            Should.BeOfType<IViewModel>(viewModelType, "viewModelType");
            Should.NotBeNull(viewType, nameof(viewType));
            _viewModelType = viewModelType;
            _viewType = viewType;
            _name = name;
            _uri = uri ?? Empty;
        }

        #endregion

        #region Implementation of IViewPageMappingItem

        public string Name => _name;

        public Type ViewType => _viewType;

        public Type ViewModelType => _viewModelType;

        public Uri Uri => _uri;

        #endregion

        #region Overrides of Object

        public override string ToString()
        {
            return $"View: {ViewType}, ViewModelType: {ViewModelType}, Name: {Name}, Uri: {Uri}";
        }

        #endregion
    }
}
