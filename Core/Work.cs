﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading.Tasks;

namespace Greatbone.Core
{
    /// <summary>
    /// A work is a virtual web folder that contains a single or collection of resources along with operations on it or them.
    /// </summary>
    /// <remarks>A work can contain child/sub works.</remarks>
    public abstract class Work : Nodule
    {
        internal static readonly AuthorizeException AuthorizeEx = new AuthorizeException();

        // max nesting levels
        const int MaxNesting = 6;

        // underlying file directory name
        const string _VAR_ = "VAR";

        // state-passing
        readonly WorkContext ctx;

        readonly TypeInfo typeinfo;

        // declared actions 
        readonly Roll<ActionInfo> actions;

        // the default action, can be null
        readonly ActionInfo @default;

        // actions with Ui attribute
        readonly ActionInfo[] uiActions;

        readonly int buttons;

        // subworks, if any
        internal Roll<Work> works;

        // variable-key subwork, if any
        internal Work varWork;

        // to obtain a string key from a data object.
        protected Work(WorkContext wc) : base(wc.Name, null)
        {
            this.ctx = wc;

            // gather actions
            actions = new Roll<ActionInfo>(32);
            Type typ = GetType();
            typeinfo = typ.GetTypeInfo();

            foreach (MethodInfo mi in typ.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                // verify the return type
                Type ret = mi.ReturnType;
                bool async;
                if (ret == typeof(Task)) async = true;
                else if (ret == typeof(void)) async = false;
                else continue;

                ParameterInfo[] pis = mi.GetParameters();
                ActionInfo ai;
                if (pis.Length == 1 && pis[0].ParameterType == typeof(ActionContext))
                {
                    ai = new ActionInfo(this, mi, async, false);
                }
                else if (pis.Length == 2 && pis[0].ParameterType == typeof(ActionContext) && pis[1].ParameterType == typeof(int))
                {
                    LimitAttribute limit = (LimitAttribute) pis[1].GetCustomAttribute(typeof(LimitAttribute));
                    ai = new ActionInfo(this, mi, async, true, limit?.Value ?? 20);
                }
                else continue;

                actions.Add(ai);
                if (ai.Name.Equals("default"))
                {
                    @default = ai;
                }
            }

            // gather ui actions
            int btns = 0;
            List<ActionInfo> uias = null;
            for (int i = 0; i < actions.Count; i++)
            {
                ActionInfo ai = actions[i];
                if (ai.HasUi)
                {
                    if (uias == null) uias = new List<ActionInfo>();
                    uias.Add(ai);
                    if (ai.Ui.IsButton) btns++;
                }
            }
            uiActions = uias?.ToArray();
            buttons = btns;
        }

        /// <summary>
        /// Create a fixed-key subwork.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="attach">an attachment object</param>
        /// <typeparam name="W"></typeparam>
        /// <returns>The newly created and subwork instance.</returns>
        /// <exception cref="ServiceException">Thrown if error</exception>
        public W Create<W>(string name, object attach = null) where W : Work
        {
            if (Level >= MaxNesting)
            {
                throw new ServiceException("allowed work nesting " + MaxNesting);
            }

            if (works == null)
            {
                works = new Roll<Work>(16);
            }
            // create instance by reflection
            Type typ = typeof(W);
            ConstructorInfo ci = typ.GetConstructor(new[] {typeof(WorkContext)});
            if (ci == null)
            {
                throw new ServiceException(typ + "no valid constructor");
            }
            WorkContext wc = new WorkContext(name)
            {
                Attach = attach,
                Parent = this,
                Level = Level + 1,
                Directory = (Parent == null) ? name : Path.Combine(Parent.Directory, name),
                Service = Service
            };
            W work = (W) ci.Invoke(new object[] {wc});
            Works.Add(work);

            return work;
        }

        /// <summary>
        /// Create a variable-key subwork.
        /// </summary>
        /// <param name="keyer"></param>
        /// <param name="attach">an attachment object</param>
        /// <typeparam name="W"></typeparam>
        /// <typeparam name="K"></typeparam>
        /// <returns>The newly created subwork instance.</returns>
        /// <exception cref="ServiceException">Thrown if error</exception>
        public W CreateVar<W, K>(Func<IData, K> keyer = null, object attach = null) where W : Work where K : IComparable<K>, IEquatable<K>
        {
            if (Level >= MaxNesting)
            {
                throw new ServiceException("allowed work nesting " + MaxNesting);
            }

            // create instance
            Type typ = typeof(W);
            ConstructorInfo ci = typ.GetConstructor(new[] {typeof(WorkContext)});
            if (ci == null)
            {
                throw new ServiceException(typ + " no valid constructor");
            }
            WorkContext wc = new WorkContext(_VAR_)
            {
                Keyer = keyer,
                Attach = attach,
                Parent = this,
                IsVar = true,
                Level = Level + 1,
                Directory = (Parent == null) ? _VAR_ : Path.Combine(Parent.Directory, _VAR_),
                Service = Service
            };
            W work = (W) ci.Invoke(new object[] {wc});
            varWork = work;
            return work;
        }

        internal Criteria CreateCriteria(string method)
        {
            MethodInfo mi = typeinfo.GetMethod(method);
            return (Criteria) mi?.CreateDelegate(typeof(Criteria), this);
        }

        public Roll<ActionInfo> Actions => actions;

        public ActionInfo[] UiActions => uiActions;

        public int Buttons => buttons;

        public bool HasDefault => @default != null;

        public Roll<Work> Works => works;

        public Work VarWork => varWork;

        public string Directory => ctx.Directory;

        public Work Parent => ctx.Parent;

        public bool IsVar => ctx.IsVar;

        public int Level => ctx.Level;

        public override Service Service => ctx.Service;

        public bool HasKeyer => ctx.Keyer != null;

        public string ObtainVarKey(IData obj)
        {
            Delegate keyer = ctx.Keyer;
            if (keyer is Func<IData, string>)
            {
                return ((Func<IData, string>) keyer)(obj);
            }
            return null;
        }

        public void OutputVarKey(IData obj, DynamicContent @out)
        {
            Delegate keyer = ctx.Keyer;
            if (keyer is Func<IData, string>)
            {
                @out.Add(((Func<IData, string>) keyer)(obj));
            }
            else if (keyer is Func<IData, long>)
            {
                @out.Add(((Func<IData, long>) keyer)(obj));
            }
            else if (keyer is Func<IData, int>)
            {
                @out.Add(((Func<IData, int>) keyer)(obj));
            }
        }

        internal void Describe(XmlContent cont)
        {
            cont.ELEM(Name,
                delegate
                {
                    for (int i = 0; i < Actions.Count; i++)
                    {
                        ActionInfo act = Actions[i];
                        cont.Put(act.Name, "");
                    }
                },
                delegate
                {
                    if (works != null)
                    {
                        for (int i = 0; i < works.Count; i++)
                        {
                            Work wrk = works[i];
                            wrk.Describe(cont);
                        }
                    }
                    varWork?.Describe(cont);
                });
        }

        public bool IsInstanceOf(Type typ) => GetType() == typ || typeinfo.IsSubclassOf(typ);

        public ActionInfo GetAction(string method)
        {
            if (string.IsNullOrEmpty(method))
            {
                return @default;
            }
            return actions[method];
        }

        internal Work Resolve(ref string relative, ActionContext ac)
        {
            int slash = relative.IndexOf('/');
            if (slash == -1)
            {
                return this;
            }

            // seek subworks/varwork
            //
            string key = relative.Substring(0, slash);
            relative = relative.Substring(slash + 1); // adjust relative
            Work work;
            if (works != null && works.TryGet(key, out work)) // if child
            {
                ac.Chain(key, work);
                return work.Resolve(ref relative, ac);
            }
            if (varWork != null) // if variable-key sub
            {
                IData prin = ac.Principal;
                if (key.Length == 0 && varWork.HasKeyer) // resolve shortcut
                {
                    if (prin == null) throw AuthorizeEx;
                    if ((key = varWork.ObtainVarKey(prin)) == null)
                    {
                        throw AuthorizeEx;
                    }
                }
                ac.Chain(key, varWork);
                return varWork.Resolve(ref relative, ac);
            }
            return null;
        }

        /// <summary>
        /// To hndle a request/response context. authorize, before/after filters
        /// </summary>
        /// <param name="rsc">the resource path</param>
        /// <param name="ac">ActionContext</param>
        /// <exception cref="AuthorizeException">Thrown when authorization is required and false is returned by checking</exception>
        /// <seealso cref="AuthorizeAttribute.Check"/>
        internal async Task HandleAsync(string rsc, ActionContext ac)
        {
            ac.Work = this;

            // authorize and filters
            if (!DoAuthorize(ac)) throw AuthorizeEx;
            Before?.Do(ac);
            if (BeforeAsync != null) await BeforeAsync.DoAsync(ac);

            int dot = rsc.LastIndexOf('.');
            if (dot != -1) // file
            {
                // try in cache 
                if (!Service.TryGiveFromCache(ac))
                {
                    DoFile(rsc, rsc.Substring(dot), ac);
                    Service.Cache(ac); // try cache it
                }
            }
            else // action
            {
                string name = rsc;
                int subscpt = 0;
                int dash = rsc.LastIndexOf('-');
                if (dash != -1)
                {
                    name = rsc.Substring(0, dash);
                    ac.Subscript = subscpt = rsc.Substring(dash + 1).ToInt();
                }
                ActionInfo ai = string.IsNullOrEmpty(name) ? @default : GetAction(name);
                if (ai == null)
                {
                    ac.Give(404); // not found
                    return;
                }

                ac.Doer = ai;

                // action's authorization check
                if (!ai.DoAuthorize(ac)) throw AuthorizeEx;

                // action's before filtering
                ai.Before?.Do(ac);
                if (ai.BeforeAsync != null) await ai.BeforeAsync.DoAsync(ac);

                // try in cache
                if (!Service.TryGiveFromCache(ac))
                {
                    // method invocation
                    if (ai.IsAsync)
                    {
                        await ai.DoAsync(ac, subscpt); // invoke action method
                    }
                    else
                    {
                        ai.Do(ac, subscpt);
                    }
                    Service.Cache(ac); // try cache it
                }

                // action's after filtering
                ai.After?.Do(ac);
                if (ai.AfterAsync != null) await ai.AfterAsync.DoAsync(ac);

                ac.Doer = null;
            }

            After?.Do(ac);
            if (AfterAsync != null) await AfterAsync.DoAsync(ac);

            ac.Work = null;
        }

        public void DoFile(string filename, string ext, ActionContext ac)
        {
            if (filename.StartsWith("$")) // private resource
            {
                ac.Give(403); // forbidden
                return;
            }

            string ctyp;
            if (!StaticContent.TryGetType(ext, out ctyp))
            {
                ac.Give(415); // unsupported media type
                return;
            }

            string path = Path.Combine(Directory, filename);
            if (!File.Exists(path))
            {
                ac.Give(404); // not found
                return;
            }

            DateTime modified = File.GetLastWriteTime(path);

            // load file content
            byte[] bytes;
            bool gzip = false;
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                int len = (int) fs.Length;
                if (len > 4024)
                {
                    var ms = new MemoryStream(len);
                    using (GZipStream gzs = new GZipStream(ms, CompressionMode.Compress))
                    {
                        fs.CopyTo(gzs);
                    }
                    bytes = ms.ToArray();
                    gzip = true;
                }
                else
                {
                    bytes = new byte[len];
                    fs.Read(bytes, 0, len);
                }
            }

            StaticContent cont = new StaticContent(bytes, bytes.Length)
            {
                Name = filename,
                Type = ctyp,
                Modified = modified,
                GZip = gzip
            };
            ac.Give(200, cont, pub: true, maxage: 60 * 15);
        }
    }
}