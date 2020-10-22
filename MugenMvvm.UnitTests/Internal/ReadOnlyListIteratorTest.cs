﻿using System;
using System.Collections.Generic;
using MugenMvvm.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Internal
{
    public class ReadOnlyListIteratorTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void IndexShouldThrowOutOfRange()
        {
            var iterator = new ReadOnlyListIterator<object, List<object>>(0, null, null);
            try
            {
                var o = iterator[0];
                throw new NotSupportedException();
            }
            catch (ArgumentOutOfRangeException)
            {
                ;
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        public void IndexCountEnumeratorShouldBeCorrect(int count)
        {
            var objects = new List<object>();
            for (var i = 0; i < count; i++)
                objects.Add(new object());
            var itemOrList = ItemOrList.FromList(objects);

            var iterator = new ReadOnlyListIterator<object, List<object>>(count, itemOrList.Item, itemOrList.List);
            iterator.Count.ShouldEqual(objects.Count);
            for (var i = 0; i < iterator.Count; i++)
                objects[i].ShouldEqual(iterator[i]);

            var enumResult = new List<object>();
            foreach (var item in iterator)
                enumResult.Add(item);
            enumResult.ShouldEqual(objects);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        public void ToArrayShouldBeCorrect(int count)
        {
            var objects = new List<object>();
            for (var i = 0; i < count; i++)
                objects.Add(new object());
            var itemOrList = ItemOrList.FromList(objects);

            var iterator = new ReadOnlyListIterator<object, List<object>>(count, itemOrList.Item, itemOrList.List);
            iterator.ToArray().ShouldEqual(objects);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        public void AsListShouldBeCorrect1(int count)
        {
            var objects = new List<object>();
            for (var i = 0; i < count; i++)
                objects.Add(new object());
            var itemOrList = ItemOrList.FromList(objects);

            var iterator = new ReadOnlyListIterator<object, List<object>>(count, itemOrList.Item, itemOrList.List);
            iterator.AsList().ShouldEqual(objects);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        public void AsListShouldBeCorrect2(int count)
        {
            var objects = new List<object>();
            for (var i = 0; i < count; i++)
                objects.Add(new object());
            var itemOrList = ItemOrList.FromList(objects);

            var iterator = new ReadOnlyListIterator<object, List<object>>(count, itemOrList.Item, itemOrList.List);
            iterator.AsList(() => new List<object>(), o => new List<object> {o}).ShouldEqual(objects);
        }

        #endregion
    }
}