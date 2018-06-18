// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Kongrevsky.EntityFramework.Functions
{
    using System.Data.Entity;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;

    public static class DbContextExtensions
    {
        public static ObjectContext ObjectContext
            (this DbContext context) => (context as IObjectContextAdapter)?.ObjectContext;
    }
}
