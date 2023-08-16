using System.Reflection;
using AddOn_API.DTOs.MailService;
using AddOn_API.Installers;
using Autofac;
using Autofac.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.InstallServiceInAssembly(builder.Configuration);



/// autofac add services  by theerayut
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());



builder.Host.ConfigureContainer<ContainerBuilder>(builder =>
{   
    builder.RegisterAssemblyTypes(Assembly.GetEntryAssembly())
    .Where(t=> t.Name.EndsWith("Service"))
    .AsImplementedInterfaces();
});


// builder.Services.AddControllers();
// // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

var app = builder.Build();



  
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowSpecificOrigins");

app.UseDeveloperExceptionPage();


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
