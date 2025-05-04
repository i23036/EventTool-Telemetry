using Microsoft.Extensions.Options;

namespace ET_Backend.Services.Helper.Authentication;

public class JwtOptionsSetup : IConfigureOptions<JwtOptions>
{
    private IConfiguration _configuration;

    public JwtOptionsSetup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Configure(JwtOptions options)
    {
        _configuration.GetSection("Jwt").Bind(options);
    }
}