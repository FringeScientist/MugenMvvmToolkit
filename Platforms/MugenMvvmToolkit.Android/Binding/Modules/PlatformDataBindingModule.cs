﻿#region Copyright

// ****************************************************************************
// <copyright file="PlatformDataBindingModule.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion

using System;
using System.Collections;
using System.Reflection;
using Android.App;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Lang;
using JetBrains.Annotations;
using MugenMvvmToolkit.Android.Binding.Infrastructure;
using MugenMvvmToolkit.Android.Binding.Interfaces;
using MugenMvvmToolkit.Android.Binding.Models;
using MugenMvvmToolkit.Android.Infrastructure;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Behaviors;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Binding.Modules;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;
using Object = Java.Lang.Object;

namespace MugenMvvmToolkit.Android.Binding.Modules
{
    public partial class PlatformDataBindingModule : DataBindingModule
    {
        #region Nested types

        private sealed class DateChangedListener : Object, DatePicker.IOnDateChangedListener
        {
            #region Fields

            private const string Key = "#DateChangedListener";
            public static readonly DateChangedListener Instance;

            #endregion

            #region Constructors

            static DateChangedListener()
            {
                Instance = new DateChangedListener();
            }

            public DateChangedListener(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
            {
            }

            private DateChangedListener()
            {
            }

            #endregion

            #region Implementation of IOnDateChangedListener

            public void OnDateChanged(DatePicker view, int year, int monthOfYear, int dayOfMonth)
            {
                Raise(view);
            }

            #endregion

            #region Methods

            public static IDisposable AddDateChangedListener(DatePicker datePicker, IEventListener listener)
            {
                return EventListenerList.GetOrAdd(datePicker, Key).AddWithUnsubscriber(listener);
            }

            private static void Raise(DatePicker picker)
            {
                EventListenerList.Raise(picker, Key, EventArgs.Empty);
            }

            #endregion
        }

        private sealed class ContentChangeListener : Object, ViewGroup.IOnHierarchyChangeListener
        {
            #region Fields

            public static readonly ContentChangeListener Instance;
            private static readonly EventHandler<ISourceValue, EventArgs> BindingContextChangedDelegate;

            #endregion

            #region Constructors

            static ContentChangeListener()
            {
                Instance = new ContentChangeListener();
                BindingContextChangedDelegate = BindingContextChanged;
            }

            public ContentChangeListener(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
            {
            }

            private ContentChangeListener()
            {
            }

            #endregion

            #region Implementation of IOnHierarchyChangeListener

            public void OnChildViewAdded(View parent, View child)
            {
                var viewGroup = (ViewGroup)parent;
                if (viewGroup.IndexOfChild(child) == 0)
                {
                    var underlyingView = GetUnderlyingView(child);
                    if (underlyingView != null)
                    {
                        var dataContext = BindingServiceProvider.ContextManager.GetBindingContext(underlyingView);
                        dataContext.ValueChanged += BindingContextChangedDelegate;
                        UpdataContext(viewGroup, underlyingView, dataContext);
                    }
                }
                ParentObserver.Raise(child);
                var childViewGroup = child as ViewGroup;
                if (childViewGroup != null && !childViewGroup.GetBindingMemberValue(AttachedMembers.ViewGroup.DisableHierarchyListener))
                    childViewGroup.SetOnHierarchyChangeListener(GlobalViewParentListener.Instance);
            }

            public void OnChildViewRemoved(View parent, View child)
            {
                var viewGroup = (ViewGroup)parent;
                if (viewGroup.ChildCount == 0 || viewGroup.GetChildAt(0) == child)
                {
                    var underlyingView = GetUnderlyingView(child);
                    if (underlyingView != null)
                        BindingServiceProvider.ContextManager.GetBindingContext(underlyingView).ValueChanged -= BindingContextChangedDelegate;
                    viewGroup.SetBindingMemberValue(AttachedMembers.ViewGroup.Content, RemoveViewValue);
                }
                ParentObserver.Raise(child);
            }

            #endregion

            #region Methods

            [CanBeNull]
            private static View GetUnderlyingView(View child)
            {
                if (IsNoSaveStateFrameLayout(child))
                {
                    var layout = (FrameLayout)child;
                    if (layout.ChildCount == 0)
                        return null;
                    return layout.GetChildAt(0);
                }
                return child;
            }

            private static View GetParent(View view)
            {
                var parent = view.Parent as View;
                if (IsNoSaveStateFrameLayout(parent))
                    return parent.Parent as View;
                return parent;
            }

            private static bool IsNoSaveStateFrameLayout(View view)
            {
                return view != null && view.Class.CanonicalName.SafeContains("NoSaveStateFrameLayout", StringComparison.OrdinalIgnoreCase);
            }

            private static void BindingContextChanged(ISourceValue value, EventArgs args)
            {
                var context = (IBindingContext)value;
                UpdataContext(null, context.Source as View, context);
            }

            private static void UpdataContext(View parent, View view, IBindingContext context)
            {
                if (view == null)
                    return;
                if (parent == null)
                    parent = GetParent(view);
                if (parent != null && !Equals(parent.DataContext(), context.Value))
                {
                    var viewGroup = parent as ViewGroup;
                    if (viewGroup != null)
                        viewGroup.SetBindingMemberValue(AttachedMembers.ViewGroup.Content,
                            new[] { context.Value, AddViewValue });
                }
            }

            #endregion
        }

        #endregion

        #region Fields

        private static Func<object, object> _rawAdapterGetter;
        private static Action<object, object> _rawAdapterSetter;
        private static readonly object AddViewValue;
        private static readonly object[] RemoveViewValue;
        private static readonly IntPtr TextViewSetTextMethodId;
        private static readonly Java.Lang.String EmptyString;
        private static readonly JValue[] NullJValue;
        private static readonly JValue[] EmptyStringJValue;

        #endregion

        #region Constructors

        static PlatformDataBindingModule()
        {
            AddViewValue = new object();
            RemoveViewValue = new object[] { null };
            NullJValue = new[] { new JValue(IntPtr.Zero) };
            EmptyString = new Java.Lang.String("");
            EmptyStringJValue = new[] { new JValue(EmptyString.Handle) };
            try
            {
                //we can get method from TextView because it is marked as final
                TextViewSetTextMethodId = JNIEnv.GetMethodID(Class.FromType(typeof(TextView)).Handle, "setText", "(Ljava/lang/CharSequence;)V");
            }
            catch
            {
                ;
            }
        }

        #endregion

        #region Methods

        private static void Register(IBindingMemberProvider memberProvider)
        {
            BindingServiceProvider.BindingMemberPriorities[AttachedMembers.Object.StableIdProvider] = BindingServiceProvider.TemplateMemberPriority - 1;
            RegisterMenuMembers(memberProvider);
            RegisterViewMembers(memberProvider);
            RegisterPreferenceMembers(memberProvider);
            BindingBuilderExtensions.RegisterDefaultBindingMember<Button>(nameof(Button.Click));
            BindingBuilderExtensions.RegisterDefaultBindingMember<TextView>(nameof(TextView.Text));
            BindingBuilderExtensions.RegisterDefaultBindingMember<CheckBox>(nameof(CheckBox.Checked));
            BindingBuilderExtensions.RegisterDefaultBindingMember<CompoundButton>(nameof(CompoundButton.Checked));
            BindingBuilderExtensions.RegisterDefaultBindingMember<SeekBar>(nameof(SeekBar.Progress));

            //Object
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.Object.StableIdProvider));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty<Object, ICollectionViewManager>(AttachedMembers.ViewGroup.CollectionViewManager.Path));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty<Object, IContentViewManager>(AttachedMembers.ViewGroup.ContentViewManager.Path));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(ItemsSourceGeneratorBase.MemberDescriptor,
                (o, args) =>
                {
                    IEnumerable itemsSource = null;
                    if (args.OldValue != null)
                    {
                        itemsSource = args.OldValue.ItemsSource;
                        args.OldValue.SetItemsSource(null);
                    }
                    if (args.NewValue != null)
                        args.NewValue.SetItemsSource(itemsSource);
                }));

            //Dialog
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.Dialog.Title,
                (dialog, args) => dialog.SetTitle(args.NewValue.ToStringSafe())));

            //Activity
            //to suppress message about parent
            memberProvider.Register(AttachedBindingMember.CreateMember<Activity, object>(AttachedMemberConstants.ParentExplicit, (info, activity) => null, null));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty<Activity, string>(nameof(Activity.Title),
                (activity, args) => activity.Title = args.NewValue, getDefaultValue: (activity, info) => activity.Title));
            memberProvider.Register(AttachedBindingMember.CreateMember<Activity, object>(AttachedMemberConstants.FindByNameMethod, ActivityFindByNameMember));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.Activity.ToastTemplateSelector));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.Activity.StartActivityDelegate));

            //RatingBar
            BindingBuilderExtensions.RegisterDefaultBindingMember<RatingBar>(nameof(RatingBar.Rating));
            memberProvider.Register(AttachedBindingMember
                .CreateMember<RatingBar, float>(nameof(RatingBar.Rating), (info, btn) => btn.Rating,
                    (info, btn, value) => btn.Rating = value, nameof(RatingBar.RatingBarChange)));

            //AdapterView            
            memberProvider.Register(AttachedBindingMember
                .CreateAutoProperty(AttachedMembers.AdapterView.DropDownItemTemplate, ViewGroupTemplateChanged));
            memberProvider.Register(AttachedBindingMember
                .CreateAutoProperty(AttachedMembers.AdapterView.DropDownItemTemplateSelector, ViewGroupTemplateChanged));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.AdapterView.SelectedItem, AdapterViewSelectedItemChanged,
                AdapterViewSelectedItemMemberAttached));
            var selectedItemPosMember = AttachedBindingMember.CreateAutoProperty(AttachedMembers.AdapterView.SelectedItemPosition,
                AdapterViewSelectedItemPositionChanged, AdapterViewSelectedItemPositionMemberAttached, (view, info) => view.SelectedItemPosition);
            memberProvider.Register(selectedItemPosMember);
            memberProvider.Register(typeof(AdapterView), "SelectedIndex", selectedItemPosMember, true);
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.AdapterView.ScrollToSelectedItem));

            //ViewGroup
            BindingBuilderExtensions.RegisterDefaultBindingMember(AttachedMembers.ViewGroup.ItemsSource);
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.ViewGroup.ItemsSource, ViewGroupItemsSourceChanged));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.ViewGroup.ItemTemplate, ViewGroupTemplateChanged));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.ViewGroup.ItemTemplateSelector, ViewGroupTemplateChanged));

            BindingBuilderExtensions.RegisterDefaultBindingMember(AttachedMembers.ViewGroup.Content.Override<FrameLayout>());
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.ViewGroup.Content, ContentMemberChanged, ContentMemberAttached));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.ViewGroup.ContentTemplate, ContentTemplateIdChanged));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.ViewGroup.ContentTemplateSelector, ContentTemplateSelectorChanged));

            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.ViewGroup.DisableHierarchyListener, (view, args) =>
            {
                view.SetOnHierarchyChangeListener(args.NewValue ? null : GlobalViewParentListener.Instance);
            }));

            //TabHost
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.TabHost.RestoreSelectedIndex));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.TabHost.SelectedItem, TabHostSelectedItemChanged));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.ViewGroup.ItemsSource.Override<TabHost>(), TabHostItemsSourceChanged));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.ViewGroup.ItemTemplate.Override<TabHost>(), TabHostTemplateChanged));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.ViewGroup.ItemTemplateSelector.Override<TabHost>(), TabHostTemplateChanged));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.ViewGroup.ContentTemplate.Override<TabHost>(), TabHostTemplateChanged));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.ViewGroup.ContentTemplateSelector.Override<TabHost>(), TabHostTemplateChanged));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.TabSpec.Title, (spec, args) => spec.SetIndicator(args.NewValue)));

            //TextView
            if (TextViewSetTextMethodId != IntPtr.Zero)
            {
                var fastTextMember = AttachedBindingMember.CreateMember<TextView, string>("TextEx", (info, view) => view.Text, (info, view, arg3) =>
                {
                    //Default Xamarin implementation creates and release new Java.Lang.String on every text change, can be replaced with direct method call
                    //                    Java.Lang.String @string = value != null ? new Java.Lang.String(value) : (Java.Lang.String)null;
                    //                    this.TextFormatted = (ICharSequence)@string;
                    //                    if (@string == null)
                    //                        return;
                    //                    @string.Dispose();

                    if (arg3 == null)
                        JNIEnv.CallVoidMethod(view.Handle, TextViewSetTextMethodId, NullJValue);
                    else if (arg3 == "")
                        JNIEnv.CallVoidMethod(view.Handle, TextViewSetTextMethodId, EmptyStringJValue);
                    else
                    {
                        var stringPtr = JNIEnv.NewString(arg3);
                        try
                        {
                            unsafe
                            {
                                JValue* ptr = stackalloc JValue[1];
                                *ptr = new JValue(stringPtr);
                                JNIEnv.CallVoidMethod(view.Handle, TextViewSetTextMethodId, ptr);
                            }
                        }
                        finally
                        {
                            JNIEnv.DeleteLocalRef(stringPtr);
                        }
                    }
                }, nameof(TextView.TextChanged));
                memberProvider.Register(fastTextMember);
                if (PlatformExtensions.EnableFastTextViewTextProperty)
                    memberProvider.Register(nameof(TextView.Text), fastTextMember);
            }

            //AutoCompleteTextView
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.AutoCompleteTextView.FilterText));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.AutoCompleteTextView.ItemTemplate, (view, args) => AutoCompleteTextViewTemplateChanged(view)));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.AutoCompleteTextView.ItemTemplateSelector, (view, args) => AutoCompleteTextViewTemplateChanged(view)));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.AutoCompleteTextView.ItemsSource, AutoCompleteTextViewItemsSourceChanged));

            //DatePicker
            BindingBuilderExtensions.RegisterDefaultBindingMember(AttachedMembers.DatePicker.SelectedDate);
            var selectedDateMember = AttachedBindingMember.CreateMember(AttachedMembers.DatePicker.SelectedDate,
                (info, picker) => picker.DateTime, (info, picker, value) => picker.DateTime = value,
                ObserveSelectedDate, SelectedDateMemberAttached);
            memberProvider.Register(selectedDateMember);
            memberProvider.Register("DateTime", selectedDateMember);

            //TimePicker
            BindingBuilderExtensions.RegisterDefaultBindingMember(AttachedMembers.TimePicker.SelectedTime);
            var selectedTimeMember = AttachedBindingMember.CreateMember(AttachedMembers.TimePicker.SelectedTime, GetTimePickerValue, SetTimePickerValue, nameof(TimePicker.TimeChanged));
            memberProvider.Register(selectedTimeMember);
            memberProvider.Register("Value", selectedTimeMember);

            //ImageView
            BindingBuilderExtensions.RegisterDefaultBindingMember(AttachedMembers.ImageView.ImageSource);
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.ImageView.ImageSource,
                (view, args) =>
                {
                    if (args.NewValue == null)
                    {
                        view.SetImageBitmap(null);
                        return;
                    }
                    var bitmap = args.NewValue as Bitmap;
                    if (bitmap != null)
                    {
                        view.SetImageBitmap(bitmap);
                        return;
                    }
                    var drawable = args.NewValue as Drawable;
                    if (drawable != null)
                    {
                        view.SetImageDrawable(drawable);
                        return;
                    }
                    var uri = args.NewValue as global::Android.Net.Uri;
                    if (uri != null)
                    {
                        view.SetImageURI(uri);
                        return;
                    }
                    view.SetImageResource((int)args.NewValue);
                }));

            //Toolbar
            if (PlatformExtensions.IsApiGreaterThanOrEqualTo21)
            {
                memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.Toolbar.IsActionBar, ToolbarIsActionBarChanged));
                memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.Toolbar.MenuTemplate, ToolbarMenuTemplateChanged));
            }
        }

        private static void AutoCompleteTextViewItemsSourceChanged(AutoCompleteTextView sender, AttachedMemberChangedEventArgs<IEnumerable> args)
        {
            var listAdapter = sender.Adapter as IItemsSourceAdapter;
            if (listAdapter == null)
            {
                listAdapter = ItemsSourceAdapter.Factory(sender, sender.Context, DataContext.Empty);
                sender.Adapter = listAdapter;
            }
            listAdapter.ItemsSource = args.NewValue;
        }

        private static void AutoCompleteTextViewTemplateChanged(AutoCompleteTextView sender)
        {
            var listAdapter = sender.Adapter as BaseAdapter;
            if (listAdapter != null)
                listAdapter.NotifyDataSetChanged();
        }

        internal static object GetAdapter(AdapterView item)
        {
            if (_rawAdapterGetter == null)
            {
                var property = GetRawAdapterProperty();
                if (property == null)
                    _rawAdapterGetter = o => null;
                else
                    _rawAdapterGetter = ServiceProvider.ReflectionManager.GetMemberGetter<object>(property);
            }
            return _rawAdapterGetter(item);
        }

        internal static void SetAdapter(AdapterView item, IAdapter adapter)
        {
            if (_rawAdapterSetter == null)
            {
                var property = GetRawAdapterProperty();
                if (property == null)
                    _rawAdapterSetter = (o, v) => { };
                else
                    _rawAdapterSetter = ServiceProvider.ReflectionManager.GetMemberSetter<object>(property);
            }
            _rawAdapterSetter(item, adapter);
        }

        private static PropertyInfo GetRawAdapterProperty()
        {
            var rawAdapterProp = typeof(AdapterView).GetPropertyEx("RawAdapter", MemberFlags.Instance | MemberFlags.NonPublic | MemberFlags.Public);
            if (rawAdapterProp == null)
                Tracer.Error("The AdapterView does not contain RawAdapter property");
            return rawAdapterProp;
        }

        private static void ToolbarMenuTemplateChanged(Toolbar view, AttachedMemberChangedEventArgs<int> args)
        {
            var activity = view.Context.GetActivity();
            if (activity != null)
                activity.MenuInflater.Inflate(args.NewValue, view.Menu, view);
        }

        private static void ToolbarIsActionBarChanged(Toolbar view, AttachedMemberChangedEventArgs<bool> args)
        {
            if (!args.NewValue)
                return;
            var activity = view.Context.GetActivity();
            if (activity != null)
                activity.SetActionBar(view);
        }

        private static object ActivityFindByNameMember(IBindingMemberInfo bindingMemberInfo, Activity target, object[] arg3)
        {
            return ViewFindByNameMember(bindingMemberInfo, target.FindViewById(global::Android.Resource.Id.Content), arg3);
        }

        #region TabHost

        private static void TabHostSelectedItemChanged(TabHost tabHost, AttachedMemberChangedEventArgs<object> arg)
        {
            var generator = tabHost.GetBindingMemberValue(AttachedMembers.ViewGroup.ItemsSourceGenerator) as IItemsSourceGeneratorEx;
            if (generator != null)
                generator.SelectedItem = arg.NewValue;
        }

        private static void TabHostTemplateChanged<T>(TabHost tabHost, AttachedMemberChangedEventArgs<T> args)
        {
            var generator = tabHost.GetBindingMemberValue(AttachedMembers.ViewGroup.ItemsSourceGenerator);
            if (generator != null)
                generator.Reset();
        }

        private static void TabHostItemsSourceChanged(TabHost tabHost, AttachedMemberChangedEventArgs<IEnumerable> arg)
        {
            var generator = tabHost.GetBindingMemberValue(AttachedMembers.ViewGroup.ItemsSourceGenerator);
            if (generator == null)
            {
                generator = new TabHostItemsSourceGenerator(tabHost);
                tabHost.SetBindingMemberValue(AttachedMembers.ViewGroup.ItemsSourceGenerator, generator);
            }
            generator.SetItemsSource(arg.NewValue);
        }

        #endregion

        #region AdapterView

        private static void AdapterViewSelectedItemPositionChanged(AdapterView sender, AttachedMemberChangedEventArgs<int> args)
        {
            if (sender.GetBindingMemberValue(AttachedMembers.AdapterView.ScrollToSelectedItem).GetValueOrDefault(true) || sender is Spinner)
                sender.SetSelection(args.NewValue);
            var adapter = GetAdapter(sender) as IItemsSourceAdapter;
            if (adapter != null)
                sender.SetBindingMemberValue(AttachedMembers.AdapterView.SelectedItem, adapter.GetRawItem(args.NewValue));
        }

        private static void AdapterViewSelectedItemChanged(AdapterView sender, AttachedMemberChangedEventArgs<object> args)
        {
            var adapter = GetAdapter(sender) as IItemsSourceAdapter;
            if (adapter != null)
                sender.SetBindingMemberValue(AttachedMembers.AdapterView.SelectedItemPosition, adapter.GetPosition(args.NewValue));
        }

        private static void AdapterViewSelectedItemMemberAttached(AdapterView adapterView, MemberAttachedEventArgs arg)
        {
            //to invoke the AdapterViewSelectedItemPositionMemberAttached method.
            int value;
            adapterView.TryGetBindingMemberValue(AttachedMembers.AdapterView.SelectedItemPosition, out value);
        }

        private static void AdapterViewSelectedItemPositionMemberAttached(AdapterView adapterView, MemberAttachedEventArgs arg)
        {
            if (adapterView is ListView)
                adapterView.ItemClick += (sender, args) => SetSelectedIndexAdapterView((AdapterView)sender, args.Position);
            else
            {
                adapterView.ItemSelected += (sender, args) => SetSelectedIndexAdapterView((AdapterView)sender, args.Position);
                adapterView.NothingSelected += (sender, args) => SetSelectedIndexAdapterView((AdapterView)sender, -1);
            }
        }

        private static void SetSelectedIndexAdapterView(AdapterView adapter, int index)
        {
            var oldValue = adapter.GetBindingMemberValue(AttachedMembers.AdapterView.ScrollToSelectedItem);
            if (oldValue != null && !oldValue.Value)
                adapter.SetBindingMemberValue(AttachedMembers.AdapterView.SelectedItemPosition, index);
            else
            {
                adapter.SetBindingMemberValue(AttachedMembers.AdapterView.ScrollToSelectedItem, false);
                adapter.SetBindingMemberValue(AttachedMembers.AdapterView.SelectedItemPosition, index);
                adapter.SetBindingMemberValue(AttachedMembers.AdapterView.ScrollToSelectedItem, oldValue);
            }
        }

        #endregion

        #region DatePicker

        private static IDisposable ObserveSelectedDate(IBindingMemberInfo bindingMemberInfo, DatePicker datePicker, IEventListener arg3)
        {
            return DateChangedListener.AddDateChangedListener(datePicker, arg3);
        }

        private static void SelectedDateMemberAttached(DatePicker picker, MemberAttachedEventArgs memberAttachedEventArgs)
        {
            picker.Init(picker.Year, picker.Month, picker.DayOfMonth, DateChangedListener.Instance);
        }

        #endregion

        #region TimePicker

        private static void SetTimePickerValue(IBindingMemberInfo bindingMemberInfo, TimePicker timePicker, TimeSpan value)
        {
            timePicker.CurrentHour = new Integer(value.Hours);
            timePicker.CurrentMinute = new Integer(value.Minutes);
        }

        private static TimeSpan GetTimePickerValue(IBindingMemberInfo bindingMemberInfo, TimePicker timePicker)
        {
            int currentHour = timePicker.CurrentHour.IntValue();
            int currentMinute = timePicker.CurrentMinute.IntValue();
            return new TimeSpan(currentHour, currentMinute, 0);
        }

        #endregion

        #region ViewGroup

        private static void ViewGroupTemplateChanged<T>(ViewGroup sender, AttachedMemberChangedEventArgs<T> args)
        {
            var container = sender as AdapterView;
            if (container == null)
            {
                var sourceGenerator = sender.GetBindingMemberValue(AttachedMembers.ViewGroup.ItemsSourceGenerator);
                if (sourceGenerator != null)
                    sourceGenerator.Reset();
                return;
            }
            var adapter = GetAdapter(container) as BaseAdapter;
            if (adapter != null)
                adapter.NotifyDataSetChanged();
        }

        private static void ViewGroupItemsSourceChanged(ViewGroup sender, AttachedMemberChangedEventArgs<IEnumerable> args)
        {
            var container = sender as AdapterView;
            if (container == null)
            {
                var sourceGenerator = sender.GetBindingMemberValue(AttachedMembers.ViewGroup.ItemsSourceGenerator);
                if (sourceGenerator == null)
                {
                    sourceGenerator = new ViewGroupItemsSourceGenerator(sender);
                    sender.SetBindingMemberValue(AttachedMembers.ViewGroup.ItemsSourceGenerator, sourceGenerator);
                }
                sourceGenerator.SetItemsSource(args.NewValue);
                return;
            }
            var adapter = GetAdapter(container) as IItemsSourceAdapter;
            if (adapter == null)
            {
                adapter = ItemsSourceAdapter.Factory(container, container.Context, DataContext.Empty);
                SetAdapter(container, adapter);
            }
            adapter.ItemsSource = args.NewValue;
        }

        private static void ContentMemberAttached(ViewGroup viewGroup, MemberAttachedEventArgs args)
        {
            viewGroup.SetBindingMemberValue(AttachedMembers.ViewGroup.DisableHierarchyListener, true);
            viewGroup.SetOnHierarchyChangeListener(ContentChangeListener.Instance);
        }

        private static void ContentTemplateSelectorChanged(ViewGroup sender,
            AttachedMemberChangedEventArgs<IDataTemplateSelector> args)
        {
            UpdateContent(sender, sender.GetBindingMemberValue(AttachedMembers.ViewGroup.Content), args.Args);
        }

        private static void ContentMemberChanged(ViewGroup sender, AttachedMemberChangedEventArgs<object> args)
        {
            UpdateContent(sender, args.NewValue, args.Args);
        }

        private static void ContentTemplateIdChanged(ViewGroup sender, AttachedMemberChangedEventArgs<int?> args)
        {
            UpdateContent(sender, sender.GetBindingMemberValue(AttachedMembers.ViewGroup.Content), args.Args);
        }

        private static void UpdateContent(ViewGroup sender, object newContent, object[] args)
        {
            if (newContent == null && args == RemoveViewValue)
                return;

            //NOTE cheking if it's a view group listener.
            if (args != null && args.Length == 2 && args[1] == AddViewValue)
                return;
            var templateId = sender.GetBindingMemberValue(AttachedMembers.ViewGroup.ContentTemplate);
            var templateSelector = sender.GetBindingMemberValue(AttachedMembers.ViewGroup.ContentTemplateSelector);
            newContent = PlatformExtensions.GetContentView(sender, sender.Context, newContent, templateId, templateSelector);
            var contentViewManager = sender.GetBindingMemberValue(AttachedMembers.ViewGroup.ContentViewManager);
            if (contentViewManager == null)
                PlatformExtensions.SetContentView(sender, newContent);
            else
                contentViewManager.SetContent(sender, newContent);
        }

        #endregion

        #endregion

        #region Overrides of DataBindingModule

        protected override void OnLoaded(IModuleContext context)
        {
            Register(BindingServiceProvider.MemberProvider);
            BindingServiceProvider.BindingProvider.DefaultBehaviors.Add(DisableEqualityCheckingBehavior.TargetTrueNotTwoWay);
            base.OnLoaded(context);
        }

        protected override IBindingErrorProvider GetBindingErrorProvider(IModuleContext context)
        {
            return new BindingErrorProvider();
        }

        #endregion
    }
}
