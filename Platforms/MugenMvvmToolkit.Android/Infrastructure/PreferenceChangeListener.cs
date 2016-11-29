using System;
using Android.Content;
using Android.Preferences;
using Android.Runtime;
using MugenMvvmToolkit.Android.Binding;
using MugenMvvmToolkit.Binding;
using Object = Java.Lang.Object;

namespace MugenMvvmToolkit.Android.Infrastructure
{
    public sealed class PreferenceChangeListener : Object, ISharedPreferencesOnSharedPreferenceChangeListener
    {
        #region Fields

        private readonly PreferenceManager _preferenceManager;
        internal bool State;

        #endregion

        #region Constructors

        public PreferenceChangeListener(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
        {
        }

        public PreferenceChangeListener(PreferenceManager preferenceManager)
        {
            _preferenceManager = preferenceManager;
        }

        #endregion

        #region Implementation of ISharedPreferencesOnSharedPreferenceChangeListener

        public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
        {
            if (_preferenceManager == null)
            {
                sharedPreferences.UnregisterOnSharedPreferenceChangeListener(this);
                return;
            }
            _preferenceManager.FindPreference(key)?.TryRaiseAttachedEvent(AttachedMembers.Preference.ValueChangedEvent);
        }

        #endregion        
    }
}