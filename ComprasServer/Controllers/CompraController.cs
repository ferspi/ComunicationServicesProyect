using System;
using System.Collections.Generic;
using ComprasServer.Logic;
using Microsoft.AspNetCore.Mvc;

namespace ComprasServer.Controllers
{
    [ApiController]
    [Route("api/compra")]
    public class CompraController : ControllerBase
    {
        private readonly ComprasLogic _comprasLogic;

        public CompraController(ComprasLogic comprasLogic)
        {
            _comprasLogic = comprasLogic ?? throw new ArgumentNullException(nameof(comprasLogic));
        }

        [HttpGet]
        public ActionResult<List<Compra>> GetListadoCompras(
            [FromQuery] string? usuario = null,
            [FromQuery] string? nombreProducto = null,
            [FromQuery] string? fecha = null,
            [FromQuery] float? precio = null)
        {
            // Llama al método centralizado en ComprasLogic para obtener el listado filtrado
            List<Compra> comprasFiltradas = _comprasLogic.darListadoCompras(usuario, nombreProducto, fecha, precio);
            return Ok(comprasFiltradas);
        }

    }
}