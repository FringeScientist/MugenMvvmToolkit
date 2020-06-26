﻿using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Delegates;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Members;
using MugenMvvm.UnitTest.Binding.Members.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Members
{
    public class DelegateMethodMemberInfoTest : DelegateObservableMemberInfoTest
    {
        #region Properties

        protected override MemberType MemberType => MemberType.Method;

        #endregion

        #region Methods

        [Fact]
        public void ShouldNotBeGeneric()
        {
            var memberInfo = new DelegateMethodMemberInfo<string, object?, object?>("", typeof(object), typeof(object), MemberFlags.Dynamic, null, null, (member, target, args, metadata) => "", null, null, null, null);
            memberInfo.IsGenericMethod.ShouldBeFalse();
            memberInfo.IsGenericMethodDefinition.ShouldBeFalse();
            ShouldThrow<NotSupportedException>(() => memberInfo.GetGenericMethodDefinition());
            ShouldThrow<NotSupportedException>(() => memberInfo.MakeGenericMethod(new Type[0]));
            ShouldThrow<NotSupportedException>(() => memberInfo.GetGenericArguments());
        }

        [Fact]
        public void GetParametersShouldUseDelegate()
        {
            var memberInfo = new DelegateMethodMemberInfo<string, object?, object?>("", typeof(object), typeof(object), MemberFlags.Dynamic, null, null, (member, target, args, metadata) => "", null, null, null, null);
            memberInfo.GetParameters().ShouldBeEmpty();

            var invokeCount = 0;
            var parameters = new List<IParameterInfo> { null! };
            memberInfo = new DelegateMethodMemberInfo<string, object?, object?>("", typeof(object), typeof(object), MemberFlags.Dynamic, null, null, (member, target, args, metadata) => "", info =>
            {
                ++invokeCount;
                info.ShouldEqual(memberInfo);
                return parameters;
            }, null, null, null);
            memberInfo.GetParameters().ShouldEqual(parameters);
        }

        [Fact]
        public void TryGetAccessorShouldUseDelegate()
        {
            var flags = ArgumentFlags.Metadata;
            var values = new object[] { this };
            var memberInfo = new DelegateMethodMemberInfo<string, object?, object?>("", typeof(object), typeof(object), MemberFlags.Dynamic, null, null, (member, target, args, metadata) => "", null, null, null, null);
            memberInfo.TryGetAccessor(flags, values, DefaultMetadata).ShouldBeNull();

            var invokeCount = 0;
            var accessor = new TestAccessorMemberInfo();
            memberInfo = new DelegateMethodMemberInfo<string, object?, object?>("", typeof(object), typeof(object), MemberFlags.Dynamic, null, null, (member, target, args, metadata) => "", null,
                (member, argumentFlags, args, metadata) =>
                {
                    ++invokeCount;
                    member.ShouldEqual(memberInfo);
                    argumentFlags.ShouldEqual(flags);
                    args.ShouldEqual(values);
                    metadata.ShouldEqual(DefaultMetadata);
                    return accessor;
                }, null, null);
            memberInfo.TryGetAccessor(flags, values, DefaultMetadata).ShouldEqual(accessor);
        }

        [Fact]
        public void InvokeShouldUseDelegate()
        {
            IMemberInfo? m = null;
            var invokeCount = 0;
            string t = "";
            var objects = new object?[] { "t", 1 };
            var result = "test";
            var memberInfo = new DelegateMethodMemberInfo<string, string, object?>("", typeof(string), typeof(string), MemberFlags.Dynamic, null, null, (member, target, args, metadata) =>
            {
                ++invokeCount;
                member.ShouldEqual(m);
                target.ShouldEqual(t);
                objects.ShouldEqual(args);
                metadata.ShouldEqual(DefaultMetadata);
                return result;
            }, null, null, null, null);
            m = memberInfo;
            memberInfo.Invoke(t, objects, DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(1);
        }

        protected override DelegateObservableMemberInfo<TTarget, TState> Create<TTarget, TState>(string name, Type declaringType, Type memberType, MemberFlags accessModifiers, object? underlyingMember, in TState state,
            TryObserveDelegate<DelegateObservableMemberInfo<TTarget, TState>, TTarget>? tryObserve,
            RaiseDelegate<DelegateObservableMemberInfo<TTarget, TState>, TTarget>? raise)
        {
            return new DelegateMethodMemberInfo<TTarget, object?, TState>(name, declaringType, memberType, accessModifiers, underlyingMember, state, (member, target, args, metadata) => "",
                null, null, tryObserve, raise);
        }

        #endregion
    }
}