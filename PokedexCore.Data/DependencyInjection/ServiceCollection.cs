using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PokedexCore.Application.Events.Handlers;
using PokedexCore.Application.Interfaces;
using PokedexCore.Application.Interfaces.ExternalServices;
using PokedexCore.Application.Services;
using PokedexCore.Data.Contex;
using PokedexCore.Data.Repositories;
using PokedexCore.Data.UnitWork;
using PokedexCore.Domain.Interfaces;

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
            services.AddScoped(typeof(ITrainerRepository<>), typeof(TrainerRepository<>));
          
            services.AddScoped<IPokemonServices, PokemonServices>();
            services.AddScoped<IAuthServices, AuthServices>();
            services.AddScoped<ITrainerService, TrainerService>();

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(PokemonLevelUpEventHandler).Assembly);
            });

            return services;
        }
    }
}