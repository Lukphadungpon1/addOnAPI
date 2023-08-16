using AddOn_API.DTOs.MailService;

namespace AddOn_API.Installers
{
    public class EmailConfigInstaller : IInstallers
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            /// mail service config by  theerayut
            services.Configure<MailSettings>(configuration.GetSection("MailSettings"));


        }
    }
}