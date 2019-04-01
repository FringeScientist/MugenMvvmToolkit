﻿namespace MugenMvvm.Delegates
{
    public delegate TValue UpdateValueDelegate<in TItem, in TNewValue, TValue, in TState1, in TState2>(TItem item, TNewValue addValue, TValue currentValue, TState1 state1, TState2 state2);
}