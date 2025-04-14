using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Microsoft.EntityFrameworkCore;
using connect_us_api.Data;

var builder = WebApplication.CreateBuilder(args);

// get connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// initialize DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 25)))
);

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
