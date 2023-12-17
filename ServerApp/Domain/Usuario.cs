namespace ServerApp.Domain
{
	public class Usuario
	{
		public string mail;
		public string clave;
		public List<Producto> comprados;
		public List<Producto> publicados;

		public Usuario(string mail, string clave)
		{
			this.mail = mail;
			this.clave = clave;
			comprados = new List<Producto>();
			publicados = new List<Producto>();
		}

		public List<Producto> agregarProductoAComprados(Producto producto)
		{
			comprados.Add(producto);
			return comprados;
        }

        public List<Producto> agregarProductoAPublicados(Producto producto)
        {
            publicados.Add(producto);
            return publicados;
        }
    }
}

