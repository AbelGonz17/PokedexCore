using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PokedexCore.Application.Interfaces.ExternalServices;
using PokedexCore.Application.Services;
using PokedexCore.Data.Contex;
using PokedexCore.Data.DependencyInjection;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCustomRateLimiting();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(builder => builder.Cache());

    options.AddPolicy("PokemonCache", builder =>
        builder.Cache()
               .Expire(TimeSpan.FromMinutes(30))
               .SetVaryByQuery("page", "pageSize", "type"));
               


    options.AddPolicy("PokemonCacheByName", builder =>
        builder.Cache()
               .Expire(TimeSpan.FromMinutes(30))
               .SetVaryByRouteValue("name"));

});

builder.Services.AddControllers().AddJsonOptions(x => x.JsonSerializerOptions
            .ReferenceHandler = ReferenceHandler.IgnoreCycles).AddNewtonsoftJson();

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<PokedexDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddHttpClient<IPokemonApiService, PokemonApiService>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Trainer", politica => politica.RequireClaim(ClaimTypes.Role, "Trainer"));    
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication().AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["LlaveJWT"]!)),
        ClockSkew = TimeSpan.Zero
    };

});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "PokedexCore API",
        Version = "v1",
        Description = "API para gestionar Pokémon, entrenadores y capturas.\n\n" +
                      "Permite consultar información de Pokémon desde la PokéAPI, " +
                      "registrar entrenadores y manejar capturas de Pokémon.\n\n" +
                      "Autenticación: JWT Bearer.",
        Contact = new OpenApiContact
        {
           Name = "Abel Gonzalez",
           Email = "Theabel.17@hotmail.com"
        }

    });

    //var xmlFileName = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    //c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFileName));

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                        {
                            {
                                new OpenApiSecurityScheme
                                {
                                    Reference = new OpenApiReference
                                    {
                                        Type = ReferenceType.SecurityScheme,
                                        Id = "Bearer"
                                    }
                                },
                                new string[] { }
                            }
                        });
    });

builder.Services.AddControllers();

builder.Services.AddOpenApi();


var app = builder.Build();

// Swagger
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();

app.UseOutputCache();

app.MapControllers();

app.Run();
