﻿#region Copyright

// ****************************************************************************
// <copyright file="IViewModelPresenter.cs">
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

using System.Collections.Generic;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;

namespace MugenMvvmToolkit.Interfaces.Presenters
{
    public interface IViewModelPresenter
    {
        [NotNull]
        ICollection<IDynamicViewModelPresenter> DynamicPresenters { get; }

        [NotNull]
        INavigationOperation ShowAsync([NotNull] IViewModel viewModel, [CanBeNull] IDataContext context);

        void Restore([NotNull] IViewModel viewModel, [CanBeNull] IDataContext context);
    }
}
