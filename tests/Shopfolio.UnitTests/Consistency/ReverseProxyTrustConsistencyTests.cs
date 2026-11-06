using System.Text.RegularExpressions;

using Shopfolio.Infrastructure.Security;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// Keeps the application's trust of the reverse proxy equal to the Compose file (T215 KK3, D-112; 09 15.1 item 8; 05 6.3.3, 8.9.3): the networks the
/// forwarded headers are believed from are exactly the subnets of the <c>edge</c> network, and only the proxy publishes a port - so a request cannot reach
/// the application from outside with a header of its own. The Compose file keeps one fixed layout (two-space steps); the reader fails when it does not fit.
/// </summary>
[Trait("Consistency", "true")]
public sealed partial class ReverseProxyTrustConsistencyTests
{
    private static string[] ComposeLines() =>
        File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "deploy", "compose.yaml")).ReplaceLineEndings("\n").Split('\n');

    private static bool IsCode(string line) => line.Trim().Length > 0 && !line.TrimStart().StartsWith('#');

    // The subnets of one network under the top-level "networks:" key.
    internal static List<string> SubnetsOf(string[] lines, string network)
    {
        List<string> subnets = [];
        bool inNetworks = false;
        bool inNetwork = false;
        foreach (string line in lines.Where(IsCode))
        {
            if (!line.StartsWith(' '))
            {
                inNetworks = line.TrimEnd() == "networks:";
                inNetwork = false;
                continue;
            }

            if (!inNetworks)
            {
                continue;
            }

            Match header = NetworkHeader().Match(line);
            if (header.Success)
            {
                inNetwork = header.Groups["name"].Value == network;
                continue;
            }

            Match subnet = SubnetLine().Match(line);
            if (inNetwork && subnet.Success)
            {
                subnets.Add(subnet.Groups["subnet"].Value);
            }
        }

        return subnets;
    }

    // The services that carry a "ports:" key.
    internal static List<string> ServicesWithPorts(string[] lines)
    {
        List<string> services = [];
        bool inServices = false;
        string? service = null;
        foreach (string line in lines.Where(IsCode))
        {
            if (!line.StartsWith(' '))
            {
                inServices = line.TrimEnd() == "services:";
                service = null;
                continue;
            }

            if (!inServices)
            {
                continue;
            }

            Match header = ServiceHeader().Match(line);
            if (header.Success)
            {
                service = header.Groups["name"].Value;
            }
            else if (line.TrimEnd() == "    ports:" && service is not null)
            {
                services.Add(service);
            }
        }

        return services;
    }

    [Fact]
    public void TheEdgeNetworksSubnets_AreTheNetworksTheApplicationTrusts_NoMoreAndNoLess()
    {
        List<string> subnets = SubnetsOf(ComposeLines(), "edge");

        Assert.NotEmpty(subnets);
        Assert.Equal(
            ReverseProxyTrust.TrustedNetworks.Select(n => System.Net.IPNetwork.Parse(n).ToString()).Order(StringComparer.Ordinal),
            subnets.Select(n => System.Net.IPNetwork.Parse(n).ToString()).Order(StringComparer.Ordinal));
    }

    [Fact]
    public void NoOtherNetworkOfTheFile_IsATrustedNetwork()
    {
        string[] lines = ComposeLines();
        foreach (string network in new[] { "app-data", "db-egress", "backup-egress" })
        {
            List<string> subnets = SubnetsOf(lines, network);
            Assert.NotEmpty(subnets);
            foreach (string subnet in subnets)
            {
                Assert.DoesNotContain(subnet, ReverseProxyTrust.TrustedNetworks);
                Assert.DoesNotContain(ReverseProxyTrust.TrustedNetworks, trusted => System.Net.IPNetwork.Parse(trusted).Contains(System.Net.IPNetwork.Parse(subnet).BaseAddress));
            }
        }
    }

    [Fact]
    public void OnlyTheProxy_PublishesPorts()
    {
        Assert.Equal(["proxy"], ServicesWithPorts(ComposeLines()));
    }

    [Fact]
    public void TheReaders_FindWhatTheyLookFor_OnASampleLayout()
    {
        string[] sample =
        [
            "services:",
            "  proxy:",
            "    ports:",
            "      - \"80:80\"",
            "  app:",
            "    networks:",
            "      - edge",
            "networks:",
            "  edge:",
            "    ipam:",
            "      config:",
            "        - subnet: 10.0.0.0/24",
            "        - subnet: fd00::/64",
            "  other:",
            "    ipam:",
            "      config:",
            "        - subnet: 10.9.0.0/24",
        ];

        Assert.Equal(["10.0.0.0/24", "fd00::/64"], SubnetsOf(sample, "edge"));
        Assert.Equal(["10.9.0.0/24"], SubnetsOf(sample, "other"));
        Assert.Equal(["proxy"], ServicesWithPorts(sample));
        Assert.Equal(["proxy", "app"], ServicesWithPorts([.. sample.Take(5), "    ports:", .. sample.Skip(5)]));
    }

    [GeneratedRegex(@"^  (?<name>[A-Za-z0-9_-]+):\s*$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex NetworkHeader();

    [GeneratedRegex(@"^  (?<name>[A-Za-z0-9_-]+):\s*$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex ServiceHeader();

    [GeneratedRegex(@"^\s+- subnet:\s*(?<subnet>\S+)\s*$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex SubnetLine();
}
