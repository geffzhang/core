﻿using System.Collections.Generic;

namespace Greatbone.Core
{
    public interface IDataInput
    {
        bool GotStart();

        bool GotEnd();

        bool Got(string name, out int value);

        bool Got(string name, out decimal value);

        bool Got(string name, out string value);

        bool Got<T>(string name, out List<T> value) where T : IData;
    }
}