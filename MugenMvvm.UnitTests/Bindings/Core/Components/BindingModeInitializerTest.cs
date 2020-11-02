﻿using System;
using System.Collections.Generic;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Core;
using MugenMvvm.Bindings.Core.Components;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Core.Components
{
    public class BindingModeInitializerTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void InitializeShouldUseDefaultMode()
        {
            var bindingModeInitializer = new BindingModeInitializer();
            var context = new BindingExpressionInitializerContext(this);
            context.Initialize(this, this, MemberExpressionNode.Empty, MemberExpressionNode.Action, default, DefaultMetadata);

            bindingModeInitializer.DefaultMode = OneWayBindingMode.Instance;
            bindingModeInitializer.Initialize(null!, context);
            context.BindingComponents.Count.ShouldEqual(1);
            context.BindingComponents[BindingParameterNameConstant.Mode].ShouldEqual(bindingModeInitializer.DefaultMode);

            context.BindingComponents.Clear();
            bindingModeInitializer.DefaultMode = null;
            bindingModeInitializer.Initialize(null!, context);
            context.BindingComponents.Count.ShouldEqual(0);
        }

        [Fact]
        public void InitializeShouldIgnoreInitializedMode()
        {
            var bindingModeInitializer = new BindingModeInitializer();
            var context = new BindingExpressionInitializerContext(this);
            context.Initialize(this, this, MemberExpressionNode.Empty, MemberExpressionNode.Action, default, DefaultMetadata);

            context.BindingComponents[BindingParameterNameConstant.Mode] = this;
            bindingModeInitializer.Initialize(null!, context);
            context.BindingComponents.Count.ShouldEqual(1);
            context.BindingComponents[BindingParameterNameConstant.Mode].ShouldEqual(this);

            context.BindingComponents[BindingParameterNameConstant.Mode] = null;
            bindingModeInitializer.Initialize(null!, context);
            context.BindingComponents.Count.ShouldEqual(0);
        }

        [Fact]
        public void InitializeShouldCheckModeParameters()
        {
            var bindingModeInitializer = new BindingModeInitializer();
            var context = new BindingExpressionInitializerContext(this);
            context.Initialize(this, this, MemberExpressionNode.Empty, MemberExpressionNode.Action, default, DefaultMetadata);

            bindingModeInitializer.BindingModes.ShouldNotBeNull();
            foreach (var bindingMode in bindingModeInitializer.BindingModes)
            {
                context.BindingComponents.Clear();
                context.Parameters = ItemOrList.FromItem<IExpressionNode, IList<IExpressionNode>>(new BinaryExpressionNode(BinaryTokenType.Assignment, new MemberExpressionNode(null, BindingParameterNameConstant.Mode), new MemberExpressionNode(null, bindingMode.Key)));
                bindingModeInitializer.Initialize(null!, context);
                if (bindingMode.Value == null)
                    context.BindingComponents.Count.ShouldEqual(0);
                else
                {
                    context.BindingComponents.Count.ShouldEqual(1);
                    context.BindingComponents[BindingParameterNameConstant.Mode].ShouldEqual(bindingMode.Value);
                }
            }
        }

        [Fact]
        public void InitializeShouldCheckModeInlineParameters()
        {
            var bindingModeInitializer = new BindingModeInitializer();
            var context = new BindingExpressionInitializerContext(this);
            context.Initialize(this, this, MemberExpressionNode.Empty, MemberExpressionNode.Action, default, DefaultMetadata);

            bindingModeInitializer.BindingModes.ShouldNotBeNull();
            foreach (var bindingMode in bindingModeInitializer.BindingModes)
            {
                context.BindingComponents.Clear();
                context.Parameters = ItemOrList.FromItem<IExpressionNode, IList<IExpressionNode>>(new MemberExpressionNode(null, bindingMode.Key));
                bindingModeInitializer.Initialize(null!, context);
                if (bindingMode.Value == null)
                    context.BindingComponents.Count.ShouldEqual(0);
                else
                {
                    context.BindingComponents.Count.ShouldEqual(1);
                    context.BindingComponents[BindingParameterNameConstant.Mode].ShouldEqual(bindingMode.Value);
                }
            }
        }

        [Fact]
        public void InitializeShouldThrowInvalidMode()
        {
            var bindingModeInitializer = new BindingModeInitializer();
            var context = new BindingExpressionInitializerContext(this);
            context.Initialize(this, this, MemberExpressionNode.Empty, MemberExpressionNode.Action, default, DefaultMetadata);

            context.Parameters = ItemOrList.FromItem<IExpressionNode, IList<IExpressionNode>>(new BinaryExpressionNode(BinaryTokenType.Assignment, new MemberExpressionNode(null, BindingParameterNameConstant.Mode),
                new MemberExpressionNode(null, "Test")));
            ShouldThrow<InvalidOperationException>(() => bindingModeInitializer.Initialize(null!, context));
        }

        #endregion
    }
}