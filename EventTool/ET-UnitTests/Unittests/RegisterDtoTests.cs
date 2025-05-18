using System.ComponentModel.DataAnnotations;
using ET.Shared.DTOs;
using Xunit;
using System.Collections.Generic;
using System.Linq;

namespace ET_UnitTests.Unittests
{
    public class RegisterDtoTests
    {
        [Fact]
        public void RegisterDto_WithValidData_IsValid()
        {
            // Arrange
            var dto = new RegisterDto("Max", "Mustermann", "max@firma.de", "geheim");


            // Act
            var context = new ValidationContext(dto, null, null);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(dto, context, results, true);

            // Assert
            Assert.True(isValid);
            Assert.Empty(results);
        }

    }
}
