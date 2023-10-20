using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SnackbarB2C2PI4_LeviFunk_MVC.Controllers;
using SnackbarB2C2PI4_LeviFunk_MVC.Data;
using System.Net.Http.Headers;

namespace SnackbarB2C2PI4_LeviFunk_MVC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container. --> Authentication context
            builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(
                builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            // Add 2nd DataContext
            // command for migrations: add-migration <name> -OutputDir <location> -Context <ContextName>
            // Example: add-migration InitialCreate -OutputDir Data\MigrationsLibrary -Context LibraryDbContext
            builder.Services.AddDbContext<LibraryDbContext>(options => options.UseSqlServer(
                builder.Configuration.GetConnectionString("LibraryConnection")));

            // Add API --> httprequest services
            builder.Services.AddHttpClient<ApiService>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:5001/");
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            });

            // Add Identity services
            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();
            builder.Services.AddControllersWithViews();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            app.Run();
        }
    }
}