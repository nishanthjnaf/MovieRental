using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using MovieRentalAPI.Interfaces;
using MovieRentalAPI.Models;
using MovieRentalAPI.Repositories;
using MovieRentalAPI.Services;
using MovieRentalModels;
using System.Text;
using System.Threading.RateLimiting;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Sample API", Version = "v1" });

    c.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme."
    });

    c.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("bearer", document)] = []
        });
});
//builder.Services.AddRateLimiter(options =>
//{
//    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
//        RateLimitPartition.GetFixedWindowLimiter(
//            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
//            _ => new FixedWindowRateLimiterOptions
//            {
//                PermitLimit = 3,
//                Window = TimeSpan.FromSeconds(10),
//                QueueLimit = 0
//            }));

//    options.RejectionStatusCode = 429;
//});
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

#region Contexts
builder.Services.AddDbContext<MovieRentalContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Development"));
});
#endregion

#region CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
#endregion

#region Repositories
builder.Services.AddScoped<IRepository<int, User>, Repository<int, User>>();
builder.Services.AddScoped<IRepository<int, Movie>,Repository<int,Movie>>();
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

#region Middlewares
string key = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Secret key not found in configuration.");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(option =>
    option.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        ValidateIssuerSigningKey = true
    }
);

#endregion
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseIpRateLimiting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();