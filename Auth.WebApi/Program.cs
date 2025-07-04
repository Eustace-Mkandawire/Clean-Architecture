
using Auth.Application.Commands;
using Auth.Application.Infrastructures;
using Auth.Application.Services;
using Auth.Domain.Contracts;
using Auth.Domain.Interfaces;
using Auth.Domain.Repositories;
using Auth.Infrastructure.Cache;
using Auth.Infrastructure.Helpers;
using Auth.Infrastructure.Persistence;
using Auth.Infrastructure.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
using System.Reflection;


var builder = WebApplication.CreateBuilder(args);
Log.Logger = new LoggerConfiguration()
       .ReadFrom.Configuration(builder.Configuration)
       .Enrich.FromLogContext()
       .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddDbContext<AuthDbContext>(options =>
                options.UseNpgsql(
                    builder.Configuration.GetConnectionString("AuthServiceConnection"),
                    npgsqlOptions =>
                    {
                        npgsqlOptions.MigrationsAssembly(typeof(AuthDbContext).Assembly.FullName);
                        npgsqlOptions.EnableRetryOnFailure();
                    }
                ));

builder.Services.AddMediator(cfg =>
{
    cfg.AddConsumers(Assembly.GetExecutingAssembly());
});

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "Auth:";
});

builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

builder.Services.Configure<RabbitMqTransportOptions>(builder.Configuration.GetSection("RabbitMq"));
builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();
    x.AddConsumer<LoginConsumer>();
    x.AddConsumer<RegisterConsumer>();
    x.UsingRabbitMq((context, configure) =>
    {
        configure.ConfigureEndpoints(context);
    });

});
builder.Services.AddOptions<MassTransitHostOptions>().Configure(options =>
{
    options.WaitUntilStarted = true;
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapScalarApiReference();
app.MapControllers();

app.Run();