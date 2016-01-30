﻿using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Binding.Behaviors;

namespace MugenMvvmToolkit.Test.Bindings.Behaviors
{
    [TestClass]
    public class DefaultValueOnExceptionBehaviorTest : BindingBehaviorTestBase<DefaultValueOnExceptionBehavior>
    {
        #region Overrides of BindingBehaviorTestBase<DefaultValueOnExceptionBehavior>

        [Ignore]
        public override void BehaviorCanBeAttachedRepeatedly()
        {
        }

        protected override DefaultValueOnExceptionBehavior CreateBehavior()
        {
            return new DefaultValueOnExceptionBehavior(null);
        }

        #endregion
    }
}
