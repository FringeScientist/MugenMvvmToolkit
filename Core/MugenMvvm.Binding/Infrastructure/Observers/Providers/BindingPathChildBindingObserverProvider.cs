﻿using System;
using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;

// ReSharper disable once CheckNamespace
namespace MugenMvvm.Binding.Infrastructure.Observers
{
    public sealed class BindingPathChildBindingObserverProvider : IBindingPathChildBindingObserverProvider
    {
        #region Fields

        private readonly CacheDictionary _cache;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public BindingPathChildBindingObserverProvider()
        {
            _cache = new CacheDictionary();
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = 10;

        public bool UseCache { get; set; } = true;

        #endregion

        #region Implementation of interfaces

        public bool TryGetMemberObserver(Type type, object member, IReadOnlyMetadataContext metadata, out BindingMemberObserver observer)
        {
            observer = default;
            return false;
        }

        public IBindingPath TryGetBindingPath(object path, IReadOnlyMetadataContext metadata)
        {
            if (!(path is string stringPath))
                return null;

            if (stringPath.Length == 0)
                return EmptyBindingPath.Instance;

            var hasDot = stringPath.IndexOf('.') >= 0;
            var hasBracket = stringPath.IndexOf('[') >= 0;
            if (!hasDot && !hasBracket)
                return new SimpleBindingPath(stringPath);

            if (UseCache)
                return GetFromCache(stringPath, hasBracket);

            return new MultiBindingPath(stringPath, hasBracket);
        }

        public IBindingPathObserver? TryGetBindingPathObserver(object source, IBindingPath path, IReadOnlyMetadataContext metadata)
        {
            return null;
        }

        #endregion

        #region Methods

        private MultiBindingPath GetFromCache(string path, bool hasBracket)
        {
            if (!_cache.TryGetValue(path, out var value))
            {
                value = new MultiBindingPath(path, hasBracket);
                _cache[path] = value;
            }

            return value;
        }

        #endregion

        #region Nested types

        private sealed class CacheDictionary : LightDictionaryBase<string, MultiBindingPath>
        {
            #region Constructors

            public CacheDictionary() : base(59)
            {
            }

            #endregion

            #region Methods

            protected override bool Equals(string x, string y)
            {
                return x.Equals(y);
            }

            protected override int GetHashCode(string key)
            {
                return key.GetHashCode();
            }

            #endregion
        }

        private sealed class EmptyBindingPath : IBindingPath
        {
            #region Fields

            public static readonly EmptyBindingPath Instance = new EmptyBindingPath();

            #endregion

            #region Constructors

            private EmptyBindingPath()
            {
            }

            #endregion

            #region Properties

            public string Path => "";

            public string[] Parts => Default.EmptyArray<string>();

            public bool IsSingle => false;

            #endregion
        }

        private sealed class SimpleBindingPath : IBindingPath
        {
            #region Fields

            private string[]? _parts;

            #endregion

            #region Constructors

            public SimpleBindingPath(string path)
            {
                Path = path;
            }

            #endregion

            #region Properties

            public string Path { get; }

            public string[] Parts
            {
                get
                {
                    if (_parts == null)
                        _parts = new[] {Path};
                    return _parts;
                }
            }

            public bool IsSingle => true;

            #endregion
        }

        private sealed class MultiBindingPath : IBindingPath
        {
            #region Constructors

            public MultiBindingPath(string path, bool hasIndexer)
            {
                Path = path;
                Parts = path.Split(BindingMugenExtensions.DotSeparator, StringSplitOptions.RemoveEmptyEntries);

                if (hasIndexer)
                {
                    var items = new List<string>();
                    for (var index = 0; index < Parts.Length; index++)
                    {
                        var s = Parts[index];
                        var start = s.IndexOf('[');
                        var end = s.IndexOf(']');
                        if (start <= 0 || end < 0)
                        {
                            items.Add(s.Trim());
                            continue;
                        }

                        var indexer = s.Substring(start, end - start + 1).Trim();
                        items.Add(s.Substring(0, start).Trim());
                        items.Add(indexer);
                    }


                    Parts = items.ToArray();
                }
            }

            #endregion

            #region Properties

            public string Path { get; }

            public string[] Parts { get; }

            public bool IsSingle => false;

            #endregion
        }

        #endregion
    }
}