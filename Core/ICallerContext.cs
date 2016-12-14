﻿namespace Greatbone.Core
{
    ///
    /// A context that can initiate remote call.
    ///
    public interface ICallerContext
    {
        IPrincipal Principal { get; }

        string Token { get; }

        bool IsCookied { get; }
    }
}