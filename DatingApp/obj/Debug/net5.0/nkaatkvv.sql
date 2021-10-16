BEGIN TRANSACTION;
GO

ALTER TABLE [Users] ADD [PasswordHash] varbinary(max) NULL;
GO

ALTER TABLE [Users] ADD [PasswordSalt] varbinary(max) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20211015120916_AddFieldsPassword', N'5.0.11');
GO

COMMIT;
GO

