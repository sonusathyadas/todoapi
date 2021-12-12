using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoApi.Models;

namespace TodoApi
{
    public static class DbInitializer
    {
        public static void InitializeDatabase(TodoContext dbContext)
        {
            dbContext.Database.EnsureCreated();
        }
    }
}
