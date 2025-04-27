/* Table [dbo].[User] data */
SET IDENTITY_INSERT [dbo].[User] ON;
GO


MERGE INTO [dbo].[User] AS t
USING
(
    VALUES
    (1000, N'William Adama', N'william.adama@battlestar.com', 0),
    (1001, N'Laura Roslin', N'laura.roslin@battlestar.com', 0),
    (1002, N'Kara Thrace', N'kara.thrace@battlestar.com', 0),
    (1003, N'Lee Adama', N'lee.adama@battlestar.com', 0),
    (1004, N'Gaius Baltar', N'gaius.baltar@battlestar.com', 0),
    (1005, N'Saul Tigh', N'saul.tigh@battlestar.com', 0),
    (1006, N'Number Six', N'six@cylon.com', 0)
)
AS s
([Id], [DisplayName], [EmailAddress], [IsDeleted])
ON (t.[Id] = s.[Id])
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([Id], [DisplayName], [EmailAddress], [IsDeleted])
    VALUES (s.[Id], s.[DisplayName], s.[EmailAddress], s.[IsDeleted])
WHEN MATCHED THEN
    UPDATE SET t.[DisplayName] = s.[DisplayName], t.[EmailAddress] = s.[EmailAddress], t.[IsDeleted] = s.[IsDeleted]
OUTPUT $action as MergeAction;

SET IDENTITY_INSERT [dbo].[User] OFF;
GO

