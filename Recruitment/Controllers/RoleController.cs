using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Recruitment.Data;

namespace Recruitment.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : Controller
    {
        private readonly AppDbContext _context;

        public RoleController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var vendedores = await _context.Role.ToListAsync();
            return Ok(vendedores);
        }
    }
}
