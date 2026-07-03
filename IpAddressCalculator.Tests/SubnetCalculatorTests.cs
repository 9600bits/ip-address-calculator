using IpAddressCalculator.Core;

namespace IpAddressCalculator.Tests;

public class SubnetCalculatorTests
{
    [Fact]
    public void Ipv4_Cidr24_ReturnsExpectedNetworkAndBroadcast()
    {
        var result = SubnetCalculator.Calculate("192.168.1.10/24");

        Assert.Equal(IpAddressKind.IPv4, result.Kind);
        Assert.Equal("192.168.1.0", Value(result, "网络地址"));
        Assert.Equal("192.168.1.255", Value(result, "广播地址"));
        Assert.Equal("192.168.1.1", Value(result, "首个可用地址"));
        Assert.Equal("192.168.1.254", Value(result, "最后可用地址"));
        Assert.Equal("255.255.255.0", Value(result, "子网掩码"));
        Assert.Equal("0.0.0.255", Value(result, "反掩码"));
    }

    [Fact]
    public void Ipv4_Prefix8_ReturnsExpectedMask()
    {
        var result = SubnetCalculator.Calculate("10.0.0.1", "8");

        Assert.Equal("10.0.0.0", Value(result, "网络地址"));
        Assert.Equal("10.255.255.255", Value(result, "广播地址"));
        Assert.Equal("255.0.0.0", Value(result, "子网掩码"));
    }

    [Fact]
    public void Ipv4_Prefix31_TreatsBothAddressesAsUsable()
    {
        var result = SubnetCalculator.Calculate("192.168.1.0/31");

        Assert.Equal("192.168.1.0", Value(result, "首个可用地址"));
        Assert.Equal("192.168.1.1", Value(result, "最后可用地址"));
        Assert.Equal("2", Value(result, "可用主机数"));
    }

    [Fact]
    public void Ipv4_Prefix32_TreatsSingleAddressAsUsable()
    {
        var result = SubnetCalculator.Calculate("192.168.1.1/32");

        Assert.Equal("192.168.1.1", Value(result, "网络地址"));
        Assert.Equal("192.168.1.1", Value(result, "广播地址"));
        Assert.Equal("1", Value(result, "可用主机数"));
    }

    [Fact]
    public void Ipv4_DottedMask_IsSupported()
    {
        var result = SubnetCalculator.Calculate("192.168.1.10", "255.255.255.0");

        Assert.Equal(24, result.PrefixLength);
        Assert.Equal("192.168.1.0", Value(result, "网络地址"));
    }

    [Fact]
    public void Ipv4_NonContiguousMask_Throws()
    {
        var ex = Assert.Throws<FormatException>(() => SubnetCalculator.Calculate("192.168.1.10", "255.0.255.0"));

        Assert.Contains("连续", ex.Message);
    }

    [Fact]
    public void Ipv6_Prefix64_ReturnsExpectedRange()
    {
        var result = SubnetCalculator.Calculate("2001:db8::1/64");

        Assert.Equal(IpAddressKind.IPv6, result.Kind);
        Assert.Equal("2001:db8::/64", Value(result, "网络前缀"));
        Assert.Equal("2001:db8::", Value(result, "范围首地址"));
        Assert.Equal("2001:db8::ffff:ffff:ffff:ffff", Value(result, "范围末地址"));
    }

    [Fact]
    public void Ipv6_Prefix128_ReturnsSingleAddressRange()
    {
        var result = SubnetCalculator.Calculate("::1/128");

        Assert.Equal("::1", Value(result, "范围首地址"));
        Assert.Equal("::1", Value(result, "范围末地址"));
        Assert.Equal("1", Value(result, "地址总数"));
    }

    [Fact]
    public void Ipv6_Prefix0_ReturnsFullRange()
    {
        var result = SubnetCalculator.Calculate("::/0");

        Assert.Equal("::", Value(result, "范围首地址"));
        Assert.Equal("ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff", Value(result, "范围末地址"));
    }

    [Fact]
    public void Ipv6_InvalidPrefix_Throws()
    {
        var ex = Assert.Throws<FormatException>(() => SubnetCalculator.Calculate("2001:db8::1/129"));

        Assert.Contains("0-128", ex.Message);
    }

    [Fact]
    public void InvalidIp_Throws()
    {
        var ex = Assert.Throws<FormatException>(() => SubnetCalculator.Calculate("not-an-ip/24"));

        Assert.Contains("格式", ex.Message);
    }

    [Fact]
    public void DivideIpv4_Cidr24To26_ReturnsFourSubnets()
    {
        var result = SubnetCalculator.DivideSubnets("192.168.1.0/24", null, "26");

        Assert.Equal(IpAddressKind.IPv4, result.Kind);
        Assert.Equal("192.168.1.0/24", result.ParentCidr);
        Assert.Equal("4", result.TotalSubnets);
        Assert.False(result.IsTruncated);
        Assert.Equal("192.168.1.0/26", result.Rows[0].Cidr);
        Assert.Equal("192.168.1.63", result.Rows[0].BroadcastAddress);
        Assert.Equal("192.168.1.64/26", result.Rows[1].Cidr);
        Assert.Equal("192.168.1.192/26", result.Rows[3].Cidr);
    }

    [Fact]
    public void DivideIpv4_RespectsMaxRows()
    {
        var result = SubnetCalculator.DivideSubnets("10.0.0.0/8", null, "24", maxRows: 3);

        Assert.True(result.IsTruncated);
        Assert.Equal(3, result.Rows.Count);
        Assert.Equal("10.0.0.0/24", result.Rows[0].Cidr);
        Assert.Equal("10.0.2.0/24", result.Rows[2].Cidr);
    }

    [Fact]
    public void DivideIpv6_Cidr48To64_ReturnsExpectedFirstSubnets()
    {
        var result = SubnetCalculator.DivideSubnets("2001:db8::/48", null, "64", maxRows: 2);

        Assert.Equal(IpAddressKind.IPv6, result.Kind);
        Assert.Equal("2001:db8::/48", result.ParentCidr);
        Assert.Equal("65,536", result.TotalSubnets);
        Assert.True(result.IsTruncated);
        Assert.Equal("2001:db8::/64", result.Rows[0].Cidr);
        Assert.Equal("2001:db8:0:1::/64", result.Rows[1].Cidr);
        Assert.Equal("-", result.Rows[0].BroadcastAddress);
    }

    [Fact]
    public void DivideSubnets_NewPrefixCannotBeSmallerThanParentPrefix()
    {
        var ex = Assert.Throws<FormatException>(() => SubnetCalculator.DivideSubnets("192.168.1.0/24", null, "23"));

        Assert.Contains("大于或等于", ex.Message);
    }

    private static string Value(AddressCalculationResult result, string label)
    {
        return result.Rows.Single(row => row.Label == label).Value;
    }
}
