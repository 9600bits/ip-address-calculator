# IPv4 IPv6 地址计算器

一个 Windows 桌面版 IP 地址计算工具，支持 IPv4 和 IPv6 子网计算。

## 功能

- IPv4 CIDR 计算，例如 `192.168.1.10/24`
- IPv4 点分十进制掩码，例如 `255.255.255.0`
- IPv6 CIDR 计算，例如 `2001:db8::1/64`
- 输出网络地址、广播地址、首末可用地址、地址总数、掩码和反掩码
- IPv6 输出压缩地址、展开地址、网络前缀和范围
- 中文 WinForms 界面，支持单项结果复制

## 项目结构

- `IpAddressCalculator.App`：Windows 窗体应用
- `IpAddressCalculator.Core`：IPv4/IPv6 计算核心逻辑
- `IpAddressCalculator.Tests`：xUnit 单元测试

## 构建与测试

```powershell
dotnet build
dotnet test
dotnet publish .\IpAddressCalculator.App\IpAddressCalculator.App.csproj -c Release -r win-x64 --self-contained false
```

发布后的程序位于：

```text
IpAddressCalculator.App\bin\Release\net10.0-windows\win-x64\publish\IPv4 IPv6地址计算器.exe
```

## 技术栈

- .NET 10
- Windows Forms
- xUnit
