using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokedexCore.Data.Contex
{
    //public class PokedexDbContextFactory : IDesignTimeDbContextFactory<PokedexDbContext>
    //{
    //    public PokedexDbContext CreateDbContext(string[] args)
    //    {
    //        var optionsBuilder = new DbContextOptionsBuilder<PokedexDbContext>();

    //        // CAMBIA esta cadena según tu configuración real
    //        var connectionString = "Server=localhost;Database=PokedexDb;Trusted_Connection=True;TrustServerCertificate=True;";
    //        optionsBuilder.UseSqlServer(connectionString);

    //        return new PokedexDbContext(optionsBuilder.Options);
    //    }
    //}
}
