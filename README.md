# ArcGisMapDemo 使用说明

这是一个基于ArcGIS Runtime for .NET的开发学习示例，它仿照ArcGIS Pro进行开发。该示例支持加载MapServer、Tiled、Vector Tile和Feature Server，并可导入.geodatabase文件。本地和在线图层可以叠加显示，并可控制显示和隐藏。此外，它还支持自动合并范围并缩放视图。该示例已适配.NET 8，并统一使用UTF-8编码，适用于快速验证常见GIS数据的接入和可视化。

This is a development learning example based on ArcGIS Runtime for .NET, which is modelled on ArcGIS Pro. The example supports loading MapServer, Tiled, Vector Tile and Feature Server, and can import .geodatabase files. Local and online layers can be displayed in a stack and the display can be controlled to show or hide layers. In addition, it supports automatic range merging and zooming in on the view. The example has been adapted to .NET 8 and uses UTF-8 encoding throughout, making it ideal for quickly verifying the accessibility and visualisation of common GIS data.

## 环境准备

1. 安装 [.NET 8 SDK](https://dotnet.microsoft.com/zh-cn/download)。
2. 建议安装 Visual Studio 2022，并勾选“.NET 桌面开发”工作负载。
3. 在 PowerShell 中运行 `dotnet --info` 确认安装成功。

## 获取与构建

在项目根目录执行：

```powershell
dotnet restore .\ArcGisMapDemo.csproj
dotnet build .\ArcGisMapDemo.csproj -c Debug
```

## 运行示例

```powershell
dotnet run --project .\ArcGisMapDemo.csproj
```

启动后可在顶部输入框粘贴 ArcGIS REST 服务 URL 并点击“加载”。右侧面板可选择本地 .geodatabase 文件进行导入。

## 常见问题

- 无法编译或运行：确认 .NET 8 SDK 已安装，且项目已成功还原。
- 无法加载底图或服务：检查服务 URL 是否可访问，或是否需要凭据；若使用 Esri 受限服务请确保有有效许可。
- 关于中文乱码：项目已统一为 UTF-8（带 BOM）编码；如仍遇到乱码，请在编辑器中将文件保存为 UTF-8（带签名），并避免使用 ANSI/GBK 编码。

## 常用命令（再次列出）

```powershell
dotnet restore .\ArcGisMapDemo.csproj
dotnet build .\ArcGisMapDemo.csproj -c Debug
dotnet run --project .\ArcGisMapDemo.csproj
```
