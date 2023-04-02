using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MuMuRecordScale
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        JsonNode root;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Btn_open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog { Filter = "MuMu 操作录制文件|*.json|所有文件|*" };
            if (dialog.ShowDialog() != true)
            {
                return;
            }
            try
            {
                string content = File.ReadAllText(dialog.FileName);
                JsonNode json = JsonNode.Parse(content);
                json["commands"].AsArray();
                root = json;
                this.tb_content.Text = content;
            }
            catch
            {
                MessageBox.Show("文件打开失败或不是有效操作录制文件！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Btn_adjust_Click(object sender, RoutedEventArgs e)
        {
            if(root == null)
            {
                MessageBox.Show("请先打开操作录制文件！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            uint ow, oh, aw, ah;
            try
            {
                ow = uint.Parse(this.tb_o_w.Text);
                oh = uint.Parse(this.tb_o_h.Text);
                aw = uint.Parse(this.tb_a_w.Text);
                ah = uint.Parse(this.tb_a_h.Text);
            }
            catch
            {
                MessageBox.Show("输入的四个参数必须为有效正整数像素值！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            SaveFileDialog dialog = new SaveFileDialog { FileName = "adjusted", DefaultExt = ".json", Filter = "MuMu 操作录制文件|*.json" };
            if(dialog.ShowDialog()!=true)
            {
                return;
            }
            double scaleW=aw/(double)ow,scaleH=ah/(double)oh;
            JsonArray commands = root["commands"].AsArray();
            foreach (var item in commands)
            {
                JsonNode node = item["point"];
                if (node == null)
                    continue;
                string[] point = ((string)node).Replace("(","").Replace(")","").Split(',');
                uint pw = (uint)Math.Round(uint.Parse(point[0])*scaleW);
                uint ph = (uint)Math.Round(uint.Parse(point[1]) * scaleH);
                item["point"] = $"({pw},{ph})";
            }
            string json = root.ToJsonString(new JsonSerializerOptions { WriteIndented=true,Encoder=JavaScriptEncoder.UnsafeRelaxedJsonEscaping});
            File.WriteAllText(dialog.FileName, json);
            this.tb_content.Text = json;
            MessageBox.Show("操作录制缩放成功！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
