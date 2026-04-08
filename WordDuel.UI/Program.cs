using WordDuel.BLL.WordServices;
using WordDuel.DAL.Interfaces;
using WordDuel.DAL.Repositories;



var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("https://0.0.0.0:7057", "http://0.0.0.0:5227");

// Add services to the container.
builder.Services.AddRazorPages();

// Hårdkodad ordlista för testning
var words = new[] { "STORK", "STÄPP", "SLAPP", "Släppa"};


// Registrera repository
builder.Services.AddSingleton<IWordRepository>(new WordRepository());

//// Registrera WordService
//builder.Services.AddSingleton<WordService>();




var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
