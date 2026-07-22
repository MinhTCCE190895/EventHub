using BLL;
using MVC.Configurations;

namespace MVC;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        DAL.DependencyInjection.AddDataAccessLayer(builder.Services, builder.Configuration);
        DAL.DependencyInjection.AddDataProtectionPersistence(builder.Services, "EventHub");
        builder.Services.AddBusinessLogicLayer(builder.Configuration);
        builder.Services.AddPortalOptions(builder.Configuration);
        builder.Services.AddCustomAuthentication();
        builder.Services.AddMvcPresentation();
        builder.Services.AddRequestRateLimiting();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
            await DAL.DependencyInjection.InitializeDatabaseAsync(app.Services);

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
            app.UseHttpsRedirection();
        }

        app.UseStaticFiles();
        app.UseRouting();
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Account}/{action=Login}/{id?}");

        app.Run();
    }
}
