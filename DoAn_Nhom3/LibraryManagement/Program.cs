using LibraryManagement.Models;
using LibraryManagement.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var conString = builder.Configuration.GetConnectionString("LibraryDb")
                ?? throw new InvalidOperationException("Connection string 'LibraryDb' not found.");

builder.Services.AddDbContext<LibraryDbContext>(options => options.UseSqlServer(conString));
builder.Services.AddScoped<LibraryDbContext>();
builder.Services.AddScoped<ILibraryCodeGenerator, LibraryCodeGenerator>();
builder.Services.AddScoped<IFineCalculatorService, FineCalculatorService>();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
