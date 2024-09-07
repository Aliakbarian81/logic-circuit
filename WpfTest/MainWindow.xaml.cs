using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Text.Json;
using System.IO;
using Microsoft.Win32;
using System.Windows.Media.Effects;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Xml;
using System.Windows.Markup;
using System.Text;

namespace WpfTest
{
    public partial class MainWindow : Window
    {

        // تعریف متغیر ها و اشیاء
        private bool isDragging = false;
        private bool isSimulating = false;
        private Point clickPosition;
        private List<Connection> connections = new List<Connection>();
        private Point? lastDragPoint;
        private bool isPanning;
        private bool isConnecting = false;
        private Canvas firstGateCanvas;
        private Line firstLine;
        private bool isOutput;
        private List<List<Canvas>> input_outputs = new List<List<Canvas>>();
        private List<List<Canvas>> inputs = new List<List<Canvas>>();
        private List<CheckBox> inputCheckBoxes = new List<CheckBox>();
        private List<List<Canvas>> outputs = new List<List<Canvas>>();//List<List<Canvas>>
        private List<string> outputTypes = new List<string>();
        private List<string> inputTypes = new List<string>();
        private Dictionary<int, List<UIElement>> tabElements;
        private Dictionary<int, List<Connection>> tabConnections;
        private int currentTabIndex = 0;
        private string JsonFileAddress;
        private Dictionary<int, UIElement> uiElements = new Dictionary<int, UIElement>();
        private int LastGateID = 0;
        private int LastLineID = 0;


        public MainWindow()
        {
            InitializeComponent();
            tabElements = new Dictionary<int, List<UIElement>>();
            tabConnections = new Dictionary<int, List<Connection>>();
        }



        //ساخت صفحات
        private void InitializeTabs(List<string> Pages)
        {
            for (int i = 0; i < Pages.Count; i++)
            {
                tabElements[i] = new List<UIElement>();
                tabConnections[i] = new List<Connection>();

                TabItem tabItem = new TabItem
                {
                    Header = Pages[i],
                    Tag = i
                };

                CanvasTabControl.Items.Add(tabItem);
            }
        }


        //جا به جایی بین صفحات
        private void CanvasTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CanvasTabControl.SelectedIndex != -1)
            {
                SaveCurrentTabState();
                currentTabIndex = CanvasTabControl.SelectedIndex;
                LoadCurrentTabState();
            }
        }

        //ذخیره دیتای صفحات
        private void SaveCurrentTabState()
        {
            tabElements[currentTabIndex] = MainCanvas.Children.OfType<UIElement>().ToList();
            tabConnections[currentTabIndex] = connections.ToList();
            MainCanvas.Children.Clear();
        }


        //لود کردن اطلاعات صفحات
        private void LoadCurrentTabState()
        {
            MainCanvas.Children.Clear();
            foreach (var element in tabElements[currentTabIndex])
            {
                MainCanvas.Children.Add(element);
            }
            connections = tabConnections[currentTabIndex];
        }


        //کلیک چپ کردن روی گیت برای جابه جایی
        private void DraggableSquare_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            clickPosition = e.GetPosition(sender as Canvas);
            (sender as Canvas).CaptureMouse();

        }

        // کشیدن و جا به جایی گیت
        private void DraggableSquare_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                var canvas = sender as Canvas;
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

                UpdateConnections();
            }
        }


        // دراپ کردن کلیک چپ موس هنگام جا به جا شدن گیت
        private void DraggableSquare_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            (sender as Canvas).ReleaseMouseCapture();

        }



        //کد ایجاد کردن گین جدید- این کد رو فقط مهدی حق داره تغیر بده😡
        private void logicGate_Selected(object sender, MouseButtonEventArgs e)
        {
            if (logicGateListBox.SelectedItem != null)
            {
                string? selectedGate = (logicGateListBox.SelectedItem as ListBoxItem).Content.ToString().Split('-')[0];
                int inputsNumber = Convert.ToInt32((logicGateListBox.SelectedItem as ListBoxItem).Content.ToString().Split('-')[1]);

                var gate = new Gate(selectedGate, inputsNumber);
                gate.CanvasControl.MouseLeftButtonDown += DraggableSquare_MouseLeftButtonDown;
                gate.CanvasControl.MouseLeftButtonUp += DraggableSquare_MouseLeftButtonUp;
                gate.CanvasControl.MouseMove += DraggableSquare_MouseMove;


                // اضافه کردن منوی کلیک راست
                ContextMenu contextMenu = new ContextMenu();
                MenuItem deleteGateItem = new MenuItem { Header = "Delete Gate" };
                deleteGateItem.Click += (s, args) => DeleteGate(gate);
                MenuItem deleteConnectionsItem = new MenuItem { Header = "Delete Connections" };
                deleteConnectionsItem.Click += (s, args) => DeleteConnections(gate);

                contextMenu.Items.Add(deleteGateItem);
                contextMenu.Items.Add(deleteConnectionsItem);
                gate.CanvasControl.ContextMenu = contextMenu;



                Canvas.SetTop(gate.CanvasControl, 0);
                Canvas.SetLeft(gate.CanvasControl, 20);
                MainCanvas.Children.Add(gate.CanvasControl);
            }
        }

        // حذف کردن گیت
        private void DeleteGate(Gate gate)
        {
            DeleteConnections(gate);
            MainCanvas.Children.Remove(gate.CanvasControl);
            tabElements[currentTabIndex].Remove(gate.CanvasControl);

        }

        // حذف کردن اتصالات گیت
        private void DeleteConnections(Gate gate)
        {
            var connectionsToRemove = connections.Where(c => c.Gate1 == gate.CanvasControl || c.Gate2 == gate.CanvasControl).ToList();

            foreach (var connection in connectionsToRemove)
            {
                MainCanvas.Children.Remove(connection.Line);
                MainCanvas.Children.Remove(connection.ArrowHead);
                connections.Remove(connection);
                tabConnections[currentTabIndex].Remove(connection);

            }
        }



        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //خواندن فایل جیسون
            try
            {
                var jsonFile = File.ReadAllText("logic Project.LCB");
                JsonFileAddress = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\logic Project.LCB";
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
                    JsonFileAddress = openFileDialog.FileName;

                    var jsonFile = File.ReadAllText(selectedFileName);
                    var jsonData = JsonSerializer.Deserialize<JsonClass.Root>(jsonFile);
                    CreateIN_OUT(jsonData);
                }
            }
        }

        //ایجاد اینپوت و اوتپوت ها بر اساس فایل جیسون
        private void CreateIN_OUT(JsonClass.Root? jsonData)
        {
            bool DesignAvalable = false;
            List<PagesDesignData> DesignFileData = new List<PagesDesignData>();
            if (File.Exists(System.IO.Path.ChangeExtension(JsonFileAddress, ".json"))) // بررسی وجود فایل دیزاین
            {
                try
                {
                    var jsonFile = File.ReadAllText(System.IO.Path.ChangeExtension(JsonFileAddress, ".json"));
                    DesignFileData = JsonSerializer.Deserialize<List<PagesDesignData>>(jsonFile);
                    if (jsonData.PageData.Count != DesignFileData.Count)
                    {
                        throw new Exception("Json and design files doesnt have same amount of pages");
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show("Design File Found But Program Cant Read That!", "Error!");
                    MessageBox.Show(e.Message);
                }
                DesignAvalable = MessageBox.Show("Do You Want to Load Design File?", "Design File Found", MessageBoxButton.YesNo) == MessageBoxResult.Yes;
            }

            #region حذف کمبو باکس اینپوت اوتپوت ها و خود اینئوت اوتپوت ها از لیست (input_outputs) و پیج نیم های قبلی
            inputTypes.Clear();
            outputTypes.Clear();
            PageSelector.Items.Clear();
            IiputsOutputsListBox.Items.Clear();
            input_outputs.Clear();
            inputs.Clear();
            outputs.Clear();
            CanvasTabControl.Items.Clear();
            MainCanvas.Children.Clear();
            tabElements.Clear();
            tabConnections.Clear();
            connections.Clear();
            #endregion

            InitializeTabs(jsonData.Page);

            foreach (var item in jsonData.Inputs)
            {
                inputTypes.Add(item);
            }

            foreach (var item in jsonData.OutPut)
            {
                outputTypes.Add(item);
            }

            foreach (var item in jsonData.Page)
            {
                input_outputs.Add(new List<Canvas>());
                inputs.Add(new List<Canvas>());
                outputs.Add(new List<Canvas>());
            }

            if (DesignAvalable && DesignFileData.Count > 0)
            {
                for (int i = 0; i < jsonData.PageData.Count; i++)
                {
                    for (int j = 0; j < DesignFileData[i].PageElements.Count; j++)
                    {
                        Canvas LoadedCanvas;
                        byte[] byteArray = Encoding.UTF8.GetBytes(DesignFileData[i].PageElements[j]);
                        using (MemoryStream stream = new MemoryStream(byteArray))
                        {
                            LoadedCanvas = XamlReader.Load(stream) as Canvas;
                        }

                        CanvasTabControl.SelectedIndex = i;

                        if (LoadedCanvas == null)
                        {
                            using (MemoryStream stream = new MemoryStream(byteArray))
                            {
                                var element = XamlReader.Load(stream) as UIElement;
                                MainCanvas.Children.Add(element);
                                continue;
                            }
                        }

                        if (LoadedCanvas.Tag.ToString().Split('-')[0] == "input")
                        {
                            var OutputLine = LoadedCanvas.Children.OfType<Line>().FirstOrDefault();
                            OutputLine.MouseEnter += Gate.Line_MouseEnter;
                            OutputLine.MouseLeave += Gate.Line_MouseLeave;
                            OutputLine.MouseLeftButtonDown += Gate.OutputLine_MouseLeftButtonDown;

                            input_outputs[i].Add(LoadedCanvas);
                            inputs[i].Add(LoadedCanvas);

                            MainCanvas.Children.Add(LoadedCanvas);
                            LoadedCanvas.MouseLeftButtonDown += DraggableSquare_MouseLeftButtonDown;
                            LoadedCanvas.MouseLeftButtonUp += DraggableSquare_MouseLeftButtonUp;
                            LoadedCanvas.MouseMove += DraggableSquare_MouseMove;
                        }
                        else if (LoadedCanvas.Tag.ToString().Split('-')[0] == "output")
                        {
                            var OutputLine = LoadedCanvas.Children.OfType<Line>().FirstOrDefault();
                            OutputLine.MouseEnter += Gate.Line_MouseEnter;
                            OutputLine.MouseLeave += Gate.Line_MouseLeave;
                            OutputLine.MouseLeftButtonDown += Gate.OutputLine_MouseLeftButtonDown;

                            input_outputs[i].Add(LoadedCanvas);
                            outputs[i].Add(LoadedCanvas);

                            MainCanvas.Children.Add(LoadedCanvas);
                            LoadedCanvas.MouseLeftButtonDown += DraggableSquare_MouseLeftButtonDown;
                            LoadedCanvas.MouseLeftButtonUp += DraggableSquare_MouseLeftButtonUp;
                            LoadedCanvas.MouseMove += DraggableSquare_MouseMove;
                        }
                        else
                        {
                            string selectedGate = LoadedCanvas.Tag.ToString().Split('-')[0];
                            int inputsNumber = Convert.ToInt32(LoadedCanvas.Tag.ToString().Split('-')[1]);

                            var gate = new Gate(selectedGate, inputsNumber);
                            gate.CanvasControl.MouseLeftButtonDown += DraggableSquare_MouseLeftButtonDown;
                            gate.CanvasControl.MouseLeftButtonUp += DraggableSquare_MouseLeftButtonUp;
                            gate.CanvasControl.MouseMove += DraggableSquare_MouseMove;

                            ContextMenu contextMenu = new ContextMenu();
                            MenuItem deleteGateItem = new MenuItem { Header = "Delete Gate" };
                            deleteGateItem.Click += (s, args) => DeleteGate(gate);
                            MenuItem deleteConnectionsItem = new MenuItem { Header = "Delete Connections" };
                            deleteConnectionsItem.Click += (s, args) => DeleteConnections(gate);
                            contextMenu.Items.Add(deleteGateItem);
                            contextMenu.Items.Add(deleteConnectionsItem);
                            gate.CanvasControl.ContextMenu = contextMenu;

                            Canvas.SetTop(gate.CanvasControl, Canvas.GetTop(LoadedCanvas));
                            Canvas.SetLeft(gate.CanvasControl, Canvas.GetLeft(LoadedCanvas));
                            MainCanvas.Children.Add(gate.CanvasControl);
                        }
                    }
                    // بازیابی خطوط اتصال
                    foreach (var connectionData in DesignFileData[i].PageConnections)
                    {
                        var gate1 = tabElements[i].FirstOrDefault(g => Convert.ToInt32((g as Canvas).Tag.ToString().Split('-')[1]) == connectionData.Gate1Id) as Canvas;
                        var gate2 = tabElements[i].FirstOrDefault(g => Convert.ToInt32((g as Canvas).Tag.ToString().Split('-')[1]) == connectionData.Gate2Id) as Canvas;

                        if (gate1 != null && gate2 != null)
                        {
                            var startLine = gate1.Children.OfType<Line>().FirstOrDefault();
                            var endLine = gate2.Children.OfType<Line>().FirstOrDefault();

                            if (startLine != null && endLine != null)
                            {
                                startLine.X1 = connectionData.StartLineX1;
                                startLine.Y1 = connectionData.StartLineY1;
                                endLine.X2 = connectionData.EndLineX2;
                                endLine.Y2 = connectionData.EndLineY2;

                                DrawLineBetweenGates(gate1, startLine, gate2, endLine);

                            }

                        }
                        MessageBox.Show($"Loading connection: Start({connectionData.StartLineX1}, {connectionData.StartLineY1}) End({connectionData.EndLineX2}, {connectionData.EndLineY2})");

                    }

                }
            }

            // *** اینجا کد اضافه شده‌ای که شما اشاره کردید ***

            for (int j = 0; j < jsonData.PageData.Count; j++)
            {
                for (int i = 0; i < jsonData.CountInput; i++)
                {
                    #region ظاهر گیت
                    ComboBox inputComboBox = new ComboBox();
                    inputComboBox.Width = 120;
                    inputComboBox.Margin = new Thickness(0, 30, 0, 0);
                    inputComboBox.Name = "inputComboBox" + i;
                    foreach (var item in jsonData.Inputs)
                    {
                        inputComboBox.Items.Add(item);
                    }
                    inputComboBox.SelectedIndex = jsonData.PageData[0].AssignInput[i];

                    if (j == 0)
                    {
                        var listBoxItem = new ListBoxItem
                        {
                            Background = new SolidColorBrush(Color.FromRgb(233, 233, 233)),
                            BorderBrush = new SolidColorBrush(Color.FromRgb(164, 162, 162)),
                            Content = "Input " + i
                        };
                        var transformGroup = new TransformGroup();
                        listBoxItem.RenderTransform = transformGroup;
                        listBoxItem.MouseDoubleClick += InputOutput_Selected;
                        IiputsOutputsListBox.Items.Add(listBoxItem);
                    }

                    var CanvasControl = new Canvas();
                    CanvasControl.Width = 100;
                    CanvasControl.Height = 100;
                    CanvasControl.Tag = "input-";

                    var GridControl = new Grid();
                    GridControl.RowDefinitions.Add(new RowDefinition());
                    GridControl.RowDefinitions.Add(new RowDefinition());
                    GridControl.ColumnDefinitions.Add(new ColumnDefinition());

                    var border = new Border
                    {
                        Width = 50,
                        Height = 80,
                        CornerRadius = new CornerRadius(7),
                        Background = new SolidColorBrush(Color.FromArgb(220, 50, 50, 50)),
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(2),
                        Effect = new DropShadowEffect
                        {
                            Color = Colors.Black,
                            Direction = 320,
                            ShadowDepth = 5,
                            Opacity = 0.5
                        },
                        Child = GridControl
                    };
                    var OutputLine = new Line() { X1 = 48, X2 = 63, Y1 = 30, Y2 = 30, Stroke = Brushes.Black, StrokeThickness = 2 };
                    OutputLine.MouseEnter += Gate.Line_MouseEnter;
                    OutputLine.MouseLeave += Gate.Line_MouseLeave;
                    OutputLine.MouseLeftButtonDown += Gate.OutputLine_MouseLeftButtonDown;
                    CanvasControl.Children.Add(OutputLine);
                    CanvasControl.MouseLeftButtonDown += DraggableSquare_MouseLeftButtonDown;
                    CanvasControl.MouseLeftButtonUp += DraggableSquare_MouseLeftButtonUp;
                    CanvasControl.MouseMove += DraggableSquare_MouseMove;

                    var NameTextBlock = new TextBlock
                    {
                        Text = "input " + i,
                        Foreground = Brushes.White,
                        FontWeight = FontWeights.Bold,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontSize = 11
                    };
                    GridControl.Children.Add(NameTextBlock);

                    var TypeTextBlock = new TextBlock
                    {
                        Foreground = Brushes.White,
                        FontWeight = FontWeights.Bold,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Top,
                        FontSize = 9
                    };
                    GridControl.Children.Add(TypeTextBlock);
                    Grid.SetRow(NameTextBlock, 0);
                    Grid.SetRowSpan(NameTextBlock, 2);
                    Grid.SetColumn(NameTextBlock, 0);

                    border.Child = GridControl;
                    Canvas.SetLeft(border, -2);
                    Canvas.SetTop(border, -2);
                    CanvasControl.Children.Add(border);
                    var checkbox = new CheckBox() { Visibility = Visibility.Hidden };
                    checkbox.Checked += Activator_Checked;
                    checkbox.Unchecked += Activator_Unchecked;
                    checkbox.Tag = border;
                    inputCheckBoxes.Add(checkbox);
                    CanvasControl.Children.Add(checkbox);

                    CanvasControl.Visibility = Visibility.Hidden;
                    #endregion

                    if (!DesignAvalable || DesignFileData.Count <= 0)
                    {
                        input_outputs[j].Add(CanvasControl);
                        inputs[j].Add(CanvasControl);

                        CanvasTabControl.SelectedIndex = j;
                        MainCanvas.Children.Add(CanvasControl);
                    }
                }
            }

            for (int j = 0; j < jsonData.PageData.Count; j++)
            {
                for (int i = 0; i < jsonData.CountOutPut; i++)
                {
                    #region ظاهر گیت
                    ComboBox outputComboBox = new ComboBox();
                    outputComboBox.Width = 120;
                    outputComboBox.Margin = new Thickness(0, 30, 0, 0);
                    outputComboBox.Name = "outputComboBox" + i;
                    foreach (var item in jsonData.OutPut)
                    {
                        outputComboBox.Items.Add(item);
                    }
                    outputComboBox.SelectedIndex = jsonData.PageData[0].AssignOutput[i];

                    if (j == 0)
                    {
                        var listBoxItem = new ListBoxItem
                        {
                            Background = new SolidColorBrush(Color.FromRgb(233, 233, 233)),
                            BorderBrush = new SolidColorBrush(Color.FromRgb(164, 162, 162)),
                            Content = "OutPut " + i
                        };
                        var transformGroup = new TransformGroup();
                        listBoxItem.RenderTransform = transformGroup;
                        listBoxItem.MouseDoubleClick += InputOutput_Selected;
                        IiputsOutputsListBox.Items.Add(listBoxItem);
                    }

                    var CanvasControl = new Canvas();
                    CanvasControl.Width = 100;
                    CanvasControl.Height = 100;
                    CanvasControl.Tag = "output-";

                    var GridControl = new Grid();
                    GridControl.RowDefinitions.Add(new RowDefinition());
                    GridControl.RowDefinitions.Add(new RowDefinition());
                    GridControl.ColumnDefinitions.Add(new ColumnDefinition());

                    var border = new Border
                    {
                        Width = 50,
                        Height = 80,
                        CornerRadius = new CornerRadius(7),
                        Background = new SolidColorBrush(Color.FromArgb(220, 50, 50, 50)),
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(2),
                        Effect = new DropShadowEffect
                        {
                            Color = Colors.Black,
                            Direction = 320,
                            ShadowDepth = 5,
                            Opacity = 0.5
                        },
                        Child = GridControl
                    };

                    var OutputLine = new Line() { X1 = -15, X2 = 0, Y1 = 30, Y2 = 30, Stroke = Brushes.Black, StrokeThickness = 2 };
                    OutputLine.MouseEnter += Gate.Line_MouseEnter;
                    OutputLine.MouseLeave += Gate.Line_MouseLeave;
                    OutputLine.MouseLeftButtonDown += Gate.InputLine_MouseLeftButtonDown;
                    CanvasControl.Children.Add(OutputLine);
                    CanvasControl.MouseLeftButtonDown += DraggableSquare_MouseLeftButtonDown;
                    CanvasControl.MouseLeftButtonUp += DraggableSquare_MouseLeftButtonUp;
                    CanvasControl.MouseMove += DraggableSquare_MouseMove;

                    var NameTextBlock = new TextBlock
                    {
                        Text = "Out " + i,
                        Foreground = Brushes.White,
                        FontWeight = FontWeights.Bold,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontSize = 11
                    };
                    GridControl.Children.Add(NameTextBlock);

                    var TypeTextBlock = new TextBlock
                    {
                        Foreground = Brushes.White,
                        FontWeight = FontWeights.Bold,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Top,
                        FontSize = 9
                    };
                    GridControl.Children.Add(TypeTextBlock);

                    Grid.SetRow(NameTextBlock, 0);
                    Grid.SetRowSpan(NameTextBlock, 2);
                    Grid.SetColumn(NameTextBlock, 0);

                    border.Child = GridControl;
                    Canvas.SetLeft(border, -2);
                    Canvas.SetTop(border, -2);
                    CanvasControl.Children.Add(border);

                    Canvas.SetLeft(CanvasControl, 20);
                    CanvasControl.Visibility = Visibility.Hidden;
                    #endregion

                    if (!DesignAvalable || DesignFileData.Count <= 0)
                    {
                        input_outputs[j].Add(CanvasControl);
                        outputs[j].Add(CanvasControl);

                        CanvasTabControl.SelectedIndex = j;
                        MainCanvas.Children.Add(CanvasControl);
                    }
                }
            }

            // *** پایان کد اضافه شده ***

            for (int i = 0; i < jsonData.Page.Count; i++)
            {
                PageSelector.Items.Add(jsonData.Page[i]);
            }
            PageSelector.SelectedIndex = 0;
            CanvasTabControl.SelectedIndex = 0;
        }

        //ایجاد اینپوت یا اوتپوت
        private void InputOutput_Selected(object sender, MouseButtonEventArgs e)
        {
            int currentPageNumber = CanvasTabControl.SelectedIndex;
            if (IiputsOutputsListBox.SelectedItem != null)
            {
                string? selected = (IiputsOutputsListBox.SelectedItem as ListBoxItem).Content.ToString();
                if (selected.Contains("Input"))
                {
                    ComboBoxForm cbForm = new ComboBoxForm(inputTypes);
                    cbForm.Owner = this;
                    cbForm.ShowDialog();
                    string selectedOption = cbForm.cmbOptions.SelectedItem as string;
                    var TypeTextBlock = ((Grid)(inputs[currentPageNumber][int.Parse(selected[6].ToString())].Children.OfType<Border>().FirstOrDefault().Child)).Children.OfType<TextBlock>().ElementAtOrDefault(1);
                    if (TypeTextBlock != null)
                    {
                        TypeTextBlock.Text = selectedOption;
                    }

                    inputs[currentPageNumber][int.Parse(selected[6].ToString())].Visibility = Visibility.Visible;
                }
                else if (selected.Contains("OutPut"))
                {
                    ComboBoxForm cbForm = new ComboBoxForm(outputTypes);
                    cbForm.Owner = this;
                    cbForm.ShowDialog();
                    string selectedOption = cbForm.cmbOptions.SelectedItem as string;
                    var TypeTextBlock = ((Grid)(outputs[currentPageNumber][int.Parse(selected[7].ToString())].Children.OfType<Border>().FirstOrDefault().Child)).Children.OfType<TextBlock>().ElementAtOrDefault(1);
                    if (TypeTextBlock != null)
                    {
                        TypeTextBlock.Text = selectedOption;
                    }
                    outputs[currentPageNumber][int.Parse(selected[7].ToString())].Visibility = Visibility.Visible;
                }
            }
        }

        private void Activator_Checked(object sender, RoutedEventArgs e)//for input checkboxes
        {
            ((sender as CheckBox).Tag as Border).Background = Brushes.Green;
            SimulationLogic();
        }
        private void Activator_Unchecked(object sender, RoutedEventArgs e)//for input checkboxes
        {
            ((sender as CheckBox).Tag as Border).Background = new SolidColorBrush(Color.FromArgb(180, 50, 50, 50));
            SimulationLogic();
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
                if (output != isOutput && firstGateCanvas != gateCanvas)
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
                    MessageBox.Show("impossible connection.");
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
            Point startPoint = gate1.TransformToAncestor(MainCanvas).Transform(new Point(line1.X1, line1.Y1));
            Point endPoint = gate2.TransformToAncestor(MainCanvas).Transform(new Point(line2.X2, line2.Y2));

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

            connections.Add(new Connection(gate1, gate2, connectionLine, arrowHead, line1, line2));
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

            // موقعیت فلش روی خط، بدون چرخش
            Canvas.SetLeft(arrowHead, midPoint.X);
            Canvas.SetTop(arrowHead, midPoint.Y);

            return arrowHead;
        }



        //محدود کردن نقاط اتصال به لبه های گیت
        private Point LimitToGateBounds(Canvas gate, Point point)
        {
            var bounds = new Rect(0, 0, gate.ActualWidth, gate.ActualHeight);
            var transformedBounds = gate.TransformToAncestor(MainCanvas).TransformBounds(bounds);

            if (point.X < transformedBounds.Left)
                point.X = transformedBounds.Left;
            else if (point.X > transformedBounds.Right)
                point.X = transformedBounds.Right;

            if (point.Y < transformedBounds.Top)
                point.Y = transformedBounds.Top;
            else if (point.Y > transformedBounds.Bottom)
                point.Y = transformedBounds.Bottom;

            return point;
        }



        // آپدیت کردن اتصال خطوط هنگان جا به جایی گیت ها
        private void UpdateConnections()
        {
            foreach (var connection in connections)
            {
                var startCanvas = connection.Gate1;
                var endCanvas = connection.Gate2;
                var startLine = connection.StartLine;
                var endLine = connection.EndLine;
                // محاسبه نقاط شروع و پایان خط اتصال
                var startPoint = startCanvas.TransformToAncestor(MainCanvas).Transform(new Point(startLine.X1, startLine.Y1));
                var endPoint = endCanvas.TransformToAncestor(MainCanvas).Transform(new Point(endLine.X2, endLine.Y2));

                startPoint = LimitToGateBounds(startCanvas, startPoint);
                endPoint = LimitToGateBounds(endCanvas, endPoint);

                // به روزرسانی نقاط خط اتصال
                var polyline = connection.Line;
                polyline.Points.Clear();
                polyline.Points.Add(startPoint);
                polyline.Points.Add(new Point((startPoint.X + endPoint.X) / 2, startPoint.Y));
                polyline.Points.Add(new Point((startPoint.X + endPoint.X) / 2, endPoint.Y));
                polyline.Points.Add(endPoint);

                // به روزرسانی فقط موقعیت فلش بدون تغییر زاویه
                var midPoint = new Point((startPoint.X + endPoint.X) / 2, (startPoint.Y + endPoint.Y) / 2);
                Canvas.SetLeft(connection.ArrowHead, midPoint.X);
                Canvas.SetTop(connection.ArrowHead, midPoint.Y);
            }
        }





        private void OpenButton_Click(object sender, RoutedEventArgs e)
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
                JsonFileAddress = openFileDialog.FileName;

                var jsonFile = File.ReadAllText(selectedFileName);
                var jsonData = JsonSerializer.Deserialize<JsonClass.Root>(jsonFile);
                CreateIN_OUT(jsonData);
            }
        }

        private void SimulationButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isSimulating)//آغاز سیمولیشن
            {
                isSimulating = true;
                PageSelector.IsEnabled = false;
                OpenBTN.IsEnabled = false;
                CompileBTN.IsEnabled = false;
                SaveBTN.IsEnabled = false;
                SimulationBTN.Background = Brushes.GreenYellow;
                foreach (var item in inputCheckBoxes)//همه چک باکس هارو نشون بده و رنگ ورودی هایی که چک باکس تیک خورده رو سبز کن
                {
                    item.Visibility = Visibility.Visible;
                    if ((bool)item.IsChecked)
                    {
                        (item.Tag as Border).Background = Brushes.Green;
                    }
                }
                //نمونه سیمولیشن نهایی برای اینپوت شماره یک
                SimulationLogic();

            }
            else//قطع سیمولیشن
            {
                isSimulating = false;
                PageSelector.IsEnabled = true;
                OpenBTN.IsEnabled = true;
                CompileBTN.IsEnabled = true;
                SaveBTN.IsEnabled = true;
                SimulationBTN.Background = Brushes.LightGray;
                foreach (var item in inputCheckBoxes)
                {
                    item.Visibility = Visibility.Hidden;
                    (item.Tag as Border).Background = new SolidColorBrush(Color.FromArgb(180, 50, 50, 50));
                }
                foreach (var item in outputs)
                {
                    foreach (var item2 in item)
                    {
                        item2.Children.OfType<Border>().FirstOrDefault().Background = new SolidColorBrush(Color.FromArgb(180, 50, 50, 50));
                    }
                }
            }
        }
        public void SimulationLogic()//تابع برگشتی رو برای تمام اوتپوت ها اجرا میکند
        {
            int currentPageNumber = CanvasTabControl.SelectedIndex;
            foreach (var output in outputs[currentPageNumber])
            {
                if (SimulationLogicLoop(output, "output") == true)
                {
                    output.Children.OfType<Border>().FirstOrDefault().Background = Brushes.Green;
                }
                else
                {
                    output.Children.OfType<Border>().FirstOrDefault().Background = new SolidColorBrush(Color.FromArgb(180, 50, 50, 50));
                }
            }
        }
        private bool SimulationLogicLoop(Canvas Gate, string? GateType)//تابع برگشتی برای مراحل سیمولیشن
        {
            var Gateconnections = connections.Where(c => c.Gate2 == Gate).ToList();
            if (Gateconnections.Count == 0 && GateType != "input")
                return false;

            if (GateType == "output")
            {
                return SimulationLogicLoop(Gateconnections[0].Gate1, Gateconnections[0].Gate1.Tag.ToString().Split('-')[0]);
            }
            else if (GateType == "input")
            {
                if (Gate.Children.OfType<Border>().FirstOrDefault().Background == Brushes.Green)
                {
                    return true;
                }
            }
            else if (GateType == "NOT")
            {
                return !SimulationLogicLoop(Gateconnections[0].Gate1, Gateconnections[0].Gate1.Tag.ToString().Split('-')[0]);
            }
            else if (GateType == "AND")
            {
                foreach (var connection in Gateconnections)
                {
                    if (!SimulationLogicLoop(connection.Gate1, connection.Gate1.Tag.ToString().Split('-')[0]))
                    {
                        return false;
                    }
                }
                return true;
            }
            else if (GateType == "OR")
            {
                foreach (var connection in Gateconnections)
                {
                    if (SimulationLogicLoop(connection.Gate1, connection.Gate1.Tag.ToString().Split('-')[0]))
                    {
                        return true;
                    }
                }
                return false;
            }
            else if (GateType == "NOR")
            {
                foreach (var connection in Gateconnections)
                {
                    if (SimulationLogicLoop(connection.Gate1, connection.Gate1.Tag.ToString().Split('-')[0]))
                    {
                        return false;
                    }
                }
                return true;
            }
            else if (GateType == "NAND")
            {
                foreach (var connection in Gateconnections)
                {
                    if (!SimulationLogicLoop(connection.Gate1, connection.Gate1.Tag.ToString().Split('-')[0]))
                    {
                        return true;
                    }
                }
                return false;
            }


            return false;
        }

        private void CompileBTN_Click(object sender, RoutedEventArgs e)
        {
            int SelectedPageIndex = CanvasTabControl.SelectedIndex;
            var res = new List<List<string>>();
            for (int i = 0; i < outputs.Count; i++)
            {
                CanvasTabControl.SelectedIndex = i;
                res.Add(new List<string>());
                foreach (var item in outputs[i])
                {
                    var resault = CompileOutput(inputs[i].Count, item, inputs[i]);
                    res[i].Add(resault.Substring(0, resault.Length / 2));
                    res[i].Add(resault.Substring(resault.Length / 2));
                }

            }
            CanvasTabControl.SelectedIndex = SelectedPageIndex;
            foreach (var item in input_outputs)
            {
                foreach (var item2 in item)
                {
                    item2.Children.OfType<Border>().FirstOrDefault().Background = new SolidColorBrush(Color.FromArgb(180, 50, 50, 50));
                }
            }
            //save res in file:
            var jsonFile = File.ReadAllText(JsonFileAddress);
            var jsonData = JsonSerializer.Deserialize<JsonClass.Root>(jsonFile);
            for (int i = 0; i < jsonData.PageData.Count; i++)
            {
                for (int j = 0; j < res[i].Count; j++)//jsonData.PageData[i].ValueOutput.Count
                {
                    jsonData.PageData[i].ValueOutput[j] = res[i][j];
                }
            }
            File.WriteAllText(JsonFileAddress, JsonSerializer.Serialize(jsonData, new JsonSerializerOptions() { WriteIndented = true }));
            MessageBox.Show("Resault Saved.");
        }
        public string CompileOutput(int inputsCount, Canvas output, List<Canvas> PageInputs)
        {
            string res = "";
            for (int i = 0; i < (int)Math.Pow(2, inputsCount); i++)
            {

                string binary = Convert.ToString(i, 2).PadLeft(inputsCount, '0');//00000,00001,00010,00011,...
                for (int j = 0; j < inputsCount; j++)
                {
                    if (binary[j] == '1')
                    {
                        PageInputs[j].Children.OfType<Border>().FirstOrDefault().Background = Brushes.Green;
                    }
                    else
                    {
                        PageInputs[j].Children.OfType<Border>().FirstOrDefault().Background = new SolidColorBrush(Color.FromArgb(180, 50, 50, 50));
                    }
                }
                bool result = SimulationLogicLoop(output, "output");
                res += result ? "1" : "0";

            }
            return res;
        }

        private void SaveBTN_Click(object sender, RoutedEventArgs e)
        {
            //ذخیره کردن دیتای صفحه فعال
            tabElements[currentTabIndex] = MainCanvas.Children.OfType<UIElement>().ToList();
            tabConnections[currentTabIndex] = connections.ToList();

            List<PagesDesignData> projectData = new List<PagesDesignData>();


            // تبدیل عناصر به ProjectData و سپس به XAML
            foreach (var elements in tabElements)
            {
                var pageData = new PagesDesignData { PageNumber = elements.Key };

                foreach (var connection in tabConnections[elements.Key])
                {
                    //connection.EndLine.Tag = "Line-" + LastLineID++;
                    //connection.StartLine.Tag = "Line-" + LastLineID++ + "-" + connection.EndLine.Tag.ToString().Split('-')[1];

                    var serializedConnection = new SerializedConnection
                    {
                        Gate1Id = Convert.ToInt32(connection.Gate1.Tag.ToString().Split('-')[1]),
                        Gate2Id = Convert.ToInt32(connection.Gate2.Tag.ToString().Split('-')[1]),
                        StartLineX1 = connection.StartLine.X1,
                        StartLineY1 = connection.StartLine.Y1,
                        EndLineX2 = connection.EndLine.X2,
                        EndLineY2 = connection.EndLine.Y2
                    };

                    pageData.PageConnections.Add(serializedConnection);
                    MessageBox.Show($"Saving connection: Start({connection.StartLine.X1}, {connection.StartLine.Y1}) End({connection.EndLine.X2}, {connection.EndLine.Y2})");



                }
                foreach (var element in elements.Value)
                {
                    var xamlString = XamlWriter.Save(element);
                    pageData.PageElements.Add(xamlString);
                }
                projectData.Add(pageData);
            }

            // سریالایز کردن به JSON
            File.WriteAllText(System.IO.Path.ChangeExtension(JsonFileAddress, ".json"), JsonSerializer.Serialize(projectData, new JsonSerializerOptions() { WriteIndented = true }));

            MessageBox.Show("project saved");
        }

    }
}

// نگهداری اطلاعات اتصالات
public class Connection
{
    public Canvas Gate1 { get; }
    public Canvas Gate2 { get; }
    public Polyline Line { get; }
    public Polygon ArrowHead { get; set; }
    public Line StartLine { get; }
    public Line EndLine { get; }

    public Connection(Canvas gate1, Canvas gate2, Polyline line, Polygon arrowHead, Line startLine, Line endLine)
    {
        Gate1 = gate1;
        Gate2 = gate2;
        Line = line;
        ArrowHead = arrowHead;
        StartLine = startLine;
        EndLine = endLine;
    }
}

[Serializable]
public class SerializedConnection
{
    public int Gate1Id { get; set; }
    public int Gate2Id { get; set; }
    public double StartLineX1 { get; set; }  // اضافه کردن مختصات خطوط اتصال
    public double StartLineY1 { get; set; }
    public double EndLineX2 { get; set; }
    public double EndLineY2 { get; set; }
}


public class PagesDesignData
{
    public int PageNumber { get; set; } // ذخیره ID های 
    public List<string> PageElements { get; set; } // ذخیره SerializableCanvasElement
    public List<SerializedConnection> PageConnections { get; set; } // ذخیره SerializableConnections
    public PagesDesignData()
    {
        PageElements = new List<string>();
        PageConnections = new List<SerializedConnection>();
    }
}



