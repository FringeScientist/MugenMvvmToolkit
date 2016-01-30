﻿#region Copyright

// ****************************************************************************
// <copyright file="INotifyCollectionChanging.cs">
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

using System.Collections.Specialized;
using System.ComponentModel;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Interfaces.Collections
{
    public interface INotifyCollectionChanging : INotifyCollectionChanged, INotifyPropertyChanged
    {
        event NotifyCollectionChangingEventHandler CollectionChanging;
    }
}
