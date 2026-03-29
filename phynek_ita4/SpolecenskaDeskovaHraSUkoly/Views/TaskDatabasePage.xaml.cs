using SpolecenskaDeskovaHraSUkoly.Models;
using SpolecenskaDeskovaHraSUkoly.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public partial class TaskDatabasePage : Page
    {
        private MainWindow _mainWindow;

        private TaskService _taskService;

        private List<TaskItem> _allTasks;
        public ObservableCollection<TaskItem> _filteredTasks { get; set; }

        public TaskDatabasePage(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;

            _taskService = new TaskService();

            _allTasks = _taskService.LoadTasks();

            _filteredTasks = new ObservableCollection<TaskItem>();
            foreach (TaskItem item in _allTasks)
            {
                _filteredTasks.Add(item);
            }
            TasksDataGrid.ItemsSource = _filteredTasks;

            LoadFilterOptions();

            TasksDataGrid_SelectionChanged(null, null);
        }

        private void LoadFilterOptions()
        {
            FilterComboBox.Items.Clear();

            FilterComboBox.Items.Add("Vše");

            foreach (TaskItem item in _allTasks)
            {
                bool exists = false;

                foreach (object obj in FilterComboBox.Items)
                {
                    if (obj.ToString() == item.Type)
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    FilterComboBox.Items.Add(item.Type);
                }
            }
            FilterComboBox.SelectedIndex = 0;
        }

        private void FilterChanged(object sender, EventArgs e)
        {
            TasksDataGrid.SelectedIndex = -1;
            ApplyFilter();
        }

        private void ClearFilters_Click(object sender, RoutedEventArgs e)
        {
            FilterComboBox.SelectedIndex = 0;
            SearchTextBox.Text = string.Empty;
        }

        private void ApplyFilter()
        {
            string selectedType = "Vše";

            if (FilterComboBox.SelectedItem != null)
            {
                selectedType = FilterComboBox.SelectedItem.ToString();
            }

            string searchText = SearchTextBox.Text;

            searchText = searchText.ToLower();

            _filteredTasks.Clear();

            foreach (TaskItem item in _allTasks)
            {
                bool typeMatches = false;
                bool textMatches = false;

                if (selectedType == "Vše")
                {
                    typeMatches = true;
                }
                else
                {
                    if (item.Type == selectedType)
                    {
                        typeMatches = true;
                    }
                }

                if (searchText == "")
                {
                    textMatches = true;
                }
                else
                {
                    if (item.Text != null)
                    {
                        string lowerItemText = item.Text.ToLower();

                        if (lowerItemText.Contains(searchText))
                        {
                            textMatches = true;
                        }
                    }
                }

                if (typeMatches && textMatches)
                {
                    _filteredTasks.Add(item);
                }
            }
        }

        private void TasksDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selectedCount = TasksDataGrid.SelectedItems.Count;

            AddTaskBtn.IsEnabled = true;

            if (selectedCount == 1)
            {
                EditTaskBtn.IsEnabled = true;
            }
            else
            {
                EditTaskBtn.IsEnabled = false;
            }

            if (selectedCount >= 1)
            {
                DeleteTaskBtn.IsEnabled = true;
            }
            else
            {
                DeleteTaskBtn.IsEnabled = false;
            }
        }

        private void AddTaskBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowOverlayForAdd();
        }

        private void EditTaskBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowOverlayForEdit((TaskItem)TasksDataGrid.SelectedItem);
        }

        private void DeleteTaskBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowOverlayForDelete(TasksDataGrid.SelectedItems.Cast<TaskItem>().ToList());
        }

        private void ShowOverlayForDelete(List<TaskItem> selectedItems)
        {
            OverlayContent.Children.Clear();
            OverlayBorder.Width = 800;

            Grid overlayWindowGrid = new Grid { Margin = new Thickness(60, 20, 60, 20) };

            overlayWindowGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            overlayWindowGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            overlayWindowGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            TextBlock title = new TextBlock { Text = "Opravdu chcete smazat vybrané úkoly?", FontSize = 24, FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 0, 0, 20) };
            Grid.SetRow(title, 0);
            overlayWindowGrid.Children.Add(title);

            Border dgBorder = new Border { Style = (Style)this.FindResource("BorderStyle"), Padding = new Thickness(10) };

            DataGrid dg = new DataGrid { Style = (Style)this.Resources["DataGridStyle"], MaxHeight = 350 };

            dg.ItemsSource = selectedItems;

            DataGridTextColumn colId = new DataGridTextColumn { Header = "ID", Binding = new Binding("Id"), Width = 80 };
            DataGridTextColumn colType = new DataGridTextColumn { Header = "Typ úkolu", Binding = new Binding("Type"), Width = 190 };
            DataGridTextColumn colText = new DataGridTextColumn { Header = "Popis", Binding = new Binding("Text"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) };

            dg.Columns.Add(colId);
            dg.Columns.Add(colType);
            dg.Columns.Add(colText);

            dgBorder.Child = dg;

            Grid.SetRow(dgBorder, 1);
            overlayWindowGrid.Children.Add(dgBorder);

            StackPanel sp = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Orientation = Orientation.Horizontal, Margin = new Thickness(0, 40, 0, 0) };

            Border cancelBtnBorder = CreateButtonBorder();
            cancelBtnBorder.Margin = new Thickness(0, 0, 15, 0);
            Button cancelBtn = new Button { Content = "Zrušit", Style = (Style)Application.Current.Resources["ButtonStyle"] };
            cancelBtn.Click += CancelOverlay_Click;
            cancelBtnBorder.Child = cancelBtn;

            Border deleteBtnBorder = CreateButtonBorder();
            deleteBtnBorder.Margin = new Thickness(15, 0, 0, 0);
            Button deleteBtn = new Button { Content = "Smazat", Style = (Style)Application.Current.Resources["ButtonStyle"] };
            deleteBtn.Click += ConfirmDelete_Click;
            deleteBtnBorder.Child = deleteBtn;

            sp.Children.Add(cancelBtnBorder);
            sp.Children.Add(deleteBtnBorder);

            Grid.SetRow(sp, 2);
            overlayWindowGrid.Children.Add(sp);

            OverlayContent.Children.Add(overlayWindowGrid);

            Overlay.Visibility = Visibility.Visible;
        }

        private void ConfirmDelete_Click(object sender, RoutedEventArgs e)
        {
            var toDelete = new List<TaskItem>();
            foreach (TaskItem item in TasksDataGrid.SelectedItems)
            {
                toDelete.Add(item);
            }

            foreach (TaskItem item in toDelete)
            {
                _allTasks.Remove(item);
            }

            _taskService.SaveTasks(_allTasks);
            ReloadData();

            Overlay.Visibility = Visibility.Collapsed;
        }

        private TaskItem _editOriginal;
        private TaskItem _editCopy;
        private void ShowOverlayForEdit(TaskItem selected)
        {
            OverlayContent.Children.Clear();
            OverlayBorder.Width = 800;

            Grid overlayWindowGrid = new Grid { Margin = new Thickness(60, 20, 60, 20) };
            overlayWindowGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            overlayWindowGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            overlayWindowGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            TextBlock title = new TextBlock { Text = "Upravit úkol", FontSize = 24, FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 0, 0, 20) };
            Grid.SetRow(title, 0);
            overlayWindowGrid.Children.Add(title);

            Border dgBorder = new Border { Style = (Style)this.FindResource("BorderStyle"), Padding = new Thickness(10) };

            DataGrid dg = new DataGrid { Style = (Style)this.Resources["DataGridStyle"], IsReadOnly = false, SelectionUnit = DataGridSelectionUnit.Cell, SelectionMode = DataGridSelectionMode.Single };


            _editOriginal = selected;
            _editCopy = new TaskItem { Id = selected.Id, Type = selected.Type, Text = selected.Text };
            ObservableCollection<TaskItem> editCollection = new ObservableCollection<TaskItem> { _editCopy };
            dg.ItemsSource = editCollection;

            DataGridTextColumn colId = new DataGridTextColumn { Header = "ID", Binding = new Binding("Id"), IsReadOnly = true, Width = 80 };
            DataGridTextColumn colType = new DataGridTextColumn { Header = "Typ úkolu", Binding = new Binding("Type"), IsReadOnly = false, Width = 190 };
            DataGridTextColumn colText = new DataGridTextColumn { Header = "Popis", Binding = new Binding("Text"), IsReadOnly = false, Width = new DataGridLength(1, DataGridLengthUnitType.Star) };

            dg.Columns.Add(colId);
            dg.Columns.Add(colType);
            dg.Columns.Add(colText);

            dgBorder.Child = dg;

            Grid.SetRow(dgBorder, 1);
            overlayWindowGrid.Children.Add(dgBorder);

            StackPanel sp = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Orientation = Orientation.Horizontal, Margin = new Thickness(0, 40, 0, 0) };

            Border cancelBtnBorder = CreateButtonBorder();
            cancelBtnBorder.Margin = new Thickness(0, 0, 15, 0);
            Button cancelBtn = new Button { Content = "Zrušit", Style = (Style)Application.Current.Resources["ButtonStyle"] };
            cancelBtn.Click += CancelOverlay_Click;
            cancelBtnBorder.Child = cancelBtn;

            Border editBtnBorder = CreateButtonBorder();
            editBtnBorder.Margin = new Thickness(15, 0, 0, 0);
            Button editBtn = new Button { Content = "Upravit", Style = (Style)Application.Current.Resources["ButtonStyle"] };
            editBtn.Click += ConfirmEdit_Click;
            editBtnBorder.Child = editBtn;

            sp.Children.Add(cancelBtnBorder);
            sp.Children.Add(editBtnBorder);

            Grid.SetRow(sp, 2);
            overlayWindowGrid.Children.Add(sp);

            OverlayContent.Children.Add(overlayWindowGrid);

            Overlay.Visibility = Visibility.Visible;
        }

        private void ConfirmEdit_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_editCopy.Type) || string.IsNullOrWhiteSpace(_editCopy.Text))
            {
                MessageBox.Show("Prosím, vyplňte oba údaje: Typ a Popis úkolu.", "Chyba", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            foreach (TaskItem existing in _allTasks)
            {
                if (string.Equals(existing.Type, _editCopy.Type, StringComparison.OrdinalIgnoreCase) && string.Equals(existing.Text, _editCopy.Text, StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Tento úkol již v databázi existuje!", "Chyba", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            foreach (TaskItem item in _allTasks)
            {
                if (item.Id == _editOriginal.Id)
                {
                    item.Type = _editCopy.Type;
                    item.Text = _editCopy.Text;
                    break;
                }
            }
            _taskService.SaveTasks(_allTasks);
            ReloadData();

            Overlay.Visibility = Visibility.Collapsed;
        }

        private TaskItem _newItem;
        private void ShowOverlayForAdd()
        {
            OverlayContent.Children.Clear();
            OverlayBorder.Width = 800;

            Grid overlayWindowGrid = new Grid { Margin = new Thickness(60, 20, 60, 20) };
            overlayWindowGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            overlayWindowGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            overlayWindowGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            TextBlock title = new TextBlock { Text = "Přidat úkol", FontSize = 24, FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 0, 0, 20) };
            Grid.SetRow(title, 0);
            overlayWindowGrid.Children.Add(title);


            Border dgBorder = new Border { Style = (Style)this.FindResource("BorderStyle"), Padding = new Thickness(10) };

            DataGrid dg = new DataGrid { Style = (Style)this.Resources["DataGridStyle"], IsReadOnly = false, SelectionUnit = DataGridSelectionUnit.Cell, SelectionMode = DataGridSelectionMode.Single };

            int maxId = 0;
            foreach (var t in _allTasks)
            {
                if (t.Id > maxId)
                {
                    maxId = t.Id;
                }
            }
            int nextId = maxId + 1;
            _newItem = new TaskItem { Id = nextId, Type = "", Text = "" };
            ObservableCollection<TaskItem> addCollection = new ObservableCollection<TaskItem> { _newItem };
            dg.ItemsSource = addCollection;

            DataGridTextColumn colId = new DataGridTextColumn { Header = "ID", Binding = new Binding("Id"), IsReadOnly = true, Width = 80 };
            DataGridTextColumn colType = new DataGridTextColumn { Header = "Typ úkolu", Binding = new Binding("Type"), IsReadOnly = false, Width = 190 };
            DataGridTextColumn colText = new DataGridTextColumn { Header = "Popis", Binding = new Binding("Text"), IsReadOnly = false, Width = new DataGridLength(1, DataGridLengthUnitType.Star) };

            dg.Columns.Add(colId);
            dg.Columns.Add(colType);
            dg.Columns.Add(colText);

            dgBorder.Child = dg;

            Grid.SetRow(dgBorder, 1);
            overlayWindowGrid.Children.Add(dgBorder);

            StackPanel sp = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Orientation = Orientation.Horizontal, Margin = new Thickness(0, 40, 0, 0) };

            Border cancelBtnBorder = CreateButtonBorder();
            cancelBtnBorder.Margin = new Thickness(0, 0, 15, 0);
            Button cancelBtn = new Button { Content = "Zrušit", Style = (Style)Application.Current.Resources["ButtonStyle"] };
            cancelBtn.Click += CancelOverlay_Click;
            cancelBtnBorder.Child = cancelBtn;

            Border addBtnBorder = CreateButtonBorder();
            addBtnBorder.Margin = new Thickness(15, 0, 0, 0);
            Button addBtn = new Button { Content = "Přidat", Style = (Style)Application.Current.Resources["ButtonStyle"] };
            addBtn.Click += ConfirmAdd_Click;
            addBtnBorder.Child = addBtn;

            sp.Children.Add(cancelBtnBorder);
            sp.Children.Add(addBtnBorder);

            Grid.SetRow(sp, 2);
            overlayWindowGrid.Children.Add(sp);

            OverlayContent.Children.Add(overlayWindowGrid);

            Overlay.Visibility = Visibility.Visible;
        }

        private void ConfirmAdd_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_newItem.Type) || string.IsNullOrWhiteSpace(_newItem.Text))
            {
                MessageBox.Show("Prosím, vyplňte oba údaje: Typ a Popis úkolu.", "Chyba", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            foreach (TaskItem existing in _allTasks)
            {
                if (string.Equals(existing.Type, _newItem.Type, StringComparison.OrdinalIgnoreCase) && string.Equals(existing.Text, _newItem.Text, StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Tento úkol již v databázi existuje!", "Chyba", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            _allTasks.Add(_newItem);

            _taskService.SaveTasks(_allTasks);
            ReloadData();

            Overlay.Visibility = Visibility.Collapsed;
        }

        private Border CreateButtonBorder(double width = 180, double height = 60)
        {
            return new Border
            {
                Style = (Style)this.FindResource("BorderStyle"),
                Width = width,
                Height = height,
            };
        }

        private void ReloadData()
        {
            _allTasks = _taskService.LoadTasks();

            FilterComboBox.SelectedIndex = 0;
            SearchTextBox.Text = "";

            _filteredTasks.Clear();

            foreach (TaskItem item in _allTasks)
            {
                _filteredTasks.Add(item);
            }

            LoadFilterOptions();
            ApplyFilter();
        }

        private void CancelOverlay_Click(object sender, RoutedEventArgs e)
        {
            Overlay.Visibility = Visibility.Collapsed;
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.MainFrame.Navigate(new MainMenuPage(_mainWindow));
        }
    }
}