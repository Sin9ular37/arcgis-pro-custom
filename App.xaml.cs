using System;
using System.Windows;
using Esri.ArcGISRuntime;

namespace ArcGisMapDemo;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var apiKey = Environment.GetEnvironmentVariable("ARCGIS_API_KEY");
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            ArcGISRuntimeEnvironment.ApiKey = apiKey;
        }
    }
}