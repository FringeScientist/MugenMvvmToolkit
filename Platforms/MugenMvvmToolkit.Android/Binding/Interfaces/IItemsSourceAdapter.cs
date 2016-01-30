﻿#region Copyright

// ****************************************************************************
// <copyright file="IItemsSourceAdapter.cs">
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

using System.Collections;
using Android.Widget;

namespace MugenMvvmToolkit.Android.Binding.Interfaces
{
    public interface IItemsSourceAdapter : ISpinnerAdapter, IListAdapter
    {
        IEnumerable ItemsSource { get; set; }

        int GetPosition(object value);

        object GetRawItem(int position);

        Filter Filter { get; set; }
    }
}
