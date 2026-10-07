using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketingSystem.Core.DTOs;
using TicketingSystem.Core.Interfaces;

namespace TicketingSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;
    private readonly IPermissionService _permissionService;

    public TicketsController(ITicketService ticketService, IPermissionService permissionService)
    {
        _ticketService = ticketService;
        _permissionService = permissionService;
    }

    private int GetCurrentUserId() => int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTicketRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var ticket = await _ticketService.CreateTicketAsync(request, GetCurrentUserId());
        return CreatedAtAction(nameof(Get), new { id = ticket.TicketId }, ticket);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var ticket = await _ticketService.GetTicketAsync(id, GetCurrentUserId());
        if (ticket == null) return NotFound(new { message = "التذكرة غير موجودة أو ليس لديك صلاحية" });
        return Ok(ticket);
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] FilterRequest filter)
    {
        var tickets = await _ticketService.GetTicketsAsync(filter, GetCurrentUserId());
        return Ok(tickets);
    }

    [HttpGet("my")]
    public async Task<IActionResult> MyTickets()
    {
        return Ok(await _ticketService.GetMyTicketsAsync(GetCurrentUserId()));
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] StatusUpdateRequest request)
    {
        await _ticketService.UpdateTicketStatusAsync(id, request.NewStatusId, GetCurrentUserId(), request.Comment);
        return Ok(new { message = "تم تحديث حالة التذكرة" });
    }

    [HttpPost("{id}/close")]
    public async Task<IActionResult> Close(int id, [FromBody] CloseRequest request)
    {
        await _ticketService.CloseTicketAsync(id, GetCurrentUserId(), request.ResolutionSummary);
        return Ok(new { message = "تم رفع طلب إغلاق التذكرة بانتظار موافقة المنشئ" });
    }

    [HttpPost("{id}/approve-closure")]
    public async Task<IActionResult> ApproveClosure(int id)
    {
        await _ticketService.ApproveClosureAsync(id, GetCurrentUserId());
        return Ok(new { message = "تمت الموافقة على إغلاق التذكرة" });
    }

    [HttpPost("{id}/reopen")]
    public async Task<IActionResult> Reopen(int id, [FromBody] ReopenRequest request)
    {
        await _ticketService.ReopenTicketAsync(id, GetCurrentUserId(), request.Reason);
        return Ok(new { message = "تمت إعادة فتح التذكرة" });
    }

    [HttpPost("{id}/grant-access")]
    public async Task<IActionResult> GrantAccess(int id, [FromBody] GrantAccessRequest request)
    {
        await _ticketService.GrantAccessAsync(id, request.GranteeUserId, GetCurrentUserId(), request.Reason);
        return Ok(new { message = "تم منح صلاحية المشاهدة" });
    }
}
