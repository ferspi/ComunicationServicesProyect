using ServerApp.Database;
using ServerApp.Domain;

namespace ServerApp.Logic
{
    public class UserLogic
    {
        private readonly SingletonDB _database;

        public UserLogic()
        {
            _database = SingletonDB.GetInstance();
        }

        public int VerificarLogin(string userPass)
        {
            int autenticado = 0;
            string[] userPassArray = userPass.Split('#');
            string user = userPassArray[0];
            string pass = userPassArray[1];
            List<Usuario> usuarios = _database.usuarios();

            Usuario usuario = usuarios.FirstOrDefault(u => u.mail == user);


            if (usuario != null && usuario.clave == pass)
            {
                autenticado = 1;
            }

            return autenticado;
        }
        
        public Usuario agregarUsuario(string mail, string clave)
        {
            Usuario newUser = new Usuario(mail, clave);
            validarUsuarioRepetido(newUser);
            _database.agregarUsuario(newUser);
            return newUser;
        }

        private void validarUsuarioRepetido(Usuario usuario)
        {
            if (_database.existeUsuario(usuario)) throw new Exception("El mail que intentas ingresar ya esta en uso, prueba con otro");
        }

        public List<Producto> darProductosComprados(Usuario u)
        {
            return _database.darListaProductosCompradosPorUsuario(u);
        }

        public List<Producto> agregarProductoACompras(Producto p, string user)
        {
            Usuario u = buscarUsuario(user);
            if (!_database.existeProducto(p)) throw new Exception("El producto que quieres comprar no existe");
            if (!_database.tieneStock(p)) throw new Exception("El producto que quieres comprar no tiene stock disponible");
            _database.agregarProductoACompras(p, u);
            return u.comprados; 
        }

        public Usuario buscarUsuario(string username)
        {
            return _database.buscarUsuario(username) ?? throw new Exception("El usuario no existe");
        }

        public List<Producto> ProductosComprados(Usuario usuario)
        {
            if (!_database.existeUsuario(usuario)) throw new Exception("El usuario no esta registrado ");
            List<Producto> comprados = _database.productosComprados(usuario);
            if (comprados.Count() == 0) throw new Exception("El usuario no compro ningun producto  ");
            return comprados;
        }

    }
}

