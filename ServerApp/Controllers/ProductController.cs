using System;
using System.Text;
using Communication;
using ServerApp.Domain;
using ServerApp.Logic;

namespace ServerApp.Controllers
{
	public class ProductController
	{
        private readonly ProductLogic _productLogic = new ProductLogic();
        private readonly UserLogic _userLogic = new UserLogic();
        private readonly string _filesPath;

		public ProductController(string filesPath)
        {
            _filesPath = filesPath;
        }

        public void agregarProductosBase(string nombre,string desc,float precio, int stock, string username) {
            Producto p = new Producto(nombre,desc,precio,stock);

            _productLogic.publicarProducto(p,username);
        }
        public string darProductos() {
            StringBuilder retorno = new StringBuilder();
            int i = 1;
            if (_productLogic.darListadoProductos().ToList().Count > 0)
            {
                foreach (Producto p in _productLogic.darListadoProductos().ToList())
                {
                    retorno.AppendLine(i + "- " + p.Nombre);
                    i++;
                }
            }
            else {
                return "No existen productos registrados";
            }

            return retorno.ToString();
            
        }

        public async Task<string> modificarProducto(MessageCommsHandler msgHandler, FileCommsHandler fileHandler)
        {
            string mensajeACliente = "";
            try
            {
                string info = await msgHandler.ReceiveMessageAsync();
                string[] datos = info.Split('#');
                string username = datos[0];
                string nombreProd = datos[1];
                string atributoAModificar = datos[2].ToLower();
                string nuevoValor = datos[3];

                Producto p = _productLogic.buscarUnProducto(nombreProd);

                if(atributoAModificar == "imagen")
                {
                    validarNombreArchivoSinEspacios(nuevoValor);
                    string imagenAnterior = _productLogic.CambiarImagen(p, username, await DameNombreImagen(nuevoValor));
                    if (imagenAnterior != Protocol.NoImage) await BorrarImagen(_filesPath, imagenAnterior);
                    await fileHandler.ReceiveFileAsync(_filesPath);
                    mensajeACliente = "Imagen del producto actualizada con exito.";
                } else
                {
                    mensajeACliente = _productLogic.modificarProducto(p, username, atributoAModificar, nuevoValor);
                }
            }
            catch(Exception e)
            {
                mensajeACliente = e.Message;
            }
            
            return mensajeACliente;

        }

		public async Task<string> publicarProducto(MessageCommsHandler msgHandler, FileCommsHandler fileCommsHandler)
        {
            string mensajeAlCliente = "";
            try
			{
                // Capturamos la informacion
                string info = await msgHandler.ReceiveMessageAsync();
                string[] datos = info.Split("#");
                string user = datos[0];
                string nombre = datos[1];
                string descripcion = datos[2];
                float precio = float.Parse(datos[3]);
                string pathImagen = datos[4];
                int stock = int.Parse(datos[5]);

                Producto producto;
                validarNombreArchivoSinEspacios(pathImagen);

                if(pathImagen != Protocol.NoImage)
                {
                    // Creamos el producto con la info obtenida
                    producto = new (nombre, descripcion, precio, stock, await DameNombreImagen(pathImagen));
                    // Recibimos la imagen
                    await fileCommsHandler.ReceiveFileAsync(_filesPath);
                } else
                {
                    // Creamos el producto sin imagen
                    producto = new (nombre, descripcion, precio, stock);
                }

                // Llamamos a la logica para publicarlo
                _productLogic.publicarProducto(producto, user);
                mensajeAlCliente = "Producto ingresado con exito: " + nombre;
            }
            catch (Exception e)
			{
                mensajeAlCliente = "Hubo un error: " + e.Message;
			}
            return mensajeAlCliente;
        }

        public async Task<string> eliminarProducto(MessageCommsHandler msgHandler) {
            string retorno = "";
            try {
                string[] datos = (await msgHandler.ReceiveMessageAsync()).Split("#");
                string username = datos[0];
                string nombreProd = datos[1];

                Producto eliminado = _productLogic.eliminarProducto(nombreProd, username);
                if (eliminado.Imagen != Protocol.NoImage) await BorrarImagen(_filesPath, eliminado.Imagen);

                retorno = "Se ha eliminado exitosamente el producto: "+ eliminado.Nombre;
            }
            catch (Exception e)
            {
                retorno = "Hubo un error: " + e.Message;
            }
            return retorno;
        
        }

        public async Task<string> productosBuscados(MessageCommsHandler msgHandler)
        {
            int i = 1;
            List<Producto> listaProd = new List<Producto>();
            StringBuilder retorno = new StringBuilder();
            try {
                // Capturamos la informacion
                string nombreProd = await msgHandler.ReceiveMessageAsync();

                // Buscamos el prodcuto con la informacion
                listaProd = _productLogic.BuscarProductos(nombreProd);

                foreach (Producto producto in listaProd)
                {
                    retorno.AppendLine(i + "- " + producto.Nombre);
                    i++;
                }
                return retorno.ToString();

            }
            catch (Exception e)
            {
                return "Hubo un error: " + e.Message;
            }
            
        }

        public async Task<string> verMasProducto(MessageCommsHandler msgHandler)
        {
            
            StringBuilder retorno = new StringBuilder();
            try
            {
                // Capturamos la informacion
                string nombreProd = await msgHandler.ReceiveMessageAsync();

                // Buscamos el prodcuto con la informacion
                Producto p = _productLogic.VerMasProducto(nombreProd);
                bool vaImagen = p.Imagen != Protocol.NoImage;
                if (vaImagen)
                {
                    retorno.AppendLine("1#"+p.Imagen+"#"); // indicamos que va a recibir imagen
                }
                else
                {
                    retorno.AppendLine("0# #"); // indicamos que no va imagen
                }
                retorno.AppendLine("Nombre: " + p.Nombre);
                retorno.AppendLine("Descripcion: " + p.Descripcion);
                retorno.AppendLine("Precio: " + p.Precio.ToString());
                retorno.AppendLine("Stock: " + p.Stock.ToString());
                if (vaImagen) retorno.AppendLine("Nombre de imagen: " + p.Imagen);

                if (p.calificaciones.Count > 0)
                {
                    retorno.AppendLine("Promedio de calificaciones: " + p.promedioCalificaciones);
                    retorno.AppendLine("Calificaciones: ");
                    foreach (Calificacion cal in p.calificaciones)
                    {
                        retorno.AppendLine("Puntaje: " + cal.puntaje + ". Comentario: " + cal.comentario);
                    }
                } else
                {
                    retorno.AppendLine("El producto aun no ha sido calificado");
                }
                
                return retorno.ToString();
            }
            catch (Exception e)
            {
                return "Hubo un error: " + e.Message;
            }
            

        }

        private async Task<string> DameNombreImagen(string imagen)
        {
           FileHandler _fileHandeler = new FileHandler();

           return await _fileHandeler.GetFileNameAsync(imagen);
        }

        public async Task<string> productosComprados(MessageCommsHandler msgHandler)
        {
            StringBuilder retorno = new StringBuilder();
            try
            {
                // Recibimos el user para mostrarle sus productos comprados
                string user = await msgHandler.ReceiveMessageAsync();
                Usuario u = _userLogic.buscarUsuario(user);

                List<Producto> productos = _userLogic.ProductosComprados(u);

                retorno.AppendLine("Sus productos comprados:");
                foreach (Producto prod in productos)
                {
                    retorno.AppendLine(" - " + prod.Nombre);
                }

                return retorno.ToString();
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public async Task<string> calificarProducto(MessageCommsHandler msgHandler)
        {
            string mensajeAlCliente = "";
            try
            {
                // Capturamos la informacion
                string info = await msgHandler.ReceiveMessageAsync();
                string[] datos = info.Split("#");
                string user = datos[0];
                string nombreProd = datos[1];
                string puntaje = datos[2];
                string comentario = datos[3];

                mensajeAlCliente = _productLogic.calificarProducto(user, nombreProd, puntaje, comentario);
            }
            catch (Exception e)
            {
                mensajeAlCliente = "Hubo un error: " + e.Message;
            }
            return mensajeAlCliente;
        }

        private async Task BorrarImagen(string pathImagenesGuardadas, string nombreImagen)
        {
            FileHandler _fileHandler = new FileHandler();
            await _fileHandler.DeleteFileAsync(pathImagenesGuardadas+nombreImagen);
        }

        private void validarNombreArchivoSinEspacios(string pathImagen)
        {
            string[] datos = pathImagen.Split(" ");
            if (datos.Count() > 1) throw new Exception("La imagen ingresada no puede tener espacios en blanco en su nombre. Proba cambiandolos por '_' ");
        }

        public async Task<string> calificarProductoBase(string username, string nombreProd, string puntaje, string comentario)
        {
            string mensajeAlCliente = "";
            try
            {
                mensajeAlCliente = _productLogic.calificarProducto(username, nombreProd, puntaje, comentario);
            }
            catch (Exception e)
            {
                mensajeAlCliente = "Hubo un error: " + e.Message;
            }
            return mensajeAlCliente;
        }
    }
}

