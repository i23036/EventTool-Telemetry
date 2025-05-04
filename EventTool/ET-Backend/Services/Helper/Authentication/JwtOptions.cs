namespace ET_Backend.Services.Helper.Authentication;

public class JwtOptions
{
    public String Issuer { get; init; }

    public String Audiece { get; init; }

    public int ExperationTime { get; init; }

    public String SecretKey { get; init; }
}