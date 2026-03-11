using Microsoft.AspNetCore.Mvc;
using OrderAPI.Admin.Services;

namespace OrderAPI.Admin.Controllers
{
    [ApiController]
    [Route("api/admin/orders")]
    public class AdminOrdersController : ControllerBase
    {
        private readonly IAdminOrderService _service;

        public AdminOrdersController(IAdminOrderService service)
        {
            _service = service;
        }

        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenueByYear([FromQuery] int year)
        {
            var result = await _service.GetRevenueByYearAsync(year);
            return Ok(result);
        }
    }
}
