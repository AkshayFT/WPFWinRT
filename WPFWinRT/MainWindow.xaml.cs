using System;
using System.IO;
using System.Drawing.Imaging;
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
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Foundation;
using OpenTK.Graphics;
using System.Diagnostics;
using OpenTK.Wpf;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace WPFWinRT
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        System.Windows.Point? lastCenterPositionOnTarget;
        System.Windows.Point? lastMousePositionOnTarget;
        System.Windows.Point? lastDragPoint;
        PdfDocument pdfDoc;
        System.Drawing.Bitmap bgBitmap;
        public MainWindow()
        {
            InitializeComponent();
            PdfPath = "Assets/vr50hd_manual.pdf";
            scale = 1;

            scrollViewer.ScrollChanged += OnScrollViewerScrollChanged;
            scrollViewer.MouseLeftButtonUp += OnMouseLeftButtonUp;
            scrollViewer.PreviewMouseLeftButtonUp += OnMouseLeftButtonUp;
            scrollViewer.PreviewMouseWheel += OnPreviewMouseWheel;

            scrollViewer.PreviewMouseLeftButtonDown += OnMouseLeftButtonDown;
            scrollViewer.MouseMove += OnMouseMove;

            slider.ValueChanged += OnSliderValueChanged;
        }
        private void OnStartWriting(object sender, RoutedEventArgs e)
        {
            Canvas game = new Canvas(1000, 800, "Noteshelf");
            game._backgroundTexture = Texture.LoadFromStream(bgBitmap);
            game.Run();
        }

        void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (lastDragPoint.HasValue)
            {
                System.Windows.Point posNow = e.GetPosition(scrollViewer);

                double dX = posNow.X - lastDragPoint.Value.X;
                double dY = posNow.Y - lastDragPoint.Value.Y;

                lastDragPoint = posNow;

                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - dX);
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - dY);
            }
        }

        void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var mousePos = e.GetPosition(scrollViewer);
            if (mousePos.X <= scrollViewer.ViewportWidth && mousePos.Y <
                scrollViewer.ViewportHeight) //make sure we still can use the scrollbars
            {
                scrollViewer.Cursor = Cursors.SizeAll;
                lastDragPoint = mousePos;
                Mouse.Capture(scrollViewer);
            }
        }

        void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            lastMousePositionOnTarget = Mouse.GetPosition(grid);

            if (e.Delta > 0)
            {
                slider.Value += 1;
            }
            if (e.Delta < 0)
            {
                slider.Value -= 1;
            }

            e.Handled = true;
        }

        void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            scrollViewer.Cursor = Cursors.Arrow;
            scrollViewer.ReleaseMouseCapture();
            lastDragPoint = null;
        }

        void OnSliderValueChanged(object sender,
             RoutedPropertyChangedEventArgs<double> e)
        {
            scaleTransform.ScaleX = e.NewValue;
            scaleTransform.ScaleY = e.NewValue;

            var centerOfViewport = new System.Windows.Point(scrollViewer.ViewportWidth / 2,
                                             scrollViewer.ViewportHeight / 2);
            lastCenterPositionOnTarget = scrollViewer.TranslatePoint(centerOfViewport, grid);

            scale = (int)scaleTransform.ScaleX * 2;
        }

        void OnScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentHeightChange != 0 || e.ExtentWidthChange != 0)
            {
                System.Windows.Point? targetBefore = null;
                System.Windows.Point? targetNow = null;

                if (!lastMousePositionOnTarget.HasValue)
                {
                    if (lastCenterPositionOnTarget.HasValue)
                    {
                        var centerOfViewport = new System.Windows.Point(scrollViewer.ViewportWidth / 2,
                                                         scrollViewer.ViewportHeight / 2);
                        System.Windows.Point centerOfTargetNow =
                              scrollViewer.TranslatePoint(centerOfViewport, grid);

                        targetBefore = lastCenterPositionOnTarget;
                        targetNow = centerOfTargetNow;
                    }
                }
                else
                {
                    targetBefore = lastMousePositionOnTarget;
                    targetNow = Mouse.GetPosition(grid);

                    lastMousePositionOnTarget = null;
                }

                if (targetBefore.HasValue)
                {
                    double dXInTargetPixels = targetNow.Value.X - targetBefore.Value.X;
                    double dYInTargetPixels = targetNow.Value.Y - targetBefore.Value.Y;

                    double multiplicatorX = e.ExtentWidth / grid.Width;
                    double multiplicatorY = e.ExtentHeight / grid.Height;

                    double newOffsetX = scrollViewer.HorizontalOffset -
                                        dXInTargetPixels * multiplicatorX;
                    double newOffsetY = scrollViewer.VerticalOffset -
                                        dYInTargetPixels * multiplicatorY;

                    if (double.IsNaN(newOffsetX) || double.IsNaN(newOffsetY))
                    {
                        return;
                    }

                    scrollViewer.ScrollToHorizontalOffset(newOffsetX);
                    scrollViewer.ScrollToVerticalOffset(newOffsetY);
                }
            }
        }

        #region Bindable Properties

        public string PdfPath
        {
            get { return (string)GetValue(PdfPathProperty); }
            set { SetValue(PdfPathProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PdfPath.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PdfPathProperty =
            DependencyProperty.Register("PdfPath", typeof(string), typeof(MainWindow), new PropertyMetadata(null, propertyChangedCallback: OnPdfPathChanged));

        public int scale
        {
            get { return (int)GetValue(scaleProperty); }
            set { SetValue(scaleProperty, value); }
        }

        public static readonly DependencyProperty scaleProperty =
           DependencyProperty.Register("scale", typeof(int), typeof(MainWindow), new PropertyMetadata(1, propertyChangedCallback: OnScaleChanged));

        private static async void OnScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var window = (MainWindow)d;
            if (window.scaleTransform.ScaleX > 0)
            {
                await MainWindow.PdfToImages(window, window.pdfDoc);
            }
        }
        private static void OnPdfPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var window = (MainWindow)d;

            if (!string.IsNullOrEmpty(window.PdfPath))
            {
                //making sure it's an absolute path
                var path = System.IO.Path.GetFullPath(window.PdfPath);

                StorageFile.GetFileFromPathAsync(path).AsTask()
                  //load pdf document on background thread
                  .ContinueWith(t => PdfDocument.LoadFromFileAsync(t.Result).AsTask())
                  .Unwrap()
                  //display on UI Thread
                  .ContinueWith(t2 => PdfToImages(window, t2.Result), TaskScheduler.FromCurrentSynchronizationContext());
            }

        }

        #endregion

        private static async Task PdfToImages(MainWindow window, PdfDocument pdfDoc)
        {
            window.pdfDoc = pdfDoc;
            var scale = (uint)window.scaleTransform.ScaleX;
            if (pdfDoc == null || pdfDoc.PageCount == 0) return;

            using (var page = pdfDoc.GetPage(1))
            {
                var bitmap = await window.PageToBitmapAsync(page, scale);
                var image = new Image
                {
                    Source = bitmap,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(5, 5, 5, 5),
                    MaxWidth = 1000
                };
                window.presenter.Content = image;
                window.bgBitmap = BitmapImage2Bitmap(bitmap);
            }
        }

        private async Task<BitmapImage> PageToBitmapAsync(PdfPage page, uint scale)
        {
            BitmapImage image = new BitmapImage();
            using (var stream = new InMemoryRandomAccessStream())
            {
                PdfPageRenderOptions options = new PdfPageRenderOptions();
                options.DestinationWidth = (uint)page.Size.Width * scale;
                options.DestinationHeight = (uint)page.Size.Height * scale;
                Debug.WriteLine("Scale {0}, width: {1}, height: {2}", scale, options.DestinationWidth, options.DestinationHeight);

                await page.RenderToStreamAsync(stream, options);

                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream.AsStream();
                image.EndInit();
            }
            return image;
        }

        private static System.Drawing.Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new System.Drawing.Bitmap(bitmap);
            }
        }
    }
}
