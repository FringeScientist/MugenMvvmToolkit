﻿using System;
using System.Linq;
using MugenMvvm.Bindings.Core;
using MugenMvvm.Bindings.Core.Components;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.UnitTests.Bindings.Compiling.Internal;
using MugenMvvm.UnitTests.Bindings.Core.Internal;
using MugenMvvm.UnitTests.Bindings.Observation.Internal;
using MugenMvvm.UnitTests.Components.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Core.Components
{
    [Collection(SharedContext)]
    public class BindingCleanerTest : UnitTestBase, IDisposable
    {
        public BindingCleanerTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            MugenService.Configuration.InitializeInstance<IBindingManager>(new BindingManager(ComponentCollectionManager));
            MugenService.AddComponent(new BindingCleaner());
        }

        public void Dispose() => MugenService.Configuration.Clear<IBindingManager>();

        [Fact]
        public void ShouldClearMultiBinding()
        {
            var targetDisposed = false;
            var sourceDisposed = false;
            IMemberPathObserverListener? targetListener = null;
            IMemberPathObserverListener? sourceListener = null;
            var expression = new TestCompiledExpression();
            var target = new TestMemberPathObserver
            {
                Dispose = () => targetDisposed = true,
                AddListener = l =>
                {
                    targetListener.ShouldBeNull();
                    targetListener = l;
                },
                RemoveListener = l =>
                {
                    targetListener.ShouldEqual(l);
                    targetListener = null;
                }
            };
            var source = new TestMemberPathObserver
            {
                Dispose = () => sourceDisposed = true,
                AddListener = l =>
                {
                    sourceListener.ShouldBeNull();
                    sourceListener = l;
                },
                RemoveListener = l =>
                {
                    sourceListener.ShouldEqual(l);
                    sourceListener = null;
                }
            };

            var components = new IComponent<IBinding>[] {new TestBindingTargetObserverListener(), new TestBindingSourceObserverListener()};
            var binding = new MultiBinding(target, new ItemOrArray<object?>(source, true), expression);
            binding.State.ShouldEqual(BindingState.Valid);
            binding.Initialize(components, DefaultMetadata);
            targetListener.ShouldEqual(binding);
            sourceListener.ShouldEqual(binding);

            var disposeCount = 0;
            var testLifecycleListener = new TestBindingLifecycleListener
            {
                OnLifecycleChanged = (b, state, _, m) =>
                {
                    ++disposeCount;
                    b.ShouldEqual(binding);
                    state.ShouldEqual(BindingLifecycleState.Disposed);
                    m.ShouldBeNull();
                }
            };
            MugenService.AddComponent(testLifecycleListener);

            binding.Dispose();
            binding.State.ShouldEqual(BindingState.Disposed);
            targetDisposed.ShouldBeTrue();
            sourceDisposed.ShouldBeTrue();
            binding.GetComponents<object>().AsList().ShouldBeEmpty();
            targetListener.ShouldBeNull();
            sourceListener.ShouldBeNull();
            disposeCount.ShouldEqual(1);

            binding.TryAddComponent(components[0]).IsEmpty.ShouldBeTrue();
            binding.GetComponents<object>().AsList().ShouldBeEmpty();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ShouldClearBinding(int count)
        {
            var targetDisposed = false;
            var sourceDisposed = false;
            IMemberPathObserverListener? targetListener = null;
            IMemberPathObserverListener? sourceListener = null;
            var target = new TestMemberPathObserver
            {
                Dispose = () => targetDisposed = true,
                AddListener = l =>
                {
                    targetListener.ShouldBeNull();
                    targetListener = l;
                },
                RemoveListener = l =>
                {
                    targetListener.ShouldEqual(l);
                    targetListener = null;
                }
            };
            var source = new TestMemberPathObserver
            {
                Dispose = () => sourceDisposed = true,
                AddListener = l =>
                {
                    sourceListener.ShouldBeNull();
                    sourceListener = l;
                },
                RemoveListener = l =>
                {
                    sourceListener.ShouldEqual(l);
                    sourceListener = null;
                }
            };

            var disposeComponentCount = 0;
            var components = Enumerable
                             .Range(0, count)
                             .Select(i => new TestComponent<IBinding> {Dispose = () => ++disposeComponentCount})
                             .OfType<IComponent<IBinding>>()
                             .Concat(new IComponent<IBinding>[] {new TestBindingTargetObserverListener(), new TestBindingSourceObserverListener()})
                             .ToArray();

            var binding = new Binding(target, source);
            binding.State.ShouldEqual(BindingState.Valid);
            binding.Initialize(components, DefaultMetadata);
            targetListener.ShouldEqual(binding);
            sourceListener.ShouldEqual(binding);

            var disposeCount = 0;
            var testLifecycleListener = new TestBindingLifecycleListener
            {
                OnLifecycleChanged = (b, state, _, m) =>
                {
                    ++disposeCount;
                    b.ShouldEqual(binding);
                    state.ShouldEqual(BindingLifecycleState.Disposed);
                    m.ShouldBeNull();
                }
            };
            MugenService.AddComponent(testLifecycleListener);

            binding.Dispose();
            disposeComponentCount.ShouldEqual(count);
            binding.State.ShouldEqual(BindingState.Disposed);
            targetDisposed.ShouldBeTrue();
            sourceDisposed.ShouldBeTrue();
            binding.GetComponents<object>().AsList().ShouldBeEmpty();
            targetListener.ShouldBeNull();
            sourceListener.ShouldBeNull();
            disposeCount.ShouldEqual(1);

            binding.TryAddComponent(components[0]).IsEmpty.ShouldBeTrue();
            binding.GetComponents<object>().AsList().ShouldBeEmpty();
        }
    }
}