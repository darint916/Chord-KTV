using Microsoft.EntityFrameworkCore;
using ChordKTV.Data;
using ChordKTV.Services;
using ChordKTV.Services.Service;
using ChordKTV.Services.Api;
using ChordKTV.Data.Api.SongData;
using ChordKTV.Data.Repo;
using ChordKTV.Data.Repo.SongData;
using ChordKTV.Utils;
using ChordKTV.Dtos;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddScoped<IFullSongService, FullSongService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAutoMapper(typeof(Program).Assembly);

WebApplication app = builder.Build();

app.UseCors();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}

// Comment out HTTPS redirection for development
// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

PrepDb.Prep(app, app.Environment);

app.Run();
