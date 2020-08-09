﻿using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MugenMvvm.Binding.Attributes;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Members.Descriptors;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Members
{
    public static class BindableMembers
    {
        #region Fields

        private static MemberTypesRequest? _elementSourceMethod;
        private static MemberTypesRequest? _relativeSourceMethod;
        private static MemberTypesRequest? _hasErrorsMethod;
        private static MemberTypesRequest? _getErrorsMethod;
        private static MemberTypesRequest? _getErrorMethod;

        #endregion

        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BindableMembersDescriptor<T> For<T>() where T : class => default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BindableMembersTargetDescriptor<T> For<T>(T target) where T : class => new BindableMembersTargetDescriptor<T>(target);


        public static BindablePropertyDescriptor<T, object?> Root<T>(this BindableMembersDescriptor<T> _) where T : class => nameof(Root);

        public static BindablePropertyDescriptor<T, object?> Parent<T>(this BindableMembersDescriptor<T> _) where T : class => nameof(Parent);

        public static BindablePropertyDescriptor<T, object?> ParentNative<T>(this BindableMembersDescriptor<T> _) where T : class => nameof(ParentNative);

        public static BindablePropertyDescriptor<T, bool> Enabled<T>(this BindableMembersDescriptor<T> _) where T : class => nameof(Enabled);

        public static BindablePropertyDescriptor<T, object?> DataContext<T>(this BindableMembersDescriptor<T> _) where T : class => nameof(DataContext);

        public static BindableMethodDescriptor<T, object, object?> ElementSourceMethod<T>(this BindableMembersDescriptor<T> _) where T : class =>
            _elementSourceMethod ??= new MemberTypesRequest(nameof(ElementSource), Default.Types<object>());

        public static BindableMethodDescriptor<T, string, int, object?> RelativeSourceMethod<T>(this BindableMembersDescriptor<T> _) where T : class =>
            _relativeSourceMethod ??= new MemberTypesRequest(nameof(RelativeSource), Default.Types<string, int>());

        public static BindableMethodDescriptor<T, string[], bool> HasErrorsMethod<T>(this BindableMembersDescriptor<T> _) where T : class =>
            _hasErrorsMethod ??= new MemberTypesRequest(nameof(HasErrors), Default.Types<string[]>());

        public static BindableMethodDescriptor<T, string[], IReadOnlyList<object>> GetErrorsMethod<T>(this BindableMembersDescriptor<T> _) where T : class =>
            _getErrorsMethod ??= new MemberTypesRequest(nameof(GetErrors), Default.Types<string[]>());

        public static BindableMethodDescriptor<T, string[], object?> GetErrorMethod<T>(this BindableMembersDescriptor<T> _) where T : class =>
            _getErrorMethod ??= new MemberTypesRequest(nameof(GetError), Default.Types<string[]>());


        [BindingMember(nameof(Root))]
        public static object? Root<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : class => Root<T>(_: default).GetValue(descriptor.Target);


        [BindingMember(nameof(Parent))]
        public static object? Parent<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : class => Parent<T>(_: default).GetValue(descriptor.Target);

        public static void SetParent<T>(this BindableMembersTargetDescriptor<T> descriptor, object? value) where T : class => Parent<T>(_: default).SetValue(descriptor.Target, value);


        [BindingMember(nameof(Enabled))]
        public static bool Enabled<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : class => Enabled<T>(_: default).GetValue(descriptor.Target);

        public static void SetEnabled<T>(this BindableMembersTargetDescriptor<T> descriptor, bool value) where T : class => Enabled<T>(_: default).SetValue(descriptor.Target, value);


        [BindingMember(nameof(DataContext))]
        public static object? DataContext<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : class => DataContext<T>(_: default).GetValue(descriptor.Target);

        public static void SetDataContext<T>(this BindableMembersTargetDescriptor<T> descriptor, object? value) where T : class => DataContext<T>(_: default).SetValue(descriptor.Target, value);


        public static object? ElementSource<T>(this BindableMembersTargetDescriptor<T> descriptor, object name) where T : class => ElementSourceMethod<T>(default).Invoke(descriptor.Target, name);

        public static object? RelativeSource<T>(this BindableMembersTargetDescriptor<T> descriptor, string name, int level) where T : class => RelativeSourceMethod<T>(default).Invoke(descriptor.Target, name, level);

        public static bool HasErrors<T>(this BindableMembersTargetDescriptor<T> descriptor, params string[] members) where T : class => HasErrorsMethod<T>(default).Invoke(descriptor.Target, members);

        public static IReadOnlyList<object> GetErrors<T>(this BindableMembersTargetDescriptor<T> descriptor, params string[] members) where T : class => GetErrorsMethod<T>(default).Invoke(descriptor.Target, members);

        public static object? GetError<T>(this BindableMembersTargetDescriptor<T> descriptor, params string[] members) where T : class => GetErrorMethod<T>(default).Invoke(descriptor.Target, members);

        #endregion
    }
}