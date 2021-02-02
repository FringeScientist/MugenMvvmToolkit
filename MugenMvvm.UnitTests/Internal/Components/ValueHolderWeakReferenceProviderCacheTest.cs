﻿using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Internal;
using MugenMvvm.Internal.Components;
using MugenMvvm.UnitTests.Internal.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Internal.Components
{
    public class ValueHolderWeakReferenceProviderCacheTest : UnitTestBase
    {
        [Fact]
        public void ShouldCacheWeakReference()
        {
            var invokeCount = 0;
            var weak = new WeakReferenceImpl(this, true);
            var manager = new WeakReferenceManager(ComponentCollectionManager);
            manager.AddComponent(new ValueHolderWeakReferenceProviderCache());
            manager.AddComponent(new TestWeakReferenceProviderComponent(manager)
            {
                TryGetWeakReference = (o, context) =>
                {
                    ++invokeCount;
                    return weak;
                }
            });

            var target = new TestValueHolder<IWeakReference>();
            manager.TryGetWeakReference(target, DefaultMetadata).ShouldEqual(weak);
            manager.TryGetWeakReference(target, DefaultMetadata).ShouldEqual(weak);
            target.Value.ShouldEqual(weak);
            invokeCount.ShouldEqual(1);
        }
    }
}