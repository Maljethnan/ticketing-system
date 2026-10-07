using System;
using System.Collections.Generic;

namespace TicketingSystem.Core.DTOs;

public class TicketDto
{
    public int TicketId { get; set; }
    public string TicketNumber { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public int StatusId { get; set; }
    public string StatusName { get; set; } = "";
    public int PriorityId { get; set; }
    public string PriorityName { get; set; } = "";
    public int SystemId { get; set; }
    public string SystemName { get; set; } = "";
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int CreatedBy { get; set; }
    public string ReporterName { get; set; } = "";
    public int? AssignedTo { get; set; }
    public string AssigneeName { get; set; } = "";
    public List<AttachmentDto>? Attachments { get; set; }
}

public class AttachmentDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = "";
    public long FileSizeBytes { get; set; }
    public DateTime UploadedAt { get; set; }
}

public class CommentDto
{
    public int Id { get; set; }
    public string Text { get; set; } = "";
    public int AuthorId { get; set; }
    public string AuthorName { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

public class FilterRequest
{
    public string? Search { get; set; }
    public int? SystemId { get; set; }
    public int? StatusId { get; set; }
    public int? PriorityId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
