﻿#region Copyright

// ****************************************************************************
// <copyright file="InverseBooleanValueConverter.cs">
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
using System.Globalization;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Converters
{
    public sealed class InverseBooleanValueConverter : ValueConverterBase<bool?, bool?>
    {
        #region Fields

        public static readonly InverseBooleanValueConverter Instance;

        #endregion

        #region Constructors

        static InverseBooleanValueConverter()
        {
            Instance = new InverseBooleanValueConverter();
        }

        #endregion

        #region Overrides of ValueConverterBase

        protected override bool? Convert(bool? value, Type targetType, object parameter, CultureInfo culture, IDataContext context)
        {
            if (value.HasValue)
                return !value.Value;
            return null;
        }

        protected override bool? ConvertBack(bool? value, Type targetType, object parameter, CultureInfo culture, IDataContext context)
        {
            return Convert(value, targetType, parameter, culture, context);
        }

        #endregion
    }
}
