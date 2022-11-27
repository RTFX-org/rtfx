using Rtfx.Server.Models;

namespace Rtfx.Server.Services;

public interface IIdHashingService
{
    string EncodeId(long id, IdType type);
    bool TryDecodeId(string? idHash, IdType type, out long id);
}
