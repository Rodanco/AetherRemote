using AetherRemoteCommon.Domain;
using AetherRemoteServer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AetherRemoteServerTests;

public class SandboxTesting
{
    [Test]
    public void Sandbox()
    {
        var networkService = new NetworkService();
        var friends = new List<CommonFriend>();

        networkService.Register("test", "1", friends);
    }
}
