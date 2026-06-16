using Blazor.Components;
using BLL;
using DAL;

namespace Blazor
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            // Register BLL & DAL services
            builder.Services.AddDataAccessLayer(builder.Configuration);
            builder.Services.AddBusinessLogicLayer();

            // Register Cookie Authentication
            builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    // Vì trang Login nằm bên MVC, cứ để tạm (bên Blazor không có UI login, ta sẽ redirect tay nếu cần)
                    options.LoginPath = "/Account/Login";
                });
            builder.Services.AddCascadingAuthenticationState();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.MapHub<BLL.SignalR.EventHub>("/eventhub");

            // Seed Data
            using (var scope = app.Services.CreateScope())
            {
                await DAL.Data.DbInitializer.SeedAsync(scope.ServiceProvider);
            }

            app.Run();
        }
    }
}
