using Communication;
using RabbitMQ.Client;
using ServerApp.Domain;
using ServerApp.Logic;
using System.Text;
using System.Text.Json;

namespace ServerApp.Controllers
{
    public class UserController
    {
        private readonly UserLogic _userLogic;
        private readonly ProductLogic _productLogic;
        public UserController()
        {
            _userLogic = new UserLogic();
            _productLogic = new ProductLogic();
        }

        public int VerificarLogin(string userPass)
        {
            return _userLogic.VerificarLogin(userPass);
        }

        public void crearUsuario(string mail, string clave)
        {
            _userLogic.agregarUsuario(mail, clave);
        }

        public async Task<Compra> agregarProductoACompras(MessageCommsHandler msgHandler) {
            string mensajeACliente = "";
            Compra compra = new Compra(mensajeACliente);
            string user = "";
            string nombreProd = "";
            try {
                int i = 1;
                
                StringBuilder retorno = new StringBuilder();
                string informacionRecibida = await msgHandler.ReceiveMessageAsync();
                string[] info = informacionRecibida.Split("#");
                user = info[0];
                nombreProd = info[1];
                Usuario u = _userLogic.buscarUsuario(user);

                Producto p = _productLogic.buscarUnProducto(nombreProd);

                _userLogic.agregarProductoACompras(p, user);
                retorno = retorno.AppendLine();
                foreach (Producto prod  in _userLogic.darProductosComprados(u))
                {
                    retorno.AppendLine(i + "- " + prod.Nombre);
                    i++;

                }
                mensajeACliente = "Producto agregado a lista de compras" + retorno.ToString();
                compra.Usuario = u.mail;
                compra.NombreProducto = p.Nombre;
                compra.Precio = p.Precio;
                compra.Fecha = DateTime.Now.ToShortDateString();
                compra.MensajeEntregadoACliente = mensajeACliente;

                EnviarMensajeRabbitMQ(compra);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                mensajeACliente = "Hubo un error: " + ex.Message;
                compra.MensajeEntregadoACliente = mensajeACliente;

            }
            return compra;
        }

        public async Task<Compra> agregarProductoAComprasAdmin(string user, string nombreProd)
        {
            string mensajeACliente = "";
            Compra compra = new Compra(mensajeACliente);
            try
            {
                Usuario u = _userLogic.buscarUsuario(user);

                Producto p = _productLogic.buscarUnProducto(nombreProd);

                _userLogic.agregarProductoACompras(p, user);
                string productoComprado = null;
                foreach (Producto prod in _userLogic.darProductosComprados(u))
                {
                    if (prod.Nombre == nombreProd) productoComprado = prod.Nombre; // con esto validamos que se haya agregado

                }
                compra.Usuario = u.mail;
                compra.NombreProducto = p.Nombre;
                compra.Precio = p.Precio;
                compra.Fecha = DateTime.Now.ToShortDateString();
                compra.MensajeEntregadoACliente = "Producto agregado a lista de compras: " + productoComprado;

                EnviarMensajeRabbitMQ(compra);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                mensajeACliente = "Hubo un error: " + ex.Message;
                compra.MensajeEntregadoACliente = mensajeACliente;

            }
            return compra;
        }

        private static void EnviarMensajeRabbitMQ(Compra compra)
        {
            string mensajeARabbit = JsonSerializer.Serialize(compra);
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "Compras", type: ExchangeType.Fanout);

                var body = Encoding.UTF8.GetBytes(mensajeARabbit);

                channel.BasicPublish(exchange: "Compras",
                                     routingKey: "",
                                     basicProperties: null,
                                     body: body);

                Console.WriteLine(" [x] Mensaje Enviado a RabbitMQ: {0}", mensajeARabbit);
            }
        }
    }
}
