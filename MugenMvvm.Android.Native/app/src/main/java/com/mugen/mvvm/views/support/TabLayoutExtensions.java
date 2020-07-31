package com.mugen.mvvm.views.support;

import android.view.View;
import androidx.viewpager.widget.ViewPager;
import com.google.android.material.tabs.TabLayout;
import com.mugen.mvvm.MugenNativeService;
import com.mugen.mvvm.views.ViewExtensions;
import com.mugen.mvvm.views.ViewGroupExtensions;

public abstract class TabLayoutExtensions extends ViewExtensions {
    public static final int ItemsSourceProviderType = ViewGroupExtensions.ContentRawType;

    public static boolean isSupported(View view) {
        return MugenNativeService.isMaterialSupported() && view instanceof TabLayout;
    }

    public static Object newTab(View view) {
        return ((TabLayout) view).newTab();
    }

    public static Object getTabAt(View view, int index) {
        return ((TabLayout) view).getTabAt(index);
    }

    public static void selectTab(View view, Object tab) {
        ((TabLayout) view).selectTab((TabLayout.Tab) tab);
    }

    public static void addTab(View view, Object tab, boolean setSelected) {
        ((TabLayout) view).addTab((TabLayout.Tab) tab, setSelected);
    }

    public static void addTab(View view, Object tab, int position, boolean setSelected) {
        ((TabLayout) view).addTab((TabLayout.Tab) tab, position, setSelected);
    }

    public static void removeTab(View view, Object tab) {
        ((TabLayout) view).removeTab((TabLayout.Tab) tab);
    }

    public static void removeTab(View view, int position) {
        ((TabLayout) view).removeTabAt(position);
    }

    public static void clearTabs(View view) {
        ((TabLayout) view).removeAllTabs();
    }

    public static void setupWithViewPager(View view, View viewPager) {
        ((TabLayout) view).setupWithViewPager((ViewPager) viewPager);
    }

    public static int getSelectedTabPosition(View view) {
        return ((TabLayout) view).getSelectedTabPosition();
    }

    public static void setSelectedTabPosition(View view, int position) {
        TabLayout tabLayout = (TabLayout) view;
        tabLayout.selectTab(tabLayout.getTabAt(position));
    }

    public static int getTabCount(View view) {
        return ((TabLayout) view).getTabCount();
    }

    public static int getTabGravity(View view) {
        return ((TabLayout) view).getTabGravity();
    }

    public static int getTabIndicatorGravity(View view) {
        return ((TabLayout) view).getTabIndicatorGravity();
    }

    public static int getTabMode(View view) {
        return ((TabLayout) view).getTabMode();
    }

    public static void setInlineLabel(View view, boolean inline) {
        ((TabLayout) view).setInlineLabel(inline);
    }

    public static void setSelectedTabIndicatorColor(View view, int color) {
        ((TabLayout) view).setSelectedTabIndicatorColor(color);
    }

    public static void setSelectedTabIndicatorGravity(View view, int gravity) {
        ((TabLayout) view).setSelectedTabIndicatorGravity(gravity);
    }

    public static void setTabGravity(View view, int gravity) {
        ((TabLayout) view).setTabGravity(gravity);
    }

    public static void setTabIndicatorFullWidth(View view, boolean fullWidth) {
        ((TabLayout) view).setTabIndicatorFullWidth(fullWidth);
    }

    public static void setTabMode(View view, int mode) {
        ((TabLayout) view).setTabMode(mode);
    }

    public static void setTabTextColors(View view, int normalColor, int selectedColor) {
        ((TabLayout) view).setTabTextColors(normalColor, selectedColor);
    }
}