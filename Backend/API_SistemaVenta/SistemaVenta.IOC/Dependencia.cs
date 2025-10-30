using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SistemaVenta.DAL.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//REFERENCIAS A PROYECTOS DEL SISTEMA DE VENTA
using SistemaVenta.DAL.Repositorios.Contrato;
using SistemaVenta.DAL.Repositorios;

//REFERENCIA A UTILITIES PARA AUTO MAPPER
using SistemaVenta.Utility;
using SistemaVenta.BLL.Servicios.Contrato;
using SistemaVenta.BLL.Servicios;

// Archivo: Dependencia.cs
// Propósito:
// - Este archivo centraliza la configuración de todos los servicios y repositorios que tu aplicación necesita.
// - Contiene extensiones para centralizar el registro de dependencias (Inyección de dependencias / IoC).
// - Aquí se registra el DbContext de EF Core y cualquier servicio, repositorio o servicio de negocio que quieras exponer vía DI.
//
// Relación con otros proyectos:
// - Registra el __DbventaContext__ (del proyecto __SistemaVenta.DAL__) usando la cadena de conexión definida en __SistemaVenta.API__ (appsettings.json).
// - Normalmente se añadirán las interfaces/implementaciones de __SistemaVenta.BLL__ y repositorios de __SistemaVenta.DAL__ aquí.
// - Es llamada desde `Program.cs` en __SistemaVenta.API__ para preparar el contenedor de servicios antes de construir la aplicación.
//
namespace SistemaVenta.IOC
{
    public static class Dependencia
    {
        //"IServiceCollection services" → contenedor de servicios de .NET Core donde registras todo lo que quieres inyectar.
        //"IConfiguration configuration" → permite acceder a appsettings.json o variables de configuración (por ejemplo, la cadena de conexión a SQL Server).
        public static void InyectarDependencias(this IServiceCollection services, IConfiguration configuration)
        {
            //Esto le dice a .NET Core que cuando alguien pida un DbventaContext (_dbcontext)
            //Se cree una instancia usando SQL Server y use la cadena de conexión definida en appsettings.json con el nombre "cadenaSQL".
            services.AddDbContext<DbventaContext>(options => {
                options.UseSqlServer(configuration.GetConnectionString("cadenaSQL"));
            });

            //INYECTAR LOS REPOSITORIOS DE DAL
            services.AddTransient(typeof(IGenericRepository<>), typeof(GenericRepository<>));// El GenericRepository es genérico, se utiliza para cualquier modelo
            services.AddScoped<IVentaRepository, VentaRepository>(); // Repositorio especifico para Ventas

            //INYECTAR AUTOMAPPER DE UTILITY
            services.AddAutoMapper(typeof(AutoMapperProfile)); //Registrar el perfil de AutoMapper para mapeo de objetos


            //INYECTAR LOS SERVICIOS DE BLL
            services.AddScoped<IRolService, RolService>(); // Servicio para manejar roles
            services.AddScoped<IUsuarioService, UsuarioService>();
            services.AddScoped<ICategoriaService, CategoriaService>();
            services.AddScoped<IProductoService, ProductoService>(); 
            services.AddScoped<IVentaService, VentaService>();
            services.AddScoped<IDashBoardService, DashBoardService>(); 
            services.AddScoped<IMenuService, MenuService>(); 
        }
        //ejemplo

    }
}
