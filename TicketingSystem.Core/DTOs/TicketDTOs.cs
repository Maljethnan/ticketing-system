namespace TicketingSystem.Core.DTOs;

public record CreateTicketRequest(string Title, string Description, int SystemId, int IssueTypeId, int PriorityId);
public record FilterRequest(string? Search, int? SystemId, int? StatusId, int? PriorityId, DateTime? FromDate, DateTime? ToDate, int Page = 1, int PageSize = 20);
public record TicketDto(int TicketId, string TicketNumber, string Title, string StatusName, string PriorityName, string SystemName, DateTime CreatedAt);
public record TicketListDto(int TicketId, string TicketNumber, string Title, string StatusName, string PriorityName, string PriorityColor, string SystemName, string AssignedToName, DateTime CreatedAt, int CommentCount);
public record TicketDetailDto(TicketDto Base, string Description, List<CommentDto> Comments, List<AttachmentDto> Attachments, List<StatusChangeDto> StatusHistory, string? ResolutionSummary, bool CanClose, bool CanApprove);
public record CommentDto(int CommentId, string Content, string SenderName, string SenderRole, DateTime SentAt, bool IsEdited, bool CanEdit, bool CanDelete, List<AttachmentDto> Attachments, List<CommentDto> Replies);
public record AttachmentDto(int AttachmentId, string FileName, long FileSizeBytes, string MimeType, DateTime UploadedAt);
public record StatusChangeDto(int OldStatusName, int NewStatusName, string ChangedByName, DateTime ChangedAt, string? Comment);
public record StatusUpdateRequest(int NewStatusId, string? Comment);
public record CloseRequest(string ResolutionSummary);
public record ReopenRequest(string Reason);
public record GrantAccessRequest(int GranteeUserId, string? Reason);
public record AddCommentRequest(string Content, int? ParentCommentId, bool? IsInternal);
public record EditCommentRequest(string Content);
