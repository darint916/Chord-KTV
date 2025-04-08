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
using System.Security.Claims;

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

builder.Services.AddScoped<IYouTubeClientService, YouTubeApiClientService>();
builder.Services.AddScoped<ISongRepo, SongRepo>();
builder.Services.AddScoped<IAlbumRepo, AlbumRepo>();
builder.Services.AddScoped<IQuizRepo, QuizRepo>();
builder.Services.AddScoped<IFullSongService, FullSongService>();
builder.Services.AddScoped<IHandwritingService, HandwritingService>();
builder.Services.AddScoped<IUserRepo, UserRepo>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IQuizService, QuizService>();
builder.Services.AddScoped<IGeniusMetaDataRepo, GeniusMetaDataRepo>();
builder.Services.AddScoped<IUserActivityRepo, UserActivityRepo>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ChordKTV API", Version = "v1" });
    
    // Add JWT Authentication
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
    // Add this at the top to check versions
    var logger = builder.Services.BuildServiceProvider()
        .GetRequiredService<ILogger<Program>>();
    
    var tokenVersion = typeof(Microsoft.IdentityModel.Tokens.Base64UrlEncoder)
        .Assembly.GetName().Version;
    logger.LogInformation("Token package version: {Version}", tokenVersion);
    
    options.Authority = "https://accounts.google.com";
    options.Audience = "626673404798-lc7pm62to3kkgp641d68bj691c9tk2u6.apps.googleusercontent.com";
    
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            
            var authHeader = context.HttpContext.Request.Headers["Authorization"].ToString();
            logger.LogInformation("Raw Auth Header received: {Length} chars", authHeader?.Length ?? 0);
            logger.LogInformation("First 50 chars of auth header: '{Start}'", 
                authHeader?.Substring(0, Math.Min(50, authHeader?.Length ?? 0)));
            
            // If it starts with Bearer, strip it
            if (authHeader?.StartsWith("Bearer ") == true)
            {
                context.Token = authHeader.Substring("Bearer ".Length);
                logger.LogInformation("Stripped Bearer prefix");
            }
            else if (!string.IsNullOrEmpty(authHeader))
            {
                context.Token = authHeader;
                logger.LogInformation("Using raw header as token");
            }
            
            if (context.Token != null)
            {
                logger.LogInformation("First 50 chars of token: '{Start}'", 
                    context.Token.Substring(0, Math.Min(50, context.Token.Length)));
                logger.LogInformation("Token contains dots: {DotCount}", 
                    context.Token.Count(c => c == '.'));
            }
            
            return Task.CompletedTask;
        },
        
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError("Authentication failed: {Error}", context.Exception.Message);
            logger.LogError("Full Exception: {Ex}", context.Exception.ToString());
            return Task.CompletedTask;
        }
    };
});

WebApplication app = builder.Build();

// First Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Then CORS
app.UseCors();

// Then the rest
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

PrepDb.Prep(app, app.Environment);

app.Run();
