using Microsoft.Extensions.Options;
using projeto_final_LV.Models.Options;
using projeto_final_LV.Services.Tmdb;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddMemoryCache();

builder.Services.Configure<TmdbOptions>(builder.Configuration.GetSection("Tmdb"));

builder.Services.AddHttpClient<ITmdbApiService, TmdbApiService>((sp, http) =>
{
    var opt = sp.GetRequiredService<IOptions<TmdbOptions>>().Value;
    http.BaseAddress = new Uri(opt.BaseUrl);
});

var app = builder.Build();

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
