using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SistemaVenta.BLL.Servicios.Contrato;
using SistemaVenta.DAL.Repositorios.Contrato;
using SistemaVenta.DTO;
using SistemaVenta.Model;

namespace SistemaVenta.BLL.Servicios
{
    public class VentaService : IVentaService
    {
        private readonly IVentaRepository _ventaRepositorio;
        private readonly IGenericRepository<DetalleVenta> _detalleVentaRepositorio;
        private readonly IMapper _mapper;

        public VentaService(IVentaRepository ventaRepositorio, IGenericRepository<DetalleVenta> detalleVentaRepositorio, IMapper mapper)
        {
            _ventaRepositorio = ventaRepositorio;
            _detalleVentaRepositorio = detalleVentaRepositorio;
            _mapper = mapper;
        }

        public async Task<VentaDTO> Registrar(VentaDTO modelo)
        {
            try
            {
                var ventaGenerada = await _ventaRepositorio.Registrar(_mapper.Map<Venta>(modelo));

                if (ventaGenerada.IdVenta == 0)
                    throw new TaskCanceledException("No se pudo crear");

                return _mapper.Map<VentaDTO>(ventaGenerada);
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<VentaDTO>> Historial(string buscarPor, string numeroVenta, string fechalnicio, string fechaFin)
        {
            // Obtener la consulta inicial de ventas desde el repositorio, sin ejecutar aun
            // IQueryable permite construir consultas dinámicas que se ejecutan en la base de datos solo cuando es necesario
            IQueryable<Venta> query = await _ventaRepositorio.Consultar(); // = SELECT * FROM Venta (todavía no se ejecuta)
            var ListaResultado = new List<Venta>(); // Lista para almacenar los resultados

            try
            {
                if (buscarPor == "fecha")
                {
                    DateTime fech_Inicio = DateTime.ParseExact(fechalnicio, "dd/MM/yyyy", new CultureInfo("es-ES"));
                    DateTime fech_Fin = DateTime.ParseExact(fechaFin, "dd/MM/yyyy", new CultureInfo("es-ES"));

                    // En query se filtran las ventas por fecha y se incluyen los detalles y productos relacionados
                    // EF Core toma todo lo que definido en query + Where + Include + ThenInclude y lo convierte en una sola consulta SQL optimizada.
                    ListaResultado = await query.Where(v => 
                                            v.FechaRegistro.Value.Date >= fech_Inicio.Date && 
                                            v.FechaRegistro.Value.Date <= fech_Fin.Date)
                                            .Include(dv => dv.DetalleVenta)
                                            .ThenInclude(p => p.IdProductoNavigation).ToListAsync();
                }
                else
                {
                    ListaResultado = await query.Where(v => v.NumeroDocumento == numeroVenta)
                                           .Include(dv => dv.DetalleVenta)
                                           .ThenInclude(p => p.IdProductoNavigation).ToListAsync();
                }
                //La consulta no se ejecuta hasta que llamamos a: .ToListAsync(), .FirstAsync(), .First() o .Count()
            }
            catch
            {
                throw;
            }

            // Retornar la lista de ventas mapeada a DTOs
            //Se retorna fuera de try-catch para que si hay un error en la consulta dentro de Try{} no intente mapear una lista vacia
            return _mapper.Map<List<VentaDTO>>(ListaResultado);
        }

        public async Task<List<ReporteDTO>> Reporte(string fechalnicio, string fechaFin)
        {
            IQueryable<DetalleVenta> query = await _detalleVentaRepositorio.Consultar();
            var ListaResultado = new List<DetalleVenta>();
            try
            {
                DateTime fech_Inicio = DateTime.ParseExact(fechalnicio, "dd/MM/yyyy", new CultureInfo("es-ES"));
                DateTime fech_Fin = DateTime.ParseExact(fechaFin, "dd/MM/yyyy", new CultureInfo("es-ES"));

                ListaResultado = await query
                                .Include(p => p.IdProductoNavigation)
                                .Include(v => v.IdVentaNavigation)
                                .Where(dv =>
                                    dv.IdVentaNavigation.FechaRegistro.Value.Date >= fech_Inicio.Date &&
                                    dv.IdVentaNavigation.FechaRegistro.Value.Date <= fech_Fin.Date
                                ).ToListAsync();
            }
            catch
            {
                throw;
            }
            //Se retorna fuera de try-catch para que si hay un error en la consulta dentro de Try{} no intente mapear una lista vacia
            return _mapper.Map<List<ReporteDTO>>(ListaResultado);
        }
    }
}
