namespace AddOn_API.Installers
{
    public class EndpointsApiExplorerInstaller : IInstallers
    {
         public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddEndpointsApiExplorer();
        }
    }
}