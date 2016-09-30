﻿using Greatbone.Core;
using System.Net;
using System.Collections.Generic;

namespace Greatbone.Sample
{
    public class PostHub : WebHub
    {
        public PostHub(WebConfig cfg) : base(cfg)
        {
            SetVarHub<PostVarHub>(false);
        }

        public void top(WebContext wc)
        {
            int page = 0;
            if (wc.GetParam("page", ref page))
            {
                wc.Response.StatusCode = 400;
                return;
            }

            using (var dc = Service.NewDbContext())
            {
                if (dc.Query(@"SELECT * FROM posts ORDER BY id DESC LIMIT @limit OFFSET @offset", p => p.Put(20).Put(20 * page)))
                {
                    List<Post> list = null;
                    while (dc.NextRow())
                    {
                    }
                    wc.Response.StatusCode = (int)HttpStatusCode.OK;
                    // wc.Response.SetContent(list);
                }
                else
                {
                    wc.Response.StatusCode = (int)HttpStatusCode.NoContent;
                }
            }
        }


        public void @new(WebContext wc)
        {
            IToken tok = wc.Token;

            using (var dc = Service.NewDbContext())
            {
                dc.Execute("INSERT INTO posts () VALUES ()", p => p.Put(tok.Key).Put(tok.Name));
            }
        }

        public void remove(WebContext wc)
        {
        }
    }
}