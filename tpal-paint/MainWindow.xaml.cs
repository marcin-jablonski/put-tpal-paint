using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace tpal_paint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Point _currentPoint;

        private Color _selectedColor;

        private Rectangle _currentRectangle;

        private Ellipse _currentEllipse;

        public string BrushSize { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            ColorSelector.ItemsSource = typeof(Colors).GetProperties();
            ColorSelector.SelectedItem = typeof(Colors).GetProperties().First(x => x.Name == "Black");
            ToolSelector.ItemsSource = Enum.GetValues(typeof(ToolType)).Cast<ToolType>();
            ToolSelector.SelectedItem = ToolType.Pencil;
            PaintSurface.Background = new SolidColorBrush(Colors.White);
            BrushSize = "2";
            BindingOperations.GetBindingExpression(BrushSizeTextBox, TextBox.TextProperty).UpdateTarget();
            _selectedColor = Colors.Black;
        }

        private void PaintSurface_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            switch ((ToolType)ToolSelector.SelectedItem)
            {
                case ToolType.Pencil:
                    _currentPoint = e.GetPosition((Canvas)sender);
                    break;
                case ToolType.Rectangle:
                    _currentPoint = e.GetPosition((Canvas)sender);
                    _currentRectangle = new Rectangle
                    {
                        Stroke = new SolidColorBrush(_selectedColor),
                        StrokeThickness = double.Parse(BrushSize)
                    };
                    Canvas.SetLeft(_currentRectangle, _currentPoint.X);
                    Canvas.SetTop(_currentRectangle, _currentPoint.X);
                    PaintSurface.Children.Add(_currentRectangle);
                    break;
                case ToolType.Ellipse:
                    _currentPoint = e.GetPosition((Canvas)sender);
                    _currentEllipse = new Ellipse
                    {
                        Stroke = new SolidColorBrush(_selectedColor),
                        StrokeThickness = double.Parse(BrushSize)
                    };
                    Canvas.SetLeft(_currentEllipse, _currentPoint.X);
                    Canvas.SetTop(_currentEllipse, _currentPoint.X);
                    PaintSurface.Children.Add(_currentEllipse);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void PaintSurface_OnMouseMove(object sender, MouseEventArgs e)
        {
            switch ((ToolType)ToolSelector.SelectedItem)
            {
                case ToolType.Pencil:
                    if (e.LeftButton == MouseButtonState.Pressed)
                    {
                        Line line = new Line
                        {
                            Stroke = new SolidColorBrush(_selectedColor),
                            StrokeThickness = double.Parse(BrushSize),
                            X1 = _currentPoint.X,
                            Y1 = _currentPoint.Y,
                            X2 = e.GetPosition((Canvas)sender).X,
                            Y2 = e.GetPosition((Canvas)sender).Y
                        };

                        _currentPoint = e.GetPosition((Canvas)sender);

                        PaintSurface.Children.Add(line);
                    }
                    break;
                case ToolType.Rectangle:
                    if (e.LeftButton == MouseButtonState.Released || _currentRectangle == null)
                        return;

                    var pos = e.GetPosition(PaintSurface);

                    var x = Math.Min(pos.X, _currentPoint.X);
                    var y = Math.Min(pos.Y, _currentPoint.Y);

                    var w = Math.Max(pos.X, _currentPoint.X) - x;
                    var h = Math.Max(pos.Y, _currentPoint.Y) - y;

                    _currentRectangle.Width = w;
                    _currentRectangle.Height = h;

                    Canvas.SetLeft(_currentRectangle, x);
                    Canvas.SetTop(_currentRectangle, y);
                    break;
                case ToolType.Ellipse:
                    if (e.LeftButton == MouseButtonState.Released || _currentEllipse == null)
                        return;

                    var pos1 = e.GetPosition(PaintSurface);

                    var x1 = Math.Min(pos1.X, _currentPoint.X);
                    var y1 = Math.Min(pos1.Y, _currentPoint.Y);

                    var w1 = Math.Max(pos1.X, _currentPoint.X) - x1;
                    var h1 = Math.Max(pos1.Y, _currentPoint.Y) - y1;

                    _currentEllipse.Width = w1;
                    _currentEllipse.Height = h1;

                    Canvas.SetLeft(_currentEllipse, x1);
                    Canvas.SetTop(_currentEllipse, y1);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ColorSelector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedColor = (Color) (ColorSelector.SelectedItem as PropertyInfo).GetValue(null, null);
        }

        private void PaintSurface_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            switch ((ToolType)ToolSelector.SelectedItem)
            {
                case ToolType.Pencil:
                    break;
                case ToolType.Rectangle:
                    _currentRectangle = null;
                    break;
                case ToolType.Ellipse:
                    _currentEllipse = null;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void UIElement_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = new Regex("[^0-9.-]+").IsMatch(e.Text);
        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)PaintSurface.RenderSize.Width,
                (int)PaintSurface.RenderSize.Height, 96d, 96d, PixelFormats.Default);

            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
                drawingContext.DrawRectangle(PaintSurface.Background, null,
                    new Rect(0, 0, PaintSurface.ActualWidth, PaintSurface.ActualHeight));

            rtb.Render(drawingVisual);
            foreach (object paintSurfaceChild in PaintSurface.Children)
            {
                rtb.Render((Visual)paintSurfaceChild);
            }

            BitmapEncoder pngEncoder = new JpegBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(rtb));

            using (var fs = System.IO.File.OpenWrite("logo.jpg"))
            {
                pngEncoder.Save(fs);
            }
        }

        private void Open_OnClick(object sender, RoutedEventArgs e)
        {
            ImageBrush brush = new ImageBrush
            {
                ImageSource = new BitmapImage(new Uri(@"logo.jpg", UriKind.Relative))
            };
            PaintSurface.Background = brush;
        }
    }
}
