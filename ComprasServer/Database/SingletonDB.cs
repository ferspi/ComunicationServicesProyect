namespace ComprasServer.Database
{
    public class SingletonDB
    {
        private SingletonDB()
        {
            _compras = new List<Compra>();
        }

        private static SingletonDB? _instance;
        private List<Compra> _compras;


        private static readonly object _lock = new object();

        public static SingletonDB GetInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new SingletonDB();
                    }
                }
            }
            return _instance;
        }
        public List<Compra> darListaCompras()
        {
            return this._compras;
        }

        public List<Compra> agregarCompra(Compra compra)
        {
            _compras.Add(compra);
            return _compras;
        }

        public List<Compra> FiltrarCompras(
            string? usuario = null,
            string? nombreProducto = null,
            string? fecha = null,
            float? precio = null)
        {
            var comprasFiltradas = _compras
                .Where(compra =>
                    (usuario == null || compra.Usuario.Equals(usuario, StringComparison.OrdinalIgnoreCase)) &&
                    (nombreProducto == null || compra.NombreProducto.Contains(nombreProducto, StringComparison.OrdinalIgnoreCase)) &&
                    (fecha == null || compra.Fecha == fecha) &&
                    (!precio.HasValue || compra.Precio == precio.Value))
                .ToList();

            return comprasFiltradas;
        }
    }
}
