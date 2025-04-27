/* Table [dbo].[Status] data */
SET IDENTITY_INSERT [dbo].[Status] ON;
GO


MERGE INTO [dbo].[Status] AS t
USING
(
    VALUES
    (1, N'Not Started', N'Not Started', 1, 1),
    (2, N'In Progress', N'In Progress', 2, 1),
    (3, N'Completed', N'Completed', 3, 1),
    (4, N'Blocked', N'Blocked', 4, 1),
    (5, N'Deferred', N'Deferred', 5, 1)
)
AS s
([Id], [Name], [Description], [DisplayOrder], [IsActive])
ON (t.[Id] = s.[Id])
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([Id], [Name], [Description], [DisplayOrder], [IsActive])
    VALUES (s.[Id], s.[Name], s.[Description], s.[DisplayOrder], s.[IsActive])
WHEN MATCHED THEN
    UPDATE SET t.[Name] = s.[Name], t.[Description] = s.[Description], t.[DisplayOrder] = s.[DisplayOrder], t.[IsActive] = s.[IsActive]
OUTPUT $action as MergeAction;

SET IDENTITY_INSERT [dbo].[Status] OFF;
GO

