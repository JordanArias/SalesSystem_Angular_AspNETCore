using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SistemaVenta.DTO;

namespace SistemaVenta.BLL.Servicios.Contrato
{
    public interface IProductoService
    {
        //Creamos los metodos que va a tener el servicio de categoria
        Task<List<ProductoDTO>> Lista(); //Método para obtener la lista de usuarios
        Task<ProductoDTO> Crear(ProductoDTO modelo); // Método para crear un nuevo usuario
        Task<bool> Editar(ProductoDTO modelo);
        Task<bool> Eliminar(int id);
    }
}
