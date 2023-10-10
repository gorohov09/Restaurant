using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Restaurant.Services.OrderAPI;
using Restaurant.Services.OrderAPI.DbContexts;
using Restaurant.Services.OrderAPI.Messaging;
using Restaurant.Services.OrderAPI.RabbitMQSender;
using Restaurant.Services.OrderAPI.Repository;

var builder = WebApplication.CreateBuilder(args);

/// Add services to the container.
var services = builder.Services;
services.AddControllers();

services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = "https://localhost:7120/";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false
        };
    });

services.AddAuthorization(options =>
{
    options.AddPolicy("ApiScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "restaurant");
    });
});

services.AddEndpointsApiExplorer();
services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Mango.Services.OrderAPI", Version = "v1" });
    c.EnableAnnotations();
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"Enter 'Bearer' [space] and your token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type=ReferenceType.SecurityScheme,
                                Id="Bearer"
                            },
                            Scheme="oauth2",
                            Name="Bearer",
                            In=ParameterLocation.Header
                        },
                        new List<string>()
                    }

                });
});

services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

IMapper mapper = MappingConfig.RegisterMaps().CreateMapper();
services.AddSingleton(mapper);
services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

//Суть действий ниже, зарегестрировать OrderRepository как Singleton, так как он будет использоваться в классе,
//который тоже зарегестрирован как Singleton
var optionBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
optionBuilder.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
services.AddSingleton<IOrderRepository>(new OrderRepository(optionBuilder.Options));

services.AddSingleton<IRabbitMQOrderMessageSender, RabbitMQOrderMessageSender>();

services.AddHostedService<RabbitMQCheckoutConsumer>();
services.AddHostedService<RabbitMQPaymentConsumer>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
