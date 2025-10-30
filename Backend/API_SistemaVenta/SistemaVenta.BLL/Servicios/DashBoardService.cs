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
                    .GroupBy(v => v.FechaRegistro.Value.Date)/* =  g.Key(fecha) |  Elementos dentro del grupo(g)
                                                                    2025-10-20  |    [Venta 1, Venta 2]
                                                                    2025-10-21  |    [Venta 3, Venta 4]
                                                                    2025-10-22  |    [Venta 5]               */

                    // Solo Ordena los grupos por la fecha del grupo (la clave de agrupación), g.Key representa la fecha de cada grupo.
                    .OrderBy(g => g.Key) // g.Key se vuelve key que es la fecha


                    // Crea una proyección (un nuevo objeto) con dos campos:
                    // "fecha": la fecha del grupo convertida a texto con formato "dd/MM/yyyy"
                    // "total": la cantidad de ventas en ese día (se usa Count() para contarlas)
                    .Select(dv => new { fecha = dv.Key.ToString("dd/MM/yyyy"), total = dv.Count() })/* =  (fecha)dv.Key | (total)Elementos dentro del grupo(dv)
                                                                                                           2025-10-20   |    2
                                                                                                           2025-10-21   |    2
                                                                                                           2025-10-22   |    1            */
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


        //************************************************************************************************************
        //************************************************* RESUMEN **************************************************
        //************************************************************************************************************
        public async Task<DashboardDTO> Resumen()
        {
            DashboardDTO vmDahsBoard = new DashboardDTO(); // Instancia un nuevo objeto DashboardDTO para almacenar el resumen

            try
            {
                vmDahsBoard.TotalVentas = await TotalVentasUltimaSemana(); 
                vmDahsBoard.TotalIngresos = await TotalIngresosUltimaSemana();
                vmDahsBoard.TotalProductos = await TotalProductos();

                List<VentaSemanaDTO> listaVentaSemana = new List<VentaSemanaDTO>(); // Lista para almacenar las ventas de la ultima semana

               /* Recorre el diccionario de ventas de la ultima semana y lo convierte en una lista de VentaSemanaDTO
                  - Un Dictionary : es la colección completa, una colección de varios KeyValuePair.
                  - Cada KeyValuePair : representa cada elemento dentro del diccionario, que tiene una clave (Key) y un valor (Value).
                    Ejemplo:
                    | KeyValuePair | Key (string) | Value (int) |
                    | ------------ | ------------ | ----------- |
                    | 1            | "20/10/2025" | 5           |
                    | 2            | "21/10/2025" | 7           |
                    | 3            | "22/10/2025" | 3           |           */

                foreach (KeyValuePair<string, int> item in await VentasUltimaSemana())
                {
                    // Cada par (clave, valor) representa:
                    //    - Key: la fecha
                    //    - Value: el total de ventas de ese día

                    // Dentro del foreach, "item" representa cada elemento del diccionario (KeyValuePair)
                    // Ejemplo: item.Key = "20/10/2025", item.Value = 5

                    listaVentaSemana.Add(new VentaSemanaDTO() // Se crea un nuevo objeto VentaSemanaDTO con esos datos y se agrega a la lista
                    {
                        Fecha = item.Key,  // Ejemplo: "20/10/2025"
                        Total = item.Value // Ejemplo: 5

                    });
                }

                // Asigna la lista de ventas semanales al objeto del Dashboard
                // Ahora vmDashboard tiene:
                //    - TotalVentas
                //    - TotalIngresos
                //    - TotalProductos
                //    - VentasUltimaSemana (lista de objetos con fecha y total)
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
