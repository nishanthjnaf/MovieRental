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
builder.Services.AddScoped<NotificationService>();
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

// Ensure required Movie columns exist for older databases.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MovieRentalContext>();
    db.Database.ExecuteSqlRaw(@"
IF COL_LENGTH('Movies', 'Cast') IS NULL
    ALTER TABLE Movies ADD Cast nvarchar(max) NOT NULL CONSTRAINT DF_Movies_Cast DEFAULT('');
IF COL_LENGTH('Movies', 'Director') IS NULL
    ALTER TABLE Movies ADD Director nvarchar(max) NOT NULL CONSTRAINT DF_Movies_Director DEFAULT('');
IF COL_LENGTH('Movies', 'ContentRating') IS NULL
    ALTER TABLE Movies ADD ContentRating nvarchar(max) NOT NULL CONSTRAINT DF_Movies_ContentRating DEFAULT('');
IF COL_LENGTH('Movies', 'ContentAdvisory') IS NULL
    ALTER TABLE Movies ADD ContentAdvisory nvarchar(max) NOT NULL CONSTRAINT DF_Movies_ContentAdvisory DEFAULT('');

IF EXISTS (
    SELECT 1
    FROM sys.columns c
    JOIN sys.types t ON c.user_type_id = t.user_type_id
    WHERE c.object_id = OBJECT_ID('Reviews')
      AND c.name = 'Rating'
      AND t.name = 'int'
)
    ALTER TABLE Reviews ALTER COLUMN Rating float NOT NULL;

IF OBJECT_ID('UserPreferences', 'U') IS NULL
BEGIN
    CREATE TABLE UserPreferences (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL UNIQUE,
        PreferredGenres NVARCHAR(MAX) NOT NULL DEFAULT '',
        PreferredLanguages NVARCHAR(MAX) NOT NULL DEFAULT '',
        Theme NVARCHAR(20) NOT NULL DEFAULT 'dark',
        IsSet BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_UserPreferences_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
    );
END

IF OBJECT_ID('CartItems', 'U') IS NULL
BEGIN
    CREATE TABLE CartItems (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        MovieId INT NOT NULL,
        RentalDays INT NOT NULL DEFAULT 7,
        CONSTRAINT FK_CartItems_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
        CONSTRAINT FK_CartItems_Movies FOREIGN KEY (MovieId) REFERENCES Movies(Id) ON DELETE CASCADE,
        CONSTRAINT UQ_CartItems_User_Movie UNIQUE (UserId, MovieId)
    );
END

IF COL_LENGTH('Payments', 'RefundAmount') IS NULL
    ALTER TABLE Payments ADD RefundAmount float NULL;
IF COL_LENGTH('Payments', 'RefundedAt') IS NULL
    ALTER TABLE Payments ADD RefundedAt datetime2 NULL;

IF OBJECT_ID('RentalItemRefunds', 'U') IS NULL
BEGIN
    CREATE TABLE RentalItemRefunds (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        RentalItemId INT NOT NULL,
        RentalId INT NOT NULL,
        UserId INT NOT NULL,
        RefundAmount float NOT NULL DEFAULT 0,
        RefundedAt datetime2 NOT NULL
    );
END

IF OBJECT_ID('Notifications', 'U') IS NULL
BEGIN
    CREATE TABLE Notifications (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        Title NVARCHAR(200) NOT NULL DEFAULT '',
        Message NVARCHAR(MAX) NOT NULL DEFAULT '',
        Type NVARCHAR(50) NOT NULL DEFAULT '',
        IsRead BIT NOT NULL DEFAULT 0,
        CreatedAt datetime2 NOT NULL,
        RelatedId INT NULL
    );
END

IF OBJECT_ID('BroadcastMessages', 'U') IS NULL
BEGIN
    CREATE TABLE BroadcastMessages (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        SentByUserId INT NOT NULL DEFAULT 0,
        SentByUsername NVARCHAR(100) NOT NULL DEFAULT '',
        Title NVARCHAR(200) NOT NULL DEFAULT '',
        Message NVARCHAR(MAX) NOT NULL DEFAULT '',
        SentAt datetime2 NOT NULL
    );
END
");
}

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