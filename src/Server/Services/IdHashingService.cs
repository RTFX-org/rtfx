using HashidsNet;

namespace Rtfx.Server.Services;

public class IdHashingService : IIdHashingService
{
    private readonly Hashids _hashids;

    public IdHashingService(IConfigurationService configuration)
    {
        _hashids = new Hashids(
            salt: configuration.GetIdHashSalt(),
            minHashLength: 8,
            alphabet: "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890");
    }

    public string EncodeId(long id)
    {
        return _hashids.EncodeLong(id);
    }

    public bool TryDecodeId(string? idHash, out long id)
    {
        return _hashids.TryDecodeSingleLong(idHash, out id);
    }
}
