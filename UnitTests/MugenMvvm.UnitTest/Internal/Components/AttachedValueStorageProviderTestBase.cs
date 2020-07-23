﻿using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Internal.Components
{
    public abstract class AttachedValueStorageProviderTestBase : UnitTestBase
    {
        #region Fields

        protected const string TestPath = "Test";

        #endregion

        #region Methods

        [Fact]
        public virtual void ShouldGetSetValues()
        {
            var item = GetSupportedItem();
            var manager = new AttachedValueManager();
            manager.AddComponent(GetComponent());
            var attachedValues = manager.TryGetAttachedValues(item, DefaultMetadata);

            attachedValues.Contains(TestPath).ShouldBeFalse();
            attachedValues.TryGet(TestPath, out var value).ShouldBeFalse();
            attachedValues.GetCount().ShouldEqual(0);
            value.ShouldBeNull();

            attachedValues.Set(TestPath, this, out var old);
            old.ShouldBeNull();
            attachedValues.TryGet(TestPath, out value).ShouldBeTrue();
            attachedValues.Contains(TestPath).ShouldBeTrue();
            attachedValues.GetCount().ShouldEqual(1);
            value.ShouldEqual(this);

            attachedValues.Set(TestPath, new object(), out old);
            old.ShouldEqual(this);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public virtual void TryGetValuesShouldReturnAllValues(int count)
        {
            var item = GetSupportedItem();
            var manager = new AttachedValueManager();
            manager.AddComponent(GetComponent());
            var attachedValues = manager.TryGetAttachedValues(item, DefaultMetadata);

            var hashSet = new HashSet<KeyValuePair<string, object?>>();
            var values = new List<KeyValuePair<string, object?>>();
            for (var i = 0; i < count; i++)
            {
                var pair = new KeyValuePair<string, object?>(TestPath + i, i);
                attachedValues.Set(pair.Key, pair.Value, out _);
                values.Add(pair);
            }

            attachedValues.GetValues(this, (o, pair, arg3) =>
            {
                o.ShouldEqual(item);
                arg3.ShouldEqual(this);
                hashSet.Remove(pair);
                return false;
            }).AsList().ShouldBeEmpty();
            hashSet.Count.ShouldEqual(0);
            attachedValues.GetValues(this).AsList().SequenceEqual(values).ShouldBeTrue();
        }

        [Fact]
        public virtual void AddOrUpdateShouldAddNewValue1()
        {
            var item = GetSupportedItem();
            var manager = new AttachedValueManager();
            manager.AddComponent(GetComponent());
            var attachedValues = manager.TryGetAttachedValues(item, DefaultMetadata);

            attachedValues.AddOrUpdate(TestPath, this, this, (_, __, ___, ____) => throw new NotSupportedException()).ShouldEqual(this);
            attachedValues.TryGet(TestPath, out object? v).ShouldBeTrue();
            v.ShouldEqual(this);
        }

        [Fact]
        public virtual void AddOrUpdateShouldAddNewValue2()
        {
            var invokeCount = 0;
            var item = GetSupportedItem();
            var manager = new AttachedValueManager();
            manager.AddComponent(GetComponent());
            var attachedValues = manager.TryGetAttachedValues(item, DefaultMetadata);

            attachedValues.AddOrUpdate(TestPath, this, (i, state) =>
            {
                ++invokeCount;
                i.ShouldEqual(item);
                state.ShouldEqual(this);
                return this;
            }, (_, __, ___, ____) => throw new NotSupportedException()).ShouldEqual(this);
            attachedValues.TryGet(TestPath, out object? v).ShouldBeTrue();
            v.ShouldEqual(this);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public virtual void AddOrUpdateShouldUpdateOldValue1()
        {
            var invokeCount = 0;
            var oldValue = new object();
            var newValue = new object();
            var item = GetSupportedItem();
            var manager = new AttachedValueManager();
            manager.AddComponent(GetComponent());
            var attachedValues = manager.TryGetAttachedValues(item, DefaultMetadata);

            attachedValues.Set(TestPath, oldValue, out _);
            attachedValues.AddOrUpdate(TestPath, newValue, this, (o, value, currentValue, state) =>
            {
                ++invokeCount;
                o.ShouldEqual(item);
                value.ShouldEqual(newValue);
                currentValue.ShouldEqual(oldValue);
                state.ShouldEqual(this);
                return newValue;
            }).ShouldEqual(newValue);
            attachedValues.TryGet(TestPath, out object? v).ShouldBeTrue();
            v.ShouldEqual(newValue);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public virtual void AddOrUpdateShouldUpdateOldValue2()
        {
            var invokeCount = 0;
            var oldValue = new object();
            var newValue = new object();
            var item = GetSupportedItem();
            var manager = new AttachedValueManager();
            manager.AddComponent(GetComponent());
            var attachedValues = manager.TryGetAttachedValues(item, DefaultMetadata);

            attachedValues.Set(TestPath, oldValue, out _);
            attachedValues.AddOrUpdate(TestPath, this, (it, state) =>
            {
                it.ShouldEqual(item);
                state.ShouldEqual(this);
                return newValue;
            }, (o, value, currentValue, state) =>
            {
                ++invokeCount;
                o.ShouldEqual(item);
                value(o, state).ShouldEqual(newValue);
                currentValue.ShouldEqual(oldValue);
                state.ShouldEqual(this);
                return value(o, state);
            }).ShouldEqual(newValue);
            attachedValues.TryGet(TestPath, out object? v).ShouldBeTrue();
            v.ShouldEqual(newValue);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public virtual void GetOrAddShouldAddNewValue1()
        {
            var item = GetSupportedItem();
            var manager = new AttachedValueManager();
            manager.AddComponent(GetComponent());
            var attachedValues = manager.TryGetAttachedValues(item, DefaultMetadata);

            attachedValues.GetOrAdd(TestPath, this).ShouldEqual(this);
            attachedValues.TryGet(TestPath, out object? v).ShouldBeTrue();
            v.ShouldEqual(this);
        }

        [Fact]
        public virtual void GetOrAddShouldAddNewValue2()
        {
            int invokeCount = 0;
            var item = GetSupportedItem();
            var manager = new AttachedValueManager();
            manager.AddComponent(GetComponent());
            var attachedValues = manager.TryGetAttachedValues(item, DefaultMetadata);

            attachedValues.GetOrAdd(TestPath, manager, (o, providerComponent) =>
             {
                 ++invokeCount;
                 o.ShouldEqual(item);
                 providerComponent.ShouldEqual(manager);
                 return this;
             }).ShouldEqual(this);
            attachedValues.TryGet(TestPath, out object? v).ShouldBeTrue();
            v.ShouldEqual(this);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public virtual void GetOrAddShouldGetOldValue1()
        {
            var oldValue = new object();
            var newValue = new object();
            var item = GetSupportedItem();
            var manager = new AttachedValueManager();
            manager.AddComponent(GetComponent());
            var attachedValues = manager.TryGetAttachedValues(item, DefaultMetadata);

            attachedValues.Set(TestPath, oldValue, out _);
            attachedValues.GetOrAdd(TestPath, newValue).ShouldEqual(oldValue);
            attachedValues.TryGet(TestPath, out object? v).ShouldBeTrue();
            v.ShouldEqual(oldValue);
        }

        [Fact]
        public virtual void GetOrAddShouldGetOldValue2()
        {
            var oldValue = new object();
            var item = GetSupportedItem();
            var manager = new AttachedValueManager();
            manager.AddComponent(GetComponent());
            var attachedValues = manager.TryGetAttachedValues(item, DefaultMetadata);

            attachedValues.Set(TestPath, oldValue, out _);
            attachedValues.GetOrAdd<object, object>(TestPath, this, (_, __) => throw new NotSupportedException()).ShouldEqual(oldValue);
            attachedValues.TryGet(TestPath, out object? v).ShouldBeTrue();
            v.ShouldEqual(oldValue);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public virtual void ClearShouldClearByPath(int count)
        {
            var item = GetSupportedItem();
            var manager = new AttachedValueManager();
            manager.AddComponent(GetComponent());
            var attachedValues = manager.TryGetAttachedValues(item, DefaultMetadata);

            for (var i = 0; i < count; i++)
            {
                var pair = new KeyValuePair<string, object>(TestPath + i, i + 1);
                attachedValues.Set(pair.Key, pair.Value, out _);
            }

            for (var i = 0; i < count; i++)
            {
                attachedValues.TryGet(TestPath + i, out var v).ShouldBeTrue();
                v.ShouldEqual(i + 1);

                attachedValues.Remove(TestPath + i, out var old).ShouldBeTrue();
                old.ShouldEqual(i + 1);

                attachedValues.TryGet(TestPath + i, out v).ShouldBeFalse();
                v.ShouldNotEqual(i + 1);
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public virtual void ClearShouldClearAll(int count)
        {
            var item = GetSupportedItem();
            var manager = new AttachedValueManager();
            manager.AddComponent(GetComponent());
            var attachedValues = manager.TryGetAttachedValues(item, DefaultMetadata);

            for (var i = 0; i < count; i++)
            {
                var pair = new KeyValuePair<string, object>(TestPath + i, i + 1);
                attachedValues.Set(pair.Key, pair.Value, out _);
            }

            attachedValues.Clear();
            for (var i = 0; i < count; i++)
            {
                attachedValues.TryGet(TestPath + i, out var v).ShouldBeFalse();
                v.ShouldNotEqual(i + 1);
            }
        }

#if !DEBUG
        [Fact]
        public virtual void ClearShouldNotKeepRef()
        {
            var item = GetSupportedItem();
            var manager = new AttachedValueManager();
            manager.AddComponent(GetComponent());
            var attachedValues = manager.TryGetAttachedValues(item, DefaultMetadata);

            var value = new object();
            var weakReference = new WeakReference(value);
            attachedValues.Set(TestPath, value, out _);

            attachedValues.Remove(TestPath, out _);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            weakReference.IsAlive.ShouldBeFalse();
        }

        [Fact]
        public virtual void ShouldBeEphemeron1()
        {
            object? item = GetSupportedItem();
            var manager = new AttachedValueManager();
            manager.AddComponent(GetComponent());
            var attachedValues = manager.TryGetAttachedValues(item, DefaultMetadata);

            var value = new object();
            var weakReference = new WeakReference(value);
            attachedValues.Set(TestPath, value, out _);

            attachedValues = default;
            item = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            weakReference.IsAlive.ShouldBeFalse();
        }

        [Fact]
        public virtual void ShouldBeEphemeron2()
        {
            object? item = GetSupportedItem();
            var manager = new AttachedValueManager();
            manager.AddComponent(GetComponent());
            var attachedValues = manager.TryGetAttachedValues(item, DefaultMetadata);

            var value = item;
            var weakReference = new WeakReference(value);
            attachedValues.Set(TestPath, value, out _);

            attachedValues = default;
            item = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            weakReference.IsAlive.ShouldBeFalse();
        }
#endif

        protected abstract object GetSupportedItem();

        protected abstract IAttachedValueStorageProviderComponent GetComponent();

        #endregion
    }
}