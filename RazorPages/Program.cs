using BLL;
using DAL;
using DAL.Data;
using RazerPages.Configurations;

namespace RazerPages
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Modular Configurations
            builder.Services.AddCustomAuthorization();
            builder.Services.AddCustomAuthentication();
            builder.Services.AddCustomDataProtection();

            // Register BLL & DAL services (Feature-based Registration)
            builder.Services.AddDataAccessLayer(builder.Configuration);
            builder.Services.AddCoreBusinessServices(builder.Configuration);
            builder.Services.AddUserManagementServices();
            builder.Services.AddEventManagementServices();
            builder.Services.AddFeedbackManagementServices();


            var app = builder.Build();

            // Chạy migration và seed data khi khởi động — chỉ chạy nếu DB chưa có dữ liệu
            await DbInitializer.SeedAsync(app.Services);

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();
        }
    }
}
