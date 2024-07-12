using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media;
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

        public Gate(string? type, int inputs)//gate constructor
        {
            InputLines = new List<Line>();
            this.Inputs = inputs;
            Type = type;
            
            //ایجاد کانواز
            CanvasControl = new Canvas();
            CanvasControl.Width = 100;
            CanvasControl.Height = 100;

            //ایجاد رکتنگل یا همان مستطیل
            RectangleControl = new Rectangle();
            //TB_Control.Text = Type;
            //TB_Control.FontSize = 16;
            RectangleControl.Width = 50;
            RectangleControl.Height = 80;
            RectangleControl.Fill = Brushes.Gray;
            CanvasControl.Children.Add(RectangleControl);

            //ایجاد لاین ها
            OutputLine = new Line() { X1 = 50,X2 = 65 ,Y1 = 40, Y2 = 40 ,Stroke = Brushes.Black,StrokeThickness = 2};
            switch (inputs)
            {
                case 1:
                    InputLines.Add(new Line() { X1 = -15, X2 = 0, Y1 = 40, Y2 = 40, Stroke = Brushes.Black, StrokeThickness = 2 });
                    break;
                case 2:
                    InputLines.Add(new Line() { X1 = -15, X2 = 0, Y1 = 20, Y2 = 20, Stroke = Brushes.Black, StrokeThickness = 2 });
                    InputLines.Add(new Line() { X1 = -15, X2 = 0, Y1 = 60, Y2 = 60, Stroke = Brushes.Black, StrokeThickness = 2 });
                    break;
                case 3:
                    InputLines.Add(new Line() { X1 = -15, X2 = 0, Y1 = 15, Y2 = 15, Stroke = Brushes.Black, StrokeThickness = 2 });
                    InputLines.Add(new Line() { X1 = -15, X2 = 0, Y1 = 40, Y2 = 40, Stroke = Brushes.Black, StrokeThickness = 2 });
                    InputLines.Add(new Line() { X1 = -15, X2 = 0, Y1 = 65, Y2 = 65, Stroke = Brushes.Black, StrokeThickness = 2 });
                    break;

            }
            CanvasControl.Children.Add(OutputLine);
            foreach (var line in InputLines)
                CanvasControl.Children.Add(line);


        }
    }
}
