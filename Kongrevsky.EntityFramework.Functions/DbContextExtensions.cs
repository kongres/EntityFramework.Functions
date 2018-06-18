// Copyright (c) 2015 Dixin Yan. All rights reserved.
// Licensed under the MIT License, Version 2.0. See LICENSE in the project root for license information.

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
