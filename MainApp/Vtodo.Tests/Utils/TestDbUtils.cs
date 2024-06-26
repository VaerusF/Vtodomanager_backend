using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Vtodo.DataAccess.Postgres;

namespace Vtodo.Tests.Utils
{
    public static class TestDbUtils
    {
        internal static AppDbContext SetupTestDbContextInMemory()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }
    }
}