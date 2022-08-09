using FFPP.Api.v10;

namespace FFPP.Tests.v10;

public class SimulateAuthedRouteCalls
{
    [Fact]
    public void TestCurrentRouteVersion()
    {
        var output = Routes.CurrentRouteVersion();

        Assert.Equal(output, new Routes.CurrentApiRoute());
    }
}