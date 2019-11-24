﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using MugenMvvm.Attributes;
using MugenMvvm.Collections;
using MugenMvvm.Collections.Internal;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Internal.Components
{
    public sealed class ExpressionReflectionDelegateProviderComponent : IReflectionDelegateProviderComponent, IActivatorReflectionDelegateProviderComponent,
        IMemberReflectionDelegateProviderComponent, IMethodReflectionDelegateProviderComponent, IHasPriority//todo use buffers
    {
        #region Fields

        private static readonly ParameterExpression EmptyParameterExpression = Expression.Parameter(typeof(object));

        private static readonly MemberInfoDelegateCache<MethodInfo?> CachedDelegates =
            new MemberInfoDelegateCache<MethodInfo?>();

        private static readonly MemberInfoLightDictionary<ConstructorInfo, Func<object?[], object>> ActivatorCache =
            new MemberInfoLightDictionary<ConstructorInfo, Func<object?[], object>>(59);

        private static readonly MemberInfoLightDictionary<MethodInfo, Func<object?, object?[], object?>> InvokeMethodCache =
            new MemberInfoLightDictionary<MethodInfo, Func<object?, object?[], object?>>(59);

        private static readonly MemberInfoDelegateCache<Delegate> InvokeMethodCacheDelegate =
            new MemberInfoDelegateCache<Delegate>();

        private static readonly MemberInfoDelegateCache<Delegate> MemberGetterCache =
            new MemberInfoDelegateCache<Delegate>();

        private static readonly MemberInfoDelegateCache<Delegate> MemberSetterCache =
            new MemberInfoDelegateCache<Delegate>();

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ExpressionReflectionDelegateProviderComponent()
        {
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        public Func<object?[], object>? TryGetActivator(ConstructorInfo constructor)
        {
            lock (ActivatorCache)
            {
                if (!ActivatorCache.TryGetValue(constructor, out var value))
                {
                    value = GetActivator(constructor);
                    ActivatorCache[constructor] = value;
                }

                return value;
            }
        }

        public Func<object?, TType>? TryGetMemberGetter<TType>(MemberInfo member)
        {
            var key = new MemberInfoDelegateCacheKey(member, typeof(TType));
            lock (MemberGetterCache)
            {
                if (!MemberGetterCache.TryGetValue(key, out var value))
                {
                    value = GetMemberGetter<TType>(member);
                    MemberGetterCache[key] = value;
                }

                return (Func<object?, TType>)value;
            }
        }

        public Action<object?, TType>? TryGetMemberSetter<TType>(MemberInfo member)
        {
            var key = new MemberInfoDelegateCacheKey(member, typeof(TType));
            lock (MemberSetterCache)
            {
                if (!MemberSetterCache.TryGetValue(key, out var value))
                {
                    value = GetMemberSetter<TType>(member);
                    MemberSetterCache[key] = value;
                }

                return (Action<object?, TType>)value;
            }
        }

        public Func<object?, object?[], object?>? TryGetMethodInvoker(MethodInfo method)
        {
            lock (InvokeMethodCache)
            {
                if (!InvokeMethodCache.TryGetValue(method, out var value))
                {
                    value = GetMethodInvoker(method);
                    InvokeMethodCache[method] = value;
                }

                return value;
            }
        }

        public Delegate? TryGetMethodInvoker(Type delegateType, MethodInfo method)
        {
            var cacheKey = new MemberInfoDelegateCacheKey(method, delegateType);
            lock (InvokeMethodCacheDelegate)
            {
                if (!InvokeMethodCacheDelegate.TryGetValue(cacheKey, out var value))
                {
                    value = GetMethodInvoker(delegateType, method);
                    InvokeMethodCacheDelegate[cacheKey] = value;
                }

                return value;
            }
        }

        public bool CanCreateDelegate(Type delegateType, MethodInfo method)
        {
            return TryGetMethodDelegateInternal(delegateType, method) != null;
        }

        public Delegate? TryCreateDelegate(Type delegateType, object? target, MethodInfo method)
        {
            method = TryGetMethodDelegateInternal(delegateType, method)!;
            if (method == null)
                return null;

            if (target == null)
                return method.CreateDelegate(delegateType);
            return method.CreateDelegate(delegateType, target);
        }

        #endregion

        #region Methods

        public static MethodInfo? TryGetMethodDelegate(Type delegateType, MethodInfo method)
        {
            if (!typeof(Delegate).IsAssignableFrom(delegateType))
                return null;

            var mParameters = method.GetParameters();
            var eParameters = delegateType.GetMethod(nameof(Action.Invoke), BindingFlagsEx.InstancePublic)?.GetParameters();
            if (eParameters == null || mParameters.Length != eParameters.Length)
                return null;
            if (method.IsGenericMethodDefinition)
            {
                var genericArguments = method.GetGenericArguments();
                var types = new Type[genericArguments.Length];
                var index = 0;
                for (var i = 0; i < mParameters.Length; i++)
                {
                    if (mParameters[i].ParameterType.IsGenericParameter)
                        types[index++] = eParameters[i].ParameterType;
                }

                try
                {
                    method = method.MakeGenericMethod(types);
                }
                catch (Exception e)
                {
                    Tracer.Warn(e.Flatten(true));
                    return null;
                }

                mParameters = method.GetParameters();
            }

            for (var i = 0; i < mParameters.Length; i++)
            {
                var mParameter = mParameters[i].ParameterType;
                var eParameter = eParameters[i].ParameterType;
                if (!mParameter.IsAssignableFrom(eParameter) || mParameter.IsValueType != eParameter.IsValueType)
                    return null;
            }

            return method;
        }

        public static Func<object?[], object> GetActivator(ConstructorInfo constructor)
        {
            var expressions = GetParametersExpression(constructor, out var parameterExpression);
            var newExpression = Expression.New(constructor, expressions).ConvertIfNeed(typeof(object), false);
            return Expression.Lambda<Func<object?[], object>>(newExpression, parameterExpression).CompileEx();
        }

        public static Func<object?, object?[], object?> GetMethodInvoker(MethodInfo method)
        {
            var expressions = GetParametersExpression(method, out var parameterExpression);
            if (method.IsStatic)
            {
                return Expression
                    .Lambda<Func<object?, object?[], object?>>(Expression
                        .Call(null, method, expressions)
                        .ConvertIfNeed(typeof(object), false), EmptyParameterExpression, parameterExpression)
                    .CompileEx();
            }

            var declaringType = method.DeclaringType;
            var targetExp = Expression.Parameter(typeof(object), "target");
            return Expression
                .Lambda<Func<object?, object?[], object?>>(Expression
                    .Call(targetExp.ConvertIfNeed(declaringType, false), method, expressions)
                    .ConvertIfNeed(typeof(object), false), targetExp, parameterExpression)
                .CompileEx();
        }

        public static Delegate GetMethodInvoker(Type delegateType, MethodInfo method)
        {
            var delegateMethod = delegateType.GetMethodOrThrow(nameof(Action.Invoke), BindingFlagsEx.InstanceOnly);
            var delegateParams = delegateMethod.GetParameters().ToList();
            var methodParams = method.GetParameters();
            var expressions = new List<Expression>();
            var parameters = new List<ParameterExpression>();
            if (!method.IsStatic)
            {
                var thisParam = Expression.Parameter(delegateParams[0].ParameterType, "@this");
                parameters.Add(thisParam);
                expressions.Add(thisParam.ConvertIfNeed(method.DeclaringType, false));
                delegateParams.RemoveAt(0);
            }

            Should.BeValid("delegateType", delegateParams.Count == methodParams.Length);
            for (var i = 0; i < methodParams.Length; i++)
            {
                var parameter = Expression.Parameter(delegateParams[i].ParameterType, i.ToString());
                parameters.Add(parameter);
                expressions.Add(parameter.ConvertIfNeed(methodParams[i].ParameterType, false));
            }

            Expression callExpression;
            if (method.IsStatic)
                callExpression = Expression.Call(null, method, expressions.ToArray());
            else
            {
                var @this = expressions[0];
                expressions.RemoveAt(0);
                callExpression = Expression.Call(@this, method, expressions.ToArray());
            }

            var lambdaExpression = Expression.Lambda(delegateType, callExpression.ConvertIfNeed(delegateMethod.ReturnType, false), parameters);
            return lambdaExpression.CompileEx();
        }

        public static Func<object?, TType> GetMemberGetter<TType>(MemberInfo member)
        {
            var target = Expression.Parameter(typeof(object), "instance");
            MemberExpression accessExp;
            if (member.IsStatic())
                accessExp = Expression.MakeMemberAccess(null, member);
            else
            {
                var declaringType = member.DeclaringType;
                accessExp = Expression.MakeMemberAccess(target.ConvertIfNeed(declaringType, false), member);
            }

            return Expression
                .Lambda<Func<object?, TType>>(accessExp.ConvertIfNeed(typeof(TType), false), target)
                .CompileEx();
        }

        public static Action<object?, TType> GetMemberSetter<TType>(MemberInfo member)
        {
            var declaringType = member.DeclaringType;
            var fieldInfo = member as FieldInfo;
            if (declaringType.IsValueType)
            {
                if (fieldInfo == null)
                {
                    var propertyInfo = (PropertyInfo)member;
                    return propertyInfo.SetValue<TType>;
                }

                return fieldInfo.SetValue<TType>;
            }

            Expression expression;
            var targetParameter = Expression.Parameter(typeof(object), "instance");
            var valueParameter = Expression.Parameter(typeof(TType), "value");
            var target = targetParameter.ConvertIfNeed(declaringType, false);
            if (fieldInfo == null)
            {
                var propertyInfo = member as PropertyInfo;
                MethodInfo? setMethod = null;
                if (propertyInfo != null)
                    setMethod = propertyInfo.GetSetMethod(true);
                Should.MethodBeSupported(propertyInfo != null && setMethod != null, MessageConstant.ShouldSupportOnlyFieldsReadonlyFields);
                var valueExpression = valueParameter.ConvertIfNeed(propertyInfo.PropertyType, false);
                expression = Expression.Call(setMethod.IsStatic ? null : target.ConvertIfNeed(declaringType, false), setMethod, valueExpression);
            }
            else
            {
                expression = Expression.Field(fieldInfo.IsStatic ? null : target.ConvertIfNeed(declaringType, false), fieldInfo);
                expression = Expression.Assign(expression, valueParameter.ConvertIfNeed(fieldInfo.FieldType, false));
            }

            return Expression
                .Lambda<Action<object?, TType>>(expression, targetParameter, valueParameter)
                .CompileEx();
        }

        private static Expression[] GetParametersExpression(MethodBase methodBase, out ParameterExpression parameterExpression)
        {
            var paramsInfo = methodBase.GetParameters();
            //create a single param of type object[]
            parameterExpression = Expression.Parameter(typeof(object[]), "args");
            var argsExp = new Expression[paramsInfo.Length];

            //pick each arg from the params array
            //and create a typed expression of them
            for (var i = 0; i < paramsInfo.Length; i++)
            {
                Expression paramAccessorExp = Expression.ArrayIndex(parameterExpression, MugenExtensions.GetConstantExpression(i));
                var paramCastExp = paramAccessorExp.ConvertIfNeed(paramsInfo[i].ParameterType, false);
                argsExp[i] = paramCastExp;
            }

            return argsExp;
        }

        private static MethodInfo? TryGetMethodDelegateInternal(Type delegateType, MethodInfo method)
        {
            var key = new MemberInfoDelegateCacheKey(method, delegateType);
            MethodInfo? info;
            lock (CachedDelegates)
            {
                if (!CachedDelegates.TryGetValue(key, out info))
                {
                    info = TryGetMethodDelegate(delegateType, method);
                    CachedDelegates[key] = info;
                }
            }

            return info;
        }

        #endregion

        #region Nested types

        private sealed class MemberInfoDelegateCache<TValue> : LightDictionary<MemberInfoDelegateCacheKey, TValue>
        {
            #region Constructors

            public MemberInfoDelegateCache() : base(59)
            {
            }

            #endregion

            #region Methods

            protected override bool Equals(MemberInfoDelegateCacheKey x, MemberInfoDelegateCacheKey y)
            {
                return x.DelegateType == y.DelegateType && x.Member == y.Member;
            }

            protected override int GetHashCode(MemberInfoDelegateCacheKey key)
            {
                return HashCode.Combine(key.DelegateType, key.Member);
            }

            #endregion
        }

        [StructLayout(LayoutKind.Auto)]
        private readonly struct MemberInfoDelegateCacheKey
        {
            #region Fields

            public readonly MemberInfo Member;
            public readonly Type DelegateType;

            #endregion

            #region Constructors

            public MemberInfoDelegateCacheKey(MemberInfo member, Type delegateType)
            {
                Member = member;
                DelegateType = delegateType;
            }

            #endregion
        }

        #endregion
    }
}