using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Microsoft.Win32;

namespace ArcGisMapDemo;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        InitializeMap();
    }

    private void InitializeMap()
    {
        // Initialize an empty map
        MapView.Map = new Map();
    }

    private async void OnLoadClick(object sender, RoutedEventArgs e)
    {
        var url = (UrlBox.Text ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(url))
        {
            MessageBox.Show("\u8BF7\u8F93\u5165 ArcGIS REST \u670D\u52A1 URL\u3002");
            return;
        }

        LoadBtn.IsEnabled = false;
        try { await LoadServiceAsync(url); }
        catch (Exception ex) { MessageBox.Show($"\u52A0\u8F7D\u5931\u8D25\uFF1A{ex.Message}"); }
        finally { LoadBtn.IsEnabled = true; }
    }

    private async Task LoadServiceAsync(string url)
    {
        // Ensure map is not null and clear layers
        (MapView.Map ??= new Map()).OperationalLayers.Clear();

        Exception? lastError = null;

        // 1) Map Server - dynamic map service
        try
        {
            var img = new ArcGISMapImageLayer(new Uri(url));
            await img.LoadAsync();
            MapView.Map.OperationalLayers.Add(img);
            await ZoomToExtentAsync(img.FullExtent);
            return;
        }
        catch (Exception ex) { lastError = ex; }

        // 2) Tiled service
        try
        {
            var tiled = new ArcGISTiledLayer(new Uri(url));
            await tiled.LoadAsync();
            MapView.Map.OperationalLayers.Add(tiled);
            await ZoomToExtentAsync(tiled.FullExtent);
            return;
        }
        catch (Exception ex) { lastError = ex; }

        // 3) Vector tile service
        try
        {
            var vtl = new ArcGISVectorTiledLayer(new Uri(url));
            await vtl.LoadAsync();
            MapView.Map.OperationalLayers.Add(vtl);
            await ZoomToExtentAsync(vtl.FullExtent);
            return;
        }
        catch (Exception ex) { lastError = ex; }

        // 4) Feature service - layer
        try
        {
            var table = new ServiceFeatureTable(new Uri(url));
            var fl = new FeatureLayer(table);
            await fl.LoadAsync();
            MapView.Map.OperationalLayers.Add(fl);
            await ZoomToExtentAsync(fl.FullExtent);
            return;
        }
        catch (Exception ex) { lastError = ex; }

        throw lastError ?? new InvalidOperationException("\u65E0\u6CD5\u8BC6\u522B\u7684\u670D\u52A1\u7C7B\u578B\u3002");
    }

    private async Task ZoomToExtentAsync(Geometry? extent)
    {
        if (extent == null) return;
        await MapView.SetViewpointAsync(new Viewpoint(extent));
    }

    // Right panel: import mobile geodatabase (.geodatabase)
    private void OnBrowseGdbClick(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Title = "\u9009\u62E9\u79FB\u52A8\u5730\u7406\u6570\u636E\u5E93 (.geodatabase)",
            Filter = "\u79FB\u52A8\u5730\u7406\u6570\u636E\u5E93 (*.geodatabase)|*.geodatabase|\u6240\u6709\u6587\u4EF6 (*.*)|*.*"
        };
        if (dlg.ShowDialog() == true)
        {
            GdbPathBox.Text = dlg.FileName;
        }
    }

    private async void OnImportGdbClick(object sender, RoutedEventArgs e)
    {
        var path = (GdbPathBox.Text ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(path))
        {
            MessageBox.Show("\u8BF7\u5148\u9009\u62E9 .geodatabase \u6587\u4EF6\u3002");
            return;
        }

        if (Directory.Exists(path) && path.EndsWith(".gdb", StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show("\u5F53\u524D\u793A\u4F8B\u4E0D\u76F4\u63A5\u652F\u6301\u6587\u4EF6\u5730\u7406\u6570\u636E\u5E93 (.gdb)\u3002\u8BF7\u5728 ArcGIS Pro \u4E2D\u5BFC\u51FA\u4E3A\u79FB\u52A8\u5730\u7406\u6570\u636E\u5E93 (.geodatabase) \u540E\u518D\u5BFC\u5165\u3002");
            return;
        }

        if (!File.Exists(path))
        {
            MessageBox.Show("\u8DEF\u5F84\u65E0\u6548\u6216\u6587\u4EF6\u4E0D\u5B58\u5728\u3002");
            return;
        }

        try
        {
            var gdb = await Geodatabase.OpenAsync(path);
            var tables = gdb.GeodatabaseFeatureTables.ToList();
            if (tables.Count == 0)
            {
                MessageBox.Show("\u8BE5\u79FB\u52A8\u5730\u7406\u6570\u636E\u5E93\u4E2D\u6CA1\u6709\u8981\u7D20\u8868\u3002");
                return;
            }

            (MapView.Map ??= new Map()).OperationalLayers.Clear();

            Envelope? union = null; // combine extents
            foreach (var table in tables)
            {
                await table.LoadAsync();
                var layer = new FeatureLayer(table);
                MapView.Map.OperationalLayers.Add(layer);
                if (table.Extent != null)
                {
                    // keep return type Envelope by using CombineExtents
                    union = union == null
                        ? table.Extent
                        : GeometryEngine.CombineExtents(union, table.Extent);
                }
            }

            if (union != null)
                await ZoomToExtentAsync(union);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"\u5BFC\u5165\u5931\u8D25\uFF1A{ex.Message}");
        }
    }
}
