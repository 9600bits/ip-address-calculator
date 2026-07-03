using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Numerics;

namespace IpAddressCalculator.Core;

public static class SubnetCalculator
{
    private static readonly BigInteger Ipv6AllBits = (BigInteger.One << 128) - BigInteger.One;

    public static AddressCalculationResult Calculate(string addressInput, string? prefixInput = null)
    {
        var parsed = ParseAddressAndPrefix(addressInput, prefixInput);
        return parsed.Kind == IpAddressKind.IPv4
            ? CalculateIpv4(parsed.Address, parsed.PrefixLength)
            : CalculateIpv6(parsed.Address, parsed.PrefixLength);
    }

    public static SubnetDivisionResult DivideSubnets(
        string addressInput,
        string? prefixInput,
        string newPrefixInput,
        int maxRows = 256)
    {
        var parsed = ParseAddressAndPrefix(addressInput, prefixInput);
        if (string.IsNullOrWhiteSpace(newPrefixInput))
        {
            throw new FormatException("请输入划分后的新前缀长度。");
        }

        if (maxRows < 1 || maxRows > 4096)
        {
            throw new FormatException("最多显示条数必须在 1-4096 之间。");
        }

        if (parsed.Kind == IpAddressKind.IPv4)
        {
            var newPrefix = ParseNumericPrefix(newPrefixInput.Trim(), 32, "IPv4 新前缀长度必须是 0-32。");
            if (newPrefix < parsed.PrefixLength)
            {
                throw new FormatException("新前缀长度必须大于或等于母网前缀长度。");
            }

            return DivideIpv4Subnets(parsed.Address, parsed.PrefixLength, newPrefix, maxRows);
        }

        if (newPrefixInput.Contains('.', StringComparison.Ordinal))
        {
            throw new FormatException("IPv6 新前缀长度必须是 0-128 的数字。");
        }

        var ipv6NewPrefix = ParseNumericPrefix(newPrefixInput.Trim(), 128, "IPv6 新前缀长度必须是 0-128。");
        if (ipv6NewPrefix < parsed.PrefixLength)
        {
            throw new FormatException("新前缀长度必须大于或等于母网前缀长度。");
        }

        return DivideIpv6Subnets(parsed.Address, parsed.PrefixLength, ipv6NewPrefix, maxRows);
    }

    private static ParsedAddress ParseAddressAndPrefix(string addressInput, string? prefixInput)
    {
        if (string.IsNullOrWhiteSpace(addressInput))
        {
            throw new FormatException("请输入 IPv4 或 IPv6 地址。");
        }

        var addressPart = addressInput.Trim();
        string? cidrPrefix = null;
        var slashIndex = addressPart.IndexOf('/');
        if (slashIndex >= 0)
        {
            if (slashIndex == 0 || slashIndex == addressPart.Length - 1 || addressPart.IndexOf('/', slashIndex + 1) >= 0)
            {
                throw new FormatException("CIDR 格式不正确，请使用类似 192.168.1.10/24 的格式。");
            }

            cidrPrefix = addressPart[(slashIndex + 1)..].Trim();
            addressPart = addressPart[..slashIndex].Trim();
        }

        if (!IPAddress.TryParse(addressPart, out var ipAddress))
        {
            throw new FormatException("IP 地址格式不正确。");
        }

        var family = ipAddress.AddressFamily;
        if (family != AddressFamily.InterNetwork && family != AddressFamily.InterNetworkV6)
        {
            throw new FormatException("只支持 IPv4 和 IPv6 地址。");
        }

        var rawPrefix = ResolvePrefixInput(cidrPrefix, prefixInput);
        if (family == AddressFamily.InterNetwork)
        {
            var prefixLength = ParseIpv4Prefix(rawPrefix);
            return new ParsedAddress(IpAddressKind.IPv4, ipAddress, prefixLength);
        }

        if (rawPrefix.Contains('.', StringComparison.Ordinal))
        {
            throw new FormatException("IPv6 不支持点分十进制子网掩码，请输入 0-128 的前缀长度。");
        }

        var ipv6Prefix = ParseNumericPrefix(rawPrefix, 128, "IPv6 前缀长度必须是 0-128。");
        return new ParsedAddress(IpAddressKind.IPv6, ipAddress, ipv6Prefix);
    }

    private static string ResolvePrefixInput(string? cidrPrefix, string? prefixInput)
    {
        var trimmedPrefix = prefixInput?.Trim();
        if (!string.IsNullOrWhiteSpace(cidrPrefix) && !string.IsNullOrWhiteSpace(trimmedPrefix) &&
            !string.Equals(cidrPrefix, trimmedPrefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new FormatException("地址中的 CIDR 前缀与单独输入的前缀不一致。");
        }

        var resolved = !string.IsNullOrWhiteSpace(cidrPrefix) ? cidrPrefix : trimmedPrefix;
        if (string.IsNullOrWhiteSpace(resolved))
        {
            throw new FormatException("请输入前缀长度；IPv4 也可以输入点分十进制子网掩码。");
        }

        return resolved.Trim();
    }

    private static AddressCalculationResult CalculateIpv4(IPAddress address, int prefixLength)
    {
        var ip = ReadUInt32(address);
        var mask = PrefixToIpv4Mask(prefixLength);
        var wildcard = ~mask;
        var network = ip & mask;
        var broadcast = network | wildcard;
        var totalAddresses = 1UL << (32 - prefixLength);
        var usableHosts = prefixLength switch
        {
            32 => 1UL,
            31 => 2UL,
            _ => totalAddresses - 2UL
        };

        var firstUsable = prefixLength >= 31 ? network : network + 1U;
        var lastUsable = prefixLength == 32 ? network : prefixLength == 31 ? broadcast : broadcast - 1U;
        var formattedAddress = UInt32ToIpv4(ip);
        var cidr = $"{formattedAddress}/{prefixLength}";

        var rows = new List<CalculationRow>
        {
            new("地址类型", "IPv4"),
            new("IP 地址", formattedAddress),
            new("CIDR", cidr),
            new("前缀长度", prefixLength.ToString(CultureInfo.InvariantCulture)),
            new("子网掩码", UInt32ToIpv4(mask)),
            new("反掩码", UInt32ToIpv4(wildcard)),
            new("网络地址", UInt32ToIpv4(network)),
            new("广播地址", UInt32ToIpv4(broadcast)),
            new("首个可用地址", UInt32ToIpv4(firstUsable)),
            new("最后可用地址", UInt32ToIpv4(lastUsable)),
            new("地址总数", totalAddresses.ToString("N0", CultureInfo.InvariantCulture)),
            new("可用主机数", usableHosts.ToString("N0", CultureInfo.InvariantCulture))
        };

        return new AddressCalculationResult(IpAddressKind.IPv4, formattedAddress, prefixLength, cidr, rows);
    }

    private static AddressCalculationResult CalculateIpv6(IPAddress address, int prefixLength)
    {
        var ip = ReadUInt128(address);
        var hostMask = prefixLength == 128 ? BigInteger.Zero : (BigInteger.One << (128 - prefixLength)) - BigInteger.One;
        var network = ip & (Ipv6AllBits ^ hostMask);
        var last = network | hostMask;
        var totalAddresses = BigInteger.One << (128 - prefixLength);
        var compressedAddress = ToIpv6(ip);
        var cidr = $"{compressedAddress}/{prefixLength}";

        var rows = new List<CalculationRow>
        {
            new("地址类型", "IPv6"),
            new("压缩地址", compressedAddress),
            new("展开地址", ToExpandedIpv6(ip)),
            new("CIDR", cidr),
            new("前缀长度", prefixLength.ToString(CultureInfo.InvariantCulture)),
            new("网络前缀", $"{ToIpv6(network)}/{prefixLength}"),
            new("范围首地址", ToIpv6(network)),
            new("范围末地址", ToIpv6(last)),
            new("地址总数", FormatBigInteger(totalAddresses))
        };

        return new AddressCalculationResult(IpAddressKind.IPv6, compressedAddress, prefixLength, cidr, rows);
    }

    private static SubnetDivisionResult DivideIpv4Subnets(IPAddress address, int parentPrefix, int newPrefix, int maxRows)
    {
        var ip = ReadUInt32(address);
        var parentMask = PrefixToIpv4Mask(parentPrefix);
        var parentNetwork = ip & parentMask;
        var subnetCount = 1UL << (newPrefix - parentPrefix);
        var subnetSize = 1UL << (32 - newPrefix);
        var rowsToTake = (int)Math.Min((ulong)maxRows, subnetCount);
        var rows = new List<SubnetDivisionRow>(rowsToTake);

        for (var index = 0; index < rowsToTake; index++)
        {
            var network = parentNetwork + (uint)(subnetSize * (ulong)index);
            var broadcast = network + (uint)(subnetSize - 1UL);
            var firstUsable = newPrefix >= 31 ? network : network + 1U;
            var lastUsable = newPrefix == 32 ? network : newPrefix == 31 ? broadcast : broadcast - 1U;
            var usableHosts = newPrefix switch
            {
                32 => 1UL,
                31 => 2UL,
                _ => subnetSize - 2UL
            };

            rows.Add(new SubnetDivisionRow(
                index + 1,
                $"{UInt32ToIpv4(network)}/{newPrefix}",
                UInt32ToIpv4(network),
                UInt32ToIpv4(broadcast),
                UInt32ToIpv4(broadcast),
                UInt32ToIpv4(firstUsable),
                UInt32ToIpv4(lastUsable),
                subnetSize.ToString("N0", CultureInfo.InvariantCulture),
                usableHosts.ToString("N0", CultureInfo.InvariantCulture)));
        }

        return new SubnetDivisionResult(
            IpAddressKind.IPv4,
            $"{UInt32ToIpv4(parentNetwork)}/{parentPrefix}",
            parentPrefix,
            newPrefix,
            subnetCount.ToString("N0", CultureInfo.InvariantCulture),
            subnetCount > (ulong)rowsToTake,
            rows);
    }

    private static SubnetDivisionResult DivideIpv6Subnets(IPAddress address, int parentPrefix, int newPrefix, int maxRows)
    {
        var ip = ReadUInt128(address);
        var parentHostMask = parentPrefix == 128 ? BigInteger.Zero : (BigInteger.One << (128 - parentPrefix)) - BigInteger.One;
        var parentNetwork = ip & (Ipv6AllBits ^ parentHostMask);
        var subnetCount = BigInteger.One << (newPrefix - parentPrefix);
        var subnetSize = BigInteger.One << (128 - newPrefix);
        var rowsToTake = (int)BigInteger.Min(new BigInteger(maxRows), subnetCount);
        var rows = new List<SubnetDivisionRow>(rowsToTake);

        for (var index = 0; index < rowsToTake; index++)
        {
            var network = parentNetwork + (subnetSize * index);
            var last = network + subnetSize - BigInteger.One;

            rows.Add(new SubnetDivisionRow(
                index + 1,
                $"{ToIpv6(network)}/{newPrefix}",
                ToIpv6(network),
                ToIpv6(last),
                "-",
                ToIpv6(network),
                ToIpv6(last),
                FormatBigInteger(subnetSize),
                "-"));
        }

        return new SubnetDivisionResult(
            IpAddressKind.IPv6,
            $"{ToIpv6(parentNetwork)}/{parentPrefix}",
            parentPrefix,
            newPrefix,
            FormatBigInteger(subnetCount),
            subnetCount > rowsToTake,
            rows);
    }

    private static int ParseIpv4Prefix(string value)
    {
        if (!value.Contains('.', StringComparison.Ordinal))
        {
            return ParseNumericPrefix(value, 32, "IPv4 前缀长度必须是 0-32。");
        }

        if (!IPAddress.TryParse(value, out var maskAddress) || maskAddress.AddressFamily != AddressFamily.InterNetwork)
        {
            throw new FormatException("IPv4 子网掩码格式不正确。");
        }

        var mask = ReadUInt32(maskAddress);
        var sawZero = false;
        var prefixLength = 0;
        for (var bit = 31; bit >= 0; bit--)
        {
            var isOne = ((mask >> bit) & 1U) == 1U;
            if (isOne && sawZero)
            {
                throw new FormatException("IPv4 子网掩码必须是连续的。");
            }

            if (isOne)
            {
                prefixLength++;
            }
            else
            {
                sawZero = true;
            }
        }

        return prefixLength;
    }

    private static int ParseNumericPrefix(string value, int max, string errorMessage)
    {
        if (!int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var prefixLength) ||
            prefixLength < 0 || prefixLength > max)
        {
            throw new FormatException(errorMessage);
        }

        return prefixLength;
    }

    private static uint PrefixToIpv4Mask(int prefixLength)
    {
        return prefixLength == 0 ? 0U : uint.MaxValue << (32 - prefixLength);
    }

    private static uint ReadUInt32(IPAddress address)
    {
        var bytes = address.GetAddressBytes();
        return ((uint)bytes[0] << 24) | ((uint)bytes[1] << 16) | ((uint)bytes[2] << 8) | bytes[3];
    }

    private static BigInteger ReadUInt128(IPAddress address)
    {
        return new BigInteger(address.GetAddressBytes(), isUnsigned: true, isBigEndian: true);
    }

    private static string UInt32ToIpv4(uint value)
    {
        return string.Create(CultureInfo.InvariantCulture, $"{(value >> 24) & 0xff}.{(value >> 16) & 0xff}.{(value >> 8) & 0xff}.{value & 0xff}");
    }

    private static string ToIpv6(BigInteger value)
    {
        return new IPAddress(ToBytes(value)).ToString();
    }

    private static string ToExpandedIpv6(BigInteger value)
    {
        var bytes = ToBytes(value);
        var groups = new string[8];
        for (var i = 0; i < groups.Length; i++)
        {
            groups[i] = $"{bytes[i * 2]:x2}{bytes[i * 2 + 1]:x2}";
        }

        return string.Join(':', groups);
    }

    private static byte[] ToBytes(BigInteger value)
    {
        var bytes = value.ToByteArray(isUnsigned: true, isBigEndian: true);
        if (bytes.Length == 16)
        {
            return bytes;
        }

        var padded = new byte[16];
        bytes.CopyTo(padded, 16 - bytes.Length);
        return padded;
    }

    private static string FormatBigInteger(BigInteger value)
    {
        var exact = value.ToString("N0", CultureInfo.InvariantCulture);
        var plain = value.ToString(CultureInfo.InvariantCulture);
        if (plain.Length <= 20)
        {
            return exact;
        }

        var mantissa = $"{plain[0]}.{plain[1..Math.Min(4, plain.Length)]}";
        return $"{exact} (约 {mantissa}e{plain.Length - 1})";
    }

    private sealed record ParsedAddress(IpAddressKind Kind, IPAddress Address, int PrefixLength);
}
