using HashidsNet;
using Microsoft.Extensions.Options;

namespace Rtfx.Server.Services;

public sealed class IdHashingService : IIdHashingService, IDisposable
{
    private Hashids _hashids;
    private IDisposable? _onOptionsChangeToken;

    public IdHashingService(IOptionsMonitor<SecurityOptions> securityOptions)
    {
        _hashids = CreateHashidsProvider(securityOptions.CurrentValue.IdHashSalt);

        _onOptionsChangeToken = securityOptions.OnChange(o => _hashids = CreateHashidsProvider(o.IdHashSalt));
    }

    public string EncodeId(long id)
    {
        return _hashids.EncodeLong(id);
    }

    public bool TryDecodeId(string? idHash, out long id)
    {
        return _hashids.TryDecodeSingleLong(idHash, out id);
    }

    public void Dispose()
    {
        _onOptionsChangeToken?.Dispose();
    }

    private static Hashids CreateHashidsProvider(string salt)
    {
        return new Hashids(
            salt: salt,
            minHashLength: 8,
            alphabet: "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890");
    }
}
