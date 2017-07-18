using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Ploeh.AutoFixture.Xunit2;


namespace TaskServiceTest
{
    /**
     
 ______   _______  ______      _________ _______  _______ _________ _______ 
(  ___ \ (  ___  )(  __  \     \__   __/(  ____ \(  ____ \\__   __/(  ____ \
| (   ) )| (   ) || (  \  )       ) (   | (    \/| (    \/   ) (   | (    \/
| (__/ / | (___) || |   ) |       | |   | (__    | (_____    | |   | (_____ 
|  __ (  |  ___  || |   | |       | |   |  __)   (_____  )   | |   (_____  )
| (  \ \ | (   ) || |   ) |       | |   | (            ) |   | |         ) |
| )___) )| )   ( || (__/  )       | |   | (____/\/\____) |   | |   /\____) |
|/ \___/ |/     \|(______/        )_(   (_______/\_______)   )_(   \_______)
                                                                            

     */


    public class UnitTest1
    {
        private readonly IFixture _fixture;

        public UnitTest1()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
        }

        [Fact]
        public async Task Test1()
        {
            Mock<IAmADependency> _mockDependency = _fixture.Freeze<Mock<IAmADependency>>();
            _mockDependency.Setup(d => d.DoSomething(It.IsAny<string>())).ReturnsAsync((string a) => a.ToUpper());
            var sut = _fixture.Create<Sut>();
            var dto = _fixture.Create<RandomDto>();
            await sut.PrefixWithDance(dto);
            Assert.StartsWith("DANCE", dto.Frodo);
        }

        [Theory, AutoMoqData]
        public async Task Test2([Frozen] Mock<IAmADependency> deps, Sut sut, RandomDto dto)
        {
            var charlie = dto.Frodo;
            deps.Setup(d => d.DoSomething(It.IsAny<string>())).ReturnsAsync("Apple");
            await sut.PrefixWithDance(dto);
            deps.Verify(d => d.DoSomething(charlie), Times.Once);
        }
    }


    [AttributeUsage(AttributeTargets.All)]
    public class AutoMoqDataAttribute : AutoDataAttribute
    {
        public AutoMoqDataAttribute()
            : base(new Fixture()
                .Customize(new AutoMoqCustomization()))
        {
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
            dto.Frodo = $"DANCE{foo}";
        }
    }
}