namespace IpAddressCalculator.Core;

public enum IpAddressKind
{
    IPv4,
    IPv6
}

public sealed record CalculationRow(string Label, string Value);

public sealed record AddressCalculationResult(
    IpAddressKind Kind,
    string Address,
    int PrefixLength,
    string Cidr,
    IReadOnlyList<CalculationRow> Rows);

public sealed record SubnetDivisionRow(
    int Index,
    string Cidr,
    string NetworkAddress,
    string LastAddress,
    string BroadcastAddress,
    string FirstUsableAddress,
    string LastUsableAddress,
    string TotalAddresses,
    string UsableHosts);

public sealed record SubnetDivisionResult(
    IpAddressKind Kind,
    string ParentCidr,
    int ParentPrefixLength,
    int NewPrefixLength,
    string TotalSubnets,
    bool IsTruncated,
    IReadOnlyList<SubnetDivisionRow> Rows);
