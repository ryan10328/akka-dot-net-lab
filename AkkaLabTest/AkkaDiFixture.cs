using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace AkkaLabTest;

public class AkkaDiFixture : IDisposable
{
    public void Dispose()
    {
    }

    public ServiceProvider SetupTestWithDependencies(Action<ServiceCollection> registration)
    {
        var services = new ServiceCollection();
        registration(services); // let the caller determine how many services should register
        // provider(services.BuildServiceProvider());

        var provider = services.BuildServiceProvider();
        return provider;
    }
    
}