using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SistemaVenta.DTO;

namespace SistemaVenta.BLL.Servicios.Contrato
{
    public interface IUsuarioService
    {
        //Creamos los metodos que va a tener el servicio de usuario
        Task<List<UsuarioDTO>> Lista(); //Método para obtener la lista de usuarios
        Task<SesionDTO> ValidarCredenciales(string correo, string clave); // Método para validar las credenciales del usuario
        Task<UsuarioDTO> Crear(UsuarioDTO modelo); // Método para crear un nuevo usuario
        Task<bool> Editar(UsuarioDTO modelo);
        Task<bool> Eliminar(int id);
    }
}
