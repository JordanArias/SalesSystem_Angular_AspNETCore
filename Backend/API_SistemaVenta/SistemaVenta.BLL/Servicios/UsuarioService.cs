using System;
using System.Collections.Generic;
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
    public class UsuarioService : IUsuarioService
    {
        private readonly IGenericRepository<Usuario> _usuarioRepositorio;
        private readonly IMapper _mapper;

        public UsuarioService(IGenericRepository<Usuario> usuarioRepositorio, IMapper mapper)
        {
            _usuarioRepositorio = usuarioRepositorio;
            _mapper = mapper;
        }

        public async Task<List<UsuarioDTO>> Lista()
        {
            try 
            {
                var queryUsuario = await _usuarioRepositorio.Consultar(); // Obtener la consulta de usuarios, pero no ejecuta aun
                var listaUsuarios = queryUsuario.Include(rol => rol.IdRolNavigation).ToList(); //Ejecuta la consulta incluyendo la relacion con Rol
                //La consulta no se ejecuta hasta que llamamos a: .ToListAsync(), .FirstAsync(), .First() o .Count()

                return _mapper.Map<List<UsuarioDTO>>(listaUsuarios); // Devuelve la lista mapeada a DTOs
            }
            catch
            {
                throw;
            }
        }

        public async Task<SesionDTO> ValidarCredenciales(string correo, string clave)
        {
            try
            {
                // Consulta para buscar el usuario con el correo y clave proporcionados
                var queryUsuario = await _usuarioRepositorio.Consultar(u =>
                    u.Correo == correo &&
                    u.Clave == clave
                );

                // Verificar si se encontró un usuario que coincida
                if (queryUsuario.FirstOrDefault() == null) 
                {
                    throw new TaskCanceledException("El usuario no existe");
                }

                // Devolver el usuario con su rol incluido
                Usuario devolverUsuario = queryUsuario.Include(rol => rol.IdRolNavigation).First();
                //La consulta no se ejecuta hasta que llamamos a: .ToListAsync(), .FirstAsync(), .First() o .Count()

                // Devolver el usuario mapeado a SesionDTO
                return _mapper.Map<SesionDTO>(devolverUsuario);

            }
            catch
            {
                throw;
            }
        }

        public async Task<UsuarioDTO> Crear(UsuarioDTO modelo)
        {
            try
            {
                // Crea el usuario mapeando el DTO a la entidad Usuario, por que el repositorio trabaja con la entidad y no con el DTO
                var usuarioCreado = await _usuarioRepositorio.Crear(_mapper.Map<Usuario>(modelo));

                // Si el IdUsuario es 0, significa que no se creó correctamente
                if (usuarioCreado.IdUsuario == 0)
                {
                    throw new TaskCanceledException("No se pudo crear");
                }

                // Consulta para obtener el usuario creado con su rol incluido
                var query = await _usuarioRepositorio.Consultar(u => u.IdUsuario == usuarioCreado.IdUsuario);

                // Obtener el usuario con el rol incluido
                usuarioCreado = query.Include(rol => rol.IdRolNavigation).First();

                // Devolver el usuario creado mapeado a UsuarioDTO
                return _mapper.Map<UsuarioDTO>(usuarioCreado); // Retorna el usuario creado como DTO
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> Editar(UsuarioDTO modelo)
        {
            try
            {
                // Mapear el DTO a la entidad Usuario
                var usuarioModelo = _mapper.Map<Usuario>(modelo);
                // Verificar si el usuario existe, obteniendo el usuario por su IdUsuario
                var usuarioEncontrado = await _usuarioRepositorio.Obtener(u => u.IdUsuario == usuarioModelo.IdUsuario);

                if (usuarioEncontrado == null)
                {
                    throw new TaskCanceledException("El usuario no existe");
                }

                // Actualizar los campos del usuario encontrado con los valores del modelo
                usuarioEncontrado.NombreCompleto = usuarioModelo.NombreCompleto;
                usuarioEncontrado.Correo = usuarioModelo.Correo;
                usuarioEncontrado.IdRol = usuarioModelo.IdRol;
                usuarioEncontrado.Clave = usuarioModelo.Clave;
                usuarioEncontrado.EsActivo = usuarioModelo.EsActivo;

                // Llamar al repositorio para editar el usuario
                bool respuesta = await _usuarioRepositorio.Editar(usuarioEncontrado); //Retorna true o false

                // Verificar si la edición fue exitosa
                if (!respuesta)
                {
                    throw new TaskCanceledException("No se pudo editar");
                }

                return respuesta; // Retorna true si se editó correctamente

            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> Eliminar(int id)
        {
            try
            {
                // Obtener el usuario por su IdUsuario
                var usuarioEncontrado = await _usuarioRepositorio.Obtener(u => u.IdUsuario == id);

                if (usuarioEncontrado == null)
                {
                    throw new TaskCanceledException("El usuario no existe");
                }

                // Llamar al repositorio para eliminar el usuario
                bool respuesta = await _usuarioRepositorio.Eliminar(usuarioEncontrado);

                // Verificar si la eliminación fue exitosa
                if (!respuesta)
                {
                    throw new TaskCanceledException("No se pudo eliminar");
                }

                return respuesta; // Retorna true si se eliminó correctamente
            }
            catch
            {
                throw;
            }
        }

    }
}
