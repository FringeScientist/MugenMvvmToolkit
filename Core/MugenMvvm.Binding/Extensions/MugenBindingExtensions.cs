﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using MugenMvvm.Binding.Compiling;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Compiling;
using MugenMvvm.Binding.Interfaces.Converters;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Members.Descriptors;
using MugenMvvm.Binding.Metadata;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

// ReSharper disable once CheckNamespace
namespace MugenMvvm.Binding
{
    public static partial class MugenBindingExtensions
    {
        #region Fields

        public static readonly char[] CommaSeparator = { ',' };
        public static readonly char[] DotSeparator = { '.' };

        #endregion

        #region Methods

        [DoesNotReturn]
        public static void ThrowCannotCompile(this ILinqExpressionBuilderContext context, IExpressionNode expression)
        {
            var errors = context.TryGetErrors();
            if (errors != null && errors.Count != 0)
            {
                errors.Reverse();
                BindingExceptionManager.ThrowCannotCompileExpression(expression, BindingMessageConstant.PossibleReasons + string.Join(Environment.NewLine, errors));
            }
            else
                BindingExceptionManager.ThrowCannotCompileExpression(expression);
        }

        public static List<string>? TryGetErrors(this ILinqExpressionBuilderContext context)
        {
            Should.NotBeNull(context, nameof(context));
            if (context.HasMetadata && context.Metadata.TryGet(CompilingMetadata.CompilingErrors, out var errors))
                return errors;
            return null;
        }

        public static object? Invoke(this ICompiledExpression? expression, object? sourceRaw, IReadOnlyMetadataContext? metadata)
        {
            if (expression == null)
                return BindingMetadata.UnsetValue;
            ItemOrList<ExpressionValue, ExpressionValue[]> values;
            if (sourceRaw == null)
                values = Default.EmptyArray<ExpressionValue>();
            else if (sourceRaw is IMemberPathObserver[] sources)
            {
                var expressionValues = new ExpressionValue[sources.Length];
                for (var i = 0; i < sources.Length; i++)
                {
                    var members = sources[i].GetLastMember(metadata);
                    var value = members.GetValue(metadata);
                    if (value.IsUnsetValueOrDoNothing())
                        return value;
                    expressionValues[i] = new ExpressionValue(value?.GetType() ?? members.Member.Type, null);
                }

                values = expressionValues;
            }
            else
            {
                var members = ((IMemberPathObserver)sourceRaw).GetLastMember(metadata);
                var value = members.GetValue(metadata);
                if (value.IsUnsetValueOrDoNothing())
                    return value;

                values = new ExpressionValue(value?.GetType() ?? members.Member.Type, value);
            }
            return expression.Invoke(values, metadata);
        }

        public static bool EqualsEx(this IMemberInfo x, IMemberInfo y)
        {
            if (x.MemberType != y.MemberType || x.Name != y.Name || x.DeclaringType != y.DeclaringType)
                return false;

            if (x.MemberType != MemberType.Method)
                return true;

            var xM = ((IMethodInfo)x).GetParameters();
            var yM = ((IMethodInfo)y).GetParameters();
            if (xM.Count != yM.Count)
                return false;

            for (var i = 0; i < xM.Count; i++)
            {
                if (xM[i].ParameterType != yM[i].ParameterType)
                    return false;
            }

            return true;
        }

        public static int GetHashCodeEx(this IMemberInfo memberInfo)
        {
            return HashCode.Combine(memberInfo.DeclaringType, (int)memberInfo.MemberType, memberInfo.Name);
        }

        public static object? GetParent(object? target, IReadOnlyMetadataContext? metadata = null, IMemberProvider? provider = null)
        {
            return target?.GetBindableMemberValue(BindableMembers.Object.Parent, null, MemberFlags.All, metadata, provider);
        }

        public static object? FindElementSource(object target, string elementName, IReadOnlyMetadataContext? metadata = null, IMemberProvider? provider = null)
        {
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(elementName, nameof(elementName));
            var args = new object[1];
            while (target != null)
            {
                args[0] = elementName;
                var result = target.TryInvokeBindableMethod(BindableMembers.Object.FindByName, args, MemberFlags.All, metadata, provider);
                if (result != null)
                    return result;
                target = GetParent(target, metadata, provider)!;
            }

            return null;
        }

        public static object? FindRelativeSource(object target, string typeName, int level, IReadOnlyMetadataContext? metadata = null, IMemberProvider? provider = null)
        {
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(typeName, nameof(typeName));
            object? fullNameSource = null;
            object? nameSource = null;
            var fullNameLevel = 0;
            var nameLevel = 0;

            target = GetParent(target, metadata, provider)!;
            while (target != null)
            {
                TypeNameEqual(target.GetType(), typeName, out var shortNameEqual, out var fullNameEqual);
                if (shortNameEqual)
                {
                    nameSource = target;
                    nameLevel++;
                }

                if (fullNameEqual)
                {
                    fullNameSource = target;
                    fullNameLevel++;
                }

                if (fullNameSource != null && fullNameLevel == level)
                    return fullNameSource;
                if (nameSource != null && nameLevel == level)
                    return nameSource;

                target = GetParent(target, metadata, provider)!;
            }

            return null;
        }

        public static bool IsAllMembersAvailable(this ItemOrList<IMemberPathObserver, IMemberPathObserver[]> observers)
        {
            var list = observers.List;
            if (list == null)
            {
                var item = observers.Item;
                return item == null || item.IsAllMembersAvailable();
            }

            for (var i = 0; i < list.Length; i++)
            {
                if (!list[i].IsAllMembersAvailable())
                    return false;
            }

            return true;
        }

        public static bool IsAllMembersAvailable(this IMemberPathObserver observer)
        {
            return observer.GetLastMember().IsAvailable;
        }

        public static object? GetValueFromPath(this IMemberPath path, Type type, object? target, MemberFlags flags,
            int firstMemberIndex = 0, IReadOnlyMetadataContext? metadata = null, IMemberProvider? memberProvider = null)
        {
            Should.NotBeNull(type, nameof(type));
            memberProvider = memberProvider.DefaultIfNull();
            if (path.Members.Count == 0)
            {
                if (firstMemberIndex > 1)
                    ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(firstMemberIndex));

                if (firstMemberIndex == 1)
                    return target;

                return memberProvider.GetValue(type, target, path.Path, flags, metadata);
            }

            if (firstMemberIndex > path.Members.Count)
                ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(firstMemberIndex));

            for (var index = firstMemberIndex; index < path.Members.Count; index++)
            {
                var pathMember = path.Members[index];
                if (index == 1)
                    flags = flags.SetInstanceOrStaticFlags(false);
                target = memberProvider.GetValue(type, target, pathMember, flags, metadata);
                if (target.IsNullOrUnsetValue())
                    return target;
                type = target.GetType();
            }

            return target;
        }

        public static IMemberInfo? GetLastMemberFromPath(this IMemberPath path, Type type, object? target, MemberFlags flags,
            MemberType lastMemberType, IReadOnlyMetadataContext? metadata = null, IMemberProvider? memberProvider = null)
        {
            Should.NotBeNull(type, nameof(type));
            memberProvider = memberProvider.DefaultIfNull();
            string lastMemberName;
            if (path.Members.Count == 0)
                lastMemberName = path.Path;
            else
            {
                for (var i = 0; i < path.Members.Count - 1; i++)
                {
                    if (i == 1)
                        flags = flags.SetInstanceOrStaticFlags(false);
                    var member = memberProvider.GetMember(type, path.Members[i], MemberType.Accessor, flags, metadata);
                    if (!(member is IMemberAccessorInfo accessor) || !accessor.CanRead)
                        return null;

                    target = accessor.GetValue(target, metadata);
                    if (target.IsNullOrUnsetValue())
                        return null;
                    type = target.GetType();
                }

                flags = flags.SetInstanceOrStaticFlags(false);
                lastMemberName = path.Members[path.Members.Count - 1];
            }

            return memberProvider.GetMember(type, lastMemberName, lastMemberType, flags, metadata);
        }

        public static bool TryBuildBindingMemberPath(this IExpressionNode? target, StringBuilder builder, out IExpressionNode? firstExpression)
        {
            return TryBuildBindingMemberPath(target, builder, null, out firstExpression);
        }

        public static bool TryBuildBindingMemberPath(this IExpressionNode? target, StringBuilder builder, Func<IExpressionNode, bool>? condition, out IExpressionNode? firstExpression)
        {
            Should.NotBeNull(builder, nameof(builder));
            firstExpression = null;
            builder.Clear();
            if (target == null)
                return false;
            while (target != null)
            {
                firstExpression = target;
                if (condition != null && !condition(target))
                    return false;

                if (target is IMemberExpressionNode memberExpressionNode)
                {
                    var memberName = memberExpressionNode.MemberName.Trim();
                    builder.Insert(0, memberName);
                    if (memberExpressionNode.Target != null)
                        builder.Insert(0, '.');
                    target = memberExpressionNode.Target;
                }
                else
                {
                    if (target is IIndexExpressionNode indexExpressionNode && indexExpressionNode.Arguments.All(arg => arg.NodeType == ExpressionNodeType.Constant))
                    {
                        var args = indexExpressionNode.Arguments;
                        builder.Insert(0, ']');
                        if (args.Count > 0)
                        {
                            args.Last().ToStringValue(builder);
                            for (var i = args.Count - 2; i >= 0; i--)
                            {
                                builder.Insert(0, ',');
                                args[i].ToStringValue(builder);
                            }
                        }

                        builder.Insert(0, '[');
                        target = indexExpressionNode.Target;
                    }
                    else if (target is IMethodCallExpressionNode methodCallExpression && methodCallExpression.Arguments.All(arg => arg.NodeType == ExpressionNodeType.Constant))
                    {
                        var args = methodCallExpression.Arguments;
                        builder.Insert(0, ')');
                        if (args.Count > 0)
                        {
                            args.Last().ToStringValue(builder);
                            for (var i = args.Count - 2; i >= 0; i--)
                            {
                                builder.Insert(0, ',');
                                args[i].ToStringValue(builder);
                            }
                        }

                        builder.Insert(0, '(');
                        builder.Insert(0, methodCallExpression.MethodName);
                        builder.Insert(0, '.');
                        target = methodCallExpression.Target;
                    }
                    else
                        return false;
                }
            }

            return true;
        }

        public static IExpressionNode? TryGetRootMemberExpression(this IExpressionNode? target, Func<IExpressionNode, bool>? condition = null)
        {
            if (target == null)
                return null;
            IExpressionNode? result = null;
            while (target != null)
            {
                result = target;
                if (condition != null && !condition(target))
                    return null;

                switch (target)
                {
                    case IMemberExpressionNode memberExpressionNode:
                        target = memberExpressionNode.Target;
                        break;
                    case IIndexExpressionNode indexExpressionNode when indexExpressionNode.Arguments.All(arg => arg.NodeType == ExpressionNodeType.Constant):
                        target = indexExpressionNode.Target;
                        break;
                    case IMethodCallExpressionNode methodCallExpression when methodCallExpression.Arguments.All(arg => arg.NodeType == ExpressionNodeType.Constant):
                        target = methodCallExpression.Target;
                        break;
                    default:
                        return null;
                }
            }

            return result;
        }

        [return: MaybeNull]
        public static TValue GetBindableMemberValue<TTarget, TValue>(this TTarget target,
            BindableAccessorDescriptor<TTarget, TValue> bindableMember, TValue defaultValue = default, MemberFlags flags = MemberFlags.All,
            IReadOnlyMetadataContext? metadata = null, IMemberProvider? provider = null) where TTarget : class
        {
            var propertyInfo = provider
                .DefaultIfNull()
                .GetMember(target.GetType(), bindableMember.Name, MemberType.Accessor, flags, metadata) as IMemberAccessorInfo;
            if (propertyInfo == null)
                return defaultValue;
            if (propertyInfo is IMemberAccessorInfo<TTarget, TValue> p)
                return p.GetValue(target, metadata);
            return (TValue)propertyInfo.GetValue(target, metadata)!;
        }

        public static void SetBindableMemberValue<TTarget, TValue>(this TTarget target,
            BindableAccessorDescriptor<TTarget, TValue> bindableMember, [MaybeNull] TValue value, bool throwOnError = true, MemberFlags flags = MemberFlags.All,
            IReadOnlyMetadataContext? metadata = null, IMemberProvider? provider = null) where TTarget : class
        {
            var propertyInfo = provider
                .DefaultIfNull()
                .GetMember(target.GetType(), bindableMember.Name, MemberType.Accessor, flags, metadata) as IMemberAccessorInfo;
            if (propertyInfo == null)
            {
                if (throwOnError)
                    BindingExceptionManager.ThrowInvalidBindingMember(target.GetType(), bindableMember.Name);
                return;
            }

            if (propertyInfo is IMemberAccessorInfo<TTarget, TValue> p)
                p.SetValue(target, value, metadata);
            else
                propertyInfo.SetValue(target, value, metadata);
        }

        public static ActionToken TryObserveBindableMember<TTarget, TValue>(this TTarget target,
            BindableAccessorDescriptor<TTarget, TValue> bindableMember, IEventListener listener, MemberFlags flags = MemberFlags.All,
            IReadOnlyMetadataContext? metadata = null, IMemberProvider? provider = null) where TTarget : class
        {
            var propertyInfo = provider
                .DefaultIfNull()
                .GetMember(target.GetType(), bindableMember.Name, MemberType.Accessor, flags, metadata) as IObservableMemberInfo;
            if (propertyInfo == null)
                return default;
            return propertyInfo.TryObserve(target, listener, metadata);
        }

        public static ActionToken TrySubscribeBindableEvent<TTarget>(this TTarget target,
            BindableEventDescriptor<TTarget> eventMember, IEventListener listener, MemberFlags flags = MemberFlags.All,
            IReadOnlyMetadataContext? metadata = null, IMemberProvider? provider = null) where TTarget : class
        {
            var eventInfo = provider
                .DefaultIfNull()
                .GetMember(target.GetType(), eventMember.Name, MemberType.Event, flags, metadata) as IEventInfo;
            if (eventInfo == null)
                return default;
            return eventInfo.TrySubscribe(target, listener, metadata);
        }

        public static object? TryInvokeBindableMethod<TTarget>(this TTarget target,
            BindableMethodDescriptor<TTarget> methodMember, object?[]? args = null, MemberFlags flags = MemberFlags.All,
            IReadOnlyMetadataContext? metadata = null, IMemberProvider? provider = null) where TTarget : class
        {
            var methodInfo = provider
                .DefaultIfNull()
                .GetMember(target.GetType(), methodMember.Name, MemberType.Method, flags, metadata) as IMethodInfo;
            return methodInfo?.Invoke(target, args ?? Default.EmptyArray<object>());
        }

        public static WeakEventListener ToWeak(this IEventListener listener)
        {
            return new WeakEventListener(listener);
        }

        public static WeakEventListener<TState> ToWeak<TState>(this IEventListener listener, TState state)
        {
            return new WeakEventListener<TState>(listener, state);
        }

        public static TItem[] ConvertValues<TItem>(this IGlobalValueConverter? converter, string[] args, IReadOnlyMetadataContext? metadata)
        {
            converter = converter.DefaultIfNull();
            var result = new TItem[args.Length];
            for (var i = 0; i < args.Length; i++)
            {
                var s = args[i];
                if (!string.IsNullOrEmpty(s) && s[0] == '\"' && s.EndsWith("\""))
                    s = s.RemoveBounds();
                result[i] = (TItem)(s == "null" ? null : converter.Convert(s, typeof(TItem), metadata: metadata))!;
            }

            return result;
        }

        public static object?[] ConvertValues(this IGlobalValueConverter? converter, string[] args, ParameterInfo[]? parameters,
            Type? castType, IReadOnlyMetadataContext? metadata, int parametersStartIndex = 0)
        {
            if (parameters == null)
                Should.NotBeNull(castType, nameof(castType));
            else
                Should.NotBeNull(parameters, nameof(parameters));
            if (args.Length == 0)
                return Default.EmptyArray<object?>();

            converter = converter.DefaultIfNull();
            var result = new object?[args.Length];
            for (var i = 0; i < args.Length; i++)
            {
                var s = args[i];
                if (parameters != null)
                    castType = parameters[i + parametersStartIndex].ParameterType;
                if (!string.IsNullOrEmpty(s) && s[0] == '\"' && s.EndsWith("\""))
                    s = s.RemoveBounds();
                result[i] = s == "null" ? null : converter.Convert(s, castType!, parameters?[i + parametersStartIndex], metadata);
            }

            return result;
        }

        public static string[]? GetIndexerArgsRaw(string path)
        {
            int start = 1;
            if (path.StartsWith("Item[", StringComparison.Ordinal))
                start = 5;
            else if (!path.StartsWith("[", StringComparison.Ordinal) || !path.EndsWith("]", StringComparison.Ordinal))
                return null;

            return path
                .RemoveBounds(start)
                .Split(CommaSeparator, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string[]? GetMethodArgsRaw(string path, out string methodName)
        {
            var startIndex = path.IndexOf('(');
            if (startIndex < 0 || !path.EndsWith(")", StringComparison.Ordinal))
            {
                methodName = path;
                return null;
            }

            methodName = path.Substring(0, startIndex);
            return path
                .RemoveBounds(startIndex + 1)
                .Split(CommaSeparator, StringSplitOptions.RemoveEmptyEntries);
        }

        public static Type GetTargetType(this MemberFlags flags, object target)
        {
            if (flags.HasFlagEx(MemberFlags.Static))
                return target as Type ?? target.GetType();
            return target.GetType();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasFlagEx(this MemberFlags value, MemberFlags flag)
        {
            return (value & flag) == flag;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasFlagEx(this BindingMemberExpressionFlags value, BindingMemberExpressionFlags flag)
        {
            return (value & flag) == flag;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasFlagEx(this MemberType value, MemberType flag)
        {
            return (value & flag) == flag;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static MemberFlags SetInstanceOrStaticFlags(this MemberFlags value, bool isStatic)
        {
            return isStatic ? (value | MemberFlags.Static) & ~MemberFlags.Instance : (value | MemberFlags.Instance) & ~MemberFlags.Static;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static MemberFlags ClearInstanceOrStaticFlags(this MemberFlags value, bool isStatic)
        {
            return isStatic ? value & ~MemberFlags.Instance : value & ~MemberFlags.Static;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsNullOrUnsetValue([NotNullWhen(false)]this object? value)
        {
            return value == null || ReferenceEquals(value, BindingMetadata.UnsetValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsUnsetValueOrDoNothing(this object? value)
        {
            return ReferenceEquals(value, BindingMetadata.UnsetValue) || ReferenceEquals(value, BindingMetadata.DoNothing);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsDoNothing(this object? value)
        {
            return ReferenceEquals(value, BindingMetadata.DoNothing);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsUnsetValue(this object? value)
        {
            return ReferenceEquals(value, BindingMetadata.UnsetValue);
        }

        internal static string GetPath(this StringBuilder memberNameBuilder)
        {
            if (memberNameBuilder.Length != 0 && memberNameBuilder[0] == '.')
                memberNameBuilder.Remove(0, 1);
            return memberNameBuilder.ToString();
        }

        internal static T[] InsertFirstArg<T>(this T[] args, T firstArg)
        {
            if (args == null || args.Length == 0)
                return new[] { firstArg };
            var objects = new T[args.Length + 1];
            objects[0] = firstArg;
            Array.Copy(args, 0, objects, 1, args.Length);
            return objects;
        }

        internal static void AddMethodObserver(this ObserverBase.IMethodPathObserver observer, object? target, IMemberInfo? lastMember, ref ActionToken unsubscriber, ref IWeakReference? lastValueRef)
        {
            unsubscriber.Dispose();
            if (target == null || !(lastMember is IMemberAccessorInfo propertyInfo))
            {
                unsubscriber = ActionToken.NoDoToken;
                return;
            }

            var value = propertyInfo.GetValue(target);
            if (ReferenceEquals(value, lastValueRef?.Target))
                return;

            if (value.IsNullOrUnsetValue())
            {
                unsubscriber = ActionToken.NoDoToken;
                return;
            }

            var type = value.GetType();
            if (type.IsValueType)
            {
                unsubscriber = ActionToken.NoDoToken;
                return;
            }

            lastValueRef = value.ToWeakReference();
            var member = MugenBindingService.MemberProvider.GetMember(type, observer.Method, MemberType.Method, observer.MemberFlags.SetInstanceOrStaticFlags(false));
            if (member is IObservableMemberInfo observable)
                unsubscriber = observable.TryObserve(target, observer.GetMethodListener());
            if (unsubscriber.IsEmpty)
                unsubscriber = ActionToken.NoDoToken;
        }

        private static object? GetValue(this IMemberProvider memberProvider, Type type, object? target, string path, MemberFlags flags, IReadOnlyMetadataContext? metadata)
        {
            var member = memberProvider.GetMember(type, path, MemberType.Accessor, flags, metadata) as IMemberAccessorInfo;
            if (member == null)
                BindingExceptionManager.ThrowInvalidBindingMember(type, path);
            return member.GetValue(target, metadata);
        }

        private static string RemoveBounds(this string st, int start = 1) //todo Span?
        {
            return st.Substring(start, st.Length - start - 1);
        }

        private static void ToStringValue(this IExpressionNode expression, StringBuilder builder)
        {
            var constantExpressionNode = (IConstantExpressionNode)expression;
            var value = constantExpressionNode.Value;

            if (value == null)
            {
                builder.Insert(0, "null");
                return;
            }

            if (value is string st)
            {
                builder.Insert(0, '"');
                builder.Insert(0, st);
                builder.Insert(0, '"');
                return;
            }

            builder.Insert(0, value);
        }

        #endregion
    }
}