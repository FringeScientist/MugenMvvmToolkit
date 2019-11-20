﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Compiling;
using MugenMvvm.Binding.Interfaces.Compiling.Components;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Interfaces.Resources;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Compiling.Components
{
    public sealed class MethodIndexerLinqExpressionBuilderComponent : ILinqExpressionBuilderComponent, IHasPriority
    {
        #region Fields

        private readonly IMemberProvider? _memberProvider;
        private readonly IResourceResolver? _resourceResolver;

        private const float NotExactlyEqualWeight = 1f;
        private const float NotExactlyEqualBoxWeight = 1.1f;
        private const float NotExactlyEqualUnsafeCastWeight = 1000f;

        private static readonly Expression[] ExpressionCallBuffer = new Expression[5];
        private static readonly int[] ArraySize = new int[1];
        private static readonly MethodInfo InvokeMethod = typeof(IMethodInfo).GetMethodOrThrow(nameof(IMethodInfo.Invoke), BindingFlagsEx.InstancePublic);
        private static readonly MethodInfo MethodInvokerInvokeMethod = typeof(MethodInvoker).GetMethodOrThrow(nameof(MethodInvoker.Invoke), BindingFlagsEx.InstancePublic);

        #endregion

        #region Constructors

        public MethodIndexerLinqExpressionBuilderComponent(IMemberProvider? memberProvider = null, IResourceResolver? resourceResolver = null)
        {
            _memberProvider = memberProvider;
            _resourceResolver = resourceResolver;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = LinqCompilerPriority.Member;

        public MemberFlags MemberFlags { get; set; } = MemberFlags.All & ~MemberFlags.NonPublic;

        #endregion

        #region Implementation of interfaces

        public Expression? TryBuild(ILinqExpressionBuilderContext context, IExpressionNode expression)
        {
            switch (expression)
            {
                case IIndexExpressionNode indexExpression:
                    return TryBuildIndex(context, indexExpression);
                case IMethodCallExpressionNode methodCallExpression:
                    return TryBuildMethod(context, methodCallExpression);
                default:
                    return null;
            }
        }

        #endregion

        #region Methods

        private Expression? TryBuildMethod(ILinqExpressionBuilderContext context, IMethodCallExpressionNode methodCallExpression)
        {
            if (methodCallExpression.Target == null)
                return null;

            var target = context.Build(methodCallExpression.Target);
            var type = MugenBindingExtensions.GetTargetType(ref target);

            if (methodCallExpression.Method != null)
                return GenerateMethodCall(context, methodCallExpression.Method, target, methodCallExpression.Arguments);

            var targetData = new TargetData(methodCallExpression.Target!, type, target);
            var args = new ArgumentData[methodCallExpression.Arguments.Count];
            for (var i = 0; i < args.Length; i++)
            {
                var node = methodCallExpression.Arguments[i];
                args[i] = new ArgumentData(node, node.NodeType == ExpressionNodeType.Lambda ? null : context.Build(methodCallExpression.Arguments[i]), null);
            }

            return TryBuildExpression(context, methodCallExpression.MethodName, targetData, args, GetTypes(methodCallExpression.TypeArgs));
        }

        private Expression? TryBuildIndex(ILinqExpressionBuilderContext context, IIndexExpressionNode indexExpression)
        {
            if (indexExpression.Target == null)
                return null;

            var target = context.Build(indexExpression.Target);
            var type = MugenBindingExtensions.GetTargetType(ref target);

            if (indexExpression.Indexer != null)
                return GenerateMethodCall(context, indexExpression.Indexer, target, indexExpression.Arguments);

            if (target.Type.IsArray)
                return Expression.ArrayIndex(target, ToExpressions(context, indexExpression.Arguments, null, typeof(int)));

            var targetData = new TargetData(indexExpression.Target!, type, target);
            var args = new ArgumentData[indexExpression.Arguments.Count];
            for (var i = 0; i < args.Length; i++)
            {
                var node = indexExpression.Arguments[i];
                args[i] = new ArgumentData(node, node.NodeType == ExpressionNodeType.Lambda ? null : context.Build(indexExpression.Arguments[i]), null);
            }

            return TryBuildExpression(context, type == typeof(string) ? "get_Chars" : "get_Item", targetData, args, Default.EmptyArray<Type>());
        }

        private Expression? TryBuildExpression(ILinqExpressionBuilderContext context, string methodName, in TargetData targetData, ArgumentData[] args, Type[] typeArgs)
        {
            var methods = FindBestMethods(targetData, GetMethods(targetData.Type, methodName, targetData.IsStatic, null, context.GetMetadataOrDefault()), args, typeArgs);
            var expression = TryGenerateMethodCall(context, methods, targetData, args);
            if (expression != null)
                return expression;

            if (targetData.Expression == null)
                return null;

            var arrayArgs = new Expression[args.Length];
            for (var i = 0; i < args.Length; i++)
            {
                var data = args[i];
                if (data.IsLambda || data.Expression == null)
                    return null;
                arrayArgs[i] = data.Expression.ConvertIfNeed(typeof(object), false);
            }

            try
            {
                ExpressionCallBuffer[0] = targetData.Expression;
                ExpressionCallBuffer[1] = Expression.Constant(methodName);
                ExpressionCallBuffer[2] = Expression.NewArrayInit(typeof(object), arrayArgs);
                ExpressionCallBuffer[3] = Expression.Constant(typeArgs);
                ExpressionCallBuffer[4] = context.MetadataParameter;
                return Expression.Call(Expression.Constant(new MethodInvoker(this)), MethodInvokerInvokeMethod, ExpressionCallBuffer);
            }
            finally
            {
                Array.Clear(ExpressionCallBuffer, 0, ExpressionCallBuffer.Length);
            }
        }

        private Type[] GetTypes(IReadOnlyList<string>? types)
        {
            if (types == null || types.Count == 0)
                return Default.EmptyArray<Type>();
            var resolver = _resourceResolver.DefaultIfNull();
            var typeArgs = new Type[types.Count];
            for (var i = 0; i < types.Count; i++)
            {
                var type = resolver.TryGetType(types[i]);
                if (type == null)
                    BindingExceptionManager.ThrowCannotResolveType(types[i]);
                typeArgs[i] = type!;
            }

            return typeArgs;
        }

        private static MethodData[] FindBestMethods(in TargetData target, MethodData[] methods, ArgumentData[] arguments, Type[] typeArgs)
        {
            for (var index = 0; index < methods.Length; index++)
            {
                try
                {
                    var methodInfo = methods[index];
                    if (methodInfo.IsEmpty)
                        continue;

                    methods[index] = default;
                    var methodData = TryInferMethod(methodInfo.Method, arguments, typeArgs);
                    if (methodData.IsEmpty)
                        continue;

                    var parameters = methodInfo.Parameters;
                    var optionalCount = parameters.Count(info => info.HasDefaultValue);
                    var requiredCount = parameters.Count - optionalCount;
                    var hasParams = false;
                    if (parameters.Count != 0)
                    {
                        hasParams = parameters[parameters.Count - 1].IsParamArray();
                        if (hasParams)
                            requiredCount -= 1;
                    }

                    if (requiredCount > arguments.Length)
                        continue;
                    if (parameters.Count < arguments.Length && !hasParams)
                        continue;
                    var count = parameters.Count > arguments.Length ? arguments.Length : parameters.Count;
                    var valid = true;
                    for (var i = 0; i < count; i++)
                    {
                        if (!IsCompatible(parameters[i].ParameterType, arguments[i].Node))
                        {
                            valid = false;
                            break;
                        }
                    }

                    if (valid)
                        methods[index] = methodData;
                }
                catch
                {
                    ;
                }
            }

            return methods;
        }

        private static Expression? TryGenerateMethodCall(ILinqExpressionBuilderContext context, MethodData[] methods, in TargetData target, ArgumentData[] arguments)
        {
            if (methods.Length == 0)
                return null;

            for (var i = 0; i < methods.Length; i++)
            {
                try
                {
                    var method = methods[i];
                    if (method.IsEmpty)
                        continue;

                    var expressions = new Expression[arguments.Length];
                    for (var index = 0; index < arguments.Length; index++)
                    {
                        var data = arguments[index];
                        if (data.IsLambda)
                        {
                            var lambdaParameter = method.Parameters[index];
                            context.SetLambdaParameter(lambdaParameter);
                            try
                            {
                                data = data.UpdateExpression(context.Build(data.Node));
                            }
                            finally
                            {
                                context.ClearLambdaParameter(lambdaParameter);
                            }

                            arguments[index] = data;
                        }

                        expressions[index] = data.Expression!;
                    }

                    methods[i] = method.TryResolve(arguments, expressions);
                }
                catch
                {
                    methods[i] = default;
                }
            }

            var resultIndex = TrySelectMethod(methods, arguments, out var resultHasParams);
            if (resultIndex < 0)
                return null;

            var result = methods[resultIndex];
            var resultArgs = ConvertParameters(context, result, resultHasParams);
            if (result.Method.UnderlyingMember is MethodInfo m)
            {
                Expression? targetExp;
                if (result.Method.AccessModifiers.HasFlagEx(MemberFlags.Extension))
                {
                    targetExp = null;
                    resultArgs = resultArgs.InsertFirstArg(target.Expression!);
                }
                else
                    targetExp = target.Expression;
                return Expression.Call(targetExp, m, resultArgs);
            }

            var invokeArgs = new Expression[3];
            invokeArgs[0] = target.Expression.ConvertIfNeed(typeof(object), false);
            invokeArgs[1] = Expression.NewArrayInit(typeof(object), resultArgs.Select(expression => expression.ConvertIfNeed(typeof(object), false)));
            invokeArgs[2] = context.MetadataParameter;
            return Expression.Call(Expression.Constant(result.Method), InvokeMethod, invokeArgs).ConvertIfNeed(result.Method.Type, true);
        }

        private static Expression GenerateMethodCall(ILinqExpressionBuilderContext context, IMethodInfo methodInfo, Expression target, IReadOnlyList<IExpressionNode> args)
        {
            var expressions = ToExpressions(context, args, methodInfo, null);
            if (methodInfo.UnderlyingMember is MethodInfo method)
                return Expression.Call(target, method, expressions);

            var invokeArgs = new Expression[3];
            invokeArgs[0] = target.ConvertIfNeed(typeof(object), false);
            invokeArgs[1] = Expression.NewArrayInit(typeof(object), expressions.Select(expression => expression.ConvertIfNeed(typeof(object), false)));
            invokeArgs[2] = context.MetadataParameter;
            return Expression.Call(Expression.Constant(methodInfo), InvokeMethod, invokeArgs).ConvertIfNeed(methodInfo.Type, true);
        }

        private static int TrySelectMethod(MethodData[] methods, ArgumentData[]? args, out bool resultHasParams)
        {
            var result = -1;
            resultHasParams = true;
            var resultUseParams = true;
            var resultNotExactlyEqual = float.MaxValue;
            var resultUsageCount = int.MinValue;
            for (var i = 0; i < methods.Length; i++)
            {
                var methodInfo = methods[i];
                if (methodInfo.IsEmpty)
                    continue;

                try
                {
                    var parameters = methodInfo.Parameters;
                    var useParams = false;
                    var hasParams = false;
                    var lastIndex = 0;
                    var usageCount = 0;
                    if (parameters.Count != 0)
                    {
                        lastIndex = parameters.Count - 1;
                        hasParams = parameters[lastIndex].IsParamArray();
                    }

                    float notExactlyEqual = methodInfo.Method.AccessModifiers.HasFlagEx(MemberFlags.Extension) ? NotExactlyEqualBoxWeight : 0;
                    var valid = true;
                    for (var j = 0; j < methodInfo.ExpectedParameterCount; j++)
                    {
                        //params
                        if (j > lastIndex)
                        {
                            valid = hasParams && CheckParamsCompatible(j - 1, lastIndex, parameters, methodInfo, ref notExactlyEqual);
                            useParams = true;
                            break;
                        }

                        var argType = methodInfo.GetExpectedParameterType(j);
                        var parameterType = parameters[j].ParameterType;
                        if (parameterType.IsByRef)
                            parameterType = parameterType.GetElementType();
                        if (parameterType == argType)
                        {
                            ++usageCount;
                            continue;
                        }

                        if (argType.IsCompatibleWith(parameterType, out var boxRequired))
                        {
                            notExactlyEqual += boxRequired ? NotExactlyEqualBoxWeight : NotExactlyEqualWeight;
                            ++usageCount;
                        }
                        else
                        {
                            if (lastIndex == j && hasParams)
                            {
                                valid = CheckParamsCompatible(j, lastIndex, parameters, methodInfo, ref notExactlyEqual);
                                useParams = true;
                                break;
                            }

                            if (argType.IsValueType)
                            {
                                valid = false;
                                break;
                            }

                            ++usageCount;
                            notExactlyEqual += NotExactlyEqualUnsafeCastWeight;
                        }
                    }

                    if (!valid)
                        continue;
                    if (notExactlyEqual > resultNotExactlyEqual)
                        continue;
                    if (notExactlyEqual == resultNotExactlyEqual)
                    {
                        if (usageCount < resultUsageCount)
                            continue;
                        if (usageCount == resultUsageCount)
                        {
                            if (useParams && !resultUseParams)
                                continue;
                            if (!useParams && !resultUseParams)
                            {
                                if (hasParams && !resultHasParams)
                                    continue;
                            }
                        }
                    }

                    result = i;
                    resultNotExactlyEqual = notExactlyEqual;
                    resultUsageCount = usageCount;
                    resultUseParams = useParams;
                    resultHasParams = hasParams;
                }
                catch
                {
                    ;
                }
            }

            if (result != -1 && args != null && resultNotExactlyEqual >= NotExactlyEqualUnsafeCastWeight && args.All(data => !data.IsLambda))
                return -1;

            return result;
        }

        private static bool CheckParamsCompatible(int startIndex, int lastIndex, IReadOnlyList<IParameterInfo> parameters, in MethodData method, ref float notExactlyEqual)
        {
            float weight = 0;
            var elementType = parameters[lastIndex].ParameterType.GetElementType();
            for (var i = startIndex; i < method.ExpectedParameterCount; i++)
            {
                var argType = method.GetExpectedParameterType(i);
                if (elementType == argType)
                    continue;
                if (argType.IsCompatibleWith(elementType, out var boxRequired))
                {
                    var w = boxRequired ? NotExactlyEqualBoxWeight : NotExactlyEqualWeight;
                    if (w > weight)
                        weight = w;
                }
                else
                {
                    if (argType.IsValueType)
                        return false;
                    if (NotExactlyEqualUnsafeCastWeight > weight)
                        weight = NotExactlyEqualUnsafeCastWeight;
                }
            }

            notExactlyEqual += weight;
            return true;
        }

        private static Expression[] ConvertParameters(ILinqExpressionBuilderContext context, in MethodData method, bool hasParams)
        {
            var parameters = method.Parameters;
            var args = (Expression[])method.Args!;
            var result = new Expression[parameters.Count];
            for (var i = 0; i < parameters.Count; i++)
            {
                //optional or params
                if (i > args.Length - 1)
                {
                    for (var j = i; j < parameters.Count; j++)
                    {
                        var parameter = parameters[j];
                        if (j == parameters.Count - 1 && hasParams)
                        {
                            var type = parameter.ParameterType.GetElementType();
                            result[j] = Expression.NewArrayInit(type, Default.EmptyArray<Expression>());
                        }
                        else
                        {
                            if (parameter.ParameterType == typeof(IReadOnlyMetadataContext))
                                result[j] = context.MetadataParameter;
                            else
                                result[j] = Expression.Constant(parameter.DefaultValue).ConvertIfNeed(parameter.ParameterType, false);
                        }
                    }

                    break;
                }

                if (i == parameters.Count - 1 && hasParams && !args[i].Type.IsCompatibleWith(parameters[i].ParameterType))
                {
                    var arrayType = parameters[i].ParameterType.GetElementType();
                    var arrayArgs = new Expression[args.Length - i];
                    for (var j = i; j < args.Length; j++)
                        arrayArgs[j - i] = args[j].ConvertIfNeed(arrayType, false);
                    result[i] = Expression.NewArrayInit(arrayType, arrayArgs);
                }
                else
                    result[i] = args[i].ConvertIfNeed(parameters[i].ParameterType, false);
            }

            return result;
        }

        private static object?[] ConvertParameters(in MethodData method, object?[] args, bool hasParams, IReadOnlyMetadataContext? metadata)
        {
            var parameters = method.Parameters!;
            var result = args.Length == parameters.Count ? args : new object?[parameters.Count];
            for (var i = 0; i < parameters.Count; i++)
            {
                //optional or params
                if (i > args.Length - 1)
                {
                    for (var j = i; j < parameters.Count; j++)
                    {
                        var parameter = parameters[j];
                        if (j == parameters.Count - 1 && hasParams)
                        {
                            ArraySize[0] = 0;
                            result[j] = Array.CreateInstance(parameter.ParameterType.GetElementType(), ArraySize);
                        }
                        else
                        {
                            if (parameter.ParameterType == typeof(IReadOnlyMetadataContext))
                                result[j] = metadata;
                            else
                                result[j] = parameter.DefaultValue;
                        }
                    }

                    break;
                }

                if (i == parameters.Count - 1 && hasParams && !parameters[i].ParameterType.IsInstanceOfType(args[i]))
                {
                    ArraySize[0] = args.Length - i;
                    var array = Array.CreateInstance(parameters[i].ParameterType.GetElementType(), ArraySize);
                    for (var j = i; j < args.Length; j++)
                    {
                        ArraySize[0] = j - i;
                        array.SetValue(args[j], ArraySize);
                    }

                    result[i] = array;
                }
                else
                    result[i] = args[i];
            }

            return result;
        }

        private static MethodData TryInferMethod(IMethodInfo method, ArgumentData[] args, Type[] typeArgs)
        {
            var m = ApplyTypeArgs(method, typeArgs);
            if (m != null)
                return new MethodData(m);
            if (!method.IsGenericMethod || typeArgs.Length != 0)
                return default;
            return TryInferGenericMethod(method, args);
        }

        private static MethodData TryInferGenericMethod(IMethodInfo method, ArgumentData[] args)
        {
            var genericMethod = TryInferGenericMethod(method, args, out var hasUnresolved);
            if (genericMethod == null)
                return default;
            if (hasUnresolved)
                return new MethodData(genericMethod, method);
            return new MethodData(genericMethod);
        }

        private static IMethodInfo? TryInferGenericMethod(IMethodInfo method, ArgumentData[] args, out bool hasUnresolved)
        {
            var parameters = method.GetParameters();
            var inferredTypes = MugenBindingExtensions.TryInferGenericParameters(method.GetGenericArguments(), parameters, info => info.ParameterType, args, (data, i) => data[i].Type, args.Length, out hasUnresolved);
            if (inferredTypes == null)
                return null;
            return method.MakeGenericMethod(inferredTypes);
        }

        private static IMethodInfo? ApplyTypeArgs(IMethodInfo m, Type[] typeArgs)
        {
            if (typeArgs.Length == 0)
            {
                if (!m.IsGenericMethodDefinition)
                    return m;
            }
            else if (m.IsGenericMethodDefinition && m.GetGenericArguments().Count == typeArgs.Length)
                return m.MakeGenericMethod(typeArgs);

            return null;
        }

        private static bool IsCompatible(Type parameterType, IExpressionNode node)
        {
            if (!(node is ILambdaExpressionNode lambdaExpressionNode))
                return true;

            if (typeof(Expression).IsAssignableFrom(parameterType) && parameterType.IsGenericType)
                parameterType = parameterType.GetGenericArguments()[0];
            if (!typeof(Delegate).IsAssignableFrom(parameterType))
                return false;

            var method = parameterType.GetMethod(nameof(Action.Invoke), BindingFlagsEx.InstancePublic);
            if (method == null || method.GetParameters().Length != lambdaExpressionNode.Parameters.Count)
                return false;
            return true;
        }

        private static Expression[] ToExpressions(ILinqExpressionBuilderContext context, IReadOnlyList<IExpressionNode> args, IMethodInfo? method, Type? convertType)
        {
            var parameters = method?.GetParameters();
            var expressions = new Expression[args.Count];
            for (var i = 0; i < expressions.Length; i++)
            {
                var expression = context.Build(args[i]);
                if (convertType != null)
                    expression = expression.ConvertIfNeed(convertType, true);
                else if (parameters != null && parameters.Count > i)
                    expression = expression.ConvertIfNeed(parameters[i].ParameterType, true);
                expressions[i] = expression;
            }
            return expressions;
        }

        private MethodData[] GetMethods(Type type, string methodName, bool isStatic, Type[]? typeArgs, IReadOnlyMetadataContext? metadata)
        {
            var members = _memberProvider
                .DefaultIfNull()
                .GetMembers(type, methodName, MemberType.Method, MemberFlags.SetInstanceOrStaticFlags(isStatic), metadata);

            var methods = new MethodData[members.Count];
            var count = 0;
            for (int i = 0; i < methods.Length; i++)
            {
                if (members[i] is IMethodInfo method)
                {
                    var m = typeArgs == null ? method : ApplyTypeArgs(method, typeArgs);
                    if (m != null)
                        methods[count++] = new MethodData(m);
                }
            }

            return methods;
        }

        #endregion

        #region Nested types

        [Preserve(AllMembers = true, Conditional = true)]
        private sealed class MethodInvoker : LightDictionary<Type[], MethodData>
        {
            #region Fields

            private readonly MethodIndexerLinqExpressionBuilderComponent _component;
            private Type? _type;

            #endregion

            #region Constructors

            public MethodInvoker(MethodIndexerLinqExpressionBuilderComponent component) : base(3)
            {
                _component = component;
            }

            #endregion

            #region Methods

            public object? Invoke(object target, string methodName, object?[] args, Type[] typeArgs, IReadOnlyMetadataContext? metadata)
            {
                var type = target.GetType();
                var types = GetArgTypes(args);
                if (type != _type || TryGetValue(types, out var method))
                {
                    _type = type;
                    var methods = _component.GetMethods(type, methodName, false, typeArgs, metadata);
                    Type[]? instanceArgs = null;
                    for (var i = 0; i < methods.Length; i++)
                        methods[i] = methods[i].WithArgs(args, ref instanceArgs);
                    var resultIndex = TrySelectMethod(methods, null, out _);
                    method = resultIndex >= 0 ? methods[resultIndex] : default;
                    this[types] = method;
                }

                if (method.IsEmpty)
                    BindingExceptionManager.ThrowInvalidBindingMember(type, methodName);
                return method.Method.Invoke(target, ConvertParameters(method, args, method.Parameters.LastOrDefault()?.IsParamArray() ?? false, metadata), metadata);
            }

            private static Type[] GetArgTypes(object?[]? args)
            {
                if (args == null || args.Length == 0)
                    return Default.EmptyArray<Type>();
                var result = new Type[args.Length];
                for (var i = 0; i < args.Length; i++)
                {
                    var o = args[i];
                    result[i] = o == null ? typeof(object) : o.GetType();
                }

                return result;
            }

            protected override bool Equals(Type[] x, Type[] y)
            {
                if (x.Length != y.Length)
                    return false;
                for (var i = 0; i < x.Length; i++)
                {
                    if (x[i] != y[i])
                        return false;
                }

                return true;
            }

            protected override int GetHashCode(Type[] key)
            {
                var hash = 0;
                unchecked
                {
                    for (var index = 0; index < key.Length; index++)
                        hash = hash * 397 ^ key[index].GetHashCode();
                }

                return hash;
            }

            #endregion
        }

        [StructLayout(LayoutKind.Auto)]
        private readonly struct TargetData
        {
            #region Fields

            public readonly Expression? Expression;
            public readonly IExpressionNode Node;
            public readonly Type Type;

            #endregion

            #region Constructors

            public TargetData(IExpressionNode node, Type type, Expression? expression)
            {
                Type = type;
                Node = node;
                Expression = expression;
            }

            #endregion

            #region Properties

            public bool IsStatic => Expression == null;

            #endregion
        }

        [StructLayout(LayoutKind.Auto)]
        private readonly struct ArgumentData
        {
            #region Fields

            public readonly IExpressionNode Node;
            public readonly Expression? Expression;
            public readonly Type? Type;

            #endregion

            #region Constuctors

            public ArgumentData(IExpressionNode node, Expression? expression, Type? type)
            {
                Should.NotBeNull(node, nameof(node));
                Node = node;
                Expression = expression;
                if (type == null && expression != null)
                    type = expression.Type;
                Type = type;
            }

            #endregion

            #region Properties

            public bool IsLambda => Node.NodeType == ExpressionNodeType.Lambda;

            #endregion

            #region Methods

            public ArgumentData UpdateExpression(Expression expression)
            {
                return new ArgumentData(Node, expression, Type);
            }

            #endregion
        }

        [StructLayout(LayoutKind.Auto)]
        private readonly struct MethodData
        {
            #region Fields

            private readonly IMethodInfo? _unresolvedMethod;

            #endregion

            #region Constructors

            public MethodData(IMethodInfo method)
                : this(method, null)
            {
                Method = method;
            }

            public MethodData(IMethodInfo method, IMethodInfo? unresolvedMethod, object? args = null)
            {
                Method = method;
                Parameters = method.GetParameters();
                _unresolvedMethod = unresolvedMethod;
                Args = args;
            }

            #endregion

            #region Properties

            public bool IsEmpty => Method == null;

            public IMethodInfo Method { get; }

            public IReadOnlyList<IParameterInfo> Parameters { get; }

            public object? Args { get; }

            public int ExpectedParameterCount
            {
                get
                {
                    if (Args == null)
                        return 0;
                    if (Args is Expression[] expressions)
                        return expressions.Length;
                    return ((Type[])Args!).Length;
                }
            }

            #endregion

            #region Methods

            public Type GetExpectedParameterType(int index)
            {
                if (Args is Expression[] expressions)
                    return expressions[index].Type;
                return ((Type[])Args!)[index];
            }

            public MethodData WithArgs(object?[] args, ref Type[]? instanceArgs)
            {
                if (instanceArgs == null)
                {
                    if (args.Length == 0)
                        instanceArgs = Default.EmptyArray<Type>();
                    else
                    {
                        instanceArgs = new Type[args.Length];
                        for (var i = 0; i < args.Length; i++)
                            instanceArgs[i] = args[i]?.GetType() ?? typeof(object);
                    }
                }

                return new MethodData(Method, null, instanceArgs);
            }

            public MethodData TryResolve(ArgumentData[] args, Expression[] expressions)
            {
                if (_unresolvedMethod == null)
                    return new MethodData(Method, null, expressions);

                var method = TryInferGenericMethod(_unresolvedMethod, args, out var unresolved);
                if (method == null || unresolved)
                    return default;
                return new MethodData(method!, null, expressions);
            }

            #endregion
        }

        #endregion
    }
}