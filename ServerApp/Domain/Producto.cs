using Communication;
namespace ServerApp.Domain
{
	public class Producto
	{
		private static int globalIdCounter;
		public int id;
		public string Nombre { get; set; }
		public string Descripcion { get; set; }
		public float Precio { get; set; }
		public string Imagen { get; set; }
        public int Stock { get; set; }
		public List<Calificacion> calificaciones;
		public int promedioCalificaciones;

		public Producto(string nombre, string descripcion, float precio, int stock, string imagen = Protocol.NoImage)
		{
			Nombre = nombre;
			Descripcion = descripcion;
			Precio = precio;
			Stock = stock;
			calificaciones = new List<Calificacion>();
			id = globalIdCounter++;
            globalIdCounter++;
			Imagen = imagen;
		}

		public int agregarStock(int cantidad)
		{
			Stock += cantidad;
			return Stock;
        }

        public int quitarStock(int cantidad)
        {
            Stock -= cantidad;
            return Stock;
        }

		private int actualizarPromedioDeCalificaciones()
		{
			int sumaPuntajes = 0;
			int totalPuntajes = 0;

			foreach(Calificacion calificacion in calificaciones)
			{
				sumaPuntajes += calificacion.puntaje;
				totalPuntajes++;
			}

			return promedioCalificaciones = sumaPuntajes / totalPuntajes;
		}

		public List<Calificacion> agregarCalificacion(Calificacion calificacion)
		{
			if (calificacion.producto.Nombre == this.Nombre)
			{
				calificaciones.Add(calificacion);
				actualizarPromedioDeCalificaciones();
                return calificaciones;
            }
			else throw new Exception("Esta calificacion no pertenece a este producto.");
		}
    }
}

