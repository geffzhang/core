﻿using Greatbone.Core;

namespace Greatbone.Sample
{
    ///
    /// /buyer/
    ///
    public class UserFolder : WebFolder
    {
        public UserFolder(WebFolderContext fc) : base(fc)
        {
            CreateVar<UserVarFolder>();
        }

        ///
        /// Get buyer's personal order list
        ///
        public void all(WebActionContext ac)
        {

        }

        ///
        /// Get shop's order list
        ///
        public void list(WebActionContext ac)
        {
            // string shopid = wc.Var(null);

        }

        ///
        /// find in shop's order list
        ///
        public void clear(WebActionContext ac)
        {
            // string shopid = wc.Var(null);

        }
    }
}