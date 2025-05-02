using Microsoft.EntityFrameworkCore;
using ChordKTV.Data;
using ChordKTV.Services.Service;
using ChordKTV.Services.Api;
using ChordKTV.Data.Api.SongData;
using ChordKTV.Data.Repo.SongData;
using ChordKTV.Utils;
using ChordKTV.Dtos;
using Serilog;
using System.Globalization;
using ChordKTV.Data.Api.UserData;
using ChordKTV.Data.Repo.UserData;
using ChordKTV.Data.Api.QuizData;
using ChordKTV.Data.Repo.QuizData;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

// Clear default claim mappings so we get the original claim names
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

//Serilog config
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .WriteTo.File("logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        formatProvider: CultureInfo.InvariantCulture)
    .CreateLogger();

builder.Logging.AddSerilog();

//Register DB
Console.WriteLine($"Connecting to: {builder.Configuration.GetConnectionString("PostgreSql")}");
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSql"), optionsBuilder => optionsBuilder.CommandTimeout(5)));


builder.Services.AddHttpContextAccessor();

builder.Services.AddCors(opt =>
{
    opt.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().Build();
    });
});

// Add logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

builder.Services.AddControllers().
    AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new SafeEnumConverter<LanguageCode>()); //convert enums to string and defaults to UNK if fails
    });

// Add services to the container.
builder.Services.AddHttpClient<ILrcService, LrcService>();
builder.Services.AddHttpClient<IGeniusService, GeniusService>();
builder.Services.AddHttpClient<IChatGptService, ChatGptService>();

//db repos
builder.Services.AddScoped<ISongRepo, SongRepo>();
builder.Services.AddScoped<IAlbumRepo, AlbumRepo>();
builder.Services.AddScoped<IQuizRepo, QuizRepo>();
builder.Services.AddScoped<IUserRepo, UserRepo>();
builder.Services.AddScoped<IUserActivityRepo, UserActivityRepo>();
builder.Services.AddScoped<IGeniusMetaDataRepo, GeniusMetaDataRepo>();
builder.Services.AddScoped<IYoutubeSongRepo, YoutubeSongRepo>();

//scoped services
builder.Services.AddScoped<IYouTubeClientService, YouTubeClientService>();
builder.Services.AddScoped<IHandwritingService, HandwritingService>();
builder.Services.AddScoped<IUserContextService, UserContextService>();
builder.Services.AddScoped<IQuizService, QuizService>();
builder.Services.AddScoped<IFullSongService, FullSongService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ChordKTV API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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
            Array.Empty<string>()
        }
    });
});

builder.Services.AddAutoMapper(typeof(Program).Assembly);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        string? jwtKey = builder.Configuration["Jwt:Key"];
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtKey!)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

WebApplication app = builder.Build();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

// Comment out HTTPS redirection for development
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.MapControllers();
// Run from static build files on deployment
if (!app.Environment.IsDevelopment())
{
    app.UseDefaultFiles();
    app.UseStaticFiles();
    app.MapFallbackToFile("index.html");
}

PrepDb.Prep(app, app.Environment);

app.Run();
