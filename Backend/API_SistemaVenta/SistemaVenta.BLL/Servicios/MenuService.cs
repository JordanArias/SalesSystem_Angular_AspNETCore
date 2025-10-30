using System;
using System.Collections.Generic;
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
    public class MenuService : IMenuService
    {
        private readonly IGenericRepository<Usuario> _usuarioRepositorio; // Repositorio genérico para la entidad Usuario
        private readonly IGenericRepository<MenuRol> _menuRolRepositorio;
        private readonly IGenericRepository<Menu> _menuRepositorio;
        private readonly IMapper _mapper;

        public MenuService(IGenericRepository<Usuario> usuarioRepositorio, IGenericRepository<MenuRol> menuRolRepositorio, IGenericRepository<Menu> menuRepositorio, IMapper mapper)
        {
            _usuarioRepositorio = usuarioRepositorio;
            _menuRolRepositorio = menuRolRepositorio;
            _menuRepositorio = menuRepositorio;
            _mapper = mapper;
        }

        public async Task<List<MenuDTO>> Lista(int idUsuario)
        {
            // Hace una consulta para obtener el usuario con el ID proporcionado, solo un usuario
            // IQueryable<Usuario> (una consulta pendiente, no ejecutada todavía).
            IQueryable<Usuario> tbUsuario = await _usuarioRepositorio.Consultar(u => u.IdUsuario == idUsuario);
            
            // Hace una consulta para obtener todos los registros de MenuRol
            IQueryable<MenuRol> tbMenuRol = await _menuRolRepositorio.Consultar();

            // Hace una consulta para obtener todos los registros de Menu
            // Cada menú representa una opción visible en la interfaz (Dashboard, Ventas, Productos, etc.)
            IQueryable<Menu> tbMenu = await _menuRepositorio.Consultar();

            try
            {
                IQueryable<Menu> tbResultado = (from u in tbUsuario
                                                join mr in tbMenuRol on u.IdRol equals mr.IdRol
                                                join m in tbMenu on mr.IdMenu equals m.IdMenu
                                                // Selecciona únicamente los objetos del menú (Menu),
                                                // ignorando los datos de Usuario y MenuRol resultantes de los joins.
                                                select m)
                                                .AsQueryable(); // AsQueryable() convierte el resultado a IQueryable nuevamente


                // Ejecuta la consulta (ToList() fuerza la ejecución en la base de datos)
                var listaMenus = tbResultado.ToList();

                // Mapea la lista de entidades Menu a objetos MenuDTO con AutoMapper
                // Esto transforma las entidades de base de datos en objetos más ligeros
                // que se envían al frontend.
                return _mapper.Map<List<MenuDTO>>(listaMenus);
            }
            catch
            {
                throw;
            }
        }
    }
}
