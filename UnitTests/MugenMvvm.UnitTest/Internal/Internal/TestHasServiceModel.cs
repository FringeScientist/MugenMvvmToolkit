﻿using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTest.Internal.Internal
{
    public class TestHasServiceModel<T> : IHasService<T> where T : class
    {
        #region Properties

        public T Service { get; set; } = default!;

        public T? ServiceOptional { get; set; }

        #endregion
    }
}