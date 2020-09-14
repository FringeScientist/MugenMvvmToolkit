﻿using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;
using Should;

namespace MugenMvvm.UnitTests.Metadata
{
    public abstract class ReadOnlyMetadataContextTestBase : UnitTestBase
    {
        #region Fields

        protected const string DefaultGetterValue = "Testf";

        protected static readonly IMetadataContextKey<string?, string?> CustomGetterKey = MetadataContextKey
            .Create<string?, string?>(nameof(CustomGetterKey))
            .Getter(Getter)
            .Build();

        protected static readonly IMetadataContextKey<int, int> CustomSetterKey = MetadataContextKey
            .Create<int, int>(nameof(CustomSetterKey))
            .Setter(Setter)
            .Build();

        #endregion

        #region Properties

        protected static IReadOnlyMetadataContext? CurrentGetterContext { get; set; }
        protected static object? CurrentGetterValue { get; set; }
        protected static int GetterCount { get; set; }
        protected static string? GetterValue { get; set; }

        protected static IReadOnlyMetadataContext? CurrentSetterContext { get; set; }
        protected static int CurrentSetterValue { get; set; }
        protected static int SetterCount { get; set; }
        protected static object? CurrentSetterOldValue { get; set; }
        protected static object? SetterValue { get; set; }

        #endregion

        #region Methods

        private static string? Getter(IReadOnlyMetadataContext arg1, IMetadataContextKey<string?, string?> arg2, object? arg3)
        {
            ++GetterCount;
            arg2.ShouldEqual(CustomGetterKey);
            CurrentGetterContext = arg1;
            CurrentGetterValue = arg3;
            return GetterValue;
        }

        private static object? Setter(IReadOnlyMetadataContext arg1, IMetadataContextKey<int, int> arg2, object? arg3, int arg4)
        {
            ++SetterCount;
            arg2.ShouldEqual(CustomSetterKey);
            CurrentSetterOldValue = arg3;
            CurrentSetterContext = arg1;
            CurrentSetterValue = arg4;
            return SetterValue;
        }

        protected void EnumeratorCountTest(IReadOnlyMetadataContext metadataContext, List<KeyValuePair<IMetadataContextKey, object?>> values)
        {
            metadataContext.Count.ShouldEqual(values.Count);
            metadataContext.SequenceEqual(values).ShouldBeTrue();
        }

        public void ContainsTest(IReadOnlyMetadataContext metadataContext, List<KeyValuePair<IMetadataContextKey, object?>> values)
        {
            foreach (var metadataContextValue in values)
                metadataContext.Contains(metadataContextValue.Key);
        }

        public void TryGetTest<T>(IReadOnlyMetadataContext context, IReadOnlyMetadataContextKey<T> key, T expectedValue)
        {
            context.TryGet(key, out var value, default!).ShouldBeTrue();
            value!.ShouldEqual(expectedValue);

            context.TryGetRaw(key, out var rawValue).ShouldBeTrue();
            key.GetValue(context, rawValue).ShouldEqual(expectedValue);
        }

        public void TryGetGetterTest(IReadOnlyMetadataContext metadataContext)
        {
            const string getterValueToSet = "Test";
            GetterCount = 0;
            GetterValue = getterValueToSet;

            metadataContext.TryGet(CustomGetterKey, out var value, null).ShouldBeTrue();
            GetterCount.ShouldEqual(1);
            CurrentGetterContext.ShouldEqual(metadataContext);
            CurrentGetterValue.ShouldEqual(DefaultGetterValue);
            value.ShouldEqual(getterValueToSet);
        }

        public void TryGetDefaultTest(IReadOnlyMetadataContext metadataContext)
        {
            const string defaultValue = "Test1";
            const string defaultValueGet = "t";
            var contextKey = MetadataContextKey.Create<string?, string>("Test").DefaultValue(defaultValue).Build();
            metadataContext.TryGet(contextKey!, out var value, null).ShouldBeFalse();
            value.ShouldEqual(defaultValue);

            metadataContext.TryGet(contextKey!, out value, defaultValueGet).ShouldBeFalse();
            value.ShouldEqual(defaultValue);

            var invokedCount = 0;
            contextKey = MetadataContextKey.Create<string?, string>("Test").DefaultValue((context, key, arg3) =>
            {
                ++invokedCount;
                context.ShouldEqual(metadataContext);
                key.ShouldEqual(contextKey);
                arg3.ShouldEqual(defaultValueGet);
                return defaultValue;
            }).Build();

            metadataContext.TryGet(contextKey, out value, defaultValueGet).ShouldBeFalse();
            value.ShouldEqual(defaultValue);
            invokedCount.ShouldEqual(1);
        }

        #endregion
    }
}