CREATE TABLE [dbo].[Status]
(
    [Id] INT IDENTITY (1, 1) NOT NULL,

    [Name] NVARCHAR(100) NOT NULL,
    [Description] NVARCHAR(255) NULL,

    [DisplayOrder] INT NOT NULL CONSTRAINT [DF_Status_DisplayOrder] DEFAULT (0),
    [IsActive] BIT NOT NULL CONSTRAINT [DF_Status_IsActive] DEFAULT (1),

    [Created] DATETIMEOFFSET NOT NULL CONSTRAINT [DF_Status_Created] DEFAULT (SYSUTCDATETIME()),
    [CreatedBy] NVARCHAR(100) NULL,
    [Updated] DATETIMEOFFSET NOT NULL CONSTRAINT [DF_Status_Updated] DEFAULT (SYSUTCDATETIME()),
    [UpdatedBy] NVARCHAR(100) NULL,
    [RowVersion] ROWVERSION NOT NULL,

    CONSTRAINT [PK_Status] PRIMARY KEY CLUSTERED ([Id] ASC),

    INDEX [IX_Status_Name] NONCLUSTERED ([Name] ASC),
    INDEX [IX_Status_DisplayOrder_IsActive] NONCLUSTERED ([DisplayOrder] ASC, [IsActive] ASC),
);
