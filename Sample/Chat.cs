using System;
using Greatbone.Core;

namespace Greatbone.Sample
{
    ///
    ///
    public class Chat : IData
    {
        public static readonly Chat Empty = new Chat();


        internal string shopid;

        internal string wx;

        internal DateTime quested;

        internal ChatMsg[] msgs;


        public void Read(IDataInput i, int proj = 0x00ff)
        {
            i.Get(nameof(shopid), ref shopid);
            i.Get(nameof(wx), ref wx);
            i.Get(nameof(quested), ref quested);
            i.Get(nameof(msgs), ref msgs);
        }

        public const int NUM = 6;

        public void Write<R>(IDataOutput<R> o, int proj = 0x00ff) where R : IDataOutput<R>
        {
            if (msgs != null && msgs.Length > 0)
            {
                int start = msgs.Length - NUM;
                if (start < 0) start = 0;
                for (int i = start; i < msgs.Length; i++)
                {
                    ChatMsg msg = msgs[i];
                    o.Put(nameof(msg.name), msg.text, msg.name);
                }
            }
        }
    }

    public struct ChatMsg : IData
    {
        internal string name;

        internal string text;

        public void Read(IDataInput i, int proj = 0x00ff)
        {
            i.Get(nameof(name), ref name);
            i.Get(nameof(text), ref text);
        }

        public void Write<R>(IDataOutput<R> o, int proj = 0x00ff) where R : IDataOutput<R>
        {
            o.Put(nameof(name), name);
            o.Put(nameof(text), text);
        }
    }
}