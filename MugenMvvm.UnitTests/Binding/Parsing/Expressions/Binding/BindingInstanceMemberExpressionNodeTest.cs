﻿using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Observation;
using MugenMvvm.Binding.Observation.Observers;
using MugenMvvm.Binding.Observation.Paths;
using MugenMvvm.Binding.Parsing.Expressions.Binding;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTests.Binding.Observation.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Binding.Parsing.Expressions.Binding
{
    public class BindingInstanceMemberExpressionNodeTest : BindingMemberExpressionNodeBaseTest
    {
        #region Methods

        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            var exp = new BindingInstanceMemberExpressionNode(this, Path);
            exp.ExpressionType.ShouldEqual(ExpressionNodeType.BindingMember);
            exp.Instance.ShouldEqual(this);
            exp.Path.ShouldEqual(Path);
            exp.Index.ShouldEqual(-1);
        }

        [Fact]
        public void GetSourceShouldReturnInstance()
        {
            var path = new SingleMemberPath(Path);
            var observationManager = new ObservationManager();
            var component = new TestMemberPathProviderComponent
            {
                TryGetMemberPath = (o, arg3) =>
                {
                    o.ShouldEqual(Path);
                    arg3.ShouldEqual(DefaultMetadata);
                    return path;
                }
            };
            observationManager.AddComponent(component);

            var exp = new BindingInstanceMemberExpressionNode(this, Path, observationManager)
            {
                MemberFlags = MemberFlags.All
            };

            var target = exp.GetSource("", "", DefaultMetadata, out var p, out var flags);
            target.ShouldEqual(this);
            flags.ShouldEqual(MemberFlags.All);
            p.ShouldEqual(path);

            target = exp.GetSource("", "", DefaultMetadata, out p, out flags);
            target.ShouldEqual(this);
            flags.ShouldEqual(MemberFlags.All);
            p.ShouldEqual(path);
        }

        [Fact]
        public void GetBindingSourceShouldReturnInstanceObserver()
        {
            var path = new SingleMemberPath(Path);
            var observer = EmptyPathObserver.Empty;
            var observationManager = new ObservationManager();

            var exp = new BindingInstanceMemberExpressionNode(this, Path, observationManager)
            {
                MemberFlags = MemberFlags.All,
                Flags = BindingMemberExpressionFlags.Observable | BindingMemberExpressionFlags.Optional | BindingMemberExpressionFlags.StablePath | BindingMemberExpressionFlags.ObservableMethods,
                ObservableMethodName = "M"
            };

            observationManager.AddComponent(new TestMemberPathProviderComponent
            {
                TryGetMemberPath = (o, arg3) =>
                {
                    o.ShouldEqual(Path);
                    arg3.ShouldEqual(DefaultMetadata);
                    return path;
                }
            });
            observationManager.AddComponent(new TestMemberPathObserverProviderComponent
            {
                TryGetMemberPathObserver = (t, req, arg4) =>
                {
                    t.ShouldEqual(this);
                    var request = (MemberPathObserverRequest) req;
                    request.Path.ShouldEqual(path);
                    request.MemberFlags.ShouldEqual(exp.MemberFlags);
                    request.ObservableMethodName.ShouldEqual(exp.ObservableMethodName);
                    request.HasStablePath.ShouldBeTrue();
                    request.Optional.ShouldBeTrue();
                    request.Observable.ShouldBeTrue();
                    arg4.ShouldEqual(DefaultMetadata);
                    return observer;
                }
            });

            exp.GetBindingSource("", "", DefaultMetadata).ShouldEqual(observer);
        }

        protected override BindingMemberExpressionNodeBase GetExpression() => new BindingInstanceMemberExpressionNode(this, Path);

        #endregion
    }
}