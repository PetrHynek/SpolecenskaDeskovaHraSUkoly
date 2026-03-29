using Microsoft.Win32;
using SpolecenskaDeskovaHraSUkoly;
using SpolecenskaDeskovaHraSUkoly.Models;
using SpolecenskaDeskovaHraSUkoly.Services;
using SpolecenskaDeskovaHraSUkoly.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
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
    public partial class MainMenuPage : Page
    {
        private MainWindow _mainWindow;
        private TaskService _taskService = new TaskService();
        public MainMenuPage(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
        }

        private void StartGameBtn_Click(object sender, RoutedEventArgs e)
        {
            List<TaskItem> tasks = _taskService.LoadTasks();

            if (tasks.Count == 0)
            {
                MessageBox.Show("V aplikaci nejsou žádné úkoly (soubor chybí nebo je prázdný).\n\n" +
                    "Hru nelze spustit. Přejděte na stránku Databáze úkolů a nějaké vytvořte.", "Chybějící data", MessageBoxButton.OK, MessageBoxImage.Error);

                return;
            }
            if (tasks.Count < 5)
            {
                MessageBox.Show($"Máte {GetCreateWord(tasks.Count)} pouze {tasks.Count} {GetTaskWord(tasks.Count)}, což je pro hru příliš málo.\n\n" +
                    "Prosím, přidejte na stránce Databáze úkolů další úkoly (alespoň do počtu 5).", "Málo úkolů", MessageBoxButton.OK, MessageBoxImage.Warning);

                return;
            }

            _mainWindow.MainFrame.Navigate(new GameSettingsPage(_mainWindow));
        }

        private string GetTaskWord(int count)
        {
            if (count == 1)
            {
                return "úkol";
            }
            if (count >= 2 && count <= 4)
            {
                return "úkoly";
            }
            return "úkolů";
        }

        private string GetCreateWord(int count)
        {
            if (count == 1)
            {
                return "vytvořen";
            }
            if (count >= 2 && count <= 4)
            {
                return "vytvořeny";
            }
            return "vytvořeno";
        }

        private void OpenGameBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "JSON soubor (*.json)|*.json",
                Title = "Vyberte soubor s uloženou hrou"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string jsonString = File.ReadAllText(openFileDialog.FileName);

                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                    GameState loadedState = JsonSerializer.Deserialize<GameState>(jsonString, options);

                    if (loadedState != null)
                    {
                        _mainWindow.MainFrame.Navigate(new GamePage(_mainWindow, loadedState));
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Nepodařilo se načíst hru: {ex.Message}", "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DatabaseBtn_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.MainFrame.Navigate(new TaskDatabasePage(_mainWindow));
        }

        private void RulesBtn_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.MainFrame.Navigate(new RulesPage(_mainWindow));
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}