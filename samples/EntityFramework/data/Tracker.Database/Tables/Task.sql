CREATE TABLE [dbo].[Task]
(
    [Id] INT IDENTITY (1000, 1) NOT NULL,

    [Title] NVARCHAR(255) NOT NULL,
    [Description] NVARCHAR(MAX) NULL,

    [StartDate] DATETIMEOFFSET NULL,
    [DueDate] DATETIMEOFFSET NULL,
    [CompleteDate] DATETIMEOFFSET NULL,

    [IsDeleted] BIT NOT NULL CONSTRAINT [DF_Task_IsDeleted] DEFAULT (0),

    [TenantId] INT NOT NULL,
    [StatusId] INT NOT NULL,
    [PriorityId] INT NULL,
    [AssignedId] INT NULL,

    [Created] DATETIMEOFFSET NOT NULL CONSTRAINT [DF_Task_Created] DEFAULT (SYSUTCDATETIME()),
    [CreatedBy] NVARCHAR(100) NULL,
    [Updated] DATETIMEOFFSET NOT NULL CONSTRAINT [DF_Task_Updated] DEFAULT (SYSUTCDATETIME()),
    [UpdatedBy] NVARCHAR(100) NULL,
    [RowVersion] ROWVERSION NOT NULL,

    CONSTRAINT [PK_Task] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Task_Status_StatusId] FOREIGN KEY ([StatusId]) REFERENCES [dbo].[Status] ([Id]),
    CONSTRAINT [FK_Task_Priority_PriorityId] FOREIGN KEY ([PriorityId]) REFERENCES [dbo].[Priority] ([Id]),
    CONSTRAINT [FK_Task_Tenant_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenant] ([Id]),
    CONSTRAINT [FK_Task_User_AssignedId] FOREIGN KEY ([AssignedId]) REFERENCES [dbo].[User] ([Id]),

    INDEX [IX_Task_Title] NONCLUSTERED ([Title] ASC),
    INDEX [IX_Task_StatusId] NONCLUSTERED ([StatusId] ASC),
    INDEX [IX_Task_PriorityId] NONCLUSTERED ([PriorityId] ASC),
    INDEX [IX_Task_TenantId] NONCLUSTERED ([TenantId] ASC),
    INDEX [IX_Task_AssignedId] NONCLUSTERED ([AssignedId] ASC),
    INDEX [IX_Task_IsDeleted] NONCLUSTERED ([IsDeleted] ASC),
);
