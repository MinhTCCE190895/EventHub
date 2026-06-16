using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Blazor
{
    public class TestBooking
    {
        public static async Task RunTest()
        {
            var host = Program.CreateHostBuilder(new string[0]).Build();
            using var scope = host.Services.CreateScope();
            var bookingService = scope.ServiceProvider.GetRequiredService<BLL.Services.IBookingService>();
            
            // Lấy 1 Event và 1 Student từ DB
            var dbContext = scope.ServiceProvider.GetRequiredService<DAL.Data.AppDbContext>();
            var student = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(
                System.Linq.Queryable.Where(dbContext.Users, u => u.Role == "Student"));
                
            var evt = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(dbContext.Events);
            
            if (student != null && evt != null)
            {
                Console.WriteLine($"Test booking for Event {evt.Id} and Student {student.Id}");
                try
                {
                    var ticket = await bookingService.BookTicketAsync(evt.Id, student.Id);
                    Console.WriteLine("SUCCESS! Ticket: " + ticket);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR: " + ex.ToString());
                }
            }
        }
    }
}
