using Xunit;
using Microsoft.Extensions.Configuration;
using ET_Backend.Services.Helper.Authentication;

namespace ET_UnitTests.Unittests
{
    public class JwtOptionsSetupTests
    {
        [Fact]
        public void Configure_BindsConfigurationSectionToOptions()
        {
            // Arrange: Test-Konfiguration simulieren
            var inMemorySettings = new Dictionary<string, string>
            {
                {"Jwt:Issuer", "TestIssuer"},
                {"Jwt:Audience", "TestAudience"},
                {"Jwt:ExpirationTime", "5"},
                {"Jwt:SecretKey", "SuperSecretKey"},
                {"Jwt:FrontendBaseUrl", "https://frontend/"},
                {"Jwt:BackendBaseUrl", "https://backend/"}
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var options = new JwtOptions();
            var setup = new JwtOptionsSetup(configuration);

            // Act
            setup.Configure(options);

            // Assert
            Assert.Equal("TestIssuer", options.Issuer);
            Assert.Equal("TestAudience", options.Audience);
            Assert.Equal(5, options.ExpirationTime);
            Assert.Equal("SuperSecretKey", options.SecretKey);
            Assert.Equal("https://frontend/", options.FrontendBaseUrl);
            Assert.Equal("https://backend/", options.BackendBaseUrl);
        }
    }
}
