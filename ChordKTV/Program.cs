using Microsoft.EntityFrameworkCore;
using ChordKTV.Data;
using ChordKTV.Services;
using ChordKTV.Services.Service;
using ChordKTV.Services.Api;

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


// Add services to the container.
builder.Services.AddHttpClient<ILrcService, LrcService>();

builder.Services.AddControllers();
builder.Services.AddScoped<IYouTubeService, YouTubeApiClient>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

WebApplication app = builder.Build();

app.UseCors();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

PrepDb.Prep(app, app.Environment);

app.Run();
