﻿#region Copyright

// ****************************************************************************
// <copyright file="IValidatorProvider.cs">
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
using System.Collections.Generic;
using JetBrains.Annotations;

namespace MugenMvvmToolkit.Interfaces.Validation
{
    public interface IValidatorProvider
    {
        void Register([NotNull] Type validatorType);

        [Pure]
        bool IsRegistered([NotNull] Type validatorType);

        bool Unregister([NotNull] Type validatorType);

        [NotNull]
        IList<Type> GetValidatorTypes();

        [NotNull]
        IList<IValidator> GetValidators([NotNull] IValidatorContext context);

        [NotNull]
        IValidatorAggregator GetValidatorAggregator();
    }
}
