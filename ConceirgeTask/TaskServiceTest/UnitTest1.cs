using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;

namespace TaskServiceTest
{
    public class UnitTest1
    {
        private readonly Mock<IAmADependency> _mockDependency;
        private readonly IFixture _fixture;

        public UnitTest1()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _mockDependency = _fixture.Freeze<Mock<IAmADependency>>();
        }

        [Fact]
        public async Task Test1()
        {
            _mockDependency.Setup(d => d.DoSomething(It.IsAny<string>())).ReturnsAsync((string a) => a.ToUpper());
            var sut = _fixture.Create<Sut>();
            var dto = _fixture.Create<RandomDto>();
            await sut.PrefixWithDance(dto);
            Assert.StartsWith("Dance", dto.Frodo);
        }
    }


    public interface IAmADependency
    {
        Task<string> DoSomething(string someParam);
    }

    public class RandomDto
    {
        public string Frodo { get; set; }
        public int Bilbo { get; set; }
    }

    public class Sut
    {
        private readonly IAmADependency _dependency;

        public Sut(IAmADependency dependency)
        {
            _dependency = dependency;
        }

        public async Task PrefixWithDance(RandomDto dto)
        {
            var foo = await _dependency.DoSomething(dto.Frodo);
            dto.Frodo = $"Dance{foo}";
        }
    }
}

