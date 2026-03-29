using SpolecenskaDeskovaHraSUkoly.Views;
using SpolecenskaDeskovaHraSUkoly;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SpolecenskaDeskovaHraSUkoly.Views
{
    public partial class RulesPage : Page
    {
        private MainWindow _mainWindow;
        public RulesPage(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.MainFrame.Navigate(new MainMenuPage(_mainWindow));
        }
    }
}