using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WpfTest
{
    public partial class MainWindow : Window
    {

        // تعریف متغیر ها و اشیاء
        private bool isDragging = false;
        private Point clickPosition;
        private List<Connection> connections = new List<Connection>();
        private Point? lastDragPoint;
        private bool isPanning;
        private bool isConnecting = false;
        private Canvas firstGateCanvas;
        private Line firstLine;
        private bool isOutput;

        public MainWindow()
        {
            InitializeComponent();
        }



        //کلیک چپ کردن روی گیت برای جابه جایی
        private void DraggableSquare_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            clickPosition = e.GetPosition((Rectangle)sender);
            ((Rectangle)sender).CaptureMouse();
        }



        // کشیدن و جا به جایی گیت
        private void DraggableSquare_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                var rectangle = sender as Rectangle;
                var canvas = rectangle.Parent as Canvas;

                var mousePos = e.GetPosition(MainCanvas);
                var left = mousePos.X - clickPosition.X;
                var top = mousePos.Y - clickPosition.Y;

                if (left >= 0 && left + canvas.ActualWidth <= MainCanvas.ActualWidth)
                {
                    Canvas.SetLeft(canvas, left);
                }
                if (top >= 0 && top + canvas.ActualHeight <= MainCanvas.ActualHeight)
                {
                    Canvas.SetTop(canvas, top);
                }

                UpdateConnections(); // بروزرسانی خطوط متصل به گیت ها
            }
        }


        // دراپ کردن کلیک چپ موس هنگام جا به جا شدن گیت
        private void DraggableSquare_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            ((Rectangle)sender).ReleaseMouseCapture();


        }




        //کد ایجاد کردن گین جدید- این کد رو فقط مهدی حق داره تغیر بده😡
        private void logicGate_Selected(object sender, MouseButtonEventArgs e)
        {
            if (logicGateListBox.SelectedItem != null)
            {
                string? selectedGate = (logicGateListBox.SelectedItem as ListBoxItem).Content.ToString().Split('-')[0];
                int inputsNumber = Convert.ToInt32((logicGateListBox.SelectedItem as ListBoxItem).Content.ToString().Split('-')[1]);


                var ff = new Gate(selectedGate, inputsNumber);
                ff.RectangleControl.MouseLeftButtonDown += DraggableSquare_MouseLeftButtonDown;
                ff.RectangleControl.MouseLeftButtonUp += DraggableSquare_MouseLeftButtonUp;
                ff.RectangleControl.MouseMove += DraggableSquare_MouseMove;
                MainCanvas.Children.Add(ff.CanvasControl);
            }
        }



        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }



        //زوم این و زوم اوت در صفحه با نگهداشتن دکمه کنترل
        private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                var scaleTransform = MainCanvas.LayoutTransform as ScaleTransform ?? new ScaleTransform();
                double zoom = e.Delta > 0 ? 0.1 : -0.1;

                if ((scaleTransform.ScaleX + zoom) > 0.1 && (scaleTransform.ScaleY + zoom) > 0.1)
                {
                    scaleTransform.ScaleX += zoom;
                    scaleTransform.ScaleY += zoom;
                    MainCanvas.LayoutTransform = scaleTransform;
                }

                e.Handled = true;
            }
            else
            {
                if (e.Delta < 0)
                {
                    scrollViewer.LineDown();
                }
                else
                {
                    scrollViewer.LineUp();
                }
            }
        }


        // جا به جایی در صفحه با نگهداشتن دکمه اسپیس
        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2) return;

            if (Keyboard.IsKeyDown(Key.Space))
            {
                Mouse.OverrideCursor = Cursors.Hand;

                lastDragPoint = e.GetPosition(scrollViewer);
                MainCanvas.CaptureMouse();
                isPanning = true;
            }
        }


        // اسکرول کردن با غلطک موس
        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isPanning && lastDragPoint.HasValue)
            {
                Point currentPosition = e.GetPosition(scrollViewer);
                double deltaX = currentPosition.X - lastDragPoint.Value.X;
                double deltaY = currentPosition.Y - lastDragPoint.Value.Y;

                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - deltaX);
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - deltaY);

                lastDragPoint = currentPosition;
            }
        }


        //دراپ کردن کلیک چپ موس بعد از جا به جایی در صفحه با اسپیس
        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

            if (isPanning)
            {
                Mouse.OverrideCursor = Cursors.Arrow;

                MainCanvas.ReleaseMouseCapture();
                isPanning = false;
                lastDragPoint = null;
            }
        }



        // بررسی وضعیت برای شروع اتصال خط بین گیت ها
        public void StartConnection(Canvas gateCanvas, Line line, bool output)
        {
            if (!isConnecting)
            {
                isConnecting = true;
                firstGateCanvas = gateCanvas;
                firstLine = line;
                isOutput = output;
            }
            else
            {
                if (output != isOutput)
                {
                    var secondGateCanvas = gateCanvas;
                    var secondLine = line;

                    // ایجاد اتصال بین گیت‌ها
                    DrawLineBetweenGates(firstGateCanvas, firstLine, secondGateCanvas, secondLine);

                    // ریست کردن وضعیت اتصال
                    isConnecting = false;
                    firstGateCanvas = null;
                    firstLine = null;
                    isOutput = false;
                }
                else
                {
                    MessageBox.Show("اتصال بین ورودی ها و خروجی ها امکان‌پذیر نیست.");
                    // ریست کردن وضعیت اتصال
                    isConnecting = false;
                    firstGateCanvas = null;
                    firstLine = null;
                    isOutput = false;
                }
            }
        }



        // کشیدن خط اتصال بین گیت ها
        private void DrawLineBetweenGates(Canvas gate1, Line line1, Canvas gate2, Line line2)
        {
            Point startPoint = gate1.TransformToAncestor(MainCanvas).Transform(new Point(line1.X2, line1.Y2));
            Point endPoint = gate2.TransformToAncestor(MainCanvas).Transform(new Point(line2.X1, line2.Y1));

            startPoint = LimitToGateBounds(gate1, startPoint);
            endPoint = LimitToGateBounds(gate2, endPoint);

            Polyline connectionLine = new Polyline
            {
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };
            connectionLine.Points.Add(startPoint);
            connectionLine.Points.Add(new Point((startPoint.X + endPoint.X) / 2, startPoint.Y));
            connectionLine.Points.Add(new Point((startPoint.X + endPoint.X) / 2, endPoint.Y));
            connectionLine.Points.Add(endPoint);

            MainCanvas.Children.Add(connectionLine);

            // موقعیت فلش مثلثی در وسط خط و به سمت گیت دوم
            Polygon arrowHead = CreateArrowHead(connectionLine);
            MainCanvas.Children.Add(arrowHead);

            connections.Add(new Connection(gate1, gate2, connectionLine, arrowHead));
        }





        //محدود کردن نقاط اتصال به لبه های گیت
        private Point LimitToGateBounds(Canvas gate, Point point)
        {
            double left = Canvas.GetLeft(gate);
            double top = Canvas.GetTop(gate);
            double right = left + gate.ActualWidth;
            double bottom = top + gate.ActualHeight;

            if (point.X < left) point.X = left;
            if (point.X > right) point.X = right;
            if (point.Y < top) point.Y = top;
            if (point.Y > bottom) point.Y = bottom;

            return point;
        }



        // ایجاد فلش در وسط خط اتصال بین دو گیت
        private Polygon CreateArrowHead(Polyline connectionLine)
        {
            double arrowHeadSize = 10;
            Point midPoint = new Point((connectionLine.Points[0].X + connectionLine.Points[connectionLine.Points.Count - 1].X) / 2,
                                       (connectionLine.Points[0].Y + connectionLine.Points[connectionLine.Points.Count - 1].Y) / 2);

            Polygon arrowHead = new Polygon
            {
                Fill = Brushes.Black,
                Points = new PointCollection(new Point[]
                {
            new Point(0, 0),
            new Point(-arrowHeadSize, -arrowHeadSize / 2),
            new Point(-arrowHeadSize, arrowHeadSize / 2)
                })
            };

            double angle = Math.Atan2(connectionLine.Points[connectionLine.Points.Count - 1].Y - connectionLine.Points[0].Y,
                                      connectionLine.Points[connectionLine.Points.Count - 1].X - connectionLine.Points[0].X) * 180 / Math.PI + 90;
            RotateTransform rotateTransform = new RotateTransform(angle);
            arrowHead.RenderTransform = rotateTransform;

            Canvas.SetLeft(arrowHead, midPoint.X);
            Canvas.SetTop(arrowHead, midPoint.Y);

            return arrowHead;
        }





        private void UpdateConnections()
        {
            foreach (var connection in connections)
            {
                var startCanvas = connection.Gate1;
                var endCanvas = connection.Gate2;

                var startPoint = startCanvas.TransformToAncestor(MainCanvas).Transform(new Point(startCanvas.Width / 2, startCanvas.Height / 2));
                var endPoint = endCanvas.TransformToAncestor(MainCanvas).Transform(new Point(endCanvas.Width / 2, endCanvas.Height / 2));

                startPoint = LimitToGateBounds(startCanvas, startPoint);
                endPoint = LimitToGateBounds(endCanvas, endPoint);

                var polyline = connection.Line;
                polyline.Points.Clear();
                polyline.Points.Add(startPoint);
                polyline.Points.Add(new Point((startPoint.X + endPoint.X) / 2, startPoint.Y));
                polyline.Points.Add(new Point((startPoint.X + endPoint.X) / 2, endPoint.Y));
                polyline.Points.Add(endPoint);

                var midPoint = new Point((startPoint.X + endPoint.X) / 2, (startPoint.Y + endPoint.Y) / 2);
                double arrowAngle = Math.Atan2(endPoint.Y - startPoint.Y, endPoint.X - startPoint.X) * 180 / Math.PI + 90;
                connection.ArrowHead.RenderTransform = new RotateTransform(arrowAngle);
                Canvas.SetLeft(connection.ArrowHead, midPoint.X);
                Canvas.SetTop(connection.ArrowHead, midPoint.Y);
            }
        }

        private PointCollection CreateArrowHeadPoints(Point position, Point direction)
        {
            double arrowHeadSize = 10;
            return new PointCollection(new Point[]
            {
        new Point(0, 0),
        new Point(-arrowHeadSize, -arrowHeadSize / 2),
        new Point(-arrowHeadSize, arrowHeadSize / 2)
            });
        }




        private class Connection
        {
            public Canvas Gate1 { get; }
            public Canvas Gate2 { get; }
            public Polyline Line { get; }
            public Polygon ArrowHead { get; set; }

            public Connection(Canvas gate1, Canvas gate2, Polyline line, Polygon arrowHead)
            {
                Gate1 = gate1;
                Gate2 = gate2;
                Line = line;
                ArrowHead = arrowHead;
            }
        }

    }
}

