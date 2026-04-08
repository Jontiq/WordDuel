using Microsoft.AspNetCore.Session;
using Microsoft.EntityFrameworkCore;
using WordDuel.API.Hubs;
using WordDuel.BLL.GameLogicIntefaces;
using WordDuel.BLL.GameLogicServices;
using WordDuel.BLL.WordServices;
using WordDuel.DAL.Data;
using WordDuel.DAL.Interfaces;
using WordDuel.DAL.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("https://0.0.0.0:7222", "http://0.0.0.0:5103");

// Controllers + OpenAPI
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// SignalR
builder.Services.AddSignalR();

// CORS – tillåt UI att ansluta - LÄGGA IN VÅRAN PORT!!!!!!
builder.Services.AddCors(options =>
{
    options.AddPolicy("UIPolicy", policy =>
    {
        policy.SetIsOriginAllowed(_ => true) // Tillåt alla origins på LAN
        //.WithOrigins("https://localhost:7057")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AppDbConnection")));

// DAL
builder.Services.AddScoped<IMatchRepository, MatchRepository>();
builder.Services.AddSingleton<IWordRepository, WordRepository>();

// BLL
builder.Services.AddScoped<IMatchService, MatchService>();
builder.Services.AddScoped<IWordService, WordService>();
builder.Services.AddScoped<IMatchPersistence, MatchPersistence>();
builder.Services.AddSingleton<SessionStore>();
builder.Services.AddTransient<Random>();

var app = builder.Build();

// Migrationer
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
    }
}

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("UIPolicy");
app.UseAuthorization();

app.MapControllers();
app.MapHub<GameHub>("/gameHub");

app.Run();