using NetEscapades.EnumGenerators;

namespace Rtfx.Server.Models;

[EnumExtensions]
public enum DatabaseType
{
    Sqlite,
}

[EnumExtensions]
public enum IdType
{
    Feed,
    Package,
    Artifact,
}