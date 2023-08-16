namespace AddOn_API.Installers
{
    public class ControllerInstaller : IInstallers
    {
        public void InstallServices(IServiceCollection Services, IConfiguration configuration)
        {
            Services.AddControllers();
        }
    }
}