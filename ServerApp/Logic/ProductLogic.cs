using ServerApp.Database;
using ServerApp.Domain;
using Communication;

namespace ServerApp.Logic
{
    public class ProductLogic
	{
		private readonly SingletonDB _database;
		private readonly UserLogic _userLogic;

		public ProductLogic()
		{
			_database = SingletonDB.GetInstance();
			_userLogic = new UserLogic();
		}
		public List<Producto> darListadoProductos() { 
			return _database.darListaProductos();
		}

		public Producto publicarProducto(Producto producto, string username)
		{
			validarProductoRepetido(producto);
            existeUsuario(username);
            Usuario usuario = _userLogic.buscarUsuario(username);
            _database.agregarProducto(producto);
			usuario.agregarProductoAPublicados(producto);
			return producto;
		}

		private void validarProductoRepetido(Producto producto)
		{
            ValidarNombreRepetido(producto.Nombre);
			if(producto.Imagen != Protocol.NoImage) ValidarImagenRepetida(producto.Imagen);
            if (_database.existeProducto(producto))
			{
				throw new Exception("El producto ya existe publicado en el Marketplace, te ganaron de mano :(");
			}
		}

        public void ValidarImagenRepetida(string imagen)
        {
            if (_database.existeImagen(imagen)) throw new Exception("Una imagen con ese nombre ya existe, proba cambiandolo :)");
        }

        public void ValidarNombreRepetido(string nombre)
        {
            if (buscarProductoPorNombre(nombre).Count > 0) throw new Exception("El nombre de producto ya existe :(");
        }

        public List<Producto> BuscarProductos(string palabra)
		{
			existeProducto(palabra);
			return buscarProductoPorNombre(palabra);
		}
        public List<Producto> buscarProductoPorNombre(string nombre) {
            return _database.buscarProductoPorNombre(nombre);
        }
        public Producto buscarUnProducto(string nombre) {
            return _database.buscarUnProducto(nombre);
        }

		public Producto eliminarProducto(string nombreProd, string username)
		{
			existeProducto(nombreProd);
            existeUsuario(username);
            Producto p = buscarUnProducto(nombreProd);
			tienePermisos(username, nombreProd);

            _database.eliminarProducto(p);
            return p;
        }

		public string modificarProducto (Producto producto, string user, string atributo, string nuevoValor)
		{
            string mensajeACliente = "";

            existeProducto(producto.Nombre);
            existeUsuario(user);
            tienePermisos(user, producto.Nombre);
            switch (atributo)
            {
                case "nombre":
                    ValidarNombreRepetido(nuevoValor);
                    producto.Nombre = nuevoValor;
                    mensajeACliente = "Nombre del producto actualizado con exito.";
                    break;

                case "descripcion":
                    producto.Descripcion = nuevoValor;
                    mensajeACliente = "Descripcion del producto actualizada con exito.";
                    break;

                case "precio":
                    if (float.TryParse(nuevoValor, out float nuevoPrecio))
                    {
                        producto.Precio = nuevoPrecio;
                        mensajeACliente = "Precio del producto actualizado con exito.";
                    }
                    else
                    {
                        mensajeACliente = "El nuevo valor de precio no es valido.";
                    }
                    break;

                case "stock":
                    if (int.TryParse(nuevoValor, out int nuevoStock))
                    {
                        producto.Stock = nuevoStock;
                        mensajeACliente = "Stock del producto actualizado con exito.";
                    }
                    else
                    {
                        mensajeACliente = "El nuevo valor de stock no es valido.";
                    }
                    break;

                default:
                    mensajeACliente = "Atributo no valido. No se realizo ninguna actualizacion.";
                    break;

            }

			return mensajeACliente;
		}
		public string CambiarImagen(Producto producto, string user, string nuevaImagen)
		{
			existeProducto(producto.Nombre);
            Producto prodAModificar = buscarUnProducto(producto.Nombre);
			string imagenAnterior = prodAModificar.Imagen;
            tienePermisos(user, producto.Nombre);
			ValidarImagenRepetida(nuevaImagen);
			producto.Imagen = nuevaImagen;
			return imagenAnterior;
        }
      
        public Producto VerMasProducto(string nombreProd)
		{
			existeProducto(nombreProd);
			return buscarUnProducto(nombreProd);
        }

		public string calificarProducto(string username, string nombreProducto, string puntaje, string comentario)
		{
			string ret = "";
			int punt;
			try
			{
                comproElProducto(username, nombreProducto);
                punt = validarPuntaje(puntaje);
                Producto prod = _database.agregarCalificacion(nombreProducto, punt, comentario);
                ret = "Producto " + prod.Nombre + " calificado con puntaje " + puntaje;
            } catch(Exception e)
			{
				ret = e.Message;
			}
			return ret;

        }

        private int validarPuntaje(string puntaje)
        {
            int p;
            string msjError = "Debe ingresar un valor numerico entero del 1 al 5 ";
            try
            {
                p = int.Parse(puntaje);
                if (p < 1 || p > 5) throw new Exception(msjError);
            } catch(Exception)
            {
                throw new Exception(msjError);
            }
            return p;
        }

        private void comproElProducto(string username, string nombreProducto)
        {
            existeProducto(nombreProducto);
            Usuario u = _userLogic.buscarUsuario(username);
            Producto p = buscarUnProducto(nombreProducto);
            if (!u.comprados.Contains(p)) throw new Exception("No compraste el producto " + p.Nombre);
        }

        private void existeProducto(string nombre)
        {
            if (buscarProductoPorNombre(nombre).Count == 0) throw new Exception("El producto ingresado no existe :(");
        }

        private void tienePermisos(string usuario, string producto)
        {
            if (!esQuienPublicoElProducto(usuario, producto)) throw new Exception("No tiene permiso para modificar un producto que no publico");
        }

        private bool esQuienPublicoElProducto(string username, string nombreProd)
        {
            Usuario user = _userLogic.buscarUsuario(username);
            Producto prod = buscarUnProducto(nombreProd);
            bool es = false;
            if (user.publicados.Contains(prod)) es = true;
            return es;
        }

        private void existeUsuario(string username)
        {
            if (_userLogic.buscarUsuario(username) == null) throw new Exception("El usuario ingresado no existe");
        }
    }
}

