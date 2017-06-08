using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PluginInterface;
using Color = System.Drawing.Color;

namespace NegativePlugin
{
    public class NegativePlugin : IPlugin
    {
        private Canvas _canvas;

        public void SetCanvas(Canvas canvas)
        {
            _canvas = canvas;
        }

        public ToolBar GetPluginToolbar()
        {
            ToolBar toolBar = new ToolBar();
            toolBar.Items.Add(GetNegativeButton());
            return toolBar;
        }

        private Button GetNegativeButton()
        {
            var button = new Button {Content = "Negative"};
            button.Click += button_Click;
            return button;
        }

        void button_Click(object sender, RoutedEventArgs e)
        {
            Bitmap canvasBitmap = CreateCanvasBitmap();

            for (int x = 0; x < canvasBitmap.Width; x++)
            {
                for (int y = 0; y < canvasBitmap.Height; y++)
                {
                    Color pixelColor = canvasBitmap.GetPixel(x, y);
                    Color newColor = Color.FromArgb(255 - pixelColor.R, 255 - pixelColor.G, 255 - pixelColor.B);
                    canvasBitmap.SetPixel(x, y, newColor);
                }
            }

            _canvas.Background = new ImageBrush(Imaging.CreateBitmapSourceFromHBitmap(canvasBitmap.GetHbitmap(),
                    IntPtr.Zero, Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions()));
            _canvas.Children.Clear();
        }

        private Bitmap CreateCanvasBitmap()
        {
            var bmp = CreateWritableBitmapOfCanvas();

            Bitmap canvasBitmap;
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bmp));
                enc.Save(outStream);
                canvasBitmap = new Bitmap(outStream);
            }

            return canvasBitmap;
        }

        private RenderTargetBitmap CreateWritableBitmapOfCanvas()
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
            return rtb;
        }

        public void SetUndoEvent(PluginUndo undoEventHandler)
        {
            throw new NotImplementedException();
        }
    }
}
