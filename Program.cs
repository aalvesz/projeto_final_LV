using Microsoft.Extensions.Options;
using projeto_final_LV.Models.Options;
using projeto_final_LV.Services.Tmdb;
using projeto_final_LV.Services.Weather;
using projeto_final_LV.Data;
using projeto_final_LV.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddMemoryCache();

builder.Services.Configure<TmdbOptions>(builder.Configuration.GetSection("Tmdb"));

// TMDb 
builder.Services.AddHttpClient<ITmdbApiService, TmdbApiService>(client =>
{
    client.BaseAddress = new Uri("https://api.themoviedb.org/3/");
    client.Timeout = TimeSpan.FromSeconds(10);
});

// Weather
builder.Services.AddHttpClient<IWeatherApiService, WeatherApiService>(client =>
{
    client.BaseAddress = new Uri("https://api.open-meteo.com/v1/");
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddScoped<IMovieRepository, MovieRepository>();

var app = builder.Build();

var cs = builder.Configuration.GetConnectionString("LocalDb")
         ?? throw new InvalidOperationException("ConnectionStrings:LocalDb não configurado.");
DatabaseInitializer.EnsureCreated(cs);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

