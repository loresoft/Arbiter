/* Table [dbo].[Task] data */
SET IDENTITY_INSERT [dbo].[Task] ON;
GO


MERGE INTO [dbo].[Task] AS t
USING
(
    VALUES
    (1000, N'Make it to Earth', N'Find and make it to Earth', NULL, NULL, NULL, 0, 1000, 2, 1, 1000),
    (1001, N'Destroy Humans', N'Find and destroy all humans', NULL, NULL, NULL, 0, 1001, 2, 1, 1006)
)
AS s
([Id], [Title], [Description], [StartDate], [DueDate], [CompleteDate], [IsDeleted], [TenantId], [StatusId], [PriorityId], [AssignedId])
ON (t.[Id] = s.[Id])
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([Id], [Title], [Description], [StartDate], [DueDate], [CompleteDate], [IsDeleted], [TenantId], [StatusId], [PriorityId], [AssignedId])
    VALUES (s.[Id], s.[Title], s.[Description], s.[StartDate], s.[DueDate], s.[CompleteDate], s.[IsDeleted], s.[TenantId], s.[StatusId], s.[PriorityId], s.[AssignedId])
WHEN MATCHED THEN
    UPDATE SET t.[Title] = s.[Title], t.[Description] = s.[Description], t.[StartDate] = s.[StartDate], t.[DueDate] = s.[DueDate], t.[CompleteDate] = s.[CompleteDate], t.[IsDeleted] = s.[IsDeleted], t.[TenantId] = s.[TenantId], t.[StatusId] = s.[StatusId], t.[PriorityId] = s.[PriorityId], t.[AssignedId] = s.[AssignedId]
OUTPUT $action as MergeAction;

SET IDENTITY_INSERT [dbo].[Task] OFF;
GO

