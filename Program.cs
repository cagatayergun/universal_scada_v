using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows.Forms;
using Universalscada.Core;
using Universalscada.Core.Core;
using Universalscada.Core.Repositories;
using Universalscada.Services;
using Universalscada.Repositories;
using Universalscada.Services;

namespace Universalscada
{
    static class Program
    {
        private static IServiceProvider ServiceProvider { get; set; }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 1. DI konteynerini oluþtur
            ConfigureServices();

            // 2. MainForm'u DI üzerinden resolve et ve baþlat
            var mainForm = ServiceProvider.GetService<MainForm>();
            Application.Run(mainForm);
        }

        private static void ConfigureServices()
        {
            var services = new ServiceCollection();

            // --- CORE VE VERÝTABANI HÝZMETLERÝ (Dahili SQL - SQLite) ---
            services.AddDbContext<ScadaDbContext>(options =>
            {
                // Dahili SQL gereksinimine uygun olarak SQLite kullanýlýyor.
                options.UseSqlite("Data Source=scada_db.sqlite");
            }, ServiceLifetime.Scoped);

            // Yeni Evrensel Servisler
            services.AddScoped<IMetaDataRepository, MetaDataRepository>();
            services.AddScoped<IRecipeTimeCalculator, DynamicRecipeTimeCalculator>();

            // Mevcut Repository'leri kaydet
            // Artýk bu repository'ler, constructor'larýna ScadaDbContext alabilir (Gerekiyorsa).
            // DashboardRepository'nin RecipeRepository ve IRecipeTimeCalculator baðýmlýlýklarý otomatik çözülecek.
            services.AddScoped<AlarmRepository>();
            services.AddScoped<CostRepository>();
            services.AddScoped<DashboardRepository>();
            services.AddScoped<MachineRepository>();
            services.AddScoped<ProcessLogRepository>();
            services.AddScoped<ProductionRepository>();
            services.AddScoped<RecipeRepository>();
            services.AddScoped<UserRepository>();

            // Servisler
            // FtpTransferService'in constructor'ý null alýyorsa, bunu DI'a kaydederken düzeltmek gerekir
            // VEYA sadece bir kez new'lenip MainForm'a geçirilebilir. DI'a kaydý varsayalým:
            services.AddSingleton<FtpTransferService>(s => new FtpTransferService(null));

            // Ana Formu kaydet (Form, artýk tüm baðýmlýlýklarýný constructor'dan alacak)
            services.AddTransient<MainForm>();

            ServiceProvider = services.BuildServiceProvider();
        }
    }
}