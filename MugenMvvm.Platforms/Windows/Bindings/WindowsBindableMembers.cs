﻿using System.Runtime.CompilerServices;
using System.Windows;
using MugenMvvm.Bindings.Attributes;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Members.Descriptors;
using MugenMvvm.Interfaces.Collections;

namespace MugenMvvm.Windows.Bindings
{
    public static class WindowsBindableMembers
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BindablePropertyDescriptor<T, bool> Visible<T>(this BindableMembersDescriptor<T> _) where T : UIElement => nameof(Visible);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BindablePropertyDescriptor<T, IDiffableEqualityComparer?> DiffableEqualityComparer<T>(this BindableMembersDescriptor<T> _) where T : class =>
            nameof(DiffableEqualityComparer);

        [BindingMember(nameof(Visible))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Visible<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : UIElement => Visible<T>(_: default).GetValue(descriptor.Target);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetVisible<T>(this BindableMembersTargetDescriptor<T> descriptor, bool value) where T : UIElement =>
            Visible<T>(_: default).SetValue(descriptor.Target, value);

        [BindingMember(nameof(DiffableEqualityComparer))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IDiffableEqualityComparer? DiffableEqualityComparer<T>(this BindableMembersTargetDescriptor<T> descriptor) where T : class =>
            DiffableEqualityComparer<T>(_: default).GetValue(descriptor.Target);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetDiffableEqualityComparer<T>(this BindableMembersTargetDescriptor<T> descriptor, IDiffableEqualityComparer? value) where T : class =>
            DiffableEqualityComparer<T>(_: default).SetValue(descriptor.Target, value);
    }
}