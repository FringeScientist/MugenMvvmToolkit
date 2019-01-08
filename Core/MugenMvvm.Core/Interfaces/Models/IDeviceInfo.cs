﻿using MugenMvvm.Enums;

namespace MugenMvvm.Interfaces.Models
{
    public interface IDeviceInfo
    {
        PlatformType Platform { get; }

        PlatformIdiom Idiom { get; }

        string RawVersion { get; }
    }
}