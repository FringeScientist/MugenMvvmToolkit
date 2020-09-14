﻿using System;
using System.Reflection;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Internal;
using MugenMvvm.UnitTests.Components;
using MugenMvvm.UnitTests.Internal.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Internal
{
    public class ReflectionManagerTest : ComponentOwnerTestBase<ReflectionManager>
    {
        #region Fields

        public static readonly MethodInfo TestMethod = typeof(ReflectionManagerTest).GetMethod(nameof(CanCreateDelegateShouldReturnFalseNoComponents))!;
        public static readonly ConstructorInfo TestConstructor = typeof(ReflectionManagerTest).GetConstructor(new Type[0])!;

        #endregion

        #region Methods

        [Fact]
        public void CanCreateDelegateShouldReturnFalseNoComponents() => new ReflectionManager().CanCreateDelegate(typeof(Action), TestMethod).ShouldBeFalse();

        [Fact]
        public void TryCreateDelegateShouldReturnNullNoComponents() => new ReflectionManager().TryCreateDelegate(typeof(Action), this, TestMethod).ShouldBeNull();

        [Fact]
        public void GetActivatorShouldThrowNoComponents1() => ShouldThrow<InvalidOperationException>(() => new ReflectionManager().GetActivator(TestConstructor));

        [Fact]
        public void GetActivatorShouldThrowNoComponents2() => ShouldThrow<InvalidOperationException>(() => new ReflectionManager().GetActivator(TestConstructor, typeof(Action)));

        [Fact]
        public void GetMethodInvokerShouldThrowNoComponents1() => ShouldThrow<InvalidOperationException>(() => new ReflectionManager().GetMethodInvoker(TestMethod));

        [Fact]
        public void GetMethodInvokerShouldThrowNoComponents2() => ShouldThrow<InvalidOperationException>(() => new ReflectionManager().GetMethodInvoker(TestMethod, typeof(Action)));

        [Fact]
        public void GetMemberGetterShouldThrowNoComponents() => ShouldThrow<InvalidOperationException>(() => new ReflectionManager().GetMemberGetter(TestMethod, typeof(Action)));

        [Fact]
        public void GetMemberSetterShouldThrowNoComponents() => ShouldThrow<InvalidOperationException>(() => new ReflectionManager().GetMemberSetter(TestMethod, typeof(Action)));

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void CanCreateDelegateShouldBeHandledByComponents(int count)
        {
            var invokeCount = 0;
            var delType = typeof(Action);
            var delegateProvider = new ReflectionManager();
            for (var i = 0; i < count; i++)
            {
                var canCreate = count - 1 == i;
                var component = new TestReflectionDelegateProviderComponent(delegateProvider);
                component.Priority = -i;
                component.CanCreateDelegate = (type, info) =>
                {
                    ++invokeCount;
                    type.ShouldEqual(delType);
                    info.ShouldEqual(TestMethod);
                    return canCreate;
                };
                delegateProvider.AddComponent(component);
            }

            delegateProvider.CanCreateDelegate(delType, TestMethod).ShouldEqual(true);
            invokeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void TryCreateDelegateShouldBeHandledByComponents(int count)
        {
            var invokeCount = 0;
            var delType = typeof(Action);
            var delegateProvider = new ReflectionManager();
            Action result = () => { };
            for (var i = 0; i < count; i++)
            {
                var canCreate = count - 1 == i;
                var component = new TestReflectionDelegateProviderComponent(delegateProvider);
                component.Priority = -i;
                component.TryCreateDelegate = (type, target, info) =>
                {
                    ++invokeCount;
                    target.ShouldEqual(this);
                    type.ShouldEqual(delType);
                    info.ShouldEqual(TestMethod);
                    if (canCreate)
                        return result;
                    return null;
                };
                delegateProvider.AddComponent(component);
            }

            delegateProvider.TryCreateDelegate(delType, this, TestMethod).ShouldEqual(result);
            invokeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetActivatorShouldBeHandledByComponents1(int count)
        {
            var invokeCount = 0;
            var delType = typeof(Action);
            var delegateProvider = new ReflectionManager();
            Func<object?[], object>? result = objects => objects;
            for (var i = 0; i < count; i++)
            {
                var canCreate = count - 1 == i;
                var component = new TestActivatorReflectionDelegateProviderComponent(delegateProvider);
                component.Priority = -i;
                component.TryGetActivator = info =>
                {
                    ++invokeCount;
                    info.ShouldEqual(TestConstructor);
                    if (canCreate)
                        return result;
                    return null;
                };
                delegateProvider.AddComponent(component);
            }

            delegateProvider.GetActivator(TestConstructor).ShouldEqual(result);
            invokeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetActivatorShouldBeHandledByComponents2(int count)
        {
            var invokeCount = 0;
            var delType = typeof(Action);
            var delegateProvider = new ReflectionManager();
            Func<object?[], object>? result = objects => objects;
            for (var i = 0; i < count; i++)
            {
                var canCreate = count - 1 == i;
                var component = new TestActivatorReflectionDelegateProviderComponent(delegateProvider);
                component.Priority = -i;
                component.TryGetActivator1 = (info, type) =>
                {
                    ++invokeCount;
                    info.ShouldEqual(TestConstructor);
                    type.ShouldEqual(delType.GetType());
                    if (canCreate)
                        return result;
                    return null;
                };
                delegateProvider.AddComponent(component);
            }

            delegateProvider.GetActivator(TestConstructor, delType.GetType()).ShouldEqual(result);
            invokeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetMethodInvokerShouldBeHandledByComponents1(int count)
        {
            var invokeCount = 0;
            var delType = typeof(Action);
            var delegateProvider = new ReflectionManager();
            Func<object?, object?[], object?> result = (o, objects) => o;
            for (var i = 0; i < count; i++)
            {
                var canCreate = count - 1 == i;
                var component = new TestMethodReflectionDelegateProviderComponent(delegateProvider);
                component.Priority = -i;
                component.TryGetMethodInvoker = info =>
                {
                    ++invokeCount;
                    info.ShouldEqual(TestMethod);
                    if (canCreate)
                        return result;
                    return null;
                };
                delegateProvider.AddComponent(component);
            }

            delegateProvider.GetMethodInvoker(TestMethod).ShouldEqual(result);
            invokeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetMethodInvokerShouldBeHandledByComponents2(int count)
        {
            var invokeCount = 0;
            var delType = typeof(Action);
            var delegateProvider = new ReflectionManager();
            Func<object?, object?[], object?> result = (o, objects) => o;
            for (var i = 0; i < count; i++)
            {
                var canCreate = count - 1 == i;
                var component = new TestMethodReflectionDelegateProviderComponent(delegateProvider);
                component.Priority = -i;
                component.TryGetMethodInvoker1 = (info, t) =>
                {
                    ++invokeCount;
                    info.ShouldEqual(TestMethod);
                    t.ShouldEqual(result.GetType());
                    if (canCreate)
                        return result;
                    return null;
                };
                delegateProvider.AddComponent(component);
            }

            delegateProvider.GetMethodInvoker(TestMethod, result.GetType()).ShouldEqual(result);
            invokeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetMemberGetterShouldBeHandledByComponents(int count)
        {
            var invokeCount = 0;
            var delType = typeof(Action);
            var delegateProvider = new ReflectionManager();
            Func<object?, object?[], object?> result = (o, objects) => o;
            for (var i = 0; i < count; i++)
            {
                var canCreate = count - 1 == i;
                var component = new TestMemberReflectionDelegateProviderComponent(delegateProvider);
                component.Priority = -i;
                component.TryGetMemberGetter = (info, t) =>
                {
                    ++invokeCount;
                    info.ShouldEqual(TestMethod);
                    t.ShouldEqual(result.GetType());
                    if (canCreate)
                        return result;
                    return null;
                };
                delegateProvider.AddComponent(component);
            }

            delegateProvider.GetMemberGetter(TestMethod, result.GetType()).ShouldEqual(result);
            invokeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetMemberSetterShouldBeHandledByComponents(int count)
        {
            var invokeCount = 0;
            var delType = typeof(Action);
            var delegateProvider = new ReflectionManager();
            Func<object?, object?[], object?> result = (o, objects) => o;
            for (var i = 0; i < count; i++)
            {
                var canCreate = count - 1 == i;
                var component = new TestMemberReflectionDelegateProviderComponent(delegateProvider);
                component.Priority = -i;
                component.TryGetMemberSetter = (info, t) =>
                {
                    ++invokeCount;
                    info.ShouldEqual(TestMethod);
                    t.ShouldEqual(result.GetType());
                    if (canCreate)
                        return result;
                    return null;
                };
                delegateProvider.AddComponent(component);
            }

            delegateProvider.GetMemberSetter(TestMethod, result.GetType()).ShouldEqual(result);
            invokeCount.ShouldEqual(count);
        }

        protected override ReflectionManager GetComponentOwner(IComponentCollectionManager? collectionProvider = null) => new ReflectionManager(collectionProvider);

        #endregion
    }
}