﻿using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Text.Json;
using System.IO;
using Microsoft.Win32;

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
        private List<Canvas> input_outputs = new List<Canvas>();

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


                var gate = new Gate(selectedGate, inputsNumber);
                gate.RectangleControl.MouseLeftButtonDown += DraggableSquare_MouseLeftButtonDown;
                gate.RectangleControl.MouseLeftButtonUp += DraggableSquare_MouseLeftButtonUp;
                gate.RectangleControl.MouseMove += DraggableSquare_MouseMove;

                Canvas.SetTop(gate.CanvasControl, 0);
                Canvas.SetLeft(gate.CanvasControl, 20);
                MainCanvas.Children.Add(gate.CanvasControl);
            }
        }



        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //خواندن فایل جیسون
            try
            {
                var jsonFile = File.ReadAllText("logic Project.LCB");
                var jsonData = JsonSerializer.Deserialize<JsonClass.Root>(jsonFile);
                CreateIN_OUT(jsonData);
            }
            catch (Exception)
            {
                MessageBox.Show("Error: Cant Find Json FILE (logic Project.LCB)");
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Title = "Chose logic Project.LCB", // تنظیم عنوان پنجره
                    Filter = "Chose LCB File (*.lcb)|*.lcb"// تنظیم فیلتر فایل ها
                };

                // بررسی انتخاب فایل
                if (openFileDialog.ShowDialog() == true)
                {
                    // نام فایل انتخاب شده
                    string selectedFileName = openFileDialog.FileName;

                    var jsonFile = File.ReadAllText(selectedFileName);
                    var jsonData = JsonSerializer.Deserialize<JsonClass.Root>(jsonFile);
                    CreateIN_OUT(jsonData);
                }
            }
        }

        //ایجاد اینپوت و اوتپوت ها بر اساس قایل جیسون
        private void CreateIN_OUT(JsonClass.Root? jsonData)
        {
            #region حذف کمبو باکس اینپوت اوتپوت ها و خود اینئوت اوتپوت ها از لیست (input_outputs) و پیج نیم های قبلی
            inputsList.Children.Clear();
            inputsList.Children.Add(new Label() { Content = " inputs:" });
            outputsList.Children.Clear();
            outputsList.Children.Add(new Label() { Content = " outputs:" });
            PageSelector.Items.Clear();
            foreach (var item in input_outputs)
            {
                MainCanvas.Children.Remove(item);
            }
            #endregion
            for (int i = 0; i < jsonData.CountInput; i++)
            {
                //comboBox
                ComboBox inputComboBox = new ComboBox();
                inputComboBox.Width = 120;
                inputComboBox.Margin = new Thickness(0, 30, 0, 0);
                inputComboBox.Name = "inputComboBox" + i;
                foreach (var item in jsonData.Inputs)
                {
                    inputComboBox.Items.Add(item);
                }
                inputComboBox.SelectedIndex = jsonData.PageData[0].AssignInput[i];
                inputsList.Children.Add(inputComboBox);
                //shape (canvas and rect in viewBox)
                var CanvasControl = new Canvas();
                CanvasControl.Width = 100;
                CanvasControl.Height = 100;
                var RectangleControl = new Rectangle();
                RectangleControl.Width = 50;
                RectangleControl.Height = 60;
                RectangleControl.Fill = Brushes.Gray;
                CanvasControl.Children.Add(RectangleControl);
                var OutputLine = new Line() { X1 = 50, X2 = 65, Y1 = 30, Y2 = 30, Stroke = Brushes.Black, StrokeThickness = 2 };
                OutputLine.MouseEnter += Gate.Line_MouseEnter;
                OutputLine.MouseLeave += Gate.Line_MouseLeave;
                OutputLine.MouseLeftButtonDown += Gate.OutputLine_MouseLeftButtonDown;
                CanvasControl.Children.Add(OutputLine);
                RectangleControl.MouseLeftButtonDown += DraggableSquare_MouseLeftButtonDown;
                RectangleControl.MouseLeftButtonUp += DraggableSquare_MouseLeftButtonUp;
                RectangleControl.MouseMove += DraggableSquare_MouseMove;
                // ایجاد Grid
                var GridControl = new Grid();
                GridControl.Width = 50;
                GridControl.Height = 60;
                GridControl.RowDefinitions.Add(new RowDefinition());
                GridControl.RowDefinitions.Add(new RowDefinition());
                GridControl.ColumnDefinitions.Add(new ColumnDefinition());
                // ایجاد تکست بلاک برای نمایش نام گیت
                var NameTextBlock = new TextBlock
                {
                    Text = "input " + i,
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                GridControl.Children.Add(NameTextBlock);

                Grid.SetRow(RectangleControl, 0);
                Grid.SetRowSpan(RectangleControl, 2);
                Grid.SetColumn(RectangleControl, 0);
                Grid.SetRow(NameTextBlock, 0);
                Grid.SetRowSpan(NameTextBlock, 2);
                Grid.SetColumn(NameTextBlock, 0);
                CanvasControl.Children.Add(GridControl);
                var ss = (i * 100) + 100;
                Canvas.SetTop(CanvasControl, ss);
                Canvas.SetLeft(CanvasControl, 40);
                input_outputs.Add(CanvasControl);
                MainCanvas.Children.Add(CanvasControl);
            }//ایجاد اینپوت ها
            for (int i = 0; i < jsonData.CountOutPut; i++)
            {
                //comboBox
                ComboBox outputComboBox = new ComboBox();
                outputComboBox.Width = 120;
                outputComboBox.Margin = new Thickness(0, 30, 0, 0);
                outputComboBox.Name = "outputComboBox" + i;
                foreach (var item in jsonData.OutPut)
                {
                    outputComboBox.Items.Add(item);
                }
                outputComboBox.SelectedIndex = jsonData.PageData[0].AssignOutput[i];
                outputsList.Children.Add(outputComboBox);
                //shape (canvas and rect in viewBox)
                var CanvasControl = new Canvas();
                CanvasControl.Width = 100;
                CanvasControl.Height = 100;
                var RectangleControl = new Rectangle();
                RectangleControl.Width = 50;
                RectangleControl.Height = 60;
                RectangleControl.Fill = Brushes.Gray;
                CanvasControl.Children.Add(RectangleControl);
                var OutputLine = new Line() { X1 = -15, X2 = 0, Y1 = 30, Y2 = 30, Stroke = Brushes.Black, StrokeThickness = 2 };
                OutputLine.MouseEnter += Gate.Line_MouseEnter;
                OutputLine.MouseLeave += Gate.Line_MouseLeave;
                OutputLine.MouseLeftButtonDown += Gate.OutputLine_MouseLeftButtonDown;
                CanvasControl.Children.Add(OutputLine);
                RectangleControl.MouseLeftButtonDown += DraggableSquare_MouseLeftButtonDown;
                RectangleControl.MouseLeftButtonUp += DraggableSquare_MouseLeftButtonUp;
                RectangleControl.MouseMove += DraggableSquare_MouseMove;
                // ایجاد Grid
                var GridControl = new Grid();
                GridControl.Width = 50;
                GridControl.Height = 60;
                GridControl.RowDefinitions.Add(new RowDefinition());
                GridControl.RowDefinitions.Add(new RowDefinition());
                GridControl.ColumnDefinitions.Add(new ColumnDefinition());
                // ایجاد تکست بلاک برای نمایش نام گیت
                var NameTextBlock = new TextBlock
                {
                    Text = "Out " + i,
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                GridControl.Children.Add(NameTextBlock);

                Grid.SetRow(RectangleControl, 0);
                Grid.SetRowSpan(RectangleControl, 2);
                Grid.SetColumn(RectangleControl, 0);
                Grid.SetRow(NameTextBlock, 0);
                Grid.SetRowSpan(NameTextBlock, 2);
                Grid.SetColumn(NameTextBlock, 0);
                CanvasControl.Children.Add(GridControl);
                var ss = (i * 100) + 100;
                Canvas.SetTop(CanvasControl, ss);
                Canvas.SetLeft(CanvasControl, 680);
                input_outputs.Add(CanvasControl);
                MainCanvas.Children.Add(CanvasControl);
            }//ایجاد اوتپوت ها
            for (int i = 0; i < jsonData.Page.Count; i++)
            {
                PageSelector.Items.Add(jsonData.Page[i]);
                PageSelector.SelectedIndex = 0;
            }
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
            Polygon arrowHead = CreateArrowHead(startPoint, endPoint);
            MainCanvas.Children.Add(arrowHead);

            connections.Add(new Connection(gate1, gate2, connectionLine, arrowHead));
        }



        // ایجاد فلش در وسط خط اتصال بین دو گیت
        private Polygon CreateArrowHead(Point startPoint, Point endPoint)
        {
            double arrowHeadSize = 10;
            Point midPoint = new Point((startPoint.X + endPoint.X) / 2, (startPoint.Y + endPoint.Y) / 2);

            // ایجاد مثلث
            Polygon arrowHead = new Polygon
            {
                Fill = Brushes.Black,
                Points = new PointCollection(new Point[]
                {
            new Point(0, 0),
            new Point(-arrowHeadSize, arrowHeadSize / 2),
            new Point(-arrowHeadSize, -arrowHeadSize / 2)
                })
            };

            // محاسبه زاویه چرخش فلش
            double angle = Math.Atan2(endPoint.Y - startPoint.Y, endPoint.X - startPoint.X) * 180 / Math.PI;
            RotateTransform rotateTransform = new RotateTransform(angle, 0, 0);
            arrowHead.RenderTransform = rotateTransform;

            // محاسبه مکان دقیق فلش روی خط
            double offsetX = -arrowHeadSize / 2 * Math.Cos(angle * Math.PI / 180);
            double offsetY = -arrowHeadSize / 2 * Math.Sin(angle * Math.PI / 180);
            Canvas.SetLeft(arrowHead, midPoint.X + offsetX);
            Canvas.SetTop(arrowHead, midPoint.Y + offsetY);

            return arrowHead;
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



        // آپدیت کردن اتصال خطوط هنگان جا به جایی گیت ها
        private void UpdateConnections()
        {
            foreach(var connection in connections)
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
                double arrowAngle = Math.Atan2(endPoint.Y - startPoint.Y, endPoint.X - startPoint.X) * 180 / Math.PI;
                connection.ArrowHead.RenderTransform = new RotateTransform(arrowAngle, 0, 0);
                Canvas.SetLeft(connection.ArrowHead, midPoint.X);
                Canvas.SetTop(connection.ArrowHead, midPoint.Y);
            }
        }




        // نگهداری اطلاعات اتصالات
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            // تنظیم عنوان پنجره
            openFileDialog.Title = "Chose logic Project.LCB";
            // تنظیم فیلتر فایل ها
            openFileDialog.Filter = "Chose LCB File (*.lcb)|*.lcb";
            var result = openFileDialog.ShowDialog();
            // بررسی انتخاب فایل
            if (result == true)
            {
                // نام فایل انتخاب شده
                string selectedFileName = openFileDialog.FileName;

                var jsonFile = File.ReadAllText(selectedFileName);
                var jsonData = JsonSerializer.Deserialize<JsonClass.Root>(jsonFile);
                CreateIN_OUT(jsonData);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            SimulationWindow simulationWindow = new SimulationWindow();
            simulationWindow.Show();
        }
    }
}

