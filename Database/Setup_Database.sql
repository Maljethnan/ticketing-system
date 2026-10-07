-- ===================================================================
-- نظام تتبع المشكلات التقنية — هيئة الرقابة ومكافحة الفساد (نزاها)
-- Database Schema Script for SQL Server 2019+
-- Collation: Arabic_CI_AS
-- ===================================================================

USE [master];
GO

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = N'TicketingSystem')
BEGIN
    CREATE DATABASE [TicketingSystem]
    COLLATE Arabic_CI_AS;
END
GO

USE [TicketingSystem];
GO

-- Enable TDE (optional, requires Enterprise Edition)
-- ALTER DATABASE [TicketingSystem] SET ENCRYPTION ON;
GO

-- ===================================================================
-- TABLES
-- ===================================================================

-- 1. Users
CREATE TABLE [Users] (
    [UserId] INT IDENTITY(1,1) PRIMARY KEY,
    [UserName] NVARCHAR(100) NOT NULL UNIQUE,
    [Email] NVARCHAR(200),
    [PasswordHash] NVARCHAR(MAX) NOT NULL,
    [PhoneNumber] NVARCHAR(20),
    [SubDeptId] INT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [LastLoginAt] DATETIME2,
    [IsActive] BIT DEFAULT 1,
    [IsLocked] BIT DEFAULT 0,
    [TfaSecret] NVARCHAR(255),
    [TfaEnabled] BIT DEFAULT 0,
    [PasswordChangedAt] DATETIME2,
    [FailedLoginAttempts] INT DEFAULT 0,
    [LockedUntil] DATETIME2
);
GO

-- 2. Roles
CREATE TABLE [Roles] (
    [RoleId] INT IDENTITY(1,1) PRIMARY KEY,
    [RoleName] NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(500),
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [IsActive] BIT DEFAULT 1
);
GO

-- 3. UserRoles
CREATE TABLE [UserRoles] (
    [UserId] INT NOT NULL,
    [RoleId] INT NOT NULL,
    [AssignedBy] INT NOT NULL,
    [AssignedAt] DATETIME2 DEFAULT GETUTCDATE(),
    CONSTRAINT PK_UserRoles PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT FK_UserRoles_User FOREIGN KEY ([UserId]) REFERENCES [Users]([UserId]),
    CONSTRAINT FK_UserRoles_Role FOREIGN KEY ([RoleId]) REFERENCES [Roles]([RoleId]),
    CONSTRAINT FK_UserRoles_AssignedBy FOREIGN KEY ([AssignedBy]) REFERENCES [Users]([UserId])
);
GO

-- 4. RolePermissions
CREATE TABLE [RolePermissions] (
    [PermissionId] INT IDENTITY(1,1) PRIMARY KEY,
    [RoleId] INT NOT NULL,
    [Resource] NVARCHAR(100) NOT NULL,
    [Action] NVARCHAR(50) NOT NULL,
    [Scope] NVARCHAR(50) NOT NULL,
    CONSTRAINT FK_RolePermissions_Role FOREIGN KEY ([RoleId]) REFERENCES [Roles]([RoleId])
);
GO

-- 5. GeneralDepartments
CREATE TABLE [GeneralDepartments] (
    [GeneralDeptId] INT IDENTITY(1,1) PRIMARY KEY,
    [DeptName] NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(500),
    [ParentDeptId] INT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [IsActive] BIT DEFAULT 1,
    CONSTRAINT FK_GeneralDepts_Parent FOREIGN KEY ([ParentDeptId]) REFERENCES [GeneralDepartments]([GeneralDeptId])
);
GO

-- 6. SubDepartments
CREATE TABLE [SubDepartments] (
    [SubDeptId] INT IDENTITY(1,1) PRIMARY KEY,
    [DeptName] NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(500),
    [GeneralDeptId] INT NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [IsActive] BIT DEFAULT 1,
    CONSTRAINT FK_SubDepts_General FOREIGN KEY ([GeneralDeptId]) REFERENCES [GeneralDepartments]([GeneralDeptId])
);
GO

-- 7. UserDepartmentAccess
CREATE TABLE [UserDepartmentAccess] (
    [AccessId] INT IDENTITY(1,1) PRIMARY KEY,
    [UserId] INT NOT NULL,
    [SubDeptId] INT NOT NULL,
    [GrantedBy] INT NOT NULL,
    [GrantedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [IsActive] BIT DEFAULT 1,
    CONSTRAINT FK_UserDeptAccess_User FOREIGN KEY ([UserId]) REFERENCES [Users]([UserId]),
    CONSTRAINT FK_UserDeptAccess_Dept FOREIGN KEY ([SubDeptId]) REFERENCES [SubDepartments]([SubDeptId]),
    CONSTRAINT FK_UserDeptAccess_GrantedBy FOREIGN KEY ([GrantedBy]) REFERENCES [Users]([UserId])
);
GO

-- 8. Systems
CREATE TABLE [Systems] (
    [SystemId] INT IDENTITY(1,1) PRIMARY KEY,
    [SystemName] NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(500),
    [Category] NVARCHAR(100),
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [IsActive] BIT DEFAULT 1
);
GO

-- 9. SystemSpecialists
CREATE TABLE [SystemSpecialists] (
    [SystemId] INT NOT NULL,
    [UserId] INT NOT NULL,
    [IsPrimary] BIT DEFAULT 0,
    [AddedBy] INT NOT NULL,
    [AddedAt] DATETIME2 DEFAULT GETUTCDATE(),
    CONSTRAINT PK_SystemSpecialists PRIMARY KEY ([SystemId], [UserId]),
    CONSTRAINT FK_SysSpecs_System FOREIGN KEY ([SystemId]) REFERENCES [Systems]([SystemId]),
    CONSTRAINT FK_SysSpecs_User FOREIGN KEY ([UserId]) REFERENCES [Users]([UserId]),
    CONSTRAINT FK_SysSpecs_AddedBy FOREIGN KEY ([AddedBy]) REFERENCES [Users]([UserId])
);
GO

-- 10. IssueTypes
CREATE TABLE [IssueTypes] (
    [IssueTypeId] INT IDENTITY(1,1) PRIMARY KEY,
    [TypeName] NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(500),
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [IsActive] BIT DEFAULT 1
);
GO

-- 11. Priorities
CREATE TABLE [Priorities] (
    [PriorityId] INT IDENTITY(1,1) PRIMARY KEY,
    [PriorityName] NVARCHAR(50) NOT NULL,
    [SortOrder] INT NOT NULL,
    [ColorCode] NVARCHAR(10),
    [IsActive] BIT DEFAULT 1
);
GO

-- 12. TicketStatuses
CREATE TABLE [TicketStatuses] (
    [StatusId] INT IDENTITY(1,1) PRIMARY KEY,
    [StatusName] NVARCHAR(50) NOT NULL,
    [ArabicName] NVARCHAR(100) NOT NULL,
    [SortOrder] INT NOT NULL,
    [IsFinal] BIT DEFAULT 0,
    [IsActive] BIT DEFAULT 1
);
GO

-- 13. Tickets
CREATE TABLE [Tickets] (
    [TicketId] INT IDENTITY(1,1) PRIMARY KEY,
    [TicketNumber] NVARCHAR(20) NOT NULL UNIQUE,
    [Title] NVARCHAR(300) NOT NULL,
    [Description] NVARCHAR(MAX) NOT NULL,
    [CreatorId] INT NOT NULL,
    [CreatorDeptId] INT NOT NULL,
    [SystemId] INT NOT NULL,
    [IssueTypeId] INT NOT NULL,
    [PriorityId] INT NOT NULL,
    [StatusId] INT NOT NULL,
    [AssignedTo] INT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [ResolvedAt] DATETIME2,
    [ClosedAt] DATETIME2,
    [SLADueDate] DATETIME2,
    [IsSLABreached] BIT DEFAULT 0,
    [ResolutionSummary] NVARCHAR(MAX),
    [ClosureApprovedBy] INT,
    [ClosureApprovedAt] DATETIME2,
    [ReopenReason] NVARCHAR(1000),
    [IsDeleted] BIT DEFAULT 0,
    CONSTRAINT FK_Tickets_Creator FOREIGN KEY ([CreatorId]) REFERENCES [Users]([UserId]),
    CONSTRAINT FK_Tickets_Dept FOREIGN KEY ([CreatorDeptId]) REFERENCES [SubDepartments]([SubDeptId]),
    CONSTRAINT FK_Tickets_System FOREIGN KEY ([SystemId]) REFERENCES [Systems]([SystemId]),
    CONSTRAINT FK_Tickets_IssueType FOREIGN KEY ([IssueTypeId]) REFERENCES [IssueTypes]([IssueTypeId]),
    CONSTRAINT FK_Tickets_Priority FOREIGN KEY ([PriorityId]) REFERENCES [Priorities]([PriorityId]),
    CONSTRAINT FK_Tickets_Status FOREIGN KEY ([StatusId]) REFERENCES [TicketStatuses]([StatusId]),
    CONSTRAINT FK_Tickets_AssignedTo FOREIGN KEY ([AssignedTo]) REFERENCES [Users]([UserId])
);
GO

-- 14. TicketComments
CREATE TABLE [TicketComments] (
    [CommentId] INT IDENTITY(1,1) PRIMARY KEY,
    [TicketId] INT NOT NULL,
    [SenderId] INT NOT NULL,
    [Content] NVARCHAR(MAX) NOT NULL,
    [ParentCommentId] INT NULL,
    [SentAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2,
    [IsEdited] BIT DEFAULT 0,
    [DeletedAt] DATETIME2,
    [DeletedById] INT,
    [IsInternal] BIT DEFAULT 0,
    CONSTRAINT FK_Comments_Ticket FOREIGN KEY ([TicketId]) REFERENCES [Tickets]([TicketId]),
    CONSTRAINT FK_Comments_Sender FOREIGN KEY ([SenderId]) REFERENCES [Users]([UserId]),
    CONSTRAINT FK_Comments_Parent FOREIGN KEY ([ParentCommentId]) REFERENCES [TicketComments]([CommentId])
);
GO

-- 15. TicketAttachments
CREATE TABLE [TicketAttachments] (
    [AttachmentId] INT IDENTITY(1,1) PRIMARY KEY,
    [TicketId] INT NOT NULL,
    [CommentId] INT NULL,
    [UploadedById] INT NOT NULL,
    [FileName] NVARCHAR(300) NOT NULL,
    [FilePath] NVARCHAR(500) NOT NULL,
    [FileSizeBytes] BIGINT NOT NULL,
    [MimeType] NVARCHAR(100),
    [UploadedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [DeletedAt] DATETIME2,
    [DeletedById] INT,
    CONSTRAINT FK_Attachments_Ticket FOREIGN KEY ([TicketId]) REFERENCES [Tickets]([TicketId]),
    CONSTRAINT FK_Attachments_Comment FOREIGN KEY ([CommentId]) REFERENCES [TicketComments]([CommentId]),
    CONSTRAINT FK_Attachments_Uploader FOREIGN KEY ([UploadedById]) REFERENCES [Users]([UserId])
);
GO

-- 16. TicketAccessGrants
CREATE TABLE [TicketAccessGrants] (
    [GrantId] INT IDENTITY(1,1) PRIMARY KEY,
    [TicketId] INT NOT NULL,
    [UserId] INT NOT NULL,
    [GrantedBy] INT NOT NULL,
    [GrantedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [Reason] NVARCHAR(500),
    [IsActive] BIT DEFAULT 1,
    CONSTRAINT FK_AccessGrants_Ticket FOREIGN KEY ([TicketId]) REFERENCES [Tickets]([TicketId]),
    CONSTRAINT FK_AccessGrants_User FOREIGN KEY ([UserId]) REFERENCES [Users]([UserId]),
    CONSTRAINT FK_AccessGrants_GrantedBy FOREIGN KEY ([GrantedBy]) REFERENCES [Users]([UserId])
);
GO

-- 17. TicketStatusHistory
CREATE TABLE [TicketStatusHistory] (
    [HistoryId] INT IDENTITY(1,1) PRIMARY KEY,
    [TicketId] INT NOT NULL,
    [OldStatusId] INT,
    [NewStatusId] INT NOT NULL,
    [ChangedBy] INT NOT NULL,
    [ChangedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [Comment] NVARCHAR(1000),
    CONSTRAINT FK_StatusHist_Ticket FOREIGN KEY ([TicketId]) REFERENCES [Tickets]([TicketId]),
    CONSTRAINT FK_StatusHist_OldStatus FOREIGN KEY ([OldStatusId]) REFERENCES [TicketStatuses]([StatusId]),
    CONSTRAINT FK_StatusHist_NewStatus FOREIGN KEY ([NewStatusId]) REFERENCES [TicketStatuses]([StatusId]),
    CONSTRAINT FK_StatusHist_ChangedBy FOREIGN KEY ([ChangedBy]) REFERENCES [Users]([UserId])
);
GO

-- 18. EscalationRules
CREATE TABLE [EscalationRules] (
    [RuleId] INT IDENTITY(1,1) PRIMARY KEY,
    [PriorityId] INT NOT NULL,
    [StepNumber] INT NOT NULL,
    [WaitMinutes] INT NOT NULL,
    [NotifyRole] NVARCHAR(100) NOT NULL,
    [Action] NVARCHAR(100) NOT NULL,
    [IsActive] BIT DEFAULT 1,
    [CreatedBy] INT NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    CONSTRAINT FK_EscalationRules_Priority FOREIGN KEY ([PriorityId]) REFERENCES [Priorities]([PriorityId]),
    CONSTRAINT FK_EscalationRules_CreatedBy FOREIGN KEY ([CreatedBy]) REFERENCES [Users]([UserId]),
    CONSTRAINT UQ_EscalationRules UNIQUE ([PriorityId], [StepNumber])
);
GO

-- 19. EscalationLogs
CREATE TABLE [EscalationLogs] (
    [LogId] INT IDENTITY(1,1) PRIMARY KEY,
    [TicketId] INT NOT NULL,
    [RuleId] INT NOT NULL,
    [NotifiedUserId] INT NOT NULL,
    [EscalatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [ActionTaken] NVARCHAR(100),
    CONSTRAINT FK_EscalationLogs_Ticket FOREIGN KEY ([TicketId]) REFERENCES [Tickets]([TicketId]),
    CONSTRAINT FK_EscalationLogs_Rule FOREIGN KEY ([RuleId]) REFERENCES [EscalationRules]([RuleId]),
    CONSTRAINT FK_EscalationLogs_User FOREIGN KEY ([NotifiedUserId]) REFERENCES [Users]([UserId])
);
GO

-- 20. AuditLog
CREATE TABLE [AuditLog] (
    [LogId] BIGINT IDENTITY(1,1) PRIMARY KEY,
    [UserId] INT NOT NULL,
    [Action] NVARCHAR(200) NOT NULL,
    [Entity] NVARCHAR(100),
    [EntityId] INT,
    [Details] NVARCHAR(MAX),
    [IpAddress] NVARCHAR(50),
    [LoggedAt] DATETIME2 DEFAULT GETUTCDATE(),
    CONSTRAINT FK_AuditLog_User FOREIGN KEY ([UserId]) REFERENCES [Users]([UserId])
);
GO

-- 21. KnowledgeBase
CREATE TABLE [KnowledgeBase] (
    [KBId] INT IDENTITY(1,1) PRIMARY KEY,
    [Title] NVARCHAR(300) NOT NULL,
    [ProblemDesc] NVARCHAR(MAX) NOT NULL,
    [SolutionDesc] NVARCHAR(MAX) NOT NULL,
    [SystemId] INT NOT NULL,
    [IssueTypeId] INT NOT NULL,
    [SourceTicketId] INT,
    [CreatedBy] INT NOT NULL,
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [ViewsCount] INT DEFAULT 0,
    [IsActive] BIT DEFAULT 1,
    CONSTRAINT FK_KB_System FOREIGN KEY ([SystemId]) REFERENCES [Systems]([SystemId]),
    CONSTRAINT FK_KB_IssueType FOREIGN KEY ([IssueTypeId]) REFERENCES [IssueTypes]([IssueTypeId]),
    CONSTRAINT FK_KB_SourceTicket FOREIGN KEY ([SourceTicketId]) REFERENCES [Tickets]([TicketId]),
    CONSTRAINT FK_KB_CreatedBy FOREIGN KEY ([CreatedBy]) REFERENCES [Users]([UserId])
);
GO

-- 22. Additional tables (Sessions, PasswordPolicies, etc.)
CREATE TABLE [Sessions] (
    [SessionId] INT IDENTITY(1,1) PRIMARY KEY,
    [UserId] INT NOT NULL,
    [TokenHash] NVARCHAR(500) NOT NULL,
    [IpAddress] NVARCHAR(50),
    [UserAgent] NVARCHAR(500),
    [CreatedAt] DATETIME2 DEFAULT GETUTCDATE(),
    [ExpiresAt] DATETIME2 NOT NULL,
    [IsActive] BIT DEFAULT 1,
    CONSTRAINT FK_Sessions_User FOREIGN KEY ([UserId]) REFERENCES [Users]([UserId])
);
GO

CREATE TABLE [PasswordPolicies] (
    [PolicyId] INT IDENTITY(1,1) PRIMARY KEY,
    [MinLength] INT DEFAULT 12,
    [RequireUppercase] BIT DEFAULT 1,
    [RequireLowercase] BIT DEFAULT 1,
    [RequireDigit] BIT DEFAULT 1,
    [RequireSpecialChar] BIT DEFAULT 1,
    [MaxAgeDays] INT DEFAULT 90,
    [HistoryCount] INT DEFAULT 5,
    [LockoutAfterAttempts] INT DEFAULT 5,
    [LockoutDurationMinutes] INT DEFAULT 30,
    [IsActive] BIT DEFAULT 1
);
GO

-- ===================================================================
-- INDEXES
-- ===================================================================
CREATE INDEX IX_Tickets_Status ON [Tickets]([StatusId]);
CREATE INDEX IX_Tickets_CreatedAt ON [Tickets]([CreatedAt]);
CREATE INDEX IX_Tickets_SystemId ON [Tickets]([SystemId]);
CREATE INDEX IX_Tickets_CreatorId ON [Tickets]([CreatorId]);
CREATE INDEX IX_Tickets_AssignedTo ON [Tickets]([AssignedTo]);
CREATE INDEX IX_Tickets_IsDeleted ON [Tickets]([IsDeleted]);
CREATE INDEX IX_Comments_TicketId ON [TicketComments]([TicketId]);
CREATE INDEX IX_Comments_Parent ON [TicketComments]([ParentCommentId]);
CREATE INDEX IX_Attachments_TicketId ON [TicketAttachments]([TicketId]);
CREATE INDEX IX_AuditLog_UserId ON [AuditLog]([UserId]);
CREATE INDEX IX_AuditLog_LoggedAt ON [AuditLog]([LoggedAt]);
CREATE INDEX IX_EscalationLogs_TicketId ON [EscalationLogs]([TicketId]);
GO

-- ===================================================================
-- SEED DATA
-- ===================================================================

-- Roles
INSERT INTO [Roles] ([RoleName], [Description]) VALUES
('Employee', N'موظف عادي'),
('SupportSpecialist', N'موظف دعم فني'),
('SystemManager', N'مدير النظام'),
('DepartmentManager', N'مدير إدارة'),
('GeneralManager', N'مدير عام'),
('Viewer', N'مشاهد');
GO

-- Priorities
INSERT INTO [Priorities] ([PriorityName], [SortOrder], [ColorCode]) VALUES
('Critical', 1, '#DC2626'),
('High', 2, '#EA580C'),
('Medium', 3, '#D97706'),
('Low', 4, '#16A34A');
GO

-- Ticket Statuses
INSERT INTO [TicketStatuses] ([StatusName], [ArabicName], [SortOrder], [IsFinal]) VALUES
('Open', N'مفتوحة', 1, 0),
('Assigned', N'مسندة', 2, 0),
('InProgress', N'قيد المعالجة', 3, 0),
('Resolved', N'محلولة - بانتظار الموافقة', 4, 0),
('Closed', N'مغلقة', 5, 1),
('Reopened', N'أُعيد فتحها', 6, 0);
GO

-- Issue Types
INSERT INTO [IssueTypes] ([TypeName], [Description]) VALUES
('Bug', N'خطأ برمجي'),
('Performance', N'بطء أو أداء ضعيف'),
('ServiceDown', N'توقف خدمة'),
('Security', N'مشكلة أمنية'),
('FeatureRequest', N'طلب ميزة جديدة'),
('Other', N'أخرى');
GO

-- Default Admin (password: Admin@2025!)
-- Note: In production, use a proper password hash. This is a placeholder.
DECLARE @AdminHash NVARCHAR(MAX) = 'c2FsdDEyMzQ1Njc4OTAxMjM0NTY3ODkwMTIzNDU2Nzg5MA==:aGFzaDEyMzQ1Njc4OTAxMjM0NTY3ODkwMTIzNDU2Nzg5MA==:100000';
INSERT INTO [Users] ([UserName], [Email], [PasswordHash], [IsActive])
VALUES ('admin', 'admin@nazaha.gov.sa', @AdminHash, 1);
GO

-- Admin = SystemManager
INSERT INTO [UserRoles] ([UserId], [RoleId], [AssignedBy])
VALUES (1, 3, 1);
GO

-- Default Escalation Rules
INSERT INTO [EscalationRules] ([PriorityId], [StepNumber], [WaitMinutes], [NotifyRole], [Action], [CreatedBy]) VALUES
(1, 1, 60, 'SupportSpecialist', N'تنبيه', 1),
(1, 2, 120, 'DepartmentManager', N'تصعيد', 1),
(1, 3, 240, 'SystemManager', N'تصعيد نهائي', 1),
(2, 1, 180, 'SupportSpecialist', N'تنبيه', 1),
(2, 2, 480, 'SystemManager', N'تصعيد', 1),
(3, 1, 480, 'SupportSpecialist', N'تنبيه', 1),
(3, 2, 1440, 'SystemManager', N'تصعيد', 1),
(4, 1, 1440, 'SupportSpecialist', N'تنبيه', 1);
GO

-- Default Password Policy
INSERT INTO [PasswordPolicies] ([MinLength], [RequireUppercase], [RequireLowercase], [RequireDigit], [RequireSpecialChar], [MaxAgeDays], [HistoryCount], [LockoutAfterAttempts], [LockoutDurationMinutes])
VALUES (12, 1, 1, 1, 1, 90, 5, 5, 30);
GO

PRINT 'Database schema created successfully!';
GO
