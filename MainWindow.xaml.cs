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
        // ��ʼ�յ�ͼ���û����ط���������ݺ�������ͼ��
        MapView.Map = new Map();
    }

    private async void OnLoadClick(object sender, RoutedEventArgs e)
    {
        var url = (UrlBox.Text ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(url))
        {
            MessageBox.Show("������ ArcGIS REST ���� URL��");
            return;
        }

        LoadBtn.IsEnabled = false;
        try { await LoadServiceAsync(url); }
        catch (Exception ex) { MessageBox.Show($"����ʧ�ܣ�{ex.Message}"); }
        finally { LoadBtn.IsEnabled = true; }
    }

    private async Task LoadServiceAsync(string url)
    {
        // ���֮ǰ��ͼ�� — 确保 Map 非空再清空，避免空引用告警
        (MapView.Map ??= new Map()).OperationalLayers.Clear();

        Exception? lastError = null;

        // 1) Map Server����̬ͼ�����
        try
        {
            var img = new ArcGISMapImageLayer(new Uri(url));
            await img.LoadAsync();
            MapView.Map.OperationalLayers.Add(img);
            await ZoomToExtentAsync(img.FullExtent);
            return;
        }
        catch (Exception ex) { lastError = ex; }

        // 2) դ����Ƭ����TiledLayer��
        try
        {
            var tiled = new ArcGISTiledLayer(new Uri(url));
            await tiled.LoadAsync();
            MapView.Map.OperationalLayers.Add(tiled);
            await ZoomToExtentAsync(tiled.FullExtent);
            return;
        }
        catch (Exception ex) { lastError = ex; }

        // 3) ʸ����Ƭ����VectorTileLayer��
        try
        {
            var vtl = new ArcGISVectorTiledLayer(new Uri(url));
            await vtl.LoadAsync();
            MapView.Map.OperationalLayers.Add(vtl);
            await ZoomToExtentAsync(vtl.FullExtent);
            return;
        }
        catch (Exception ex) { lastError = ex; }

        // 4) Ҫ�ط���FeatureServer ͼ���������
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

        throw lastError ?? new InvalidOperationException("�޷�ʶ��ķ������͡�");
    }

    private async Task ZoomToExtentAsync(Geometry? extent)
    {
        if (extent == null) return;
        await MapView.SetViewpointAsync(new Viewpoint(extent));
    }

    // �Ҳࣺ����뵼�� GDB���ƶ��������ݿ� .geodatabase��
    private void OnBrowseGdbClick(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Title = "ѡ���ƶ��������ݿ� (.geodatabase)",
            Filter = "�ƶ��������ݿ� (*.geodatabase)|*.geodatabase|�����ļ� (*.*)|*.*"
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
            MessageBox.Show("����ѡ�� .geodatabase �ļ���");
            return;
        }

        if (Directory.Exists(path) && path.EndsWith(".gdb", StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show("��ǰʾ����ֱ��֧���ļ��������ݿ� (.gdb)������ ArcGIS Pro �н��䵼��Ϊ�ƶ��������ݿ� (.geodatabase) ���ٵ��롣");
            return;
        }

        if (!File.Exists(path))
        {
            MessageBox.Show("·����Ч���ļ������ڡ�");
            return;
        }

        try
        {
            var gdb = await Geodatabase.OpenAsync(path);
            // ��Ҫ�ر�����ͼ��
            var tables = gdb.GeodatabaseFeatureTables.ToList();
            if (tables.Count == 0)
            {
                MessageBox.Show("�õ������ݿ���û��Ҫ�ر���");
                return;
            }

            // 确保 Map 非空再清空，避免空引用告警
            (MapView.Map ??= new Map()).OperationalLayers.Clear();

            Envelope? union = null; // 逐表合并范围
            foreach (var table in tables)
            {
                await table.LoadAsync();
                var layer = new FeatureLayer(table);
                MapView.Map.OperationalLayers.Add(layer);
                if (table.Extent != null)
                {
                    // 使用 CombineExtents 返回 Envelope，避免 Geometry 到 Envelope 的转换错误
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
            MessageBox.Show($"����ʧ�ܣ�{ex.Message}");
        }
    }
}

