using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfTest
{
    /// <summary>
    /// Interaction logic for ComboBoxForm.xaml
    /// </summary>
    public partial class ComboBoxForm : Window
    {
        static string selectedOption = "";
        public ComboBoxForm()
        {
            InitializeComponent();
        }
        public ComboBoxForm(List<string> options)
        {
            InitializeComponent();
            cmbOptions.ItemsSource = options;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // گرفتن مقدار انتخاب شده از ComboBox
            selectedOption = cmbOptions.SelectedItem as string;
            // انجام عملیات مورد نظر با توجه به مقدار انتخاب شده
            // ...
            DialogResult = true;
            Close();
        }
    }

        

}
