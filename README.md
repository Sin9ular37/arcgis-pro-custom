# ArcGisWpfApp 傻瓜式指南

本指南帮助你在一台全新 Windows 机器上运行 `ArcGisWpfApp` 示例。

## 1. 准备环境

1. 安装 [.NET 8 SDK](https://dotnet.microsoft.com/zh-cn/download)（包含运行与构建所需工具）。
2. 安装 Visual Studio 2022（建议选择“`.NET 桌面开发`”工作负载），以便调试和编辑 WPF 项目。
3. 打开“命令提示符”或 PowerShell，运行 `dotnet --info` 确认已经安装成功。

## 2. 获取项目

1. 将仓库下载/克隆到任意目录，例如 `D:\WorkSpace\Work\work-ArcGis`。
2. 进入 `ArcGisWpfApp` 文件夹。

## 3. 还原依赖

在 `ArcGisWpfApp` 目录执行：

```powershell
dotnet restore
```

该命令会下载 `Esri.ArcGISRuntime` NuGet 包。若提示找不到 `dotnet`，说明第 1 步没有完成。

> 如果打算使用 ArcGIS Online 服务，请在系统环境变量中设置 `ARCGIS_API_KEY`（“系统属性” → “高级” → “环境变量”）。

## 4. 运行示例

### 使用 Visual Studio

1. 打开 Visual Studio → “打开项目/解决方案” → 选择 `ArcGisWpfApp.csproj`。
2. 点击“生成”→“生成解决方案”。
3. 按 `F5` 或点击“本地计算机”运行程序。

### 使用命令行

```powershell
dotnet run
```

## 5. 功能说明

- 程序启动后会加载 ArcGIS 的灰色底图与示例地震要素层。
- 顶部按钮“定位示例图层”会查询一个要素并将地图缩放到该位置。
- 若未设置 `ARCGIS_API_KEY`，请改用本地数据或公开服务。

## 6. 常见问题

- **报错找不到 `dotnet`**：未安装 .NET 8 SDK。
- **无法加载地图**：检查网络连接或 API Key 是否有效；或者改为加载本地数据。
- **XAML 编译错误**：确保使用 Visual Studio 2022 或执行 `dotnet build`，因为项目需要 WPF 支持。
