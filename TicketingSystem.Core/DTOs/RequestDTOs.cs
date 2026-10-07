using System;

namespace TicketingSystem.Core.DTOs;

public class AddCommentRequest
{
    public string Text { get; set; } = "";
    public string? Content { get; set; }
    public int? ParentCommentId { get; set; }
    public bool IsInternal { get; set; }
    public AddCommentRequest(string text) => Text = text;
}

public class EditCommentRequest
{
    public string Text { get; set; } = "";
    public string? Content { get; set; }
    public EditCommentRequest(string text) => Text = text;
}

public class StatusUpdateRequest
{
    public int StatusId { get; set; }
    public int? NewStatusId { get; set; }
    public string? Note { get; set; }
    public string? Comment { get; set; }
    public StatusUpdateRequest(int statusId, string? note = null)
    {
        StatusId = statusId;
        NewStatusId = statusId;
        Note = note;
        Comment = note;
    }
}

public class CloseRequest
{
    public string ResolutionNote { get; set; } = "";
    public string? ResolutionSummary { get; set; }
    public CloseRequest(string resolutionNote = "")
    {
        ResolutionNote = resolutionNote;
        ResolutionSummary = resolutionNote;
    }
}

public class ReopenRequest
{
    public string Reason { get; set; } = "";
    public ReopenRequest(string reason = "") => Reason = reason;
}

public class GrantAccessRequest
{
    public int UserId { get; set; }
    public int? GranteeUserId { get; set; }
    public string? Reason { get; set; }
}
