using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;

namespace ArcGisWpfApp;

public partial class MainWindow : Window
{
    private FeatureLayer? _sampleLayer;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnLoadedAsync;
    }

    private async void OnLoadedAsync(object sender, RoutedEventArgs e)
    {
        ZoomButton.IsEnabled = false;
        try
        {
            await InitializeMapAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"初始化地图失败：{ex.Message}", "ArcGIS Runtime");
        }
        finally
        {
            ZoomButton.IsEnabled = true;
        }
    }

    private async Task InitializeMapAsync()
    {
        MyMapView.Map = new Map(BasemapStyle.ArcGISLightGray);

        var table = new ServiceFeatureTable(new Uri("https://services3.arcgis.com/cJ9YHowT8TU7DUyn/arcgis/rest/services/Earthquakes/FeatureServer/0"));
        _sampleLayer = new FeatureLayer(table);
        await _sampleLayer.LoadAsync();

        MyMapView.Map.OperationalLayers.Add(_sampleLayer);
        await MyMapView.SetViewpointAsync(new Viewpoint(_sampleLayer.FullExtent));
    }

    private async void OnZoomToLayerClick(object sender, RoutedEventArgs e)
    {
        if (_sampleLayer?.FeatureTable == null)
        {
            MessageBox.Show("图层尚未加载。");
            return;
        }

        ZoomButton.IsEnabled = false;
        try
        {
            var queryParams = new QueryParameters
            {
                WhereClause = "1=1",
                ReturnGeometry = true,
                MaxFeatures = 1
            };

            var result = await _sampleLayer.FeatureTable.QueryFeaturesAsync(queryParams);
            var feature = result.FirstOrDefault();
            if (feature == null || feature.Geometry == null)
            {
                MessageBox.Show("未查询到要素。");
                return;
            }

            var geometry = feature.Geometry;
            Envelope envelope = geometry switch
            {
                MapPoint point => new Envelope(point, 5000, 5000),
                Envelope env => env,
                _ => geometry.Extent
            };

            await MyMapView.SetViewpointAsync(new Viewpoint(envelope));
        }
        catch (Exception ex)
        {
            MessageBox.Show($"定位失败：{ex.Message}");
        }
        finally
        {
            ZoomButton.IsEnabled = true;
        }
    }
}
