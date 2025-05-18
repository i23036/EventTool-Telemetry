using ET_Backend.Models;
using Xunit;

namespace ET_UnitTests.Unittests
{
    public class ProcessTests
    {
        [Fact]
        public void CanCreateProcess_WithValidData()
        {
            var process = new Process
            {
                Id = 1,
                Name = "Onboarding",
                OrganizationId = 42
            };

            Assert.Equal(1, process.Id);
            Assert.Equal("Onboarding", process.Name);
            Assert.Equal(42, process.OrganizationId);
        }

        [Fact]
        public void Name_CanBeNullOrEmpty()
        {
            var process = new Process();

            process.Name = null;
            Assert.Null(process.Name);

            process.Name = "";
            Assert.Equal("", process.Name);
        }

        [Fact]
        public void OrganizationId_DefaultIsZero()
        {
            var process = new Process();
            Assert.Equal(0, process.OrganizationId);
        }
    }
}
