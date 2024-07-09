using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfTest
{
    internal class Gate
    {
        public TextBlock Control { get; set; }
        public int ID { get; set; }
        public string Type { get; set; }
        //gate constructor:
        public Gate(string type)
        {
            Control = new TextBlock();
            Type = type;
            Control.Text = Type;
            Control.FontSize = 16;
            Control.Width = 40;
            Control.Height = 40;
            Control.Background = Brushes.Gray;
        }
    }
}
