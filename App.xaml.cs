using System;
using System.Windows;
using Esri.ArcGISRuntime;

namespace ArcGisWpfApp;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 从环境变量读取 API Key，避免硬编码敏感信息。
        var apiKey = Environment.GetEnvironmentVariable("ARCGIS_API_KEY");
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            ArcGISRuntimeEnvironment.ApiKey = apiKey;
        }
    }
}
