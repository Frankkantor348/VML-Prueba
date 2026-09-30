using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using VmlTest.Application.Abstractions.Security;
using VmlTest.Domain.Users;

namespace VmlTest.Infrastructure.Security;

/// <summary>
/// Adaptador de <see cref="ITokenGenerator"/> que emite un JWT firmado con HS256.
/// </summary>
public sealed class JwtTokenGenerator : ITokenGenerator
{
    private readonly JwtOptions _options;

    public JwtTokenGenerator(IOptions<JwtOptions> options) => _options = options.Value;

    public AccessToken Generate(User user)
    {
        var expira = DateTimeOffset.UtcNow.AddMinutes(_options.ExpirationMinutes);

        var claims = new List<Claim>
        {
            // 'sub' es el identificador del usuario: es lo que leerá el cliente para
            // saber de quién es la sesión.
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email.Value),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var credenciales = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expira.UtcDateTime,
            signingCredentials: credenciales);

        return new AccessToken(new JwtSecurityTokenHandler().WriteToken(token), expira);
    }
}
