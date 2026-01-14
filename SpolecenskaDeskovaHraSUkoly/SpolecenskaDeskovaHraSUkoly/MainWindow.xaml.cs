using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using SpolecenskaDeskovaHraSUkoly.Models;
using SpolecenskaDeskovaHraSUkoly.Services;
using SpolecenskaDeskovaHraSUkoly.Views;

namespace SpolecenskaDeskovaHraSUkoly
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.Width = SystemParameters.PrimaryScreenWidth;
            this.Height = SystemParameters.PrimaryScreenHeight;
            this.MinWidth = SystemParameters.PrimaryScreenWidth - 1;
            this.MinHeight = SystemParameters.PrimaryScreenHeight - 1;


            MainFrame.Navigate(new MainMenuPage(this));
        }
    }
}
