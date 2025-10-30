using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SistemaVenta.DAL.DBContext;
using SistemaVenta.DAL.Repositorios.Contrato;
using SistemaVenta.Model;

namespace SistemaVenta.DAL.Repositorios
{
    public class VentaRepository : GenericRepository<Venta>, IVentaRepository
    {
        private readonly DbventaContext _dbcontext;

        public VentaRepository(DbventaContext dbcontext) : base(dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task<Venta> Registrar(Venta modelo)
        {
            Venta ventaGenerada = new Venta();

            using (var transaction = _dbcontext.Database.BeginTransaction())
            {
                try 
                {
                    //1.- ACTUALIZAR EL STOCK DE LOS PRODUCTOS VENDIDOS
                    foreach (DetalleVenta dv in modelo.DetalleVenta)
                    {
                        Producto producto_encontrado = _dbcontext.Productos.Where(p => p.IdProducto == dv.IdProducto).First();

                        producto_encontrado.Stock = producto_encontrado.Stock - dv.Cantidad;
                        _dbcontext.Productos.Update(producto_encontrado);
                    }
                    //Guardamos los datos de manera asincrona (Ojo puede ser al final de todo tambien)
                    await _dbcontext.SaveChangesAsync();

                    //2.- OBTENER EL NUMERO DE VENTA
                    NumeroDocumento correlativo = _dbcontext.NumeroDocumentos.First();

                    correlativo.UltimoNumero += 1;
                    correlativo.FechaRegistro = DateTime.Now;

                    _dbcontext.NumeroDocumentos.Update(correlativo);
                    //Guardamos los datos de manera asincrona
                    await _dbcontext.SaveChangesAsync();


                    //3.- GENERAR EL NUMERO DE VENTA
                    int CantidadDigitos = 4; 
                    string ceros = string.Concat(Enumerable.Repeat("0", CantidadDigitos)); //Generar una cadena con 4 ceros "0000"
                    string numeroVenta = ceros + correlativo.UltimoNumero.ToString(); //Concatenar los ceros con el numero
                    //00001  | Otro metodo (string numeroVenta = correlativo.UltimoNumero.ToString("D4");)
                    numeroVenta = numeroVenta.Substring(numeroVenta.Length - CantidadDigitos, CantidadDigitos);

                    modelo.NumeroDocumento = numeroVenta;

                    //4.- REGISTRAR LA VENTA
                    await _dbcontext.Venta.AddAsync(modelo); //Guardamos los datos de manera asincrona
                    await _dbcontext.SaveChangesAsync(); //Guardamos los cambios de manera asincrona
                    
                    ventaGenerada = modelo; //Asignamos la venta generada para devolverla

                    //5.- CONFIRMAR LA TRANSACCION
                    transaction.Commit();
                } 
                catch 
                {
                    transaction.Rollback(); //Revertir todos los cambios en caso de error
                    throw; //Devolver el error
                }
                finally
                {
                    //6.- LIBERAR LOS RECURSOS
                    transaction.Dispose();
                }

                return ventaGenerada;
            }

        }
    }
}
