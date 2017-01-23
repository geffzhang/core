using System;
using System.Collections.Generic;
using NpgsqlTypes;

namespace Greatbone.Core
{
    ///
    /// A JSON array model.
    ///
    public class JArr : IModel, ISourceSet
    {
        JMem[] elements;

        int count;

        int current;

        internal JArr(int capacity = 16)
        {
            elements = new JMem[capacity];
            count = 0;
            current = -1;
        }

        public JMem this[int index] => elements[index];

        public int Count => count;

        internal void Add(JMem elem)
        {
            int len = elements.Length;
            if (count >= len)
            {
                JMem[] alloc = new JMem[len * 4];
                Array.Copy(elements, 0, alloc, 0, len);
                elements = alloc;
            }
            elements[count++] = elem;
        }

        public void Dump<R>(ISink<R> snk) where R : ISink<R>
        {
            for (int i = 0; i < count; i++)
            {
                JMem elem = elements[i];
                JType typ = elem.type;
                if (typ == JType.Array)
                {
                    snk.Put(null, (JArr)elem);
                }
                else if (typ == JType.Object)
                {
                    snk.Put(null, (JObj)elem);
                }
                else if (typ == JType.String)
                {
                    snk.Put(null, (string)elem);
                }
                else if (typ == JType.Number)
                {
                    snk.Put(null, (JNumber)elem);
                }
                else if (typ == JType.True)
                {
                    snk.Put(null, true);
                }
                else if (typ == JType.False)
                {
                    snk.Put(null, false);
                }
                else if (typ == JType.Null)
                {
                    snk.PutNull(null);
                }
            }
        }

        public IContent Dump()
        {
            JsonContent cont = new JsonContent();
            Dump(cont);
            return cont;
        }

        public bool Get(string name, ref bool v)
        {
            JObj jobj = elements[current];
            return jobj != null && jobj.Get(name, ref v);
        }

        public bool Get(string name, ref short v)
        {
            JObj jobj = elements[current];
            return jobj != null && jobj.Get(name, ref v);
        }

        public bool Get(string name, ref int v)
        {
            JObj jobj = elements[current];
            return jobj != null && jobj.Get(name, ref v);
        }

        public bool Get(string name, ref long v)
        {
            JObj jobj = elements[current];
            return jobj != null && jobj.Get(name, ref v);
        }

        public bool Get(string name, ref double v)
        {
            JObj jobj = elements[current];
            return jobj != null && jobj.Get(name, ref v);
        }

        public bool Get(string name, ref decimal v)
        {
            JObj jobj = elements[current];
            return jobj != null && jobj.Get(name, ref v);
        }

        public bool Get(string name, ref DateTime v)
        {
            JObj jobj = elements[current];
            return jobj != null && jobj.Get(name, ref v);
        }

        public bool Get(string name, ref NpgsqlPoint v)
        {
            JObj jobj = elements[current];
            return jobj != null && jobj.Get(name, ref v);
        }

        public bool Get(string name, ref char[] v)
        {
            JObj jobj = elements[current];
            return jobj != null && jobj.Get(name, ref v);
        }

        public bool Get(string name, ref string v)
        {
            JObj jobj = elements[current];
            return jobj != null && jobj.Get(name, ref v);
        }

        public bool Get(string name, ref byte[] v)
        {
            JObj jobj = elements[current];
            return jobj != null && jobj.Get(name, ref v);
        }

        public bool Get(string name, ref ArraySegment<byte> v)
        {
            JObj jobj = elements[current];
            return jobj != null && jobj.Get(name, ref v);
        }

        public bool Get<D>(string name, ref D v, byte flags = 0) where D : IData, new()
        {
            JObj jobj = elements[current];
            return jobj != null && jobj.Get(name, ref v);
        }

        public bool Get(string name, ref JObj v)
        {
            JObj jobj = elements[current];
            return jobj != null && jobj.Get(name, ref v);
        }

        public bool Get(string name, ref JArr v)
        {
            JObj jobj = elements[current];
            return jobj != null && jobj.Get(name, ref v);
        }

        public bool Get(string name, ref short[] v)
        {
            JObj jobj = elements[current];
            return jobj != null && jobj.Get(name, ref v);
        }

        public bool Get(string name, ref int[] v)
        {
            JObj jobj = elements[current];
            return jobj != null && jobj.Get(name, ref v);
        }

        public bool Get(string name, ref long[] v)
        {
            JObj jobj = elements[current];
            return jobj != null && jobj.Get(name, ref v);
        }

        public bool Get(string name, ref string[] v)
        {
            JObj jobj = elements[current];
            return jobj != null && jobj.Get(name, ref v);
        }

        public bool Get<D>(string name, ref D[] v, byte flags = 0) where D : IData, new()
        {
            JObj jobj = elements[current];
            return jobj != null && jobj.Get(name, ref v);
        }

        public bool Get<D>(string name, ref List<D> v, byte flags = 0) where D : IData, new()
        {
            JObj jobj = elements[current];
            return jobj != null && jobj.Get(name, ref v);
        }

        public bool Next()
        {
            return ++current < count;
        }

        public override string ToString()
        {
            JsonContent cont = new JsonContent(false, true);
            cont.Add('[');
            Dump(cont);
            cont.Add(']');
            string str = cont.ToString();
            BufferUtility.Return(cont);
            return str;
        }

        public D ToUn<D>(byte flags = 0) where D : IData, new()
        {
            D dat = new D();
            dat.Load(this, flags);
            return dat;
        }

        public D[] ToArray<D>(byte flags = 0) where D : IData, new()
        {
            D[] arr = new D[count];
            for (int i = 0; i < arr.Length; i++)
            {
                D obj = new D();
                obj.Load((JObj)elements[i], flags);
                arr[i] = obj;
            }
            return arr;
        }

        public List<D> ToList<D>(byte flags = 0) where D : IData, new()
        {
            List<D> lst = new List<D>(count + 8);
            for (int i = 0; i < count; i++)
            {
                D obj = new D();
                obj.Load((JObj)elements[i], flags);
                lst.Add(obj);
            }
            return lst;
        }
    }
}