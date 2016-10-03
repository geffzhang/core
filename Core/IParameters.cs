﻿using System;
using System.Collections.Generic;

namespace Greatbone.Core
{
    /// <summary>
    /// A data outputing destination.
    /// </summary>
    public interface IParameters : ISink<IParameters>
    {
        IParameters Put(bool value);

        IParameters Put(short value);

        IParameters Put(int value);

        IParameters Put(string value);

        IParameters Put<T>(List<T> value, int x) where T : IPersist;
    }
}