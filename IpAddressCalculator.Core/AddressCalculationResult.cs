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
