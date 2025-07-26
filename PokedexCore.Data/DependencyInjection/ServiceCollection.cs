using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PokedexCore.Application.Events.Handlers;
using PokedexCore.Data.Contex;
using PokedexCore.Data.Repositories;
using PokedexCore.Data.UnitWork;
using PokedexCore.Data.UnitWork;
using PokedexCore.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PokedexCore.Data.DependencyInjection
{
    public static  class ServiceCollection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<PokedexDbContext>(optionsAction =>
            {
                optionsAction.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IPokemonRepository<>), typeof(PokemonRepository<>));
            services.AddScoped(typeof(ITrainerRepository<>), typeof(TrainerRepository<>));

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(PokemonLevelUpEventHandler).Assembly);
            });

            return services;
        }
    }
}