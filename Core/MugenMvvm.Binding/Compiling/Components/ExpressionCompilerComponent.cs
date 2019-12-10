﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Extensions.Components;
using MugenMvvm.Binding.Interfaces.Compiling;
using MugenMvvm.Binding.Interfaces.Compiling.Components;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Compiling.Components
{
    public sealed class ExpressionCompilerComponent : AttachableComponentBase<IExpressionCompiler>, IExpressionCompilerComponent, IHasPriority
    {
        #region Fields

        private readonly IMetadataContextProvider? _metadataContextProvider;

        #endregion

        #region Constructors

        public ExpressionCompilerComponent(IMetadataContextProvider? metadataContextProvider = null)
        {
            _metadataContextProvider = metadataContextProvider;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = CompilingComponentPriority.LinqCompiler;

        #endregion

        #region Implementation of interfaces

        public ICompiledExpression? TryCompile(IExpressionNode expression, IReadOnlyMetadataContext? metadata)
        {
            return new CompiledExpression(this, expression, metadata);
        }

        #endregion

        #region Nested types

        private sealed class CompiledExpression : LightDictionary<object, Func<object?[], object?>>, ICompiledExpression, IExpressionBuilderContext, IExpressionVisitor
        {
            #region Fields

            private readonly ExpressionCompilerComponent _compiler;
            private readonly IExpressionNode _expression;
            private readonly IReadOnlyMetadataContext? _inputMetadata;
            private readonly ExpressionDictionary _expressionsDict;
            private readonly object?[] _values;

            private List<IParameterInfo>? _lambdaParameters;
            private IMetadataContext? _metadata;

            private static readonly ParameterExpression[] ArrayParameterArray = { MugenExtensions.GetParameterExpression<object[]>() };

            #endregion

            #region Constructors

            public CompiledExpression(ExpressionCompilerComponent compiler, IExpressionNode expression, IReadOnlyMetadataContext? metadata)
            {
                _compiler = compiler;
                _inputMetadata = metadata;
                _expressionsDict = new ExpressionDictionary();
                _expression = expression.Accept(this, metadata);
                _values = new object[_expressionsDict.Count + 1];
                MetadataParameter = MugenExtensions.GetIndexExpression(_expressionsDict.Count).ConvertIfNeed(typeof(IReadOnlyMetadataContext), false);
            }

            #endregion

            #region Properties

            public bool HasMetadata => !(_metadata ?? _inputMetadata).IsNullOrEmpty();

            public IMetadataContext Metadata
            {
                get
                {
                    if (_metadata == null)
                        _compiler._metadataContextProvider.LazyInitialize(ref _metadata, this, _inputMetadata);
                    return _metadata;
                }
            }

            public Expression MetadataParameter { get; }

            bool IExpressionVisitor.IsPostOrder => false;

            #endregion

            #region Implementation of interfaces

            public object? Invoke(ItemOrList<ExpressionValue, ExpressionValue[]> values, IReadOnlyMetadataContext? metadata)
            {
                var list = values.List;
                var key = list ?? values.Item.Type ?? (object)Default.EmptyArray<Type>();
                if (!TryGetValue(key, out var invoker))
                {
                    invoker = CompileExpression(values);
                    if (list == null)
                        this[key] = invoker;
                    else
                    {
                        var types = new Type[list.Length];
                        for (var i = 0; i < list.Length; i++)
                            types[i] = list[i].Type;
                        this[types] = invoker;
                    }
                }

                if (list == null)
                    _values[0] = values.Item.Value;
                else
                {
                    for (var i = 0; i < list.Length; i++)
                        _values[i] = list[i].Value;
                }

                _values[_values.Length - 1] = metadata;
                try
                {
                    return invoker.Invoke(_values);
                }
                finally
                {
                    Array.Clear(_values, 0, _values.Length);
                }
            }

            IExpressionNode IExpressionVisitor.Visit(IExpressionNode expression, IReadOnlyMetadataContext? metadata)
            {
                if (expression is IBindingMemberExpressionNode memberExpression)
                {
                    if (memberExpression.Index < 0)
                    {
                        this.TryGetErrors()?.Add(BindingMessageConstant.CannotCompileBindingMemberExpressionFormat2.Format(memberExpression, memberExpression.Index));
                        this.ThrowCannotCompile(memberExpression);
                    }
                    _expressionsDict[memberExpression] = null;
                }

                return expression;
            }

            public IParameterInfo? TryGetLambdaParameter()
            {
                if (_lambdaParameters == null || _lambdaParameters.Count == 0)
                    return default;
                return _lambdaParameters[0];
            }

            public void SetLambdaParameter(IParameterInfo parameter)
            {
                Should.NotBeNull(parameter, nameof(parameter));
                if (_lambdaParameters == null)
                    _lambdaParameters = new List<IParameterInfo>(2);
                _lambdaParameters.Insert(0, parameter);
            }

            public void ClearLambdaParameter(IParameterInfo parameter)
            {
                Should.NotBeNull(parameter, nameof(parameter));
                _lambdaParameters?.Remove(parameter);
            }

            public Expression? TryGetExpression(IExpressionNode expression)
            {
                Should.NotBeNull(expression, nameof(expression));
                _expressionsDict.TryGetValue(expression, out var value);
                return value;
            }

            public void SetExpression(IExpressionNode expression, Expression value)
            {
                Should.NotBeNull(expression, nameof(expression));
                Should.NotBeNull(value, nameof(value));
                _expressionsDict[expression] = value;
            }

            public void ClearExpression(IExpressionNode expression)
            {
                Should.NotBeNull(expression, nameof(expression));
                _expressionsDict.Remove(expression);
            }

            public Expression Build(IExpressionNode expression)
            {
                Should.NotBeNull(expression, nameof(expression));
                var exp = _compiler.Owner.Components.Get<IExpressionBuilderCompilerComponent>(_inputMetadata).TryBuild(this, expression) ?? TryGetExpression(expression);
                if (exp != null)
                    return exp;

                this.ThrowCannotCompile(expression);
                return null;
            }

            #endregion

            #region Methods

            private Func<object?[], object?> CompileExpression(ItemOrList<ExpressionValue, ExpressionValue[]> values)
            {
                try
                {
                    var expressionValues = values.List;
                    foreach (var value in _expressionsDict)
                    {
                        if (!(value.Key is IBindingMemberExpressionNode memberExpression))
                            continue;

                        var index = MugenExtensions.GetIndexExpression(memberExpression.Index);
                        if (expressionValues == null)
                        {
                            if (memberExpression.Index != 0)
                                ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(values));
                            _expressionsDict[memberExpression] = index.ConvertIfNeed(values.Item.Type, false);
                        }
                        else
                            _expressionsDict[memberExpression] = index.ConvertIfNeed(expressionValues[memberExpression.Index].Type, false);
                    }

                    var expression = Build(_expression).ConvertIfNeed(typeof(object), false);
                    var lambda = Expression.Lambda<Func<object?[], object?>>(expression, ArrayParameterArray);
                    return lambda.CompileEx();
                }
                finally
                {
                    _lambdaParameters?.Clear();
                    if (_metadata != null)
                    {
                        this.TryGetErrors()?.Clear();
                        _metadata.Clear();
                        if (!_inputMetadata.IsNullOrEmpty())
                            _metadata.Merge(_inputMetadata!);
                    }
                }
            }

            protected override bool Equals(object x, object y)
            {
                var typeX = x as Type;
                var typeY = y as Type;
                if (typeX != null || typeY != null)
                {
                    if (typeX == null || typeY == null)
                        return false;
                    return typeX == typeY;
                }

                var typesX = x as Type[];
                var typesY = y as Type[];
                if (typesX == null && typesY == null)
                {
                    var valuesX = (ExpressionValue[])x;
                    var valuesY = (ExpressionValue[])y;
                    if (valuesX.Length != valuesY.Length)
                        return false;
                    for (var i = 0; i < valuesX.Length; i++)
                    {
                        if (valuesX[i].Type != valuesY[i].Type)
                            return false;
                    }

                    return true;
                }

                if (typesX == null)
                    return Equals(typesY!, (ExpressionValue[])x);
                if (typesY == null)
                    return Equals(typesX!, (ExpressionValue[])y);

                if (typesX.Length != typesY.Length)
                    return false;
                for (var i = 0; i < typesX.Length; i++)
                {
                    if (typesX[i] != typesY[i])
                        return false;
                }

                return true;
            }

            protected override int GetHashCode(object key)
            {
                if (key is Type type)
                    return type.GetHashCode();

                var hashCode = new HashCode();
                if (key is ExpressionValue[] values)
                {
                    for (var index = 0; index < values.Length; index++)
                        hashCode.Add(values[index].Type);
                }
                else
                {
                    var types = (Type[])key;
                    for (var index = 0; index < types.Length; index++)
                        hashCode.Add(types[index]);
                }

                return hashCode.ToHashCode();
            }

            private static bool Equals(Type[] types, ExpressionValue[] values)
            {
                if (values.Length != types.Length)
                    return false;
                for (var i = 0; i < values.Length; i++)
                {
                    if (values[i].Type != types[i])
                        return false;
                }

                return true;
            }

            #endregion
        }

        private sealed class ExpressionDictionary : LightDictionary<IExpressionNode, Expression?>
        {
            #region Constructors

            public ExpressionDictionary() : base(3)
            {
            }

            #endregion

            #region Methods

            protected override int GetHashCode(IExpressionNode key)
            {
                if (key is IBindingMemberExpressionNode member)
                    return HashCode.Combine(member.Index, member.Path);
                return RuntimeHelpers.GetHashCode(key);
            }

            protected override bool Equals(IExpressionNode x, IExpressionNode y)
            {
                if (x is IBindingMemberExpressionNode xP && y is IBindingMemberExpressionNode yP)
                    return xP.Index == yP.Index && xP.Path == yP.Path;

                return ReferenceEquals(x, y);
            }

            #endregion
        }

        #endregion
    }
}