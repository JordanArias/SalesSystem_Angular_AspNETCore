using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SistemaVenta.DAL.Repositorios.Contrato;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using SistemaVenta.DAL.DBContext;
//¿Qué hace GenericRepository<TModelo>?
//Es una clase genérica que encapsula las operaciones CRUD (Create, Read, Update, Delete) para cualquier entidad del sistema, usando Entity Framework Core.

namespace SistemaVenta.DAL.Repositorios
{
    public class GenericRepository<TModelo> : IGenericRepository<TModelo> where TModelo : class
    {
        private readonly DbventaContext _dbcontext;
        public GenericRepository(DbventaContext dbContext)
        {
            _dbcontext = dbContext;
        }

        // Método: Obtener
        // Propósito: buscar y devolver la primera entidad del tipo TModelo que cumpla el filtro indicado.
        // Nota: el filtro es una expresión que EF Core traducirá a SQL.
        public async Task<TModelo> Obtener(Expression<Func<TModelo, bool>> filtro)
        {
            try
            {
                // Acceder al DbSet<TModelo> del DbContext (representa la tabla/colección).
                // _dbcontext.Set<TModelo>() devuelve un IQueryable sobre el que se puede aplicar el filtro.
                // FirstOrDefaultAsync(filtro) ejecuta la consulta en la base de datos y devuelve el primer resultado o null.
                TModelo modelo = await _dbcontext.Set<TModelo>().FirstOrDefaultAsync(filtro);

                // Devolver la entidad encontrada (o null si no existe).
                return modelo;
            }
            catch 
            {
                // Relanzar la excepción para que la capa superior la gestione (preserva la pila de llamadas).
                throw;
            }
        }

        public async Task<TModelo> Crear(TModelo modelo)
        {
            try
            {
                _dbcontext.Set<TModelo>().Add(modelo);
                await _dbcontext.SaveChangesAsync();
                return modelo;
            }
            catch
            {
                throw;
            }
        }
        public async Task<bool> Editar(TModelo modelo)
        {
            try
            {
                _dbcontext.Set<TModelo>().Update(modelo);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            catch
            {
                throw;
            }
        }
        public async Task<bool> Eliminar(TModelo modelo)
        {
            try
            {
                _dbcontext.Set<TModelo>().Remove(modelo);
                await _dbcontext.SaveChangesAsync();
                return true;
            }
            catch
            {
                throw;
            }
        }
        public async Task<IQueryable<TModelo>> Consultar(Expression<Func<TModelo, bool>> filtro = null)
        {
            try
            {
                IQueryable<TModelo> queryModelo = filtro == null ? _dbcontext.Set<TModelo>() : _dbcontext.Set<TModelo>().Where(filtro);
                return queryModelo;
            }
            catch
            {
                throw;
            }
        }

    }
}
