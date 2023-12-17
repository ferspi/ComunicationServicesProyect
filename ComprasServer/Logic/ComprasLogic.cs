using ComprasServer.Database;

namespace ComprasServer.Logic
{
    public class ComprasLogic
    {
        private readonly SingletonDB _database;

        public ComprasLogic()
        {
            _database = SingletonDB.GetInstance();
        }

        public Compra agregarCompra(Compra compra)
        {
            _database.agregarCompra(compra);
            return compra;
        }

        public List<Compra> darListadoCompras(
            string? usuario = null,
            string? nombreProducto = null,
            string? fecha = null,
            float? precio = null)
        {
            // Llama al método centralizado en SingletonDB para filtrar compras
            return _database.FiltrarCompras(usuario, nombreProducto, fecha, precio);
        }
    }
}
