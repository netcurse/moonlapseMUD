using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moonlapse.Server.Data.DbContexts; 
public class MoonlapseDbContextFactory : IDesignTimeDbContextFactory<MoonlapseDbContext> {
    public MoonlapseDbContext CreateDbContext(string[] args) {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<MoonlapseDbContext>();
        optionsBuilder.UseSqlite(configuration.GetConnectionString("MoonlapseDatabase"));

        return new MoonlapseDbContext(optionsBuilder.Options);
    }
}
