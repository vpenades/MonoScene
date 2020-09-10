using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

using Microsoft.Win32;

namespace MonoGameViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void _OnClick_LoadModel(object sender, RoutedEventArgs e) { _LoadModel(sender); }        

        private static void _LoadModel(object sender)
        {
            if (sender is FrameworkElement fe)
            {
                if (fe.DataContext is MainScene scene)
                {
                    var dlg = new OpenFileDialog();
                    dlg.Filter = "glTF Files|*.gltf;*.glb;*.gltf.zip;*.vrm";
                    dlg.RestoreDirectory = true;
                    if (!dlg.ShowDialog().Value) return;

                    scene.LoadModel(dlg.FileName);
                }
            }
        }

        Point? _Last;

        private void _OnMouseMove(object sender, MouseEventArgs e)
        {
            if (sender is FrameworkElement fe)
            {
                var point = e.GetPosition(fe);
                var delta = _Last.HasValue ? point - _Last.Value : point - point;
                _Last = point;

                if (e.LeftButton == MouseButtonState.Released) return;

                const float speed = 1f / 100f;

                if (fe.DataContext is MainScene scene)
                {
                    scene.RotateModel((float)delta.X* speed, (float)delta.Y* speed);                    
                }
            }
        }
    }
}
