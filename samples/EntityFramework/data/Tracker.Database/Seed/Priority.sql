/* Table [dbo].[Priority] data */
SET IDENTITY_INSERT [dbo].[Priority] ON;
GO


MERGE INTO [dbo].[Priority] AS t
USING
(
    VALUES
    (1, N'High', N'High Priority', 1, 1),
    (2, N'Normal', N'Normal Priority', 2, 1),
    (3, N'Low', N'Low Priority', 3, 1)
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

SET IDENTITY_INSERT [dbo].[Priority] OFF;
GO

