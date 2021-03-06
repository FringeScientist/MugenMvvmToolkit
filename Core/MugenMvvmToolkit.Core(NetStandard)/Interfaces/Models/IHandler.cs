﻿#region Copyright

// ****************************************************************************
// <copyright file="IHandler.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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

using JetBrains.Annotations;
using MugenMvvmToolkit.Attributes;

namespace MugenMvvmToolkit.Interfaces.Models
{
    public interface IHandler<in TMessage>
    {
        [Preserve(Conditional = true)]
        void Handle([NotNull] object sender, [NotNull] TMessage message);
    }

    public interface IBroadcastMessage
    {
    }

    public interface ITracebleMessage
    {
    }
}
