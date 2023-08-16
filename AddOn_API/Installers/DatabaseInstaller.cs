using AddOn_API.Data;
using Microsoft.EntityFrameworkCore;

namespace AddOn_API.Installers
{
    public class DatabaseInstaller : IInstallers
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<DatabaseContext>(options =>
                  options.UseSqlServer(configuration.GetConnectionString("ConnectionSQLServer"))
            );

            services.AddDbContext<DbStoreProceduce>(options =>
                  options.UseSqlServer(configuration.GetConnectionString("ConnectionSQLServer"))
            );
        }
    }
}