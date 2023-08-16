namespace AddOn_API.Installers
{
    public class CorsInstaller :IInstallers
    {
        public void InstallServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddCors(options =>
            {
                // Specifi policy
                options.AddPolicy("AllowSpecificOrigins", builder =>
                {
                    builder.WithOrigins(
                        "https://www.w3schools.com",
                        "http://www.localhost:7000",
                        "http://www.localhost:3000",
                        "*"
                        
                    ).AllowAnyMethod().AllowAnyHeader();
                });

                // Allow all 
                // options.AddPolicy("AllowAllOrigins", builder =>
                // {
                //     builder.AllowAnyOrigin()
                //     .AllowAnyHeader()
                //     .AllowCredentials()
                //     .SetIsOriginAllowed((host) => true)
                //     .AllowAnyMethod();
                // });
            });
        }
    }
}