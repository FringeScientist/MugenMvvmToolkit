﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvm.Extensions;

namespace MugenMvvm.Internal
{
    [StructLayout(LayoutKind.Auto)]
    public struct ItemOrListEditor<TItem, TList>
        where TList : class, IList<TItem>
    {
        #region Fields

        private readonly Func<TList> _getNewList;
        private readonly Func<TItem, bool> _isEmpty;
        private TItem _item;
        private TList? _list;

        #endregion

        #region Constructors

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemOrListEditor(Func<TItem, bool> isEmpty, Func<TList> getNewList) : this(default!, null, isEmpty, getNewList)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemOrListEditor([AllowNull] TItem item, TList? list, Func<TItem, bool> isEmpty, Func<TList> getNewList)
        {
            Should.NotBeNull(isEmpty, nameof(isEmpty));
            Should.NotBeNull(getNewList, nameof(getNewList));
            _isEmpty = isEmpty;
            _getNewList = getNewList;
            _item = item!;
            _list = list;
        }

        #endregion

        #region Properties

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (_list != null)
                    return _list.Count;
                return _isEmpty(_item) ? 0 : 1;
            }
        }

        public bool IsNullOrEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _isEmpty == null || _list == null && _isEmpty(_item);
        }

        public TItem this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if ((uint) index >= (uint) Count)
                    ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));

                return _list == null ? _item : _list[index];
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if ((uint) index >= (uint) Count)
                    ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));

                if (_list != null)
                    _list[index] = value;
                else
                    _item = value;
            }
        }

        #endregion

        #region Methods

        public ItemOrListEditor<TItem, TList> AddRange(IEnumerable<TItem>? values) => AddRange(new ItemOrList<TItem, IEnumerable<TItem>>(values));

        public ItemOrListEditor<TItem, TList> AddRange<T>(ItemOrList<TItem, T> value) where T : class, IEnumerable<TItem>
        {
            if (_isEmpty(value.Item!) && value.List == null)
                return this;

            if (_list == null)
            {
                if (_isEmpty(_item))
                {
                    if (value.List == null)
                        _item = value.Item!;
                    else
                    {
                        _list = _getNewList();
                        _list.AddRange(value.List);
                    }

                    return this;
                }

                _list = _getNewList();
                _list.Add(_item);
                _item = default!;
            }

            if (value.List == null)
                _list.Add(value.Item!);
            else
                _list.AddRange(value.List);
            return this;
        }

        public ItemOrListEditor<TItem, TList> Add([AllowNull] TItem item)
        {
            if (_isEmpty(item!))
                return this;
            if (_list != null)
                _list.Add(item!);
            else if (_isEmpty(_item))
                _item = item!;
            else
            {
                _list = _getNewList();
                _list.Add(_item);
                _list.Add(item!);
                _item = default!;
            }

            return this;
        }

        public bool Remove(TItem item)
        {
            if (_list != null)
            {
                _list.Remove(item);
                return true;
            }

            if (EqualityComparer<TItem>.Default.Equals(_item, item))
            {
                _item = default!;
                return true;
            }

            return false;
        }

        public void RemoveAt(int index)
        {
            if ((uint) index >= (uint) Count)
                ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));
            if (_list == null)
                _item = default!;
            else
                _list.RemoveAt(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal object? GetRawValueInternal()
        {
            if (_list == null)
                return _item;

            if (_list.Count > 1)
                return _list;
            return _list.Count == 0 ? null : (object?) _list[0];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemOrList<TItem, TList> ToItemOrList()
        {
            if (_list == null)
                return new ItemOrList<TItem, TList>(_item, !_isEmpty(_item));

            if (_list.Count > 1)
                return new ItemOrList<TItem, TList>(_list, true);
            return _list.Count == 0 ? default! : new ItemOrList<TItem, TList>(_list[0], true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ItemOrList<TItem, TNewList> ToItemOrList<TNewList>() where TNewList : class, IEnumerable<TItem>
        {
            if (_list == null)
                return new ItemOrList<TItem, TNewList>(_item, !_isEmpty(_item));

            if (_list.Count > 1)
                return new ItemOrList<TItem, TNewList>((TNewList) (object) _list, true);
            return _list.Count == 0 ? default! : new ItemOrList<TItem, TNewList>(_list[0], true);
        }

        #endregion
    }
}