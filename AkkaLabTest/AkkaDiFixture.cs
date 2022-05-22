using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace AkkaLabTest;

public class AkkaDiFixture : IDisposable
{
    public IServiceProvider Provider { get; set; }

    public AkkaDiFixture()
    {
        var services = new ServiceCollection();

        var mockFoo = new Mock<IFoo>();
        mockFoo.Setup(g => g.Bar()).Returns(It.IsAny<int>());
        services.AddScoped<Mock<IFoo>>(ctx => mockFoo);
        Provider = services.BuildServiceProvider();
    }


    public void Dispose()
    {
    }
}