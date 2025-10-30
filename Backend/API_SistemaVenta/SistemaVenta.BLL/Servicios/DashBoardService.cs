using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutoMapper;
using SistemaVenta.BLL.Servicios.Contrato;
using SistemaVenta.DAL.Repositorios.Contrato;
using SistemaVenta.DTO;
using SistemaVenta.Model;


namespace SistemaVenta.BLL.Servicios
{
    public class DashBoardService : IDashBoardService
    {
        private readonly IVentaRepository _ventaRepositorio;
        private readonly IGenericRepository<Producto> _productoRepositorio;
        private readonly IMapper _mapper;

        public DashBoardService(IVentaRepository ventaRepositorio, IGenericRepository<Producto> productoRepositorio, IMapper mapper)
        {
            _ventaRepositorio = ventaRepositorio;
            _productoRepositorio = productoRepositorio;
            _mapper = mapper;
        }



        //******************************************* RETORNAR-VENTAS (CONSULTA) *******************************************
        //******************************************************************************************************************
        private IQueryable<Venta> retornarVentas(IQueryable<Venta> tablaventa, int restarCantidadDias)
        {
            // Este metodo solo prepara la consulta, retorna la coleccion de ventas filtrada por fecha
            // (IQueryable<Venta> tablaventa) se refiere a una coleccion de ventas que se puede consultar

            // Es consulta a la coleccion tablaventa, Se obtiene la fecha mas reciente de registro de venta
            // v es una variable que representa cada venta en la coleccion tablaventa y toma su propiedad FechaRegistro
            DateTime? ultimaFecha = tablaventa
                        .OrderByDescending(v => v.FechaRegistro)
                        .Select(v => v.FechaRegistro)
                        .First();

            // Resta la cantidad de dias especificada a la ultima fecha obtenida
            ultimaFecha = ultimaFecha.Value.AddDays(restarCantidadDias);// ultimaFecha = 2025-10-25 + (-7 días
                                                                        // ultimaFecha es nullable (DateTime?), por eso usamos .Value para obtener el DateTime real.)
            
            //Where(...) devuelve un IQueryable<Venta>, es decir una consulta pendiente, no una lista concreta.
            return tablaventa.Where(v => v.FechaRegistro >= ultimaFecha.Value.Date); //.Date devuelve solo la parte de la fecha, eliminando la hora.
            //Para cada elemento v de tablaventa, toma solo los que cumplan la condición v.FechaRegistro >= ...”
        }


        //**************************************** TOTAL-VENTAS-ULTIMA-SEMANA ****************************************
        //************************************************************************************************************
        private async Task<int> TotalVentasUltimaSemana()
        {
            int total = 0;
            IQueryable<Venta> _ventaQuery = await _ventaRepositorio.Consultar();

            if (_ventaQuery.Count() > 0)
            {
                var tablaVenta = retornarVentas(_ventaQuery, -7);
                total = tablaVenta.Count();
            }

            return total;
        }


        //*************************************** TOTAL-INGRESOS-ULTIMA-SEMANA ***************************************
        //************************************************************************************************************
        private async Task<string> TotalIngresosUltimaSemana()
        {
            decimal resultado = 0;
            IQueryable<Venta> _ventaQuery = await _ventaRepositorio.Consultar();

            if (_ventaQuery.Count() > 0)
            {
                //Enviamos _ventaQuery que tiene todas las ventas y se puede consultar, ademas de -7 dias para filtrar
                var tablaventa = retornarVentas(_ventaQuery, -7);

                //.Select() transforma la colección de ventas en una colección solo de totales: Select(v => v.Total) → [100, 50, 30]
                // v ahora es cada total de venta (decimal?) en la colección resultante.
                // .Value se usa para obtener el valor real del nullable (decimal? → decimal).
                // .Sum(...) suma todos los valores de la colección.
                resultado = tablaventa
                    .Select(v => v.Total)
                    .Sum(v => v.Value);
            }

            return Convert.ToString(resultado, new CultureInfo("es-Es")); // Formatea el decimal como cadena en formato español (con comas y puntos adecuados).

        }

        //********************************************** TOTAL-PRODUCTOS *********************************************
        //************************************************************************************************************
        private async Task<int> TotalProductos()
        {
            // Consulta para obtener todos los productos
            IQueryable<Producto> _productoQuery = await _productoRepositorio.Consultar();

            int total = _productoQuery.Count(); //Cuenta el total de productos en la consulta
            return total;
        }


        //******************************************* VENTAS-ULTIMA-SEMANA *******************************************
        //************************************************************************************************************
        private async Task<Dictionary<string, int>> VentasUltimaSemana()
        {
            // <Dictionary<string, int>> representa un diccionario donde la clave es una cadena (string) y el valor es un entero (int).
            // la clave (key) es un string → por ejemplo "Lunes", "Martes", etc.
            // el valor (value) es un int → por ejemplo 5, 10, 3, etc.
            /*      CLAVE          VALOR
                  ["Lunes"] =       5,
                  ["Martes"] =      8,
                  ["Miércoles"] =   3         */

            // Inicializa un diccionario para almacenar los resultados
            Dictionary<string, int> resultado = new Dictionary<string, int>();

            IQueryable<Venta> _ventaQuery = await _ventaRepositorio.Consultar();

            if (_ventaQuery.Count() > 0)
            {
                var tablaventa = retornarVentas(_ventaQuery, -7);


                // Se agrupan las ventas por fecha y se cuentan cuántas hubo por día.
                // Aquí empieza la consulta LINQ que transforma los datos:
                resultado = tablaventa
                    // Agrupa las ventas por el día de la fecha de registro.
                    // "v" es cada venta; "v.FechaRegistro.Value.Date" obtiene solo la parte de la fecha (sin la hora).
                    .GroupBy(v => v.FechaRegistro.Value.Date)
                    // Ordena los grupos por la fecha del grupo (la clave de agrupación).
                    // g.Key representa la fecha de cada grupo.
                    .OrderBy(g => g.Key)
                    // Crea una proyección (un nuevo objeto) con dos campos:
                    // "fecha": la fecha del grupo convertida a texto con formato "dd/MM/yyyy"
                    // "total": la cantidad de ventas en ese día (se usa Count() para contarlas)
                    .Select(dv => new { fecha = dv.Key.ToString("dd/MM/yyyy"), total = dv.Count() })
                    // Convierte la lista de objetos en un diccionario:
                    //  - La clave del diccionario será la fecha (r.fecha)
                    //  - El valor será el total de ventas (r.total)
                    .ToDictionary(keySelector: r => r.fecha,  // define la clave del diccionario
                    elementSelector: r => r.total);           // define el valor asociado a esa clave
            }
            // Retorna el diccionario resultante.
            // Ejemplo de salida:
            // {
            //   "20/10/2025": 5,
            //   "21/10/2025": 3,
            //   "22/10/2025": 7
            // }
            return resultado;
        }

        public async Task<DashboardDTO> Resumen()
        {
            DashboardDTO vmDahsBoard = new DashboardDTO();

            try
            {
                vmDahsBoard.TotalVentas = await TotalVentasUltimaSemana();
                vmDahsBoard.TotalIngresos = await TotalIngresosUltimaSemana();
                vmDahsBoard.TotalProductos = await TotalProductos();

                List<VentaSemanaDTO> listaVentaSemana = new List<VentaSemanaDTO>();

                foreach (KeyValuePair<string, int> item in await VentasUltimaSemana())
                {

                    listaVentaSemana.Add(new VentaSemanaDTO()
                    {
                        Fecha = item.Key,
                        Total = item.Value

                    });
                }
                vmDahsBoard.VentasUltimaSemana = listaVentaSemana;
                
            }
            catch
            {
                throw;
            }
            return vmDahsBoard;
        }
    }
}
