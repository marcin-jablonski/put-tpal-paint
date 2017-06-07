using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PluginInterface;

namespace GrayscalePlugin
{
    public class GrayscalePlugin : IPlugin
    {
        private ToolBarTray _toolBarTray;
        private Canvas _canvas;
        public void SetToolbar(ToolBarTray toolBarTray)
        {
            _toolBarTray = toolBarTray;
        }

        public void SetCanvas(Canvas canvas)
        {
            _canvas = canvas;
        }

        public void AddPluginControls()
        {
            _toolBarTray.ToolBars.Add(GetPluginToolbar());
        }

        private ToolBar GetPluginToolbar()
        {
            ToolBar toolBar = new ToolBar();
            toolBar.Items.Add(GetNegativeButton());
            return toolBar;
        }

        private Button GetNegativeButton()
        {
            var button = new Button { Content = "Greyscale" };
            button.Click += button_Click;
            return button;
        }

        void button_Click(object sender, RoutedEventArgs e)
        {
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)_canvas.RenderSize.Width,
                (int)_canvas.RenderSize.Height, 96d, 96d, PixelFormats.Default);

            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
                drawingContext.DrawRectangle(_canvas.Background, null,
                    new Rect(0, 0, _canvas.ActualWidth, _canvas.ActualHeight));

            rtb.Render(drawingVisual);
            foreach (object paintSurfaceChild in _canvas.Children)
            {
                rtb.Render((Visual)paintSurfaceChild);
            }

            Bitmap canvasBitmap;
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(rtb));
                enc.Save(outStream);
                canvasBitmap = new Bitmap(outStream);
            }

            Bitmap newBitmap = new Bitmap(canvasBitmap.Width, canvasBitmap.Height);

            Graphics g = Graphics.FromImage(newBitmap);

            //create the grayscale ColorMatrix
            ColorMatrix colorMatrix = new ColorMatrix(
                new float[][]
                {
                    new float[] {.3f, .3f, .3f, 0, 0},
                    new float[] {.59f, .59f, .59f, 0, 0},
                    new float[] {.11f, .11f, .11f, 0, 0},
                    new float[] {0, 0, 0, 1, 0},
                    new float[] {0, 0, 0, 0, 1}
                });

            //create some image attributes
            ImageAttributes attributes = new ImageAttributes();

            //set the color matrix attribute
            attributes.SetColorMatrix(colorMatrix);

            //draw the original image on the new image
            //using the grayscale color matrix
            g.DrawImage(canvasBitmap, new Rectangle(0, 0, canvasBitmap.Width, canvasBitmap.Height),
                0, 0, canvasBitmap.Width, canvasBitmap.Height, GraphicsUnit.Pixel, attributes);

            //dispose the Graphics object
            g.Dispose();

            _canvas.Background = new ImageBrush(Imaging.CreateBitmapSourceFromHBitmap(newBitmap.GetHbitmap(),
                    IntPtr.Zero, Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions()));
            _canvas.Children.Clear();
        }
    }
}
