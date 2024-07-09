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
        //اشیاء
        private bool isDragging = false;
        private Point clickPosition;
        private TextBlock firstSelectedTextBlock;
        private List<Connection> connections = new List<Connection>();
        private Point? lastDragPoint;
        private bool isPanning;
        private TextBlock rightClickedTextBlock;
        private Polyline connectingLine;




        public MainWindow()
        {
            InitializeComponent();
        }

        // جا به جایی گیت ها با درگ اند دراپ
        private void DraggableSquare_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            clickPosition = e.GetPosition((TextBlock)sender);
            ((TextBlock)sender).CaptureMouse();
        }


        // جا به جایی گیت ها با درگ اند دراپ
        private void DraggableSquare_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                var textBlock = sender as TextBlock;
                var mousePos = e.GetPosition(MainCanvas);
                var left = mousePos.X - clickPosition.X;
                var top = mousePos.Y - clickPosition.Y;

                if (left >= 0 && left + textBlock.ActualWidth <= MainCanvas.ActualWidth)
                {
                    Canvas.SetLeft(textBlock, left);
                }
                if (top >= 0 && top + textBlock.ActualHeight <= MainCanvas.ActualHeight)
                {
                    Canvas.SetTop(textBlock, top);
                }

                UpdateConnections(textBlock);
            }
        }


        // جا به جایی گیت ها با درگ اند دراپ
        private void DraggableSquare_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            ((TextBlock)sender).ReleaseMouseCapture();
        }


        // باز کردن منوی کلیک راست
        private void DraggableSquare_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            rightClickedTextBlock = sender as TextBlock;

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


            rightClickedTextBlock.ContextMenu = contextMenu;
        }

        //فقط حذف خطوط اتصال از گیت
        private void DeleteConnectionItem_Click(object sender, RoutedEventArgs e)
        {
            if (rightClickedTextBlock != null)
            {
                List<Connection> connectionsToRemove = new List<Connection>();
                foreach (var connection in connections)
                {
                    if (connection.Tb1 == rightClickedTextBlock || connection.Tb2 == rightClickedTextBlock)
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

                rightClickedTextBlock = null;
            }
        }




        // حذف گیت
        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {

            if (rightClickedTextBlock != null)
            {
                // حذف خطوط مرتبط با گیت
                List<Connection> connectionsToRemove = new List<Connection>();
                foreach (var connection in connections)
                {
                    if (connection.Tb1 == rightClickedTextBlock || connection.Tb2 == rightClickedTextBlock)
                    {
                        MainCanvas.Children.Remove(connection.Line);
                        MainCanvas.Children.Remove(connection.ArrowHead);
                        connectionsToRemove.Add(connection);
                    }
                }

                // حذف خطوط از لیست اتصالات
                foreach (var connection in connectionsToRemove)
                {
                    connections.Remove(connection);
                }

                // حذف خود گیت
                MainCanvas.Children.Remove(rightClickedTextBlock);
                rightClickedTextBlock = null;
                connectingLine = null;
            }

        }

        // اتصال گیت
        private void ConnectItem_Click(object sender, RoutedEventArgs e)
        {
            if (firstSelectedTextBlock == null)
            {
                firstSelectedTextBlock = rightClickedTextBlock;
            }
            else if (firstSelectedTextBlock != rightClickedTextBlock)
            {
                DrawLineBetweenTextBlocks(firstSelectedTextBlock, rightClickedTextBlock);
                firstSelectedTextBlock = null;
            }
        }



        // کشیدن فلش در وسط خط اتصال
        private Polygon CreateArrow(Point start, Point end)
        {
            const double ArrowLength = 10;
            const double ArrowWidth = 5;

            // محاسبه مختصات وسط خط
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


        // کشیدن خط بین دو گیت
        private void DrawLineBetweenTextBlocks(TextBlock tb1, TextBlock tb2)
        {
            Point tb1Pos = tb1.TransformToAncestor(MainCanvas).Transform(new Point(0, 0));
            Point tb2Pos = tb2.TransformToAncestor(MainCanvas).Transform(new Point(0, 0));

            double startX = tb1Pos.X + tb1.Width / 2;
            double startY = tb1Pos.Y + tb1.Height / 2;
            double endX = tb2Pos.X + tb2.Width / 2;
            double endY = tb2Pos.Y + tb2.Height / 2;
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


            connections.Add(new Connection(tb1, tb2, line, arrowHead));



        }


        private void UpdateConnections(TextBlock textBlock)
        {
            foreach (var connection in connections)
            {
                if (connection.Tb1 == textBlock || connection.Tb2 == textBlock)
                {
                    UpdateLine(connection);
                }
            }
        }


        // آپدیت کردن خط بین اشیاء هنگام جا به جایی گیت ها
        private void UpdateLine(Connection connection)
        {
            Point tb1Pos = connection.Tb1.TransformToAncestor(MainCanvas).Transform(new Point(0, 0));
            Point tb2Pos = connection.Tb2.TransformToAncestor(MainCanvas).Transform(new Point(0, 0));

            double startX = tb1Pos.X + connection.Tb1.Width / 2;
            double startY = tb1Pos.Y + connection.Tb1.Height / 2;
            double endX = tb2Pos.X + connection.Tb2.Width / 2;
            double endY = tb2Pos.Y + connection.Tb2.Height / 2;
            double midX = (startX + endX) / 2;

            connection.Line.Points.Clear();
            connection.Line.Points.Add(new Point(startX, startY));
            connection.Line.Points.Add(new Point(midX, startY));
            connection.Line.Points.Add(new Point(midX, endY));
            connection.Line.Points.Add(new Point(endX, endY));


            // به‌روزرسانی فلش
            Polygon newArrowHead = CreateArrow(new Point(midX, endY), new Point(endX, endY));
            MainCanvas.Children.Remove(connection.ArrowHead);
            connection.ArrowHead = newArrowHead;
            MainCanvas.Children.Add(newArrowHead);

        }



        // اضافه کردن گیت به پنل
        private void logicGate_Selected(object sender, MouseButtonEventArgs e)
        {
            if (logicGateListBox.SelectedItem != null)
            {
                string selectedItemText = (logicGateListBox.SelectedItem as ListBoxItem).Content.ToString();
                var newTextBlock = CreateDraggableTextBlock(selectedItemText.Trim());
                MainCanvas.Children.Add(newTextBlock);
            }
        }

        //اضافه کردن اسم به گیت اضافه شده
        private TextBlock CreateDraggableTextBlock(string text)
        {
            var textBlock = new TextBlock
            {
                Text = text,
                FontSize = 25,
                Width = 40,
                Height = 40,
                Background = Brushes.Gray,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            textBlock.MouseLeftButtonDown += DraggableSquare_MouseLeftButtonDown;
            textBlock.MouseLeftButtonUp += DraggableSquare_MouseLeftButtonUp;
            textBlock.MouseMove += DraggableSquare_MouseMove;
            textBlock.MouseRightButtonDown += DraggableSquare_MouseRightButtonDown;

            return textBlock;
        }



        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //var initialTextBlock1 = CreateDraggableTextBlock("&");
            //Canvas.SetLeft(initialTextBlock1, 50);
            //Canvas.SetTop(initialTextBlock1, 50);
            //MainCanvas.Children.Add(initialTextBlock1);

            //var initialTextBlock2 = CreateDraggableTextBlock("OR");
            //Canvas.SetLeft(initialTextBlock2, 200);
            //Canvas.SetTop(initialTextBlock2, 50);
            //MainCanvas.Children.Add(initialTextBlock2);
        }

        // زوم این و زوم اوت کردن هنگام فشردن دکمه کنترل و غلطک موس
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



        // جا به جایی توی پنل با نگهداشتن دکمه اسپیس
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



        // اسکرول کردن توی پنل با غلطک موس
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


        // اعمال تغییرات متغیر ها زمانی که صفحه رو جا به جا کردیم و موس رو ول کردیم
        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isPanning)
            {
                MainCanvas.ReleaseMouseCapture();
                isPanning = false;
                lastDragPoint = null;
            }
        }

        // کلاس کمکی برای نگهداری اطلاعات خطوط اتصال
        private class Connection
        {
            public TextBlock Tb1 { get; }
            public TextBlock Tb2 { get; }
            public Polyline Line { get; }
            public Polygon ArrowHead { get; set; }


            public Connection(TextBlock tb1, TextBlock tb2, Polyline line, Polygon arrowHead)
            {
                Tb1 = tb1;
                Tb2 = tb2;
                Line = line;
                ArrowHead = arrowHead;

            }
        }
    }
}
