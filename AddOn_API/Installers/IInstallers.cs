namespace AddOn_API.Installers
{
    public interface IInstallers
    {
          void InstallServices(IServiceCollection services, IConfiguration configuration); 
    }
}