using CIPP_API_ALT.Api.v10;

namespace CIPP_API_ALT.Tests.v10;

public class SimulateAuthedRouteCalls
{
    [Fact]
    public void TestCurrentRouteVersion()
    {
        var output = Routes.CurrentRouteVersion();

        Assert.Equal(output, new Routes.CurrentApiRoute());
    }
}