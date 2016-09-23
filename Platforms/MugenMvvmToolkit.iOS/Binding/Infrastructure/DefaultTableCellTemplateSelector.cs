﻿#region Copyright

// ****************************************************************************
// <copyright file="DefaultTableCellTemplateSelector.cs">
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
using Foundation;
using MugenMvvmToolkit.Binding.Builders;
using MugenMvvmToolkit.iOS.Views;
using UIKit;

namespace MugenMvvmToolkit.iOS.Binding.Infrastructure
{
    public class DefaultTableCellTemplateSelector<TSource> : TableCellTemplateSelectorBase<TSource, UITableViewCell>
    {
        #region Fields

        private readonly bool _bindable;
        private readonly Action<UITableViewCell, BindingSet<UITableViewCell, TSource>> _initializeTemplate;
        private readonly UITableViewCellStyle _style;
        private static readonly NSString Id = new NSString("Id" + nameof(TSource));

        #endregion

        #region Constructors

        public DefaultTableCellTemplateSelector(UITableViewCellStyle style, Action<UITableViewCell, BindingSet<UITableViewCell, TSource>> initializeTemplate, bool isDefaultCellPropertiesBindable = false)
        {
            Should.NotBeNull(initializeTemplate, nameof(initializeTemplate));
            _style = style;
            _initializeTemplate = initializeTemplate;
            _bindable = isDefaultCellPropertiesBindable;
        }

        #endregion

        #region Methods

        protected override NSString GetIdentifier(TSource item, UITableView container)
        {
            return Id;
        }

        protected override UITableViewCell SelectTemplate(UITableView container, NSString identifier)
        {
            if (_bindable)
                return new UITableViewCellBindable(_style, identifier);
            return new UITableViewCell(_style, identifier);
        }

        protected override void Initialize(UITableViewCell template, BindingSet<UITableViewCell, TSource> bindingSet)
        {
            _initializeTemplate(template, bindingSet);
        }

        #endregion
    }
}