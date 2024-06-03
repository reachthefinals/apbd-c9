using apbd_c9.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace apbd_c9.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly ApbdContext _context;

    public ClientsController(ApbdContext context)
    {
        _context = context;
    }

    [HttpDelete]
    [Route("{idClient:int}")]
    public async Task<IActionResult> DeleteClient(int idClient)
    {
        if (await _context.ClientTrips.Where(e => e.IdClient == idClient).AnyAsync())
        {
            return BadRequest("Client still has trips assigned");
        }
        await _context.Clients.Where(e => e.IdClient == idClient).ExecuteDeleteAsync();
        return NoContent();
    }
}