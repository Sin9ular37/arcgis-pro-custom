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
        // 初始空地图，用户加载服务或导入数据后再添加图层
        MapView.Map = new Map();
    }

    private async void OnLoadClick(object sender, RoutedEventArgs e)
    {
        var url = (UrlBox.Text ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(url))
        {
            MessageBox.Show("请输入 ArcGIS REST 服务 URL。");
            return;
        }

        LoadBtn.IsEnabled = false;
        try { await LoadServiceAsync(url); }
        catch (Exception ex) { MessageBox.Show($"加载失败：{ex.Message}"); }
        finally { LoadBtn.IsEnabled = true; }
    }

    private async Task LoadServiceAsync(string url)
    {
        // 清空之前的图层
        MapView.Map.OperationalLayers.Clear();

        Exception? lastError = null;

        // 1) Map Server（动态图像服务）
        try
        {
            var img = new ArcGISMapImageLayer(new Uri(url));
            await img.LoadAsync();
            MapView.Map.OperationalLayers.Add(img);
            await ZoomToExtentAsync(img.FullExtent);
            return;
        }
        catch (Exception ex) { lastError = ex; }

        // 2) 栅格瓦片服务（TiledLayer）
        try
        {
            var tiled = new ArcGISTiledLayer(new Uri(url));
            await tiled.LoadAsync();
            MapView.Map.OperationalLayers.Add(tiled);
            await ZoomToExtentAsync(tiled.FullExtent);
            return;
        }
        catch (Exception ex) { lastError = ex; }

        // 3) 矢量瓦片服务（VectorTileLayer）
        try
        {
            var vtl = new ArcGISVectorTiledLayer(new Uri(url));
            await vtl.LoadAsync();
            MapView.Map.OperationalLayers.Add(vtl);
            await ZoomToExtentAsync(vtl.FullExtent);
            return;
        }
        catch (Exception ex) { lastError = ex; }

        // 4) 要素服务（FeatureServer 图层或服务根）
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

        throw lastError ?? new InvalidOperationException("无法识别的服务类型。");
    }

    private async Task ZoomToExtentAsync(Geometry? extent)
    {
        if (extent == null) return;
        await MapView.SetViewpointAsync(new Viewpoint(extent));
    }

    // 右侧：浏览与导入 GDB（移动地理数据库 .geodatabase）
    private void OnBrowseGdbClick(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Title = "选择移动地理数据库 (.geodatabase)",
            Filter = "移动地理数据库 (*.geodatabase)|*.geodatabase|所有文件 (*.*)|*.*"
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
            MessageBox.Show("请先选择 .geodatabase 文件。");
            return;
        }

        if (Directory.Exists(path) && path.EndsWith(".gdb", StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show("当前示例不直接支持文件地理数据库 (.gdb)。请在 ArcGIS Pro 中将其导出为移动地理数据库 (.geodatabase) 后再导入。");
            return;
        }

        if (!File.Exists(path))
        {
            MessageBox.Show("路径无效或文件不存在。");
            return;
        }

        try
        {
            var gdb = await Geodatabase.OpenAsync(path);
            // 按要素表创建图层
            var tables = gdb.GeodatabaseFeatureTables.ToList();
            if (tables.Count == 0)
            {
                MessageBox.Show("该地理数据库中没有要素表。");
                return;
            }

            MapView.Map.OperationalLayers.Clear();
            Envelope? union = null;
            foreach (var table in tables)
            {
                await table.LoadAsync();
                var layer = new FeatureLayer(table);
                MapView.Map.OperationalLayers.Add(layer);
                if (table.Extent != null)
                    union = union == null ? table.Extent : union.Union(table.Extent);
            }

            if (union != null)
                await ZoomToExtentAsync(union);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"导入失败：{ex.Message}");
        }
    }
}