SET NOCOUNT ON;
DECLARE @UserId uniqueidentifier = '11111111-1111-1111-1111-111111111111';
DECLARE @Email varchar(254) = 'test.admin@woopi.local';
DECLARE @AdminProfileId int = 2;

IF NOT EXISTS (SELECT 1 FROM TeamProfiles WHERE TeamId = 1 AND ProfileId = 2)
    INSERT INTO TeamProfiles (TeamId, ProfileId) VALUES (1, 2);

INSERT INTO ProfilePermissions (ProfileId, PermissionId)
SELECT @AdminProfileId, p.Id
FROM Permissions p
WHERE p.Active = 1
  AND NOT EXISTS (
      SELECT 1 FROM ProfilePermissions pp
      WHERE pp.ProfileId = @AdminProfileId AND pp.PermissionId = p.Id
  );

DELETE FROM UserTeams WHERE UserId = @UserId;
DELETE FROM Users WHERE Id = @UserId OR Email = @Email;

INSERT INTO Users (Id, Name, Email, IsActive, PasswordHash, Salt, Created)
VALUES (@UserId, 'Test Admin', @Email, 1, 0x374F691F98D57813A1D95627A890927C217E25A1265C21DAF2AAEC96C7EEBBAD, 0xCFEE5A152FF2C62DBD387D516C81CD7A, GETDATE());

INSERT INTO UserTeams (UserId, TeamId) VALUES (@UserId, 1);

SELECT Id, Name, Email FROM Users WHERE Email = @Email;
