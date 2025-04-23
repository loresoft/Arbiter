-- Table [dbo].[Priority] data
SET IDENTITY_INSERT [dbo].[Priority] ON;
GO

MERGE INTO [dbo].[Priority] AS t
USING
(
    VALUES
    (1, 'High', 'High Priority', 1, 1),
    (2, 'Normal', 'Normal Priority', 2, 1),
    (3, 'Low', 'Low Priority', 3, 1)
)
AS s
([Id], [Name], [Description], [DisplayOrder], [IsActive])
ON (t.[Id] = s.[Id])
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([Id], [Name], [Description], [DisplayOrder], [IsActive])
    VALUES (s.[Id], s.[Name], s.[Description], s.[DisplayOrder], s.[IsActive])
WHEN MATCHED THEN
    UPDATE SET t.[Name] = s.[Name], t.[Description] = s.[Description], t.[DisplayOrder] = s.[DisplayOrder], t.[IsActive] = s.[IsActive]
OUTPUT $action as [Action];

SET IDENTITY_INSERT [dbo].[Priority] OFF;
GO

-- Table [dbo].[Status] data
SET IDENTITY_INSERT [dbo].[Status] ON;
GO

MERGE INTO [dbo].[Status] AS t
USING
(
    VALUES
    (1, 'Not Started', 'Not Starated', 1, 1),
    (2, 'In Progress', 'In Progress', 2, 1),
    (3, 'Completed', 'Completed', 3, 1),
    (4, 'Blocked', 'Blocked', 4, 1),
    (5, 'Deferred', 'Deferred', 5, 1),
    (6, 'Done', 'Done', 6, 1)
)
AS s
([Id], [Name], [Description], [DisplayOrder], [IsActive])
ON (t.[Id] = s.[Id])
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([Id], [Name], [Description], [DisplayOrder], [IsActive])
    VALUES (s.[Id], s.[Name], s.[Description], s.[DisplayOrder], s.[IsActive])
WHEN MATCHED THEN
    UPDATE SET t.[Name] = s.[Name], t.[Description] = s.[Description], t.[DisplayOrder] = s.[DisplayOrder], t.[IsActive] = s.[IsActive]
OUTPUT $action as [Action];

SET IDENTITY_INSERT [dbo].[Status] OFF;
GO

-- Table [dbo].[User] data
SET IDENTITY_INSERT [dbo].[User] ON;
GO

MERGE INTO [dbo].[User] AS t
USING
(
    VALUES
    (1, 'william.adama@battlestar.com', 1, 'William Adama'),
    (2, 'laura.roslin@battlestar.com', 1, 'Laura Roslin'),
    (3, 'kara.thrace@battlestar.com', 1, 'Kara Thrace'),
    (4, 'lee.adama@battlestar.com', 1, 'Lee Adama'),
    (5, 'gaius.baltar@battlestar.com', 1, 'Gaius Baltar'),
    (6, 'saul.tigh@battlestar.com', 1, 'Saul Tigh')
)
AS s
([Id], [EmailAddress], [IsEmailAddressConfirmed], [DisplayName])
ON (t.[Id] = s.[Id])
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([Id], [EmailAddress], [IsEmailAddressConfirmed], [DisplayName])
    VALUES (s.[Id], s.[EmailAddress], s.[IsEmailAddressConfirmed], s.[DisplayName])
WHEN MATCHED THEN
    UPDATE SET t.[EmailAddress] = s.[EmailAddress], t.[IsEmailAddressConfirmed] = s.[IsEmailAddressConfirmed], t.[DisplayName] = s.[DisplayName]
OUTPUT $action as [Action];

SET IDENTITY_INSERT [dbo].[User] OFF;
GO

-- Table [dbo].[Role] data
SET IDENTITY_INSERT [dbo].[Role] ON;
GO

MERGE INTO [dbo].[Role] AS t
USING
(
    VALUES
    (1, 'Administrator', 'Administrator'),
    (2, 'Manager', 'Manager'),
    (3, 'Member', 'Member')
)
AS s
([Id], [Name], [Description])
ON (t.[Id] = s.[Id])
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([Id], [Name], [Description])
    VALUES (s.[Id], s.[Name], s.[Description])
WHEN MATCHED THEN
    UPDATE SET t.[Name] = s.[Name], t.[Description] = s.[Description]
OUTPUT $action as [Action];

SET IDENTITY_INSERT [dbo].[Role] OFF;
GO

-- Table [dbo].[Tenant] data
SET IDENTITY_INSERT [dbo].[Tenant] ON;
GO

MERGE INTO [dbo].[Tenant] AS t
USING
(
    VALUES
    (1, 'Test', 'Test Tenant', 1)
)
AS s
([Id], [Name], [Description], [IsActive])
ON (t.[Id] = s.[Id])
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([Id], [Name], [Description], [IsActive])
    VALUES (s.[Id], s.[Name], s.[Description], s.[IsActive])
WHEN MATCHED THEN
    UPDATE SET t.[Name] = s.[Name], t.[Description] = s.[Description], t.[IsActive] = s.[IsActive]
OUTPUT $action as [Action];

SET IDENTITY_INSERT [dbo].[Tenant] OFF;
GO
