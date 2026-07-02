using System.Diagnostics;
using System.Text;
using Isopoh.Cryptography.Argon2;

const string Email = "test.admin@woopi.local";
const string Password = "Test@123456";
const string UserName = "Test Admin";
const string UserId = "11111111-1111-1111-1111-111111111111";
const int AdminTeamId = 1;
const int AdminProfileId = 2;

var salt = GenerateSalt();
var hash = HashPassword(Password, salt);

var sql = new StringBuilder();
sql.AppendLine("SET NOCOUNT ON;");
sql.AppendLine($"DECLARE @UserId uniqueidentifier = '{UserId}';");
sql.AppendLine($"DECLARE @Email varchar(254) = '{Email}';");
sql.AppendLine($"DECLARE @AdminProfileId int = {AdminProfileId};");
sql.AppendLine();
sql.AppendLine("IF NOT EXISTS (SELECT 1 FROM TeamProfiles WHERE TeamId = 1 AND ProfileId = 2)");
sql.AppendLine("    INSERT INTO TeamProfiles (TeamId, ProfileId) VALUES (1, 2);");
sql.AppendLine();
sql.AppendLine("INSERT INTO ProfilePermissions (ProfileId, PermissionId)");
sql.AppendLine("SELECT @AdminProfileId, p.Id");
sql.AppendLine("FROM Permissions p");
sql.AppendLine("WHERE p.Active = 1");
sql.AppendLine("  AND NOT EXISTS (");
sql.AppendLine("      SELECT 1 FROM ProfilePermissions pp");
sql.AppendLine("      WHERE pp.ProfileId = @AdminProfileId AND pp.PermissionId = p.Id");
sql.AppendLine("  );");
sql.AppendLine();
sql.AppendLine("DELETE FROM UserTeams WHERE UserId = @UserId;");
sql.AppendLine("DELETE FROM Users WHERE Id = @UserId OR Email = @Email;");
sql.AppendLine();
sql.AppendLine("INSERT INTO Users (Id, Name, Email, IsActive, PasswordHash, Salt, Created)");
sql.AppendLine($"VALUES (@UserId, '{UserName}', @Email, 1, {ToSqlHex(hash)}, {ToSqlHex(salt)}, GETDATE());");
sql.AppendLine();
sql.AppendLine($"INSERT INTO UserTeams (UserId, TeamId) VALUES (@UserId, {AdminTeamId});");
sql.AppendLine();
sql.AppendLine("SELECT Id, Name, Email FROM Users WHERE Email = @Email;");

var scriptDir = AppContext.BaseDirectory;
while (!File.Exists(Path.Combine(scriptDir, "SeedLocalTestUser.csproj")) && Directory.GetParent(scriptDir) != null)
    scriptDir = Directory.GetParent(scriptDir)!.FullName;

var sqlFile = Path.Combine(scriptDir, "seed-output.sql");
await File.WriteAllTextAsync(sqlFile, sql.ToString(), Encoding.UTF8);

Console.WriteLine("Usuário de teste (seed local)");
Console.WriteLine($"  E-mail:   {Email}");
Console.WriteLine($"  Senha:    {Password}");
Console.WriteLine($"  Tenant:   local");
Console.WriteLine($"  Time:     Admin (acesso total / isAdmin)");
Console.WriteLine();

var containerSqlPath = "/tmp/woopi-seed-test-user.sql";
using (var copy = Process.Start(new ProcessStartInfo
{
    FileName = "docker",
    Arguments = $"cp \"{sqlFile}\" woopi-ai-mssql-1:{containerSqlPath}",
    RedirectStandardOutput = true,
    RedirectStandardError = true,
    UseShellExecute = false,
})!)
{
    await copy.WaitForExitAsync();
    if (copy.ExitCode != 0)
    {
        Console.Error.WriteLine(await copy.StandardError.ReadToEndAsync());
        Environment.Exit(copy.ExitCode);
    }
}

using var process = Process.Start(new ProcessStartInfo
{
    FileName = "docker",
    Arguments =
        $"exec woopi-ai-mssql-1 /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P \"Strong!WortePass99\" -C -d WoopiAiHub -i {containerSqlPath}",
    RedirectStandardOutput = true,
    RedirectStandardError = true,
    UseShellExecute = false,
})!;

var output = await process.StandardOutput.ReadToEndAsync();
var error = await process.StandardError.ReadToEndAsync();
await process.WaitForExitAsync();

if (!string.IsNullOrWhiteSpace(output))
    Console.WriteLine(output.Trim());

if (process.ExitCode != 0)
{
    Console.Error.WriteLine(error.Trim());
    Environment.Exit(process.ExitCode);
}

static byte[] GenerateSalt(int length = 16)
{
    var salt = new byte[length];
    using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
    rng.GetBytes(salt);
    return salt;
}

static byte[] HashPassword(string password, byte[] saltBytes)
{
    var config = new Argon2Config
    {
        Type = Argon2Type.DataIndependentAddressing,
        Version = Argon2Version.Nineteen,
        TimeCost = 4,
        MemoryCost = 1 << 16,
        Lanes = 4,
        Threads = 4,
        Password = Encoding.UTF8.GetBytes(password),
        Salt = saltBytes,
        HashLength = 32,
    };

    using var argon2 = new Argon2(config);
    using var hashBytes = argon2.Hash();
    var result = new byte[hashBytes.Buffer.Length];
    Array.Copy(hashBytes.Buffer, result, hashBytes.Buffer.Length);
    return result;
}

static string ToSqlHex(byte[] bytes) => "0x" + Convert.ToHexString(bytes);
