using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace WpfTest
{
    internal class Gate
    {
        public Rectangle RectangleControl { get; set; }
        public Canvas CanvasControl { get; set; }
        public int ID { get; set; }
        public string? Type { get; set; }
        public int Inputs { get; set; }
        public Line OutputLine { get; set; }
        public List<Line> InputLines { get; set; }
        public TextBlock NameTextBlock { get; set; }
        public Grid GridControl { get; set; }

        public Gate(string? type, int inputs)//gate constructor
        {
            InputLines = new List<Line>();
            this.Inputs = inputs;
            Type = type;

            //ایجاد کانواز
            CanvasControl = new Canvas
            {
                Width = 100,
                Height = 100,
                Background = Brushes.Transparent,
                Tag = type + "-" + inputs
            };


            // ایجاد Border با CornerRadius
            var border = new Border
            {
                Width = 50,
                Height = 80,
                CornerRadius = new CornerRadius(7), // گوشه‌های گرد
                Background = new SolidColorBrush(Color.FromArgb(220, 50, 50, 50)), // خاکستری نیمه شفاف
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(2),
                Effect = new DropShadowEffect // افکت سایه
                {
                    Color = Colors.Black,
                    Direction = 320,
                    ShadowDepth = 5,
                    Opacity = 0.5
                },
                Child = GridControl
            };

            GridControl = new Grid();
            GridControl.RowDefinitions.Add(new RowDefinition());
            GridControl.RowDefinitions.Add(new RowDefinition());
            GridControl.ColumnDefinitions.Add(new ColumnDefinition());




            // ایجاد تکست بلاک برای نمایش نام گیت
            NameTextBlock = new TextBlock
            {
                Text = Type,
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 16
            };

            GridControl.Children.Add(NameTextBlock);
            Grid.SetRow(NameTextBlock, 0);
            Grid.SetRowSpan(NameTextBlock, 2);
            Grid.SetColumn(NameTextBlock, 0);

            border.Child = GridControl;
            CanvasControl.Children.Add(border);

            //ایجاد لاین ها
            OutputLine = new Line()
            {
                X1 = 50,
                X2 = 65,
                Y1 = 40,
                Y2 = 40,
                Stroke = Brushes.Green,
                StrokeThickness = 3
            };
            OutputLine.MouseEnter += Line_MouseEnter;
            OutputLine.MouseLeave += Line_MouseLeave;
            OutputLine.MouseLeftButtonDown += OutputLine_MouseLeftButtonDown;

            switch (inputs)
            {
                case 1:
                    InputLines.Add(new Line() { X1 = -15, X2 = 0, Y1 = 40, Y2 = 40, Stroke = Brushes.Blue, StrokeThickness = 2 });
                    break;                                                                                                   
                case 2:
                    InputLines.Add(new Line() { X1 = -15, X2 = 0, Y1 = 20, Y2 = 20, Stroke = Brushes.Blue, StrokeThickness = 2 });
                    InputLines.Add(new Line() { X1 = -15, X2 = 0, Y1 = 60, Y2 = 60, Stroke = Brushes.Blue, StrokeThickness = 2 });
                    break;                                                                                                   
                case 3:
                    InputLines.Add(new Line() { X1 = -15, X2 = 0, Y1 = 15, Y2 = 15, Stroke = Brushes.Blue, StrokeThickness = 2 });
                    InputLines.Add(new Line() { X1 = -15, X2 = 0, Y1 = 40, Y2 = 40, Stroke = Brushes.Blue, StrokeThickness = 2 });
                    InputLines.Add(new Line() { X1 = -15, X2 = 0, Y1 = 65, Y2 = 65, Stroke = Brushes.Blue, StrokeThickness = 2 });
                    break;

            }
            CanvasControl.Children.Add(OutputLine);
            foreach (var line in InputLines)
            {
                line.MouseEnter += Line_MouseEnter;
                line.MouseLeave += Line_MouseLeave;
                line.MouseLeftButtonDown += InputLine_MouseLeftButtonDown;
                CanvasControl.Children.Add(line);

            }
        }

        // رویدادهای موس برای خطوط
        public static void Line_MouseEnter(object sender, MouseEventArgs e)
        {
            //((Line)sender).Stroke = Brushes.Red;
            if (Mouse.OverrideCursor == Cursors.Arrow)
            {
                Mouse.OverrideCursor = Cursors.SizeWE;
            }
        }

        public static void Line_MouseLeave(object sender, MouseEventArgs e)
        {
            if (Mouse.OverrideCursor == Cursors.SizeWE)
            {
                Mouse.OverrideCursor = Cursors.Arrow;
            }
        }

        public static void OutputLine_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var line = sender as Line;
            var gateCanvas = line.Parent as Canvas;
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow.StartConnection(gateCanvas, line, true);
        }

        public static void InputLine_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var line = sender as Line;
            var gateCanvas = line.Parent as Canvas;
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow.StartConnection(gateCanvas, line, false);
        }
    }
}
