using apbd_c9.Data;
using apbd_c9.Models;
using apbd_c9.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace apbd_c9.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TripController : ControllerBase
{
    private readonly ApbdContext _context;

    public TripController(ApbdContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetTrips(
        [FromQuery] int pageSize = 10,
        [FromQuery] int page = 1
    )
    {
        if (pageSize <= 0 || page <= 0) return BadRequest();
        var tripsCount = await _context.Trips.CountAsync();
        var allPages = (tripsCount + pageSize - 1) / pageSize;
        if (page > allPages) return BadRequest();
        List<GetTripsTripDTO> dtoTrips = new List<GetTripsTripDTO>();
        await _context.Trips
            .Include(r => r.IdCountries)
            .Include(r => r.ClientTrips)
            .ThenInclude(r => r.IdClientNavigation)
            .Skip(pageSize * (page - 1))
            .Take(pageSize)
            .ForEachAsync(e =>
            {
                dtoTrips.Add(new GetTripsTripDTO()
                {
                    Name = e.Name,
                    Description = e.Description,
                    DateFrom = e.DateFrom.ToLongDateString(),
                    DateTo = e.DateTo.ToLongDateString(),
                    MaxPeople = e.MaxPeople,
                    Countries = e.IdCountries.Select(c => new TripCountryDTO()
                    {
                        Name = c.Name
                    }).ToList(),
                    Clients = e.ClientTrips.Select(t => new TripClientDTO()
                    {
                        FirstName = t.IdClientNavigation.FirstName,
                        LastName = t.IdClientNavigation.LastName
                    }).ToList(),
                });
            });
        return Ok(new GetTripsDTO()
        {
            pageNum = page,
            pageSize = pageSize,
            allPages = allPages,
            trips = dtoTrips
        });
    }

    [HttpPost]
    [Route("{idTrip:int}/clients")]
    public async Task<IActionResult> AssignClient(int idTrip, AssignClientDTO req)
    {
        // Czemu w dwóch miejscach ma być idTrip?
        if (idTrip != req.IdTrip)
        {
            return BadRequest("Different idTrip in URL and POST body");
        }
        if (await _context.Clients.Where(c => c.Pesel == req.Pesel).AnyAsync())
        {
            return BadRequest("Client with the PESEL exists");
        }
        // Punkt 2 nie ma sensu, najpierw mamy sprawdzić czy klient o takim samym PESELu
        // nie istnieje, a potem jeszcze sprawdzić czy nie jest przypisany do wycieczki?
        var trip = await _context.Trips.Where(t => t.IdTrip == idTrip).SingleOrDefaultAsync();
        if (trip is null)
        {
            return BadRequest("Trip doesn't exist");
        }
        if (trip.DateFrom < DateTime.Now)
        {
            return BadRequest("Can't assign to trip which already occured");
        }
        var newCLient = await _context.Clients.AddAsync(new Client()
        {
            FirstName = req.FirstName,
            LastName = req.LastName,
            Pesel = req.Pesel,
            Email = req.Email,
            Telephone = req.Telephone
        });
        await _context.SaveChangesAsync();
        await _context.ClientTrips.AddAsync(new ClientTrip()
        {
            IdClient = newCLient.Entity.IdClient,
            IdTrip = idTrip,
            RegisteredAt = DateTime.Now,
            PaymentDate = req.PaymentDate
        });

        return NoContent();
    }
}