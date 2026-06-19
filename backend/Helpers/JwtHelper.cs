using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace QM_AI.API.Helpers;

public static class JwtHelper
{
    public static SymmetricSecurityKey GetSymmetricSecurityKey(string secret)
    {
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
    }

    public static SigningCredentials GetSigningCredentials(string secret)
    {
        var key = GetSymmetricSecurityKey(secret);
        return new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    }

    public static TokenValidationParameters GetTokenValidationParameters(string secret, string issuer, string audience)
    {
        return new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = GetSymmetricSecurityKey(secret),
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    }
}
