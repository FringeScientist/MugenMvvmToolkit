package com.mugen.mvvm.interfaces.views;

import android.view.Menu;

import androidx.annotation.NonNull;

public interface IViewMenuManager {
    boolean isMenuSupported(@NonNull Object view);

    @NonNull
    Menu getMenu(@NonNull Object view);
}