using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SistemaVenta.DTO;

namespace SistemaVenta.BLL.Servicios.Contrato
{
    public interface IVentaService
    {
        Task<VentaDTO> Registrar(VentaDTO modelo); // Metodo para registrar una venta
        Task<List<VentaDTO>> Historial(string buscarPor, string numeroVenta, string fechalnicio, string fechaFin); // Metodo para obtener el historial de ventas
        Task<List<ReporteDTO>> Reporte(string fechalnicio, string fechaFin); // Metodo para obtener el reporte de ventas
    }
}
