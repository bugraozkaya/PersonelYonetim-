using Microsoft.EntityFrameworkCore;
using PersonelYonetim.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// ... var builder satiri zaten var ...

builder.Services.AddControllersWithViews();   // bu satir sablonda zaten var

// DbContext'i DI konteynerine ekle; baglanti dizesini appsettings'ten oku
builder.Services.AddDbContext<UygulamaDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("VarsayilanBaglanti")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
