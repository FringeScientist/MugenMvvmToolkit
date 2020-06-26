﻿using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Binding.Core.Components;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Observers.MemberPaths;
using MugenMvvm.UnitTest.Binding.Core.Internal;
using MugenMvvm.UnitTest.Binding.Observers.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Core.Components
{
    public class BindingHolderTest : UnitTestBase
    {
        #region Methods

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void BindingHolderShouldKeepBindingsUsingTargetPath(int count)
        {
            const string defaultPath = "Test.Test";

            var bindingHolder = new BindingHolder();
            var bindings = new List<IBinding>();
            for (var i = 0; i < count; i++)
            {
                var testBinding = new TestBinding {Target = new TestMemberPathObserver {Path = new MultiMemberPath(defaultPath + i)}};
                bindings.Add(testBinding);

                bindingHolder.TryRegister(this, testBinding, DefaultMetadata).ShouldBeTrue();
                bindingHolder.TryGetBindings(this, defaultPath + i, DefaultMetadata).AsList().Single().ShouldEqual(testBinding);
                var array = bindingHolder.TryGetBindings(this, null, DefaultMetadata).AsList();
                array.Count.ShouldEqual(bindings.Count);
                array.ShouldContain(bindings);
            }

            for (var i = 0; i < count; i++)
            {
                bindingHolder.TryUnregister(this, bindings[i], DefaultMetadata).ShouldBeTrue();
                bindingHolder.TryGetBindings(this, defaultPath + i, DefaultMetadata).AsList().ShouldBeEmpty();
                var array = bindingHolder.TryGetBindings(this, null, DefaultMetadata).AsList();
                array.Count.ShouldEqual(bindings.Count - i - 1);
                array.ShouldContain(bindings.Skip(i + 1));
            }
        }

        [Fact]
        public void TryRegisterShouldDisposePrevBinding()
        {
            var b1Disposed = false;
            var b2Disposed = false;
            var b1 = new TestBinding
            {
                Target = new TestMemberPathObserver {Path = new SingleMemberPath("T")},
                Dispose = () => b1Disposed = true
            };
            var b2 = new TestBinding
            {
                Target = new TestMemberPathObserver {Path = new SingleMemberPath("T")},
                Dispose = () => b2Disposed = true
            };
            var bindingHolder = new BindingHolder();

            bindingHolder.TryRegister(this, b1, DefaultMetadata).ShouldBeTrue();
            b1Disposed.ShouldBeFalse();
            b2Disposed.ShouldBeFalse();

            bindingHolder.TryRegister(this, b2, DefaultMetadata).ShouldBeTrue();
            b1Disposed.ShouldBeTrue();
            b2Disposed.ShouldBeFalse();
        }

        #endregion
    }
}