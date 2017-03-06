﻿#region Copyright

// ****************************************************************************
// <copyright file="IBindingModeInfoBehaviorSyntax.cs">
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

namespace MugenMvvmToolkit.Binding.Interfaces.Syntax
{
    public interface IBindingModeInfoBehaviorSyntax<in TSource> : IBindingModeSyntax<TSource>,
        IBindingInfoSyntax<TSource>, IBindingBehaviorSyntax<TSource>
    {
    }
}
