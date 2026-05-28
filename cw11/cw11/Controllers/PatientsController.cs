using cw11.Models;

namespace cw11.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using cw11.Data;
using cw11.DTOs;

[ApiController]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly _2019sbdContext _context;

    public PatientsController(_2019sbdContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetPatients([FromQuery] string? search)
    {
        var query = _context.Patients.AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => p.FirstName.Contains(search) || p.LastName.Contains(search));
        }

        var result = await query
            .Select(p => new PatientResponseDto
            {
                Pesel = p.Pesel,
                FirstName = p.FirstName,
                LastName = p.LastName,
                Age = p.Age,
                Sex = p.Sex
            })
            .ToListAsync();

        return Ok(result);
    }
    
    [HttpPost("{pesel}/bedassignments")]
    public async Task<IActionResult> AssignBed(string pesel, [FromBody] AssignBedRequestDto request)
    {
        var patientExists = await _context.Patients.AnyAsync(p => p.Pesel == pesel);
        if (!patientExists)
        { 
            return NotFound($"there is no patient with pesel : {pesel}");
        }
        
        if (request.To.HasValue && request.From >= request.To.Value)
        {
            return BadRequest("wrong date");
        }
        
        var availableBed = await _context.Beds
            .Include(b => b.Room)
            .Where(b => b.BedTypeId == request.BedTypeId && b.Room.WardId == request.WardId)
            .Where(b => !_context.BedAssignments.Any(ba => 
                ba.BedId == b.Id &&
                (
                    (request.From >= ba.From && (ba.To == null || request.From < ba.To)) ||
                    (request.To != null && request.To > ba.From && (ba.To == null || request.To <= ba.To)) ||
                    (request.From <= ba.From && (request.To == null || request.To >= ba.To))

                )
            )).FirstOrDefaultAsync(); 
        
        if (availableBed == null)
        {
            return NotFound($"no free bed: {request.BedTypeId} in ward with id: {request.WardId} for thos time perido");
        }

        var newAssignment = new BedAssignment()
        {
            PatientPesel = pesel,
            BedId = availableBed.Id,
            From = request.From,
            To = request.To
        };
        
        _context.BedAssignments.Add(newAssignment);
        await _context.SaveChangesAsync();
        
        return StatusCode(201, new { Message = "patiens has a bed ", BedId = availableBed.Id });

    }

}