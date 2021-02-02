﻿using System;
using System.Collections.Generic;
using MugenMvvm.Bindings.Core;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.UnitTests.Bindings.Core.Internal;
using MugenMvvm.UnitTests.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Core
{
    public class BindingManagerTest : ComponentOwnerTestBase<BindingManager>
    {
        [Fact]
        public void ParseBindingExpressionShouldThrowNoComponents()
        {
            ShouldThrow<InvalidOperationException>(() => GetComponentOwner(ComponentCollectionManager).ParseBindingExpression(this, DefaultMetadata));
        }

        [Fact]
        public void TryParseBindingExpressionShouldHandleBuildersList()
        {
            var bindingExpressions = new List<IBindingBuilder> {new TestBindingBuilder(), new TestBindingBuilder()};
            var bindingManager = GetComponentOwner(ComponentCollectionManager);
            bindingManager.TryParseBindingExpression(bindingExpressions, DefaultMetadata).List.ShouldEqual(bindingExpressions);
            bindingManager.TryParseBindingExpression(this, DefaultMetadata).IsEmpty.ShouldBeTrue();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ParseBindingExpressionShouldBeHandledByComponents(int count)
        {
            var request = "t";
            var bindingManager = GetComponentOwner(ComponentCollectionManager);
            var expression = new TestBindingBuilder();
            var invokeCount = 0;
            for (var i = 0; i < count; i++)
            {
                var isLast = i == count - 1;
                var component = new TestBindingExpressionParserComponent(bindingManager)
                {
                    Priority = -i,
                    TryParseBindingExpression = (r, m) =>
                    {
                        ++invokeCount;
                        r.ShouldEqual(request);
                        m.ShouldEqual(DefaultMetadata);
                        if (isLast)
                            return expression;
                        return default;
                    }
                };
                bindingManager.AddComponent(component);
            }

            var result = bindingManager.ParseBindingExpression(request, DefaultMetadata);
            result.Count.ShouldEqual(1);
            result.Item.ShouldEqual(expression);
            invokeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetBindingsShouldBeHandledByComponents(int count)
        {
            var target = this;
            var path = "t";
            var bindingManager = GetComponentOwner(ComponentCollectionManager);
            var list1 = new List<IBinding>();
            var list2 = new List<IBinding>();
            for (var i = 0; i < count; i++)
            {
                var binding = new TestBinding();
                list1.Add(binding);
                var component = new TestBindingHolderComponent(bindingManager)
                {
                    Priority = -i,
                    TryGetBindings = (t, p, m) =>
                    {
                        list2.Add(binding);
                        t.ShouldEqual(target);
                        p.ShouldEqual(path);
                        m.ShouldEqual(DefaultMetadata);
                        return binding;
                    }
                };
                bindingManager.AddComponent(component);
            }

            var result = bindingManager.GetBindings(target, path, DefaultMetadata);
            list1.ShouldEqual(result.AsList());
            list1.ShouldEqual(list2);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void OnLifecycleChangedShouldBeHandledByComponents(int count)
        {
            var bindingManager = GetComponentOwner(ComponentCollectionManager);
            var invokeCount = 0;
            var state = "state";
            var binding = new TestBinding();
            var lifecycleState = BindingLifecycleState.Disposed;
            for (var i = 0; i < count; i++)
            {
                var component = new TestBindingLifecycleListener(bindingManager)
                {
                    OnLifecycleChanged = (vm, viewModelLifecycleState, st, metadata) =>
                    {
                        ++invokeCount;
                        vm.ShouldEqual(binding);
                        st.ShouldEqual(state);
                        viewModelLifecycleState.ShouldEqual(lifecycleState);
                        metadata.ShouldEqual(DefaultMetadata);
                    },
                    Priority = i
                };
                bindingManager.AddComponent(component);
            }

            bindingManager.OnLifecycleChanged(binding, lifecycleState, state, DefaultMetadata);
            invokeCount.ShouldEqual(count);
        }

        protected override BindingManager GetComponentOwner(IComponentCollectionManager? componentCollectionManager = null) => new(componentCollectionManager);
    }
}