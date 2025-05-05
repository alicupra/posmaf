using System;
using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using POS.Core.Interfaces;
using POS.Core.Models;
using POS.Core.Services;
using POS.Data;
using POS.Devices;
using POS.Localization;
using POS.Reports;

namespace POS.UI
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }
        public static User CurrentUser { get; set; }
        
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();
                
            // Configuration des services
            var services = new ServiceCollection();
            ConfigureServices(services, configuration);
            
            ServiceProvider = services.BuildServiceProvider();
            
            // Initialisation de la localisation
            LocalizationManager.SetLanguage("fr-MA");
            
            // Vérification et migration de la base de données
            using (var scope = ServiceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<POSDbContext>();
                dbContext.Database.Migrate();
            }
            
            // Démarrage de l'application avec l'écran de connexion
            var loginWindow = new Views.LoginWindow();
            loginWindow.Show();
        }
        
        private void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // Base de données
            services.AddDbContext<POSDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
                
            // Repositories
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            
            // Services métier
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ISaleService, SaleService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IReceiptService, ReceiptService>();
            
            // Périphériques
            services.AddSingleton<IReceiptPrinter>(provider => 
                new ReceiptPrinter(configuration["PrinterSettings:ReceiptPrinterName"]));
                
            // Rapports
            services.AddScoped<IReportGenerator, SalesReportGenerator>();
            
            // ViewModels
            services.AddTransient<ViewModels.LoginViewModel>();
            services.AddTransient<ViewModels.SalesViewModel>();
            services.AddTransient<ViewModels.ProductsViewModel>();
            services.AddTransient<ViewModels.ReportsViewModel>();
        }
    }
}