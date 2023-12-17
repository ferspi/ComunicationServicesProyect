using System.Net.Sockets;
using System.Net;
using Communication;
using ServerApp.Controllers;
using System.Collections.Specialized;
using System.Configuration;
using RabbitMQ.Client;
using System.Text;
using ServerApp.Domain;
using System.Text.Json;

namespace ServerApp
{
    public class ProgramServer
    {
        
        public static async Task HandleServer()
        {
            SettingsManager settingsMngr = new();
            string filesPath = settingsMngr.ReadSettings(ServerConfig.imagePathconfigkey);
            ProductController _productController = new(filesPath);
            UserController _userController = new();
            NameValueCollection usuarios = ConfigurationManager.GetSection("Usuarios") as NameValueCollection;
            NameValueCollection productos = ConfigurationManager.GetSection("Productos") as NameValueCollection;
            NameValueCollection compras = ConfigurationManager.GetSection("Compras") as NameValueCollection;
            NameValueCollection calificaciones = ConfigurationManager.GetSection("Calificaciones") as NameValueCollection;
            Dictionary<TcpClient, bool> clientesConectados = new();
            bool salir = false;

            ProgramServer server = new ProgramServer();
            server.agregarUsuarios(usuarios, _userController);
            server.agregarProductos(productos, _productController);
            server.agregarCompras(compras, _userController);
            server.agregarCalificaciones(calificaciones, _productController);
            Console.WriteLine("Iniciando Aplicacion Servidor....!!!");
            Console.WriteLine("Para cerrar este servidor ingrese 'salir' en cualquier momento");

            try
            {
                string ipServer = settingsMngr.ReadSettings(ServerConfig.serverIPconfigkey);
                int ipPort = int.Parse(settingsMngr.ReadSettings(ServerConfig.serverPortconfigkey));

                var localEndpoint = new IPEndPoint(IPAddress.Parse(ipServer), ipPort);
                // puertos 0 a 65535   pero del 1 al 1024 estan reservados

                var tcpListener = new TcpListener(localEndpoint);
                Console.WriteLine("Server initialized with IP {0} and Port {1}", ipServer, ipPort);

                tcpListener.Start(2); // Pongo al Servidor en modo escucha

                int clientes = 0;

                var CerrarServidorTask = Task.Run(() => CerrarServidor(tcpListener, salir, clientesConectados)); //todo deberia ser async?

                while (!salir)
                {
                    try
                    {
                        var tcpClient = await tcpListener.AcceptTcpClientAsync();
                        clientesConectados.Add(tcpClient, true);
                        clientes++;
                        int nro = clientes;
                        Console.WriteLine("Acepte un nuevo pedido de Conexion");
                        var tarea = Task.Run(async () => await ManejarClienteAsync(tcpClient, nro, clientesConectados, _userController, _productController, filesPath));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Hubo un error al intentar aceptar un cliente: " + e.Message);
                    }
                }

            }
            catch (SocketException)
            {
                Console.WriteLine("Cliente desconectado por un error");
            }
            catch (Exception e)
            {
                Console.WriteLine("Hubo un error al iniciar el servidor: " + e.Message);
            }

            

            Console.ReadLine(); // no se que hace esto pero el profe lo tiene
        }

        static void CerrarServidor(TcpListener tcpListener, bool salir, Dictionary<TcpClient, bool> clientesConectados)
        {
            try
            {
                salir = Console.ReadLine() == "salir";
                if (salir)
                {
                    int clientenro = 1;
                    // cerramos todas las conexiones con los clientes
                    foreach (TcpClient cliente in clientesConectados.Keys)
                    {
                        if (clientesConectados[cliente]) // desconecta solo los que estaban conectados
                        {
                            cliente.Close();
                            Console.WriteLine("Desconecte cliente {0}", clientenro);
                        }
                        clientenro++;
                    }

                    //cerramos el server
                    tcpListener.Stop();
                    Console.WriteLine("Servidor cerrado con exito");
                }
            } catch(Exception e)
            {
                Console.WriteLine("Hubo un error al cerrar el servidor: " + e.Message);
            }
            
        }

        static async Task ManejarClienteAsync(TcpClient tcpClient, int nro, Dictionary<TcpClient, bool> clientesConectados, UserController _userController, ProductController _productController, string filesPath)
        {
            Console.WriteLine("Cliente {0} conectado", nro);
            MessageCommsHandler msgHandler = new(tcpClient);
            FileCommsHandler fileHandler = new(tcpClient);

            try
            {

                while (clientesConectados.First(c => c.Key == tcpClient).Value)
                {
                    // Leer la seleccion del cliente
                    string comando = await msgHandler.ReceiveMessageAsync();

                    // Procesar la seleccion del cliente
                    bool desconecta = await ProcesarSeleccion(msgHandler, comando, fileHandler, _userController, _productController, filesPath);
                    if(desconecta) clientesConectados[tcpClient] = false;
                }


                Console.WriteLine("Cliente {0} Desconectado", nro);
            }
            catch(Exception e)
            {
                Console.WriteLine("Cliente {0}: {1}", nro, e.Message);
            }
        }

        private static async Task<bool> ProcesarSeleccion(MessageCommsHandler msgHandler, string opcion, FileCommsHandler fileHandler, UserController _userController, ProductController _productController, string filesPath)
        {
            switch (opcion)
            {
                case "0":
                    string datosLogin = await msgHandler.ReceiveMessageAsync();
                    await msgHandler.SendMessageAsync(""+_userController.VerificarLogin(datosLogin));
                    break;
                case "1":
                    await msgHandler.SendMessageAsync(await _productController.publicarProducto(msgHandler, fileHandler));
                    break;
                case "2":
                    Compra compra = await _userController.agregarProductoACompras(msgHandler);
                    await msgHandler.SendMessageAsync(compra.MensajeEntregadoACliente);
                    break;
                case "3":
                    await msgHandler.SendMessageAsync(await _productController.modificarProducto(msgHandler, fileHandler));
                    break;
                case "4":
                    await msgHandler.SendMessageAsync(await _productController.eliminarProducto(msgHandler));
                    break;
                case "5":
                    await msgHandler.SendMessageAsync(await _productController.productosBuscados(msgHandler));
                    break;
                case "6":
                    string info = await _productController.verMasProducto(msgHandler);
                    string[] datos = info.Split("#");
                    string vaImagen = datos[0];
                    string nombreImagen = datos[1];
                    string mensajeAEnviar = datos[2];
                    await msgHandler.SendMessageAsync(mensajeAEnviar);
                    await msgHandler.SendMessageAsync(vaImagen);
                    if (vaImagen == "1")
                    {
                        await fileHandler.SendFileAsync(filesPath + nombreImagen);
                    }
                    break;
                case "7":
                    string productos = await _productController.productosComprados(msgHandler);
                    await msgHandler.SendMessageAsync(productos);
                    if(!productos.Contains("El usuario no compro"))
                    {
                        await msgHandler.SendMessageAsync(await _productController.calificarProducto(msgHandler));
                    }
                    break;
                case "8":
                    await msgHandler.SendMessageAsync(_productController.darProductos());
                    break;
                case "salir":
                    return true;
                default:
                    // Opcion no valida, espera otra opcion del cliente
                    break;
            }
            return false;
        }

        private void agregarUsuarios(NameValueCollection usuarios, UserController _userController)
        {
            foreach (string key in usuarios.AllKeys)
            {
                string[] userInfo = usuarios[key].Split(',');
                string correo = userInfo[0];
                string clave = userInfo[1];

             
                _userController.crearUsuario(correo, clave);
            }
        }

        private void agregarProductos(NameValueCollection productos, ProductController _productController) {
            foreach (string key in productos.AllKeys)
            {
                string[] prodInfo = productos[key].Split(',');
                string nombreProd = prodInfo[0];
                string descrProd = prodInfo[1];
                float precio = float.Parse(prodInfo[2]);
                int stock = int.Parse(prodInfo[3]);
                string username = prodInfo[4];

                _productController.agregarProductosBase(nombreProd,descrProd,precio,stock, username);
            }
        }

        private void agregarCompras(NameValueCollection compras, UserController userController)
        {
            foreach (string key in compras.AllKeys)
            {
                string[] compraInfo = compras[key].Split(',');
                string nombreProd = compraInfo[0];
                string username = compraInfo[1];

                userController.agregarProductoAComprasAdmin(username, nombreProd);
            }
            
        }

        private void agregarCalificaciones(NameValueCollection calificaciones, ProductController productController)
        {
            foreach (string key in calificaciones.AllKeys)
            {
                string[] calificacionInfo = calificaciones[key].Split(',');
                string nombreProd = calificacionInfo[0];
                string username = calificacionInfo[1];
                string puntaje = calificacionInfo[2];
                string comentario = calificacionInfo[3];

                productController.calificarProductoBase(username, nombreProd, puntaje, comentario);
            }
        }

    }
}

