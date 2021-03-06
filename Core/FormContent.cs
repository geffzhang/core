﻿using System;
using System.Collections.Generic;

namespace Greatbone.Core
{
    /// <summary>
    /// To generate a urlencoded byte or char string.
    /// </summary>
    public class FormContent : DynamicContent, IDataOutput<FormContent>
    {
        // hexidecimal characters
        protected static readonly char[] HEX =
        {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'
        };

        int ordinal = -1;

        public FormContent(bool octet, int capacity = 4092) : base(octet, capacity)
        {
        }

        public override string Type => "application/x-www-form-urlencoded";

        void AddEsc(string v)
        {
            if (v == null) return;

            for (int i = 0; i < v.Length; i++)
            {
                char c = v[i];
                if (c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z' || c >= '0' && c <= '9') // alphabetic and decimal digits
                {
                    Add(c);
                }
                else if (c == '.' || c == '-' || c == '*' || c == '_')
                {
                    Add(c);
                }
                else if (c == ' ')
                {
                    Add('+');
                }
                else
                {
                    if (c < 0x80)
                    {
                        // have at most seven bits
                        AddEscByte((byte) c);
                    }
                    else if (c < 0x800)
                    {
                        // 2 char, 11 bits
                        AddEscByte((byte) (0xc0 | (c >> 6)));
                        AddEscByte((byte) (0x80 | (c & 0x3f)));
                    }
                    else
                    {
                        // 3 char, 16 bits
                        AddEscByte((byte) (0xe0 | ((c >> 12))));
                        AddEscByte((byte) (0x80 | ((c >> 6) & 0x3f)));
                        AddEscByte((byte) (0x80 | (c & 0x3f)));
                    }
                }
            }
        }

        void AddEscByte(byte b)
        {
            Add('%');
            Add(HEX[(b >> 4) & 0x0f]);
            Add(HEX[b & 0x0f]);
        }

        //
        // SINK
        //

        public FormContent PutNull(string name)
        {
            return this;
        }

        public FormContent Put(string name, JNumber v)
        {
            return this;
        }

        public FormContent Put(string name, IDataInput v)
        {
            return this;
        }

        public FormContent PutRaw(string name, string raw)
        {
            return this;
        }

        public void Group(string label)
        {
        }

        public void UnGroup()
        {
        }

        public FormContent Put(string name, bool v, string Label = null, Func<bool, string> Opt = null)
        {
            ordinal++;

            if (ordinal > 0)
            {
                Add('&');
            }
            Add(name);
            Add('=');
            Add(v ? "true" : "false");
            return this;
        }

        public FormContent Put(string name, short v, string Label = null, IOptable<short> opt = null)
        {
            ordinal++;

            if (ordinal > 0)
            {
                Add('&');
            }
            Add(name);
            Add('=');
            Add(v);
            return this;
        }

        public FormContent Put(string name, int v, string Label = null, IOptable<int> Opt = null)
        {
            ordinal++;

            if (ordinal > 0)
            {
                Add('&');
            }
            Add(name);
            Add('=');
            Add(v);
            return this;
        }

        public FormContent Put(string name, long v, string Label = null, IOptable<long> opt = null)
        {
            ordinal++;

            if (ordinal > 0)
            {
                Add('&');
            }
            Add(name);
            Add('=');
            Add(v);
            return this;
        }

        public FormContent Put(string name, double v, string Label = null)
        {
            ordinal++;

            if (ordinal > 0)
            {
                Add('&');
            }
            Add(name);
            Add('=');
            Add(v);
            return this;
        }

        public FormContent Put(string name, decimal v, string Label = null, char format = '\0')
        {
            ordinal++;

            if (ordinal > 0)
            {
                Add('&');
            }
            Add(name);
            Add('=');
            Add(v);
            return this;
        }

        public FormContent Put(string name, DateTime v, string Label = null)
        {
            ordinal++;

            if (ordinal > 0)
            {
                Add('&');
            }
            Add(name);
            Add('=');
            Add(v);
            return this;
        }

        public FormContent Put(string name, string v, string Label = null, IOptable<string> Opt = null)
        {
            ordinal++;

            if (ordinal > 0)
            {
                Add('&');
            }
            Add(name);
            Add('=');
            AddEsc(v);
            return this;
        }

        public virtual FormContent Put(string name, ArraySegment<byte> v, string Label = null)
        {
            return this; // ignore ir
        }

        public FormContent Put(string name, short[] v, string Label = null, IOptable<short> opt = null)
        {
            ordinal++;

            if (ordinal > 0)
            {
                Add('&');
            }
            Add(name);
            Add('=');
            for (int i = 0; i < v.Length; i++)
            {
                if (i > 0) Add(',');
                Add(v[i]);
            }
            return this;
        }

        public FormContent Put(string name, int[] v, string Label = null, IOptable<int> Opt = null)
        {
            ordinal++;

            if (ordinal > 0)
            {
                Add('&');
            }
            Add(name);
            Add('=');
            for (int i = 0; i < v.Length; i++)
            {
                if (i > 0) Add(',');
                Add(v[i]);
            }
            return this;
        }

        public FormContent Put(string name, long[] v, string Label = null, IOptable<long> Opt = null)
        {
            ordinal++;

            if (ordinal > 0)
            {
                Add('&');
            }
            Add(name);
            Add('=');
            for (int i = 0; i < v.Length; i++)
            {
                if (i > 0) Add(',');
                Add(v[i]);
            }
            return this;
        }

        public FormContent Put(string name, string[] v, string Label = null, IOptable<string> Opt = null)
        {
            ordinal++;

            if (ordinal > 0)
            {
                Add('&');
            }
            Add(name);
            Add('=');
            for (int i = 0; i < v.Length; i++)
            {
                if (i > 0) Add(',');
                Add(v[i]);
            }
            return this;
        }

        public FormContent Put(string name, Dictionary<string, string> v, string Label = null)
        {
            throw new NotImplementedException();
        }

        public FormContent Put(string name, IData v, int proj = 0x00ff, string Label = null)
        {
            return this;
        }

        public FormContent Put<D>(string name, D[] v, int proj = 0x00ff, string Label = null) where D : IData
        {
            return this;
        }
    }
}