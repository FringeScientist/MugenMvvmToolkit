﻿#region Copyright

// ****************************************************************************
// <copyright file="ITableCellTemplateSelector.cs">
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
using JetBrains.Annotations;
using UIKit;

namespace MugenMvvmToolkit.iOS.Binding.Interfaces
{
    public interface ITableCellTemplateSelector
    {
        NSString GetIdentifier([CanBeNull] object item, [NotNull] UITableView container);

        nfloat? GetHeight([NotNull] UITableView container, [NotNull] NSString identifier);

        UITableViewCell SelectTemplate([NotNull] UITableView container, [NotNull] NSString identifier);
    }
}
