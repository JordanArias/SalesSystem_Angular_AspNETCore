using SistemaVenta.IOC;

//
// Archivo: Program.cs
// Prop�sito:
// - Punto de entrada de la aplicaci�n ASP.NET Core (API).
// - Configura el host, los servicios y el pipeline HTTP.
// - Registra servicios comunes como controladores, Swagger y (normalmente) las dependencias definidas en el proyecto __SistemaVenta.IOC__.
//
// Relaci�n con otros proyectos:
// - Llama a las extensiones de registro definidas en __SistemaVenta.IOC__ para inyectar el DbContext y otros servicios.
// - Expone los endpoints que consumen los servicios de la capa __SistemaVenta.BLL__ (v�a DI).
// - Usa la configuraci�n (appsettings.json) donde est� la cadena de conexi�n para __SistemaVenta.DAL__.
//
namespace SistemaVenta.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.InyectarDependencias(builder.Configuration);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
