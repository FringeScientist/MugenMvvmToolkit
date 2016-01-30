﻿#region Copyright

// ****************************************************************************
// <copyright file="LinkerIncludeLabmdaExpression.cs">
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
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Models;

#if TOUCH
namespace MugenMvvmToolkit.iOS
#elif ANDROID
namespace MugenMvvmToolkit.Android
#elif XAMARIN_FORMS
namespace MugenMvvmToolkit.Xamarin.Forms
#endif
{
    internal static partial class LinkerInclude
    {
        #region Fields

        private static readonly MethodInfo CreateLambdaGeneric;

        #endregion

        #region Constructors

        static LinkerInclude()
        {
            CreateLambdaGeneric = typeof(LinkerInclude).GetMethodEx(nameof(CreateLambdaExpressionGeneric),
                MemberFlags.Static | MemberFlags.NonPublic);
        }

        #endregion

        #region Methods

        internal static void Initialize()
        {
            ExpressionReflectionManager.CreateLambdaExpressionByType = CreateLambdaExpressionByType;
            ExpressionReflectionManager.CreateLambdaExpression = CreateLambdaExpression;
        }

        private static LambdaExpression CreateLambdaExpressionByType(Type type, Expression expression,
            IEnumerable<ParameterExpression> arg3)
        {
            return (LambdaExpression)CreateLambdaGeneric
                .MakeGenericMethod(type)
                .Invoke(null, new object[] { expression, arg3 });
        }

        private static LambdaExpression CreateLambdaExpression(Expression body,
            ParameterExpression[] parameterExpressions)
        {
            var types = new Type[parameterExpressions.Length + 1];
            if (parameterExpressions.Length > 0)
            {
                var set = new HashSet<ParameterExpression>();
                for (int index = 0; index < parameterExpressions.Length; ++index)
                {
                    ParameterExpression parameterExpression = parameterExpressions[index];
                    types[index] = !parameterExpression.IsByRef
                        ? parameterExpression.Type
                        : parameterExpression.Type.MakeByRefType();
                    if (set.Contains(parameterExpression))
                        throw BindingExtensions.DuplicateLambdaParameter(parameterExpression.ToString());
                    set.Add(parameterExpression);
                }
            }
            types[parameterExpressions.Length] = body.Type;
            Type delegateType = Expression.GetDelegateType(types);
            return CreateLambdaExpressionByType(delegateType, body, parameterExpressions);
        }

        [UsedImplicitly]
        private static LambdaExpression CreateLambdaExpressionGeneric<T>(Expression body,
            IEnumerable<ParameterExpression> expressions)
        {
            return Expression.Lambda<T>(body, expressions);
        }

        #endregion
    }
}
