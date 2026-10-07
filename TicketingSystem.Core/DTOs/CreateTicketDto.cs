using System.Collections.Generic;

namespace TicketingSystem.Core.DTOs;

public class CreateTicketDto
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public int SystemId { get; set; }
    public int CategoryId { get; set; }
    public string Priority { get; set; } = "Medium";
}
