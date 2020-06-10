﻿using System;
using System.Collections.Generic;
using MugenMvvm.Binding;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Members.Builders;
using MugenMvvm.Internal;
using MugenMvvm.UnitTest.Binding.Members.Internal;
using MugenMvvm.UnitTest.Binding.Observers.Internal;
using MugenMvvm.UnitTest.Internal.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Members.Builders
{
    public class PropertyBuilderTest : UnitTestBase
    {
        #region Methods

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ConstructorShouldInitializeValues(bool isStatic)
        {
            string name = "t";
            Type declaringType = typeof(object);
            var propertyType = typeof(Action);
            var member = new object();
            var builder = new PropertyBuilder<object, object>(name, declaringType, propertyType).UnderlyingMember(member);
            if (isStatic)
                builder = builder.Static();
            var build = builder.Build();
            build.MemberType.ShouldEqual(MemberType.Accessor);
            build.Type.ShouldEqual(propertyType);
            build.DeclaringType.ShouldEqual(declaringType);
            build.AccessModifiers.ShouldEqual(isStatic ? MemberFlags.Attached | MemberFlags.StaticPublic : MemberFlags.Attached | MemberFlags.InstancePublic);
            build.UnderlyingMember.ShouldEqual(member);
            build.Name.ShouldEqual(name);
        }

        [Fact]
        public void StaticAutoGeneratedShouldRaiseOnChange()
        {
            var message = "m";
            IAccessorMemberInfo? memberInfo = null;
            object? oldV = null;
            object? newV = this;
            var propertyChangedInvokeCount = 0;

            memberInfo = new PropertyBuilder<object, object>(nameof(StaticAutoGeneratedShouldRaiseOnChange), typeof(object), typeof(EventHandler)).Static().PropertyChangedHandler(
                (member, target, value, newValue, metadata) =>
                {
                    ++propertyChangedInvokeCount;
                    memberInfo.ShouldEqual(member);
                    target.ShouldBeNull();
                    value.ShouldEqual(oldV);
                    newValue.ShouldEqual(newV);
                    metadata.ShouldEqual(DefaultMetadata);
                }).Build();
            var testEventHandler = new TestWeakEventListener
            {
                TryHandle = (t, msg, meta) =>
                {
                    t.ShouldBeNull();
                    message.ShouldNotBeNull();
                    meta.ShouldEqual(DefaultMetadata);
                    return true;
                }
            };
            var actionToken = memberInfo.TryObserve(null, testEventHandler, DefaultMetadata);
            memberInfo.SetValue(null, newV, DefaultMetadata);
            testEventHandler.InvokeCount.ShouldEqual(1);
            propertyChangedInvokeCount.ShouldEqual(1);

            memberInfo.SetValue(null, newV, DefaultMetadata);
            testEventHandler.InvokeCount.ShouldEqual(1);
            propertyChangedInvokeCount.ShouldEqual(1);

            actionToken.Dispose();
            oldV = this;
            newV = null;
            memberInfo.SetValue(null, newV, DefaultMetadata);
            testEventHandler.InvokeCount.ShouldEqual(1);
            propertyChangedInvokeCount.ShouldEqual(2);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void InstanceAutoGeneratedShouldRaiseOnChange(bool withAttachedHandler, bool inherits)
        {
            var message = "m";
            var target = new object();
            var attachedInvokeCount = 0;
            IAccessorMemberInfo? memberInfo = null;
            object? oldV = null;
            object? newV = this;
            var propertyChangedInvokeCount = 0;
            var builder = new PropertyBuilder<object, object>("t", typeof(object), typeof(EventHandler)).PropertyChangedHandler((member, t, value, newValue, metadata) =>
            {
                ++propertyChangedInvokeCount;
                memberInfo.ShouldEqual(member);
                t.ShouldEqual(target);
                value.ShouldEqual(oldV);
                newValue.ShouldEqual(newV);
                metadata.ShouldEqual(DefaultMetadata);
            });
            if (withAttachedHandler)
            {
                builder = builder.AttachedHandler((member, t, metadata) =>
                {
                    ++attachedInvokeCount;
                    member.ShouldEqual(memberInfo);
                    t.ShouldEqual(target);
                    metadata.ShouldEqual(DefaultMetadata);
                });
            }

            if (inherits)
                builder = builder.Inherits();

            memberInfo = builder.Build();
            var testEventHandler = new TestWeakEventListener
            {
                TryHandle = (t, msg, metadata) =>
                {
                    t.ShouldEqual(target);
                    message.ShouldNotBeNull();
                    metadata.ShouldEqual(DefaultMetadata);
                    return true;
                }
            };
            var actionToken = memberInfo.TryObserve(target, testEventHandler, DefaultMetadata);
            memberInfo.SetValue(target, newV, DefaultMetadata);
            testEventHandler.InvokeCount.ShouldEqual(1);
            propertyChangedInvokeCount.ShouldEqual(1);
            if (withAttachedHandler)
                attachedInvokeCount.ShouldEqual(1);

            memberInfo.SetValue(target, newV, DefaultMetadata);
            testEventHandler.InvokeCount.ShouldEqual(1);
            propertyChangedInvokeCount.ShouldEqual(1);

            actionToken.Dispose();
            oldV = this;
            newV = null;
            memberInfo.SetValue(target, newV, DefaultMetadata);
            testEventHandler.InvokeCount.ShouldEqual(1);
            propertyChangedInvokeCount.ShouldEqual(2);
            if (withAttachedHandler)
                attachedInvokeCount.ShouldEqual(1);
        }

        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void InstanceCustomShouldUseDelegates(bool withAttachedHandler, bool isStatic)
        {
            var message = "m";
            var target = new object();
            var testEventHandler = new TestWeakEventListener();
            var attachedInvokeCount = 0;
            var invokeCount = 0;
            var raiseInvokeCount = 0;
            var result = new ActionToken((o, o1) => { });
            IAccessorMemberInfo? memberInfo = null;
            var builder = new PropertyBuilder<object, object>("t", typeof(object), typeof(EventHandler)).CustomGetter((member, o, metadata) => "").ObservableHandler((member, o, listener, metadata) =>
            {
                ++invokeCount;
                member.ShouldEqual(memberInfo);
                o.ShouldEqual(target);
                listener.ShouldEqual(testEventHandler);
                metadata.ShouldEqual(DefaultMetadata);
                return result;
            }, (member, o, msg, metadata) =>
            {
                ++raiseInvokeCount;
                member.ShouldEqual(memberInfo);
                o.ShouldEqual(target);
                msg.ShouldEqual(message);
                metadata.ShouldEqual(DefaultMetadata);
            });
            if (withAttachedHandler)
            {
                builder = builder.AttachedHandler((member, t, metadata) =>
                {
                    ++attachedInvokeCount;
                    member.ShouldEqual(memberInfo);
                    t.ShouldEqual(target);
                    metadata.ShouldEqual(DefaultMetadata);
                });
            }

            if (isStatic)
                builder = builder.Static();
            memberInfo = builder.Build();
            memberInfo.TryObserve(target, testEventHandler, DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(1);
            raiseInvokeCount.ShouldEqual(0);
            ((INotifiableMemberInfo)memberInfo).Raise(target, message, DefaultMetadata);
            invokeCount.ShouldEqual(1);
            raiseInvokeCount.ShouldEqual(1);
            if (withAttachedHandler)
                attachedInvokeCount.ShouldEqual(1);

            if (withAttachedHandler)
            {
                memberInfo.TryObserve(target, testEventHandler, DefaultMetadata);
                attachedInvokeCount.ShouldEqual(1);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void InstanceCustomObservableShouldRaise(bool withAttachedHandler)
        {
            var message = "m";
            var target = new object();
            var attachedInvokeCount = 0;
            IAccessorMemberInfo? memberInfo = null;
            var builder = new PropertyBuilder<object, object>("t", typeof(object), typeof(EventHandler)).CustomGetter((member, o, metadata) => "").Observable();
            if (withAttachedHandler)
            {
                builder = builder.AttachedHandler((member, t, metadata) =>
                {
                    ++attachedInvokeCount;
                    member.ShouldEqual(memberInfo);
                    t.ShouldEqual(target);
                    metadata.ShouldEqual(DefaultMetadata);
                });
            }

            memberInfo = builder.Build();
            var testEventHandler = new TestWeakEventListener
            {
                TryHandle = (t, msg, metadata) =>
                {
                    t.ShouldEqual(target);
                    message.ShouldEqual(msg);
                    metadata.ShouldEqual(DefaultMetadata);
                    return true;
                }
            };
            var actionToken = memberInfo.TryObserve(target, testEventHandler, DefaultMetadata);
            ((INotifiableMemberInfo)memberInfo).Raise(target, message, DefaultMetadata);
            testEventHandler.InvokeCount.ShouldEqual(1);
            if (withAttachedHandler)
                attachedInvokeCount.ShouldEqual(1);

            actionToken.Dispose();
            ((INotifiableMemberInfo)memberInfo).Raise(target, message, DefaultMetadata);
            testEventHandler.InvokeCount.ShouldEqual(1);
            if (withAttachedHandler)
            {
                memberInfo.TryObserve(target, testEventHandler, DefaultMetadata);
                attachedInvokeCount.ShouldEqual(1);
            }
        }

        [Fact]
        public void StaticCustomObservableShouldRaise()
        {
            var message = "m";
            var memberInfo = new PropertyBuilder<object, object>("t", typeof(object), typeof(EventHandler)).CustomGetter((member, o, metadata) => "").Static().Observable().Build();
            var testEventHandler = new TestWeakEventListener
            {
                TryHandle = (t, msg, meta) =>
                {
                    t.ShouldBeNull();
                    message.ShouldEqual(msg);
                    meta.ShouldEqual(DefaultMetadata);
                    return true;
                }
            };
            var actionToken = memberInfo.TryObserve(null, testEventHandler, DefaultMetadata);
            ((INotifiableMemberInfo)memberInfo).Raise(null, message, DefaultMetadata);
            testEventHandler.InvokeCount.ShouldEqual(1);

            actionToken.Dispose();
            ((INotifiableMemberInfo)memberInfo).Raise(null, message, DefaultMetadata);
            testEventHandler.InvokeCount.ShouldEqual(1);
        }

        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void DefaultValueShouldSetDefaultValue1(bool isStatic, bool inherits)
        {
            object defaultValue = this;
            var builder = new PropertyBuilder<object, object>(nameof(DefaultValueShouldSetDefaultValue1), typeof(object), typeof(EventHandler));
            builder.DefaultValue(defaultValue);
            if (isStatic)
                builder.Static();
            if (inherits)
                builder.Inherits();

            var memberInfo = builder.Build();
            memberInfo.GetValue(isStatic ? null : this, DefaultMetadata).ShouldEqual(defaultValue);
        }

        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void DefaultValueShouldSetDefaultValue2(bool isStatic, bool inherits)
        {
            var invokeCount = 0;
            var target = isStatic ? null : new object();
            object defaultValue = this;
            IAccessorMemberInfo? memberInfo = null;
            var builder = new PropertyBuilder<object, object>(nameof(DefaultValueShouldSetDefaultValue2), typeof(object), typeof(EventHandler));
            builder.DefaultValue((m, o) =>
            {
                ++invokeCount;
                o.ShouldEqual(target);
                return defaultValue;
            });
            if (isStatic)
                builder.Static();
            if (inherits)
                builder.Inherits();

            memberInfo = builder.Build();
            memberInfo.GetValue(target, DefaultMetadata).ShouldEqual(defaultValue);
            invokeCount.ShouldEqual(1);
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(1, false)]
        [InlineData(10, true)]
        [InlineData(10, false)]
        public void GetSetAutoGeneratedInstanceShouldUseCorrectValues(int count, bool inherits)
        {
            var targets = new List<object>();
            var builder = new PropertyBuilder<object, object>("t", typeof(object), typeof(object));
            if (inherits)
                builder = builder.Inherits();
            var member = builder.Build();
            for (var i = 0; i < count; i++)
            {
                var t = new object();
                targets.Add(t);
                member.GetValue(t).ShouldEqual(null);
                member.SetValue(t, i);
            }

            for (var i = 0; i < count; i++)
                member.GetValue(targets[i]).ShouldEqual(i);
        }

        [Fact]
        public void GetSetAutoGeneratedStaticShouldUseCorrectValues()
        {
            var builder = new PropertyBuilder<object, object>("t", typeof(object), typeof(object)).Static();
            var member = builder.Build();
            member.GetValue(null).ShouldBeNull();
            member.SetValue(null, this);
            member.GetValue(null).ShouldEqual(this);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetSetAutoGeneratedShouldCallAttachedHandler(bool inherits)
        {
            var target = new object();
            var attachedInvokeCount = 0;
            IAccessorMemberInfo? memberInfo = null;
            var builder = new PropertyBuilder<object, object>("t", typeof(object), typeof(object)).AttachedHandler((member, t, metadata) =>
            {
                ++attachedInvokeCount;
                member.ShouldEqual(memberInfo);
                t.ShouldEqual(target);
                metadata.ShouldEqual(DefaultMetadata);
            });
            if (inherits)
                builder.Inherits();
            memberInfo = builder.Build();
            memberInfo.GetValue(target, DefaultMetadata);
            attachedInvokeCount.ShouldEqual(1);
            memberInfo.GetValue(target, DefaultMetadata);
            memberInfo.SetValue(target, this, DefaultMetadata);
            memberInfo.SetValue(target, this, DefaultMetadata);
            attachedInvokeCount.ShouldEqual(1);

            attachedInvokeCount = 0;
            target = new object();
            memberInfo.SetValue(target, this, DefaultMetadata);
            attachedInvokeCount.ShouldEqual(1);
            memberInfo.SetValue(target, this, DefaultMetadata);
            memberInfo.GetValue(target, DefaultMetadata);
            memberInfo.GetValue(target, DefaultMetadata);
            attachedInvokeCount.ShouldEqual(1);
        }

        [Fact]
        public void GetSetCustomShouldCallAttachedHandler()
        {
            var target = new object();
            var attachedInvokeCount = 0;
            IAccessorMemberInfo? memberInfo = null;
            var builder = new PropertyBuilder<object, object>("t", typeof(object), typeof(object))
                .CustomGetter((member, o, metadata) => "")
                .CustomSetter((member, o, value, metadata) => { })
                .AttachedHandler((member, t, metadata) =>
                {
                    ++attachedInvokeCount;
                    member.ShouldEqual(memberInfo);
                    t.ShouldEqual(target);
                    metadata.ShouldEqual(DefaultMetadata);
                });

            memberInfo = builder.Build();
            memberInfo.GetValue(target, DefaultMetadata);
            attachedInvokeCount.ShouldEqual(1);
            memberInfo.GetValue(target, DefaultMetadata);
            memberInfo.SetValue(target, this, DefaultMetadata);
            memberInfo.SetValue(target, this, DefaultMetadata);
            attachedInvokeCount.ShouldEqual(1);

            attachedInvokeCount = 0;
            target = new object();
            memberInfo.SetValue(target, this, DefaultMetadata);
            attachedInvokeCount.ShouldEqual(1);
            memberInfo.SetValue(target, this, DefaultMetadata);
            memberInfo.GetValue(target, DefaultMetadata);
            memberInfo.GetValue(target, DefaultMetadata);
            attachedInvokeCount.ShouldEqual(1);
        }

        [Fact]
        public void GetSetCustomShouldUseDelegates()
        {
            var target = new object();
            var resultValue = new object();
            var setValue = new object();
            var getCount = 0;
            var setCount = 0;
            IAccessorMemberInfo? memberInfo = null;
            var builder = new PropertyBuilder<object, object>("t", typeof(object), typeof(object))
                .CustomGetter((member, t, metadata) =>
                {
                    ++getCount;
                    member.ShouldEqual(memberInfo);
                    t.ShouldEqual(target);
                    metadata.ShouldEqual(DefaultMetadata);
                    return resultValue;
                })
                .CustomSetter((member, t, value, metadata) =>
                {
                    ++setCount;
                    member.ShouldEqual(memberInfo);
                    t.ShouldEqual(target);
                    value.ShouldEqual(setValue);
                    metadata.ShouldEqual(DefaultMetadata);
                });

            memberInfo = builder.Build();
            memberInfo.GetValue(target, DefaultMetadata);
            getCount.ShouldEqual(1);
            setCount.ShouldEqual(0);
            memberInfo.SetValue(target, setValue, DefaultMetadata);
            getCount.ShouldEqual(1);
            setCount.ShouldEqual(1);
        }

        [Fact]
        public void InheritsShouldUseParentValue()
        {
            var parent = new object();
            var target = new object();
            bool canReturnParent = false;
            IEventListener? parentListener = null;
            var parentMember = new TestAccessorMemberInfo
            {
                GetValue = (o, context) =>
                {
                    if (!canReturnParent)
                        return null;
                    if (o == target)
                        return parent;
                    return null;
                },
                TryObserve = (o, listener, arg3) =>
                {
                    if (o == target)
                    {
                        if (parentListener != null)
                            throw new NotSupportedException();
                        parentListener = listener;
                        return new ActionToken((o1, o2) => parentListener = null);
                    }

                    return default;
                }
            };
            using var m = TestComponentSubscriber.Subscribe(new TestMemberManagerComponent
            {
                TryGetMembers = (type, memberType, arg3, arg4, arg5, arg6) =>
                {
                    if (BindableMembers.Object.Parent.Name.Equals(arg4))
                        return parentMember;
                    return default;
                }
            });

            int changedCount = 0;
            var parentValue = new object();
            object defaultValue = new object();
            var memberInfo = new PropertyBuilder<object, object>("t", typeof(object), typeof(object)).Inherits().DefaultValue(defaultValue).Build();
            memberInfo.GetValue(target).ShouldEqual(defaultValue);
            memberInfo.TryObserve(target, new TestWeakEventListener
            {
                TryHandle = (o, o1, arg3) =>
                {
                    ++changedCount;
                    return true;
                }
            });

            memberInfo.SetValue(parent, parentValue);
            canReturnParent = true;
            parentListener?.TryHandle(parent, parent, DefaultMetadata);
            changedCount.ShouldEqual(1);
            memberInfo.GetValue(target).ShouldEqual(parentValue);

            memberInfo.SetValue(parent, null);
            changedCount.ShouldEqual(2);
            memberInfo.GetValue(target).ShouldEqual(null);

            canReturnParent = false;
            parentListener?.TryHandle(parent, parent, DefaultMetadata);
            changedCount.ShouldEqual(3);
            memberInfo.GetValue(target).ShouldEqual(defaultValue);

            canReturnParent = true;
            parentListener?.TryHandle(parent, parent, DefaultMetadata);
            changedCount.ShouldEqual(4);
            memberInfo.GetValue(target).ShouldEqual(null);

            memberInfo.SetValue(target, this);
            changedCount.ShouldEqual(5);
            parentListener.ShouldBeNull();
            memberInfo.GetValue(target).ShouldEqual(this);
        }

        #endregion
    }
}