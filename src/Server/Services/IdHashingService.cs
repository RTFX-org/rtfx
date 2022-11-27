using HashidsNet;
using Microsoft.Extensions.Options;
using Rtfx.Server.Models;
using System.Diagnostics;

namespace Rtfx.Server.Services;

public sealed class IdHashingService : IIdHashingService, IDisposable
{
    private Dictionary<IdType, Hashids> _hashids;
    private IDisposable? _onOptionsChangeToken;

    public IdHashingService(IOptionsMonitor<SecurityOptions> securityOptions)
    {
        _hashids = CreateHashidsProviders(securityOptions.CurrentValue.IdHashSalt);

        _onOptionsChangeToken = securityOptions.OnChange(o => _hashids = CreateHashidsProviders(o.IdHashSalt));
    }

    public string EncodeId(long id, IdType type)
    {
        return GetHashidProvider(type).EncodeLong(id);
    }

    public bool TryDecodeId(string? idHash, IdType type, out long id)
    {
        return GetHashidProvider(type).TryDecodeSingleLong(idHash, out id);
    }

    public void Dispose()
    {
        _onOptionsChangeToken?.Dispose();
    }

    private static Dictionary<IdType, Hashids> CreateHashidsProviders(string salt)
    {
        return IdTypeExtensions.GetValues()
            .ToDictionary(
                keySelector: x => x,
                elementSelector: x => CreateHashidsProvider($"{salt}:{x.ToStringFast()}"));
    }

    private static Hashids CreateHashidsProvider(string salt)
    {
        return new Hashids(
            salt: salt,
            minHashLength: 8,
            alphabet: "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890");
    }

    private Hashids GetHashidProvider(IdType type)
    {
        if (_hashids.TryGetValue(type, out var hashid))
            return hashid;
        throw new UnreachableException($"Tried to get hashid provider for id type {(int)type}.");
    }
}
