namespace ComprasServer
{
    public class Compra
    {
        public string Usuario { get; set; }
        public string NombreProducto { get; set; }
        public float Precio { get; set; }
        public string Fecha { get; set; }
        public string MensajeEntregadoACliente { get; set; }

        public Compra(string user, string producto, float precio, string fecha, string mensaje)
        {
            Usuario = user;
            NombreProducto = producto;
            Precio = precio;
            Fecha = fecha;
            MensajeEntregadoACliente = mensaje;
        }

        // Constructor en caso de error en la compra
        public Compra(string mensaje)
        {
            MensajeEntregadoACliente = mensaje;
        }

        public Compra() { }
    }
}