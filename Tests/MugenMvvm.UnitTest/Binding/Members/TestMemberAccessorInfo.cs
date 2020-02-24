﻿using System;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.UnitTest.Binding.Members
{
    public class TestMemberAccessorInfo : TestMemberInfoBase, IMemberAccessorInfo
    {
        #region Properties

        public bool CanRead { get; set; }

        public bool CanWrite { get; set; }

        public Func<object?, IReadOnlyMetadataContext?, object?>? GetValue { get; set; }

        public Action<object?, object?, IReadOnlyMetadataContext?>? SetValue { get; set; }

        #endregion

        #region Implementation of interfaces

        object? IMemberAccessorInfo.GetValue(object? target, IReadOnlyMetadataContext? metadata)
        {
            return GetValue?.Invoke(target, metadata);
        }

        void IMemberAccessorInfo.SetValue(object? target, object? value, IReadOnlyMetadataContext? metadata)
        {
            SetValue?.Invoke(target, value, metadata);
        }

        #endregion
    }
}