using Rtfx.Server.Models;

namespace Rtfx.Server.Services;

public interface IConfigurationService
{
    DatabaseType GetDatabaseType();
    string GetDatabaseConnectionString();

    string GetArtifactStoragePath();

    string GetIdHashSalt();
}
