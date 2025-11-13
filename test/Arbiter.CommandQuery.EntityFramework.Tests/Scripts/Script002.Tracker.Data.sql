-- Table [dbo].[Priority] data
SET IDENTITY_INSERT [dbo].[Priority] ON;
GO

MERGE INTO [dbo].[Priority] AS t
USING
(
    VALUES
    (1, 'f6bc3530-5b30-4963-9071-1b6d329ef43f', 'High', 'High Priority', 1, 1),
    (2, '7df54a7a-6602-446d-b6f2-6565b684c2cb', 'Normal', 'Normal Priority', 2, 1),
    (3, '66cc427d-2e83-4def-9174-443c8d79e5bb', 'Low', 'Low Priority', 3, 1)
)
AS s
([Id], [Key], [Name], [Description], [DisplayOrder], [IsActive])
ON (t.[Id] = s.[Id])
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([Id], [Key], [Name], [Description], [DisplayOrder], [IsActive])
    VALUES (s.[Id], s.[Key], s.[Name], s.[Description], s.[DisplayOrder], s.[IsActive])
WHEN MATCHED THEN
    UPDATE SET t.[Key] = s.[Key], t.[Name] = s.[Name], t.[Description] = s.[Description], t.[DisplayOrder] = s.[DisplayOrder], t.[IsActive] = s.[IsActive]
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
    (1, '970842cf-84ca-498e-b3bb-1273828735ba', 'Not Started', 'Not Starated', 1, 1),
    (2, '5aa2cebe-be95-4972-ad0f-0b75f6651030', 'In Progress', 'In Progress', 2, 1),
    (3, '7fcf1408-54ed-43a1-8f6b-f1f5aa943438', 'Completed', 'Completed', 3, 1),
    (4, '8290886e-61c6-41d6-b51b-3b9d01a782b3', 'Blocked', 'Blocked', 4, 1),
    (5, 'ab2f6a84-5493-4243-a739-84651babde06', 'Deferred', 'Deferred', 5, 1),
    (6, '514f5ae3-666e-4f70-bdcd-d06af9f9ad5c', 'Done', 'Done', 6, 1)
)
AS s
([Id], [Key], [Name], [Description], [DisplayOrder], [IsActive])
ON (t.[Id] = s.[Id])
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([Id], [Key], [Name], [Description], [DisplayOrder], [IsActive])
    VALUES (s.[Id], s.[Key], s.[Name], s.[Description], s.[DisplayOrder], s.[IsActive])
WHEN MATCHED THEN
    UPDATE SET t.[Key] = s.[Key], t.[Name] = s.[Name], t.[Description] = s.[Description], t.[DisplayOrder] = s.[DisplayOrder], t.[IsActive] = s.[IsActive]
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
    (1, '32ec1124-cb07-4408-bf17-0363fbe47ec2', 'william.adama@battlestar.com', 1, 'William Adama'),
    (2, '687ad891-edb7-4742-8148-c5c179021493', 'laura.roslin@battlestar.com', 1, 'Laura Roslin'),
    (3, 'd4725ebd-5937-40b7-8db8-a385fa438d37', 'kara.thrace@battlestar.com', 1, 'Kara Thrace'),
    (4, 'e7e99361-360f-4351-9d7c-cd37b461825c', 'lee.adama@battlestar.com', 1, 'Lee Adama'),
    (5, '359721fa-ae40-4521-b522-380e2039f128', 'gaius.baltar@battlestar.com', 1, 'Gaius Baltar'),
    (6, '21b88dfd-2ad1-4e93-912a-d99f0bffc905', 'saul.tigh@battlestar.com', 1, 'Saul Tigh')
)
AS s
([Id], [Key], [EmailAddress], [IsEmailAddressConfirmed], [DisplayName])
ON (t.[Id] = s.[Id])
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([Id], [Key], [EmailAddress], [IsEmailAddressConfirmed], [DisplayName])
    VALUES (s.[Id], s.[Key], s.[EmailAddress], s.[IsEmailAddressConfirmed], s.[DisplayName])
WHEN MATCHED THEN
    UPDATE SET t.[Key] = s.[Key], t.[EmailAddress] = s.[EmailAddress], t.[IsEmailAddressConfirmed] = s.[IsEmailAddressConfirmed], t.[DisplayName] = s.[DisplayName]
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
