namespace Rtfx.Server.Services;

public interface IIdHashingService
{
    string EncodeId(long id);
    bool TryDecodeId(string? idHash, out long id);
}
