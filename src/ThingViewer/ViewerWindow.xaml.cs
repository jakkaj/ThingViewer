using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
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
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;

namespace ThingViewer
{
    /// <summary>
    /// Interaction logic for ViewerWindow.xaml
    /// </summary>
    public partial class ViewerWindow : Window
    {
        private FileSystemWatcher _watcher;

        private string _text;

        public ViewerWindow()
        {
            InitializeComponent();
            FileName.TextChanged += FileName_TextChanged;
            
        }

        private void FileName_TextChanged(object sender, TextChangedEventArgs e)
        {
            FileNameText = FileName.Text;
        }

        private void SelectFile(object sender, RoutedEventArgs e)
        {
            var d = new OpenFileDialog();
           
            if (d.ShowDialog(this) == true)
            {
                FileName.Text = d.FileName;
                _doWatch();
                _doLoad();
            }
        }

        void _doWatch()
        {
            var f = FileNameText;

            if (string.IsNullOrWhiteSpace(f))
            {
                return;
            }

            if (!File.Exists(f))
            {
                return;
            }

            if (_watcher != null)
            {
                _watcher.Changed -= _watcher_Changed;
                _watcher.EnableRaisingEvents = false;
            }

            var parentDir = System.IO.Path.GetDirectoryName(f);

            _watcher = new FileSystemWatcher(parentDir);
            

            _watcher.Changed += _watcher_Changed;
            _watcher.EnableRaisingEvents = true;
        }

        private void _watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.FullPath == FileNameText)
            {
                _doLoad();
            }
            
        }

        void _doLoad()
        {
            if (_isImage())
            {
                _showImage();
            }
        }

        void _showImage()
        {
            Dispatcher.Invoke(() =>
            {
                var fTemp = System.IO.Path.GetTempFileName();
                File.Copy(FileNameText, fTemp, true);
                using (var img = (Bitmap) Bitmap.FromFile(fTemp))
                {
                    using (var ms = new MemoryStream())
                    {
                        img.Save(ms, ImageFormat.Bmp);
                        ms.Seek(0, SeekOrigin.Begin);
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.StreamSource = ms;
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.EndInit();
                        ImageFrame.Source = bitmapImage;
                    }
                }
                 File.Delete(fTemp);

            });
           
        }



        bool _isImage()
        {
            var images = new string[]{".bmp", ".jpg", ".jpeg", ".png"};
            return images.Any(_isExt);
        }

        bool _isExt(string extCompare)
        {
            var ext = System.IO.Path.GetExtension(FileNameText);

            return extCompare.Equals(ext, StringComparison.OrdinalIgnoreCase);
        }

        public string FileNameText
        {
            get { return _text; }
            set
            {
                _text = value;
                _doWatch();
            }
        }

        public static BitmapSource ConvertBitmap(Bitmap source)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                source.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
        }
    }
}
