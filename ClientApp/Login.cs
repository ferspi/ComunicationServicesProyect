namespace ClientApp
{
    public class Login
    {
        public static string PedirDatosLogin()
        {
            Console.WriteLine("Selecciono la opcion 0: Iniciar sesion");
            Console.WriteLine("Ingrese su usuario: ");
            string user = Console.ReadLine();
            Console.WriteLine("Ingrese su contraseña: ");
            string pass = Console.ReadLine();
            return user + "#" + pass;
        }
    }
}
