using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using VehicleRentalProject.Mapper;
using VehicleRentalProject.Models;
using VehicleRentalProject.Repositories;
using VehicleRentalProject.Repositories.Implementation;
using VehicleRentalProject.Repositories.Infrastructure;
using Microsoft.AspNetCore.Identity;
using VehicleRentalProject.CustomMiddleWare;
using VehicleRentalProject.Repositories.DataSeeding;
using Stripe;
using Microsoft.AspNetCore.Identity.UI.Services;
using VehicleRentalProject.Web.Utility;
using DinkToPdf;
using DinkToPdf.Contracts;

var builder = WebApplication.CreateBuilder(args);
//configure dbcontext with connectionstring
builder.Services.AddDbContext<CarContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddDefaultTokenProviders().AddEntityFrameworkStores<CarContext>();


// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<IDbInitializer, DbInitializer>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRentalRepository, RentalRepository>();


builder.Services.AddControllersWithViews();
builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));


var config = new AutoMapper.MapperConfiguration(cfg =>
{
    cfg.AddProfile(new VehicleProfile(builder.Environment));
});
var mapper = config.CreateMapper();
builder.Services.AddSingleton(mapper);

builder.Services.AddSession(options =>
{
    options.Cookie.Name = "VehicleProjectCookie";
    options.IdleTimeout = TimeSpan.FromMinutes(1);
    options.Cookie.HttpOnly = true;
});
builder.Services.AddRazorPages();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
    options.LogoutPath = $"/Identity/Account/Logout";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseStaticFiles();


DataSeeding();
app.UseSession();

app.UseRouting();

StripeConfiguration.ApiKey = builder.Configuration.GetSection("PaymentSettings:SecretKey").Get<string>();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

#pragma warning disable ASP0014
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
            name: "areas",
            pattern: "{area:exists}/{controller=vehicles}/{action=Index}/{id?}"
          );

    endpoints.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

});
#pragma warning restore ASP0014



app.MapRazorPages();
app.Run();

void DataSeeding()
{
    using (var scope = app.Services.CreateScope())
    {
        var DbInitial = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
        DbInitial.Initialize();
    }
}