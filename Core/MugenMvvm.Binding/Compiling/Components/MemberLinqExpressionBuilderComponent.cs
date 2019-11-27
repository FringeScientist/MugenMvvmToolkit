﻿using System;
using System.Linq.Expressions;
using System.Reflection;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Compiling;
using MugenMvvm.Binding.Interfaces.Compiling.Components;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Compiling.Components
{
    public sealed class MemberLinqExpressionBuilderComponent : ILinqExpressionBuilderComponent, IHasPriority
    {
        #region Fields

        private readonly IMemberProvider? _memberProvider;
        private readonly Expression _thisExpression;

        private static readonly MethodInfo GetValuePropertyMethod =
            typeof(IMemberAccessorInfo).GetMethodOrThrow(nameof(IMemberAccessorInfo.GetValue), BindingFlagsEx.InstancePublic);

        private static readonly MethodInfo GetValueDynamicMethod = typeof(MemberLinqExpressionBuilderComponent).GetMethodOrThrow(nameof(GetValueDynamic), BindingFlagsEx.InstancePublic);

        #endregion

        #region Constructors

        public MemberLinqExpressionBuilderComponent(IMemberProvider? memberProvider = null)
        {
            _memberProvider = memberProvider;
            _thisExpression = Expression.Constant(this);
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = CompilingComponentPriority.Member;

        public MemberFlags MemberFlags { get; set; } = MemberFlags.All & ~MemberFlags.NonPublic;

        #endregion

        #region Implementation of interfaces

        public Expression? TryBuild(ILinqExpressionBuilderContext context, IExpressionNode expression)
        {
            if (!(expression is IMemberExpressionNode memberExpression) || memberExpression.Target == null)
                return null;

            Expression? target = context.Build(memberExpression.Target);
            var type = MugenBindingExtensions.GetTargetType(ref target);
            var member = memberExpression.Member;
            if (member == null)
            {
                MemberFlags flags;
                if (target == null)
                {
                    if (type.IsEnum)
                        return Expression.Constant(Enum.Parse(type, memberExpression.MemberName));
                    flags = MemberFlags.SetInstanceOrStaticFlags(true);
                }
                else
                    flags = MemberFlags.SetInstanceOrStaticFlags(false);

                member = _memberProvider
                    .DefaultIfNull()
                    .GetMember(type, memberExpression.MemberName, MemberType.Accessor, flags, context.GetMetadataOrDefault()) as IMemberAccessorInfo;
            }

            if (member == null)
            {
                if (target == null)
                {
                    context.TryGetErrors()?.Add(BindingMessageConstant.InvalidBindingMemberFormat2.Format(memberExpression.Member, type));
                    return null;
                }

                return Expression.Call(_thisExpression, GetValueDynamicMethod,
                    target.ConvertIfNeed(typeof(object), false),
                    Expression.Constant(memberExpression.MemberName),
                    context.MetadataParameter);
            }

            var result = TryCompile(target, member.UnderlyingMember);
            if (result != null)
                return result;

            var methodCall = Expression.Call(Expression.Constant(member),
                GetValuePropertyMethod, target.ConvertIfNeed(typeof(object), false), context.MetadataParameter);
            return Expression.Convert(methodCall, member.Type);
        }

        #endregion

        #region Methods

        [Preserve(Conditional = true)]
        public object? GetValueDynamic(object? target, string member, IReadOnlyMetadataContext? metadata)
        {
            if (target == null)
                return null;
            var property = MugenBindingService
                    .MemberProvider
                    .GetMember(target.GetType(), member, MemberType.Accessor, MemberFlags.SetInstanceOrStaticFlags(false), metadata) as IMemberAccessorInfo;
            if (property == null)
                BindingExceptionManager.ThrowInvalidBindingMember(target.GetType(), member);
            return property.GetValue(target, metadata);
        }

        private static Expression? TryCompile(Expression? target, object? member)
        {
            if (member == null)
                return null;
            if (member is PropertyInfo property)
                return Expression.Property(target, property);
            if (member is FieldInfo field)
                return Expression.Field(target, field);
            return null;
        }

        #endregion
    }
}