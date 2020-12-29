﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace MugenMvvm.Internal
{
    public static class ItemOrList
    {
        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrList<TItem, IReadOnlyList<TItem>> FromItem<TItem>([AllowNull] TItem item)
            where TItem : class
            => new ItemOrList<TItem, IReadOnlyList<TItem>>(item, item != null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrList<TItem, IReadOnlyList<TItem>> FromItem<TItem>([AllowNull] TItem item, bool hasItem) => new ItemOrList<TItem, IReadOnlyList<TItem>>(item, hasItem);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrList<TItem, TList> FromItem<TItem, TList>([AllowNull] TItem item)
            where TItem : class?
            where TList : class, IEnumerable<TItem> => new ItemOrList<TItem, TList>(item, item != null);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrList<TItem, TList> FromItem<TItem, TList>([AllowNull] TItem item, bool hasItem) where TList : class, IEnumerable<TItem> => new ItemOrList<TItem, TList>(item, hasItem);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrList<TItem, IReadOnlyList<TItem>> FromList<TItem>(IReadOnlyList<TItem>? readOnlyList) => new ItemOrList<TItem, IReadOnlyList<TItem>>(readOnlyList);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrList<TItem, IList<TItem>> FromList<TItem>(IList<TItem>? iList) => new ItemOrList<TItem, IList<TItem>>(iList);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrList<TItem, TList> FromList<TItem, TList>(TList? enumerable)
            where TList : class, IEnumerable<TItem> => new ItemOrList<TItem, TList>(enumerable);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrList<TItem, TList> FromRawValue<TItem, TList>(object? value)
            where TItem : class
            where TList : class, IEnumerable<TItem>
        {
            if (value is TList list)
                return new ItemOrList<TItem, TList>(list);
            return new ItemOrList<TItem, TList>((TItem) value!, value != null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrList<TItem, IReadOnlyList<TItem>> FromRawValueToReadonly<TItem>(object? value) where TItem : class => FromRawValue<TItem, IReadOnlyList<TItem>>(value);

        #endregion
    }
}