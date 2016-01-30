#region Copyright

// ****************************************************************************
// <copyright file="IPopupMenuPresenter.cs">
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
using Android.Views;

namespace MugenMvvmToolkit.Android.Interfaces
{
    public interface IPopupMenuPresenter
    {
        bool Show(View sourceView, View targetView, int templateId, object args, Action<object, IMenu> dismissHandler);
    }
}