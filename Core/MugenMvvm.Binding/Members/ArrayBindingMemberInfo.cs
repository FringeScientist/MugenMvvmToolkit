﻿using System;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Members
{
    internal sealed class ArrayBindingMemberInfo : IBindingPropertyInfo
    {
        #region Fields

        private readonly int[] _indexes;

        #endregion

        #region Constructors

        public ArrayBindingMemberInfo(string name, Type arrayType, string[] indexes)
        {
            _indexes = BindingMugenExtensions.GetIndexerValues<int>(indexes);
            Name = name;
            Type = arrayType.GetElementType();
        }

        #endregion

        #region Properties

        public string Name { get; }

        public Type Type { get; }

        public object? Member => null;

        public BindingMemberType MemberType => BindingMemberType.Property;

        public MemberFlags AccessModifiers => MemberFlags.InstancePublic;

        public bool CanRead => true;

        public bool CanWrite => true;

        #endregion

        #region Implementation of interfaces

        public object? GetValue(object? source, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(source, nameof(source));
            return ((Array) source!).GetValue(_indexes);
        }

        public void SetValue(object? source, object? value, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(source, nameof(source));
            ((Array) source!).SetValue(value, _indexes);
        }

        public IDisposable? TryObserve(object? target, IEventListener listener, IReadOnlyMetadataContext? metadata = null)
        {
            return null;
        }

        #endregion
    }
}