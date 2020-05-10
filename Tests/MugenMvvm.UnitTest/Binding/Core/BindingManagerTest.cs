﻿using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Binding.Core;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Extensions;
using MugenMvvm.Metadata;
using MugenMvvm.UnitTest.Binding.Core.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Core
{
    public class BindingManagerTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void BuildBindingExpressionShouldThrowNoComponents()
        {
            var bindingManager = new BindingManager();
            ShouldThrow<InvalidOperationException>(() => bindingManager.BuildBindingExpression(this, DefaultMetadata));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void BuildBindingExpressionShouldBeHandledByComponents(int count)
        {
            var request = "t";
            var bindingManager = new BindingManager();
            var expression = new TestBindingExpression();
            var invokeCount = 0;
            for (var i = 0; i < count; i++)
            {
                var isLast = i == count - 1;
                var component = new TestBindingExpressionBuilderComponent
                {
                    Priority = -i,
                    TryBuildBindingExpression = (r, t, m) =>
                    {
                        ++invokeCount;
                        r.ShouldEqual(request);
                        t.ShouldEqual(request.GetType());
                        m.ShouldEqual(DefaultMetadata);
                        if (isLast)
                            return expression;
                        return default;
                    }
                };
                bindingManager.AddComponent(component);
            }

            var result = bindingManager.BuildBindingExpression(request, DefaultMetadata);
            result.Count().ShouldEqual(1);
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
            var bindingManager = new BindingManager();
            var list1 = new List<IBinding>();
            var list2 = new List<IBinding>();
            for (var i = 0; i < count; i++)
            {
                var binding = new TestBinding();
                list1.Add(binding);
                var component = new TestBindingHolderComponent
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
            list1.SequenceEqual(result.ToArray()).ShouldBeTrue();
            list1.SequenceEqual(list2).ShouldBeTrue();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void OnLifecycleChangedShouldBeHandledByComponents(int count)
        {
            var manager = new BindingManager();
            var context = new MetadataContext();
            var state = "state";
            var binding = new TestBinding();
            var lifecycleState = BindingLifecycleState.Disposed;
            for (var i = 0; i < count; i++)
            {
                var ctx = new MetadataContext();
                ctx.Set(MetadataContextKey.FromKey<int, int>("i" + i), i);
                context.Merge(ctx);
                var component = new TestBindingStateDispatcherComponent
                {
                    OnLifecycleChanged = (vm, viewModelLifecycleState, st, stateType, metadata) =>
                    {
                        vm.ShouldEqual(binding);
                        st.ShouldEqual(state);
                        stateType.ShouldEqual(state.GetType());
                        viewModelLifecycleState.ShouldEqual(lifecycleState);
                        metadata.ShouldEqual(DefaultMetadata);
                        return ctx;
                    },
                    Priority = i
                };
                manager.AddComponent(component);
            }

            var changed = manager.OnLifecycleChanged(binding, lifecycleState, state, DefaultMetadata);
            changed.SequenceEqual(context).ShouldBeTrue();
        }

        #endregion
    }
}