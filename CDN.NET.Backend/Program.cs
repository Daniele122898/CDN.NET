using System;
using System.Linq;
using CDN.NET.Backend.Data;
using CDN.NET.Backend.Repositories.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CDN.NET.Backend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    using var context = services.GetRequiredService<DataContext>();
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogInformation("Applying Migrations if needed");
                    context.Database.Migrate();
                    logger.LogInformation("Finished applying migrations.");
                    IWebHostEnvironment env = services.GetRequiredService<IWebHostEnvironment>();
                    if (env.IsDevelopment())
                    {
                        // Seed in here
                        if (!context.Users.Any())
                        {
                            logger.LogInformation("Seeding database with model user");
                            var authRepo = services.GetRequiredService<IAuthRepository>();
                            authRepo.Register("admin", "password", true).GetAwaiter().GetResult();
                        }
                    }

                }
                catch (Exception e)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(e, "An error occured during migrations and seeding");
                }
            }
            
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
