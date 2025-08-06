using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using PokedexCore.Application.Interfaces.ExternalServices;
using PokedexCore.Application.Services;
using PokedexCore.Data.Contex;
using PokedexCore.Data.DependencyInjection;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(builder => builder.Cache());

    options.AddPolicy("PokemonCache", builder =>
        builder.Cache()
               .Expire(TimeSpan.FromMinutes(30))
               .SetVaryByQuery("page", "pageSize","type"));

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
        Version = "v1"
    });
});

builder.Services.AddControllers();

builder.Services.AddOpenApi();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();

}

app.UseOutputCache();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
