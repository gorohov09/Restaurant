using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Restaurant.Services.ProductAPI;
using Restaurant.Services.ProductAPI.DbContexts;
using Restaurant.Services.ProductAPI.Repository;

var builder = WebApplication.CreateBuilder(args);

//add services
var services = builder.Services;
services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

IMapper mapper = MappingConfig.RegisterMaps().CreateMapper();
services.AddSingleton(mapper);
services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

services.AddScoped<IProductRepository, ProductRepository>();

var app = builder.Build();


//configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
