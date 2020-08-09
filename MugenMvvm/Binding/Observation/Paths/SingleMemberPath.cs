﻿using System.Collections;
using System.Collections.Generic;
using MugenMvvm.Binding.Interfaces.Observation;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Observation.Paths
{
    public sealed class SingleMemberPath : IMemberPath, IValueHolder<string>, IReadOnlyList<string>
    {
        #region Constructors

        public SingleMemberPath(string path)
        {
            Should.NotBeNull(path, nameof(path));
            Path = path;
        }

        #endregion

        #region Properties

        public string Path { get; }

        public IReadOnlyList<string> Members => this;

        string? IValueHolder<string>.Value { get; set; }

        int IReadOnlyCollection<string>.Count => 1;

        string IReadOnlyList<string>.this[int index]
        {
            get
            {
                if (index != 0)
                    ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(index));
                return Path;
            }
        }

        #endregion

        #region Implementation of interfaces

        IEnumerator<string> IEnumerable<string>.GetEnumerator() => Default.SingleValueEnumerator(Path);

        IEnumerator IEnumerable.GetEnumerator() => Default.SingleValueEnumerator(Path);

        #endregion
    }
}