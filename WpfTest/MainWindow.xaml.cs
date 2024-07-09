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
        private bool isDragging = false;
        private Point clickPosition;
        private Rectangle firstSelectedRectangle;
        private List<Connection> connections = new List<Connection>();
        private Point? lastDragPoint;
        private bool isPanning;
        private Rectangle rightClickedRectangle;
        private Polyline connectingLine;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void DraggableSquare_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            clickPosition = e.GetPosition((Rectangle)sender);
            ((Rectangle)sender).CaptureMouse();
        }

        private void DraggableSquare_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                var rectangle = sender as Rectangle;
                var mousePos = e.GetPosition(MainCanvas);
                var left = mousePos.X - clickPosition.X;
                var top = mousePos.Y - clickPosition.Y;

                if (left >= 0 && left + rectangle.ActualWidth <= MainCanvas.ActualWidth)
                {
                    Canvas.SetLeft(rectangle, left);
                }
                if (top >= 0 && top + rectangle.ActualHeight <= MainCanvas.ActualHeight)
                {
                    Canvas.SetTop(rectangle, top);
                }

                UpdateConnections(rectangle);
            }
        }

        private void DraggableSquare_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            ((Rectangle)sender).ReleaseMouseCapture();
        }

        private void DraggableSquare_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            rightClickedRectangle = sender as Rectangle;

            ContextMenu contextMenu = new ContextMenu();

            MenuItem deleteItem = new MenuItem { Header = "حذف" };
            deleteItem.Click += DeleteItem_Click;

            MenuItem connectItem = new MenuItem { Header = "اتصال" };
            connectItem.Click += ConnectItem_Click;

            MenuItem deleteConnectionItem = new MenuItem { Header = "حذف اتصال" };
            deleteConnectionItem.Click += DeleteConnectionItem_Click;

            contextMenu.Items.Add(deleteItem);
            contextMenu.Items.Add(connectItem);
            contextMenu.Items.Add(deleteConnectionItem);

            rightClickedRectangle.ContextMenu = contextMenu;
        }

        private void DeleteConnectionItem_Click(object sender, RoutedEventArgs e)
        {
            if (rightClickedRectangle != null)
            {
                List<Connection> connectionsToRemove = new List<Connection>();
                foreach (var connection in connections)
                {
                    if (connection.Rect1 == rightClickedRectangle || connection.Rect2 == rightClickedRectangle)
                    {
                        MainCanvas.Children.Remove(connection.Line);
                        MainCanvas.Children.Remove(connection.ArrowHead);
                        connectionsToRemove.Add(connection);
                    }
                }

                foreach (var connection in connectionsToRemove)
                {
                    connections.Remove(connection);
                }

                rightClickedRectangle = null;
            }
        }

        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            if (rightClickedRectangle != null)
            {
                List<Connection> connectionsToRemove = new List<Connection>();
                foreach (var connection in connections)
                {
                    if (connection.Rect1 == rightClickedRectangle || connection.Rect2 == rightClickedRectangle)
                    {
                        MainCanvas.Children.Remove(connection.Line);
                        MainCanvas.Children.Remove(connection.ArrowHead);
                        connectionsToRemove.Add(connection);
                    }
                }

                foreach (var connection in connectionsToRemove)
                {
                    connections.Remove(connection);
                }

                MainCanvas.Children.Remove(rightClickedRectangle);
                rightClickedRectangle = null;
                connectingLine = null;
            }
        }

        private void ConnectItem_Click(object sender, RoutedEventArgs e)
        {
            if (firstSelectedRectangle == null)
            {
                firstSelectedRectangle = rightClickedRectangle;
            }
            else if (firstSelectedRectangle != rightClickedRectangle)
            {
                DrawLineBetweenRectangles(firstSelectedRectangle, rightClickedRectangle);
                firstSelectedRectangle = null;
            }
        }

        private Polygon CreateArrow(Point start, Point end)
        {
            const double ArrowLength = 10;
            const double ArrowWidth = 5;

            double midX = (start.X + end.X) / 2;
            double midY = (start.Y + end.Y) / 2;

            var angle = Math.Atan2(end.Y - start.Y, end.X - start.X);
            var sin = Math.Sin(angle);
            var cos = Math.Cos(angle);

            var arrowHead = new Polygon
            {
                Fill = Brushes.Black,
                Points = new PointCollection
                {
                    new Point(midX, midY),
                    new Point(midX - ArrowLength * cos - ArrowWidth * sin, midY - ArrowLength * sin + ArrowWidth * cos),
                    new Point(midX - ArrowLength * cos + ArrowWidth * sin, midY - ArrowLength * sin - ArrowWidth * cos),
                }
            };

            return arrowHead;
        }

        private void DrawLineBetweenRectangles(Rectangle rect1, Rectangle rect2)
        {
            Point rect1Pos = rect1.TransformToAncestor(MainCanvas).Transform(new Point(0, 0));
            Point rect2Pos = rect2.TransformToAncestor(MainCanvas).Transform(new Point(0, 0));

            double startX = rect1Pos.X + rect1.Width / 2;
            double startY = rect1Pos.Y + rect1.Height / 2;
            double endX = rect2Pos.X + rect2.Width / 2;
            double endY = rect2Pos.Y + rect2.Height / 2;
            double midX = (startX + endX) / 2;
            double midY = (startY + endY) / 2;

            Polyline line = new Polyline
            {
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };
            line.Points.Add(new Point(startX, startY));
            line.Points.Add(new Point(midX, startY));
            line.Points.Add(new Point(midX, endY));
            line.Points.Add(new Point(endX, endY));

            Polygon arrowHead = CreateArrow(new Point(midX, endY), new Point(endX, endY));

            MainCanvas.Children.Add(line);
            MainCanvas.Children.Add(arrowHead);

            connections.Add(new Connection(rect1, rect2, line, arrowHead));
        }

        private void UpdateConnections(Rectangle rectangle)
        {
            foreach (var connection in connections)
            {
                if (connection.Rect1 == rectangle || connection.Rect2 == rectangle)
                {
                    UpdateLine(connection);
                }
            }
        }

        private void UpdateLine(Connection connection)
        {
            Point rect1Pos = connection.Rect1.TransformToAncestor(MainCanvas).Transform(new Point(0, 0));
            Point rect2Pos = connection.Rect2.TransformToAncestor(MainCanvas).Transform(new Point(0, 0));

            double startX = rect1Pos.X + connection.Rect1.Width / 2;
            double startY = rect1Pos.Y + connection.Rect1.Height / 2;
            double endX = rect2Pos.X + connection.Rect2.Width / 2;
            double endY = rect2Pos.Y + connection.Rect2.Height / 2;
            double midX = (startX + endX) / 2;

            connection.Line.Points.Clear();
            connection.Line.Points.Add(new Point(startX, startY));
            connection.Line.Points.Add(new Point(midX, startY));
            connection.Line.Points.Add(new Point(midX, endY));
            connection.Line.Points.Add(new Point(endX, endY));

            Polygon newArrowHead = CreateArrow(new Point(midX, endY), new Point(endX, endY));
            MainCanvas.Children.Remove(connection.ArrowHead);
            connection.ArrowHead = newArrowHead;
            MainCanvas.Children.Add(newArrowHead);
        }

        private void logicGate_Selected(object sender, MouseButtonEventArgs e)
        {
            if (logicGateListBox.SelectedItem != null)
            {
                string selectedItemText = (logicGateListBox.SelectedItem as ListBoxItem).Content.ToString();
                var newRectangle = CreateDraggableRectangle(selectedItemText.Trim());
                MainCanvas.Children.Add(newRectangle);
            }
        }

        private Rectangle CreateDraggableRectangle(string text)
        {
            var rectangle = new Rectangle
            {
                Width = 50,
                Height = 80,
                Fill = Brushes.Gray
            };

            rectangle.MouseLeftButtonDown += DraggableSquare_MouseLeftButtonDown;
            rectangle.MouseLeftButtonUp += DraggableSquare_MouseLeftButtonUp;
            rectangle.MouseMove += DraggableSquare_MouseMove;
            rectangle.MouseRightButtonDown += DraggableSquare_MouseRightButtonDown;

            // ایجاد خطوط ورودی و خروجی
            var topLine = new Line
            {
                X1 = 35,
                Y1 = 70,
                X2 = 50,
                Y2 = 70,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };

            var bottomLine = new Line
            {
                X1 = 35,
                Y1 = 110,
                X2 = 50,
                Y2 = 110,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };

            var outputLine = new Line
            {
                X1 = 100,
                Y1 = 90,
                X2 = 115,
                Y2 = 90,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };

            MainCanvas.Children.Add(topLine);
            MainCanvas.Children.Add(bottomLine);
            MainCanvas.Children.Add(outputLine);

            return rectangle;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

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

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2) return;

            if (Keyboard.IsKeyDown(Key.Space))
            {
                lastDragPoint = e.GetPosition(scrollViewer);
                MainCanvas.CaptureMouse();
                isPanning = true;
            }
        }

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

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isPanning)
            {
                MainCanvas.ReleaseMouseCapture();
                isPanning = false;
                lastDragPoint = null;
            }
        }

        private class Connection
        {
            public Rectangle Rect1 { get; }
            public Rectangle Rect2 { get; }
            public Polyline Line { get; }
            public Polygon ArrowHead { get; set; }

            public Connection(Rectangle rect1, Rectangle rect2, Polyline line, Polygon arrowHead)
            {
                Rect1 = rect1;
                Rect2 = rect2;
                Line = line;
                ArrowHead = arrowHead;
            }
        }
    }
}
