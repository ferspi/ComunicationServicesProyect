using System.Net.Sockets;
using System.Net;
using System.Text;
using Communication;

namespace ClientApp
{
    public class ProgramCliente
    {
        static readonly SettingsManager settingsMngr = new SettingsManager();
        private static string filesPath = settingsMngr.ReadSettings(ClientConfig.imagePathconfigkey);
        private static string elegirOtraOpcionMsg = "Ingrese un valor del menu principal para realizar otra accion";
        private static string noEstaLogueadoMsg = "Para realizar esta accion debes estar logeado";
        private static bool estaAutenticado = false;
        private static string user = "";

        public static async Task Main(string[] args)
        {
            bool parar = false;
            Console.WriteLine("Iniciando Aplicacion Cliente....!!!");

            while (!parar)
            {
                Console.WriteLine("Presione enter para conectarse al Servidor");
                if (Console.ReadLine()!= null)
                {
                    TcpClient tcpClient = await ConectarseAlServidorAsync();

                    MessageCommsHandler msgHandler = new MessageCommsHandler(tcpClient);
                    FileCommsHandler fileHandler = new FileCommsHandler(tcpClient);

                    try
                    {
                        Console.WriteLine(GetMenu());
                        while (!parar)
                        {
                            string comando = Console.ReadLine();

                            switch (comando)
                            {
                                case "0":
                                    await Autenticarse(msgHandler);

                                    Console.WriteLine(elegirOtraOpcionMsg);
                                    break;
                                case "1":
                                    if (estaAutenticado) await PublicarProducto(msgHandler, fileHandler);
                                    else Console.WriteLine(noEstaLogueadoMsg);

                                    Console.WriteLine(elegirOtraOpcionMsg);
                                    break;
                                case "2":
                                    if (estaAutenticado) await ComprarProducto(msgHandler);
                                    else Console.WriteLine(noEstaLogueadoMsg);

                                    Console.WriteLine(elegirOtraOpcionMsg);
                                    break;
                                case "3":
                                    if (estaAutenticado) await ModificarProducto(msgHandler, fileHandler);
                                    else Console.WriteLine(noEstaLogueadoMsg);

                                    Console.WriteLine(elegirOtraOpcionMsg);
                                    break;
                                case "4":
                                    if (estaAutenticado) await EliminarProducto(msgHandler);
                                    else Console.WriteLine(noEstaLogueadoMsg);

                                    Console.WriteLine(elegirOtraOpcionMsg);
                                    break;
                                case "5":
                                    await BuscarProducto(msgHandler);

                                    Console.WriteLine(elegirOtraOpcionMsg);
                                    break;
                                case "6":
                                    await VerMasInfoProducto(msgHandler, fileHandler);

                                    Console.WriteLine(elegirOtraOpcionMsg);
                                    break;
                                case "7":
                                    if (estaAutenticado) await CalificarProducto(msgHandler);
                                    else Console.WriteLine(noEstaLogueadoMsg);

                                    Console.WriteLine(elegirOtraOpcionMsg);
                                    break;
                                case "8":
                                    await VerTodosProductos(msgHandler);
                                    
                                    Console.WriteLine(elegirOtraOpcionMsg);
                                    break;
                                case "salir":
                                    parar = true;
                                    Console.WriteLine("Desconectando");
                                    await msgHandler.SendMessageAsync("salir");
                                    tcpClient.Close();
                                    break;
                                default:
                                    Console.WriteLine("Opcion no valida. Ingrese un valor dentro las opciones indicadas previamente.");
                                    break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error de comunicacion: " + ex.Message);
                    }
                    finally
                    {
                        // Cerrar la conexion.
                        tcpClient.Close();
                        Console.WriteLine("Cliente cerrado");
                    }
                }
                
            }
                    
        }

        private static async Task<TcpClient> ConectarseAlServidorAsync()
        {
            string ipServer = settingsMngr.ReadSettings(ClientConfig.serverIPconfigkey);
            string ipClient = settingsMngr.ReadSettings(ClientConfig.clientIPconfigkey);
            int serverPort = int.Parse(settingsMngr.ReadSettings(ClientConfig.serverPortconfigkey));

            var localEndPoint = new IPEndPoint(IPAddress.Parse(ipClient), 0);
            var serverEndpoint = new IPEndPoint(IPAddress.Parse(ipServer), serverPort);
            var tcpClient = new TcpClient(localEndPoint);
            await tcpClient.ConnectAsync(serverEndpoint);
            Console.WriteLine("Cliente Conectado al Servidor...!!!");
            return tcpClient;
        }



        private static string GetMenu()
        {
            // Define el menu y devuelve su representacion en cadena
            StringBuilder menu = new StringBuilder();
            menu.AppendLine("****************************");
            menu.AppendLine("Bienvenido a dreamteam Shop");
            menu.AppendLine("* Seleccione 0 para iniciar sesion");
            menu.AppendLine("* Seleccione 1 para publicar un producto");
            menu.AppendLine("* Seleccione 2 para comprar un producto");
            menu.AppendLine("* Seleccione 3 para modificar un producto publicado");
            menu.AppendLine("* Seleccione 4 para eliminar un producto");
            menu.AppendLine("* Seleccione 5 para buscar un producto");
            menu.AppendLine("* Seleccione 6 para ver mas acerca de un producto");
            menu.AppendLine("* Seleccione 7 para calificar un producto");
            menu.AppendLine("* Seleccione 8 para ver el listado de productos");
            menu.AppendLine("Escriba 'salir' para desconectarse");
            menu.AppendLine("Muchas gracias por elegirnos!");
            menu.AppendLine("****************************");
            return menu.ToString();
        }

        private static async Task Autenticarse(MessageCommsHandler msgHandler)
        {
            await msgHandler.SendMessageAsync("0");
            string credenciales = Login.PedirDatosLogin();
            await msgHandler.SendMessageAsync(credenciales);

            string respuesta = await msgHandler.ReceiveMessageAsync();

            if (respuesta == "1")
            {
                Console.WriteLine("Login exitoso");
                estaAutenticado = true;
                user = credenciales.Split("#")[0];
            }
            else
            {
                Console.WriteLine("Usuario o contraseña incorrecta");
            }
        }

        private static async Task PublicarProducto(MessageCommsHandler msgHandler, FileCommsHandler fileHandler)
        {
            Console.WriteLine("Selecciono la opcion 1: Publicar un producto");

            // Le pedimos la informacion al cliente
            Console.WriteLine("Ingrese nombre del producto");
            string nombre = Console.ReadLine();

            Console.WriteLine("Ingrese una descripcion para su producto");
            string descripcion = Console.ReadLine();

            Console.WriteLine("Ingrese el precio");
            string precio = Console.ReadLine();

            Console.WriteLine("Desea ingresar una imagen? Responda 'si' para cargar imagen, enter para seguir sin subir imagen");
            bool subeImagen = false;
            string imagen = Protocol.NoImage;

            if (Console.ReadLine() == "si")
            {
                subeImagen = true;
                Console.WriteLine("Ingrese la ruta al archivo de imagen");
                imagen = Console.ReadLine();
            }

            Console.WriteLine("Ingrese el stock disponible");
            string stock = Console.ReadLine();

            //Mandamos al server el comando
            await msgHandler.SendMessageAsync("1");

            //Mandamos al server la informacion
            string info = user + "#" + nombre + "#" + descripcion + "#" + precio + "#" + imagen + "#" + stock;
            await msgHandler.SendMessageAsync(info);

            if (subeImagen)
            {
                //Mandamos al server el archivo de imagen
                await fileHandler.SendFileAsync(imagen);
            }

            // Esperamos exito o error del server
            Console.WriteLine(await msgHandler.ReceiveMessageAsync());
        }

        private static async Task ComprarProducto(MessageCommsHandler msgHandler)
        {
            Console.WriteLine("Selecciono la opcion 2: Comprar un producto");
            Console.WriteLine("Para comprar un producto porfavor ingrese el nombre de producto a comprar");
            string nombreProductoAComprar = Console.ReadLine();
            await msgHandler.SendMessageAsync("2");
            await msgHandler.SendMessageAsync(user + "#" + nombreProductoAComprar);
            Console.WriteLine(await msgHandler.ReceiveMessageAsync());
        }

        private static async Task ModificarProducto(MessageCommsHandler msgHandler, FileCommsHandler fileHandler)
        {
            Console.WriteLine("Selecciono la opcion 3: Modificar un producto publicado");
            Console.WriteLine("Para modificar porfavor ingrese el nombre de producto a modificar");

            string nombreProducto = Console.ReadLine();

            Console.WriteLine("Ingrese que atributo quiere modificar");
            string atributoAModificar = Console.ReadLine();

            Console.WriteLine("Ingrese el nuevo valor del atributo seleccionado, en caso de la imagen ingrese la ruta completa");
            string nuevoValorDelAtributo = Console.ReadLine();

            await msgHandler.SendMessageAsync("3");

            string informacion = user + "#" + nombreProducto + "#" + atributoAModificar + "#" + nuevoValorDelAtributo;

            await msgHandler.SendMessageAsync(informacion);

            if (atributoAModificar.ToLower() == "imagen")
            {
                await fileHandler.SendFileAsync(nuevoValorDelAtributo);
            }

            // Esperamos exito o error del server
            Console.WriteLine(await msgHandler.ReceiveMessageAsync());
        }

        private static async Task EliminarProducto(MessageCommsHandler msgHandler)
        {
            Console.WriteLine("Selecciono la opcion 4: Eliminar un producto");
            Console.WriteLine("Para eliminar porfavor ingrese el nombre del producto que quiere eliminar");
            string nombreProductoABorrar = Console.ReadLine();
            //Mandamos al server el comando
            await msgHandler.SendMessageAsync("4");
            //Mandamos al server la informacion
            await msgHandler.SendMessageAsync(user + "#" + nombreProductoABorrar);
            // Esperamos exito o error del server
            Console.WriteLine(await msgHandler.ReceiveMessageAsync());
        }

        private static async Task BuscarProducto(MessageCommsHandler msgHandler)
        {
            Console.WriteLine("Selecciono la opcion 5: Buscar un producto");
            Console.WriteLine("Para buscar porfavor ingrese alguna letra que contenga el nombre del producto que busca");
            string textoABuscar = Console.ReadLine();
            //Mandamos al server el comando
            await msgHandler.SendMessageAsync("5");
            //Mandamos al server la informacion
            await msgHandler.SendMessageAsync(textoABuscar);
            // Esperamos exito o error del server
            Console.WriteLine(await msgHandler.ReceiveMessageAsync());
        }

        private static async Task VerMasInfoProducto(MessageCommsHandler msgHandler, FileCommsHandler fileHandler)
        {
            Console.WriteLine("Selecciono la opcion 6: Ver mas acerca de un producto");
            Console.WriteLine("Para buscar porfavor ingrese nombre del producto que quiere ver mas informacion");
            string nombreProductoMasInfo = Console.ReadLine();
            //Mandamos al server el comando
            await msgHandler.SendMessageAsync("6");
            //Mandamos al server la informacion
            await msgHandler.SendMessageAsync(nombreProductoMasInfo);
            // Esperamos exito o error del server
            Console.WriteLine(await msgHandler.ReceiveMessageAsync());
            // Validamos si debemos recibir una imagen asociada
            string vieneImagen = await msgHandler.ReceiveMessageAsync();
            if (vieneImagen == "1")
            {
                await fileHandler.ReceiveFileAsync(filesPath);
                Console.WriteLine("Recibiste la imagen en tu carpeta seleccionada: " + filesPath);
            }
        }

        private static async Task CalificarProducto(MessageCommsHandler msgHandler)
        {
            Console.WriteLine("Selecciono la opcion 7: Calificar un producto");
            // Enviamos el comando y mostramos el listado de productos comprados por el user
            await msgHandler.SendMessageAsync("7");
            await msgHandler.SendMessageAsync(user);
            string productos = await msgHandler.ReceiveMessageAsync();
            Console.WriteLine(productos);

            if (!productos.Contains("El usuario no compro"))
            {
                // Le pedimos la informacion para calificar
                Console.WriteLine("Ingrese el nombre del producto que desea calificar");
                string productoACalificar = Console.ReadLine();
                Console.WriteLine("Ingrese un valor entero del 1 al 5 para calificar su producto");
                string puntaje = Console.ReadLine();
                Console.WriteLine("Escriba un comentario, si no quiere dejar comentario presione enter");
                string comentario = Console.ReadLine();

                await msgHandler.SendMessageAsync(user + "#" + productoACalificar + "#" + puntaje + "#" + comentario);
                Console.WriteLine(await msgHandler.ReceiveMessageAsync());
            }
        }

        private static async Task VerTodosProductos(MessageCommsHandler msgHandler)
        {
            Console.WriteLine("Selecciono la opcion 8: Ver todos los productos");
            await msgHandler.SendMessageAsync("8");
            Console.WriteLine(await msgHandler.ReceiveMessageAsync());
        }
    }
}
