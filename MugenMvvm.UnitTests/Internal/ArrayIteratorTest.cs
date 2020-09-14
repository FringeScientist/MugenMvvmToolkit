﻿using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Internal
{
    public class ArrayIteratorTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void IndexShouldThrowOutOfRange()
        {
            var iterator = new ArrayIterator<object>(0, null, null);
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
            var itemOrList = ItemOrList.FromList(objects.ToArray());

            var iterator = new ArrayIterator<object>(count, itemOrList.Item, itemOrList.List);
            iterator.Count.ShouldEqual(objects.Count);
            for (var i = 0; i < iterator.Count; i++)
                objects[i].ShouldEqual(iterator[i]);

            var enumResult = new List<object>();
            foreach (var item in iterator)
                enumResult.Add(item);
            enumResult.SequenceEqual(objects).ShouldBeTrue();
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
            var itemOrList = ItemOrList.FromList(objects.ToArray());

            var iterator = new ArrayIterator<object>(count, itemOrList.Item, itemOrList.List);
            iterator.ToArray().SequenceEqual(objects).ShouldBeTrue();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(10)]
        public void AsListShouldBeCorrect(int count)
        {
            var objects = new List<object>();
            for (var i = 0; i < count; i++)
                objects.Add(new object());
            var itemOrList = ItemOrList.FromList(objects.ToArray());

            var iterator = new ArrayIterator<object>(count, itemOrList.Item, itemOrList.List);
            iterator.AsList().SequenceEqual(objects).ShouldBeTrue();
        }

        #endregion
    }
}