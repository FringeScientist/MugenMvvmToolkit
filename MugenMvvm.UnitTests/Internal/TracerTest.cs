﻿using System;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Internal;
using MugenMvvm.Internal.Components;
using MugenMvvm.UnitTests.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Internal
{
    public class TracerTest : ComponentOwnerTestBase<Tracer>
    {
        #region Methods

        [Fact]
        public void CanTraceShouldReturnFalseNoComponents() => new Tracer().CanTrace(TraceLevel.Information, DefaultMetadata).ShouldBeFalse();

        [Fact]
        public void TraceShouldNotThrowNoComponents() => new Tracer().Trace(TraceLevel.Information, string.Empty, null, DefaultMetadata);

        [Theory]
        [InlineData(1, "Information", true)]
        [InlineData(1, "Information", false)]
        [InlineData(1, "Warning", false)]
        [InlineData(10, "Information", true)]
        [InlineData(10, "Information", false)]
        [InlineData(10, "Error", false)]
        public void CanTraceShouldBeHandledByComponents(int count, string traceLevel, bool withMetadata)
        {
            var traceLevelValue = TraceLevel.Parse(traceLevel);
            var tracer = new Tracer();
            var invokeCount = 0;
            for (var i = 0; i < count; i++)
            {
                var canTrace = count - 1 == i;
                var component = new DelegateTracer((level, s, arg3, arg4) => { }, (level, metadata) =>
                {
                    ++invokeCount;
                    level.ShouldEqual(traceLevelValue);
                    if (withMetadata)
                        metadata.ShouldEqual(DefaultMetadata);
                    else
                        metadata.ShouldBeNull();
                    return canTrace;
                });
                component.Priority = -i;
                tracer.AddComponent(component);
            }

            tracer.CanTrace(traceLevelValue, withMetadata ? DefaultMetadata : null).ShouldEqual(true);
            invokeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1, "Information", true)]
        [InlineData(1, "Information", false)]
        [InlineData(1, "Warning", false)]
        [InlineData(10, "Information", true)]
        [InlineData(10, "Information", false)]
        [InlineData(10, "Error", false)]
        public void TraceShouldBeHandledByComponents(int count, string traceLevel, bool withMetadata)
        {
            var traceLevelValue = TraceLevel.Parse(traceLevel);
            var message = traceLevel;
            var exception = withMetadata ? new Exception() : null;
            var tracer = new Tracer();
            var invokeCount = 0;
            for (var i = 0; i < count; i++)
            {
                var component = new DelegateTracer((level, m, exc, metadata) =>
                {
                    ++invokeCount;
                    message.ShouldEqual(m);
                    exception.ShouldEqual(exc);
                    level.ShouldEqual(traceLevelValue);
                    if (withMetadata)
                        metadata.ShouldEqual(DefaultMetadata);
                    else
                        metadata.ShouldBeNull();
                }, (level, context) => true);
                component.Priority = -i;
                tracer.AddComponent(component);
            }

            tracer.Trace(traceLevelValue, message, exception, withMetadata ? DefaultMetadata : null);
            invokeCount.ShouldEqual(count);
        }

        protected override Tracer GetComponentOwner(IComponentCollectionManager? collectionProvider = null) => new Tracer(collectionProvider);

        #endregion
    }
}