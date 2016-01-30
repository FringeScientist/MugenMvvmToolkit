﻿#region Copyright

// ****************************************************************************
// <copyright file="IMessagePresenter.cs">
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

using System.Threading.Tasks;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Interfaces.Presenters
{
    public interface IMessagePresenter
    {
        Task<MessageResult> ShowAsync(string message, string caption = "",
            MessageButton button = MessageButton.Ok,
            MessageImage icon = MessageImage.None, MessageResult defaultResult = MessageResult.None, IDataContext context = null);
    }
}
