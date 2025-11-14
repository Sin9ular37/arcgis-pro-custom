# ArcGisMapDemo 使用说明

本项目是一个基于 ArcGIS Runtime for .NET 的 WPF 示例，用于加载 MapServer/Tiled/VectorTile/FeatureServer 服务，并支持导入本地移动地理数据库（.geodatabase）。

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
