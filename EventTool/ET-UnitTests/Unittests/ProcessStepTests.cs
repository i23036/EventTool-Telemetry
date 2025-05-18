using ET_Backend.Models;
using Xunit;

namespace ET_UnitTests.Unittests
{
    public class ProcessStepTests
    {
        [Fact]
        public void CanCreateProcessStep_WithValidData()
        {
            var step = new ProcessStep
            {
                Id = 1,
                Name = "Willkommen",
                OrganizationId = 42,
                TriggerId = 5
            };

            Assert.Equal(1, step.Id);
            Assert.Equal("Willkommen", step.Name);
            Assert.Equal(42, step.OrganizationId);
            Assert.Equal(5, step.TriggerId);
        }

        [Fact]
        public void Name_CanBeNullOrEmpty()
        {
            var step = new ProcessStep();

            step.Name = null;
            Assert.Null(step.Name);

            step.Name = "";
            Assert.Equal("", step.Name);
        }

        [Fact]
        public void OrganizationId_And_TriggerId_DefaultIsZero()
        {
            var step = new ProcessStep();
            Assert.Equal(0, step.OrganizationId);
            Assert.Equal(0, step.TriggerId);
        }
    }
}
