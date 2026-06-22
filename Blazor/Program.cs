using Blazor.Components;
using BLL;
using DAL;
using DAL.Data;
using Blazor.Configurations;

namespace Blazor
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents(options => options.DetailedErrors = true);

            // Register BLL & DAL services
            builder.Services.AddDataAccessLayer(builder.Configuration);
            builder.Services.AddBusinessLogicLayer(builder.Configuration);

            // Register background worker
            builder.Services.AddHostedService<BLL.BackgroundServices.EmailReminderWorker>();

            // Modular Configurations
            builder.Services.AddCustomAuthorization();
            builder.Services.AddCustomAuthentication();
            builder.Services.AddCustomDataProtection();
            builder.Services.AddCascadingAuthenticationState();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.MapHub<BLL.SignalR.EventHub>("/eventhub").AllowAnonymous();

            // Seed Data
            using (var scope = app.Services.CreateScope())
            {
                await DAL.Data.DbInitializer.SeedAsync(scope.ServiceProvider);
            }

            app.Run();
        }
    }
}
