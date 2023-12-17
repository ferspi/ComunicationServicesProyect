using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Admin.Controllers
{
    [Route("admin")]
    [ApiController]
    public class AdminController : Controller
    {
        private Admin.AdminClient client;

        [HttpPost("products")]
        public async Task<ActionResult> PostProduct([FromBody] ProductoNuevoDTO product)
        {
            using var channel = GrpcChannel.ForAddress(ServerConfig.GrpcURL);
            client = new Admin.AdminClient(channel);
            var reply = await client.PostProductAsync(product);
            return Ok(reply.Message);
        }

        [HttpDelete("products")]
        public async Task<ActionResult> DeleteProduct([FromBody] ProductoBorrarDTO product)
        {
            using var channel = GrpcChannel.ForAddress(ServerConfig.GrpcURL);
            client = new Admin.AdminClient(channel);
            var reply = await client.DeleteProductAsync(product);
            return Ok(reply.Message);
        }

        [HttpPut("products")]
        public async Task<ActionResult> PutProduct([FromBody] ProductoModificarDTO product)
        {
            using var channel = GrpcChannel.ForAddress(ServerConfig.GrpcURL);
            client = new Admin.AdminClient(channel);
            var reply = await client.PutProductAsync(product);
            return Ok(reply.Message);
        }

        [HttpPost("compras")]
        public async Task<ActionResult> PostCompra([FromBody] CompraDTO compra)
        {
            using var channel = GrpcChannel.ForAddress(ServerConfig.GrpcURL);
            client = new Admin.AdminClient(channel);
            var reply = await client.PostCompraAsync(compra);
            return Ok(reply.Message);
        }

        [HttpGet("calificaciones")]
        public async Task<ActionResult> GetCalificaciones([FromQuery] CalificacionesDTO nombreProducto)
        {
            using var channel = GrpcChannel.ForAddress(ServerConfig.GrpcURL);
            client = new Admin.AdminClient(channel);
            var reply = await client.GetCalificacionesAsync(nombreProducto);
            return Ok(reply.Message);
        }

    }
}

