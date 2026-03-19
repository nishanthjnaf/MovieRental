using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MovieRentalAPI.Interfaces;
using MovieRentalAPI.Models;
using MovieRentalAPI.Repositories;
using MovieRentalAPI.Services;
using MovieRentalModels;
using NSwag;                                
using NSwag.Generation.Processors.Security; 
using System.Text;

var builder = WebApplication.CreateBuilder(args);

#region Controllers
builder.Services.AddControllers();
#endregion


#region NSwag (OpenAPI)
builder.Services.AddOpenApiDocument(settings =>
{
    settings.DocumentName = "v1";
    settings.Title = "MovieRentalAPI";
    settings.Version = "v1";

    // JWT Bearer auth in the UI
    settings.AddSecurity("JWT", new OpenApiSecurityScheme
    {
        Type = OpenApiSecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = OpenApiSecurityApiKeyLocation.Header,
        Name = "Authorization",
        Description = "Enter token as: Bearer {your token}"
    });
    settings.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("JWT"));
});
#endregion


#region Database
builder.Services.AddDbContext<MovieRentalContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Development"));
});
#endregion

#region CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
#endregion

#region Rate Limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(
    builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
#endregion

#region Repositories
builder.Services.AddScoped<IRepository<int, User>, Repository<int, User>>();
builder.Services.AddScoped<IRepository<int, Movie>, Repository<int, Movie>>();
builder.Services.AddScoped<IRepository<int, Genre>, Repository<int, Genre>>();
builder.Services.AddScoped<IRepository<int, Inventory>, Repository<int, Inventory>>();
builder.Services.AddScoped<IRepository<int, Rental>, Repository<int, Rental>>();
builder.Services.AddScoped<IRepository<int, RentalItem>, Repository<int, RentalItem>>();
builder.Services.AddScoped<IRepository<int, Payment>, Repository<int, Payment>>();
builder.Services.AddScoped<IRepository<int, Review>, Repository<int, Review>>();
builder.Services.AddScoped<IRepository<int, Watchlist>, Repository<int, Watchlist>>();
#endregion

#region Services
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserServices, UserServices>();
builder.Services.AddScoped<IMovieServices, MovieServices>();
builder.Services.AddScoped<IWatchlistService, WatchlistService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IRentalService, RentalService>();
builder.Services.AddScoped<IGenreService, GenreService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
#endregion

#region Authentication (JWT)
var key = builder.Configuration["Jwt:Key"]
          ?? throw new InvalidOperationException("JWT key missing");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
    };
});

builder.Services.AddAuthorization();
#endregion

var app = builder.Build();

#region Middleware Pipeline


if (app.Environment.IsDevelopment())
{
    app.UseOpenApi(cfg =>
    {
        cfg.Path = "/swagger/v1/swagger.json";
        cfg.DocumentName = "v1";
    });

    app.UseSwaggerUi(cfg =>
    {
        cfg.Path = "/swagger"; 
        cfg.SwaggerRoutes.Clear();
        cfg.SwaggerRoutes.Add(new NSwag.AspNetCore.SwaggerUiRoute(
            name: "v1",
            url: "/swagger/v1/swagger.json"
        ));
        cfg.DocumentTitle = "MovieRentalAPI - Swagger UI";
    });
}



app.UseCors("AllowAll");
app.UseIpRateLimiting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
#endregion

app.Run();