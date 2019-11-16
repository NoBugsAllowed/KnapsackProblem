using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using System.Xml;
using System.Xml.Serialization;
using KnapsackGUI.Models;

namespace KnapsackGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string ALGORITHM_INPUT_FILE_NAME = ".knapsack_input";
        private const string ALGORITHM_OUTPUT_FILE_NAME = ".knapsack_output";
        private const string ALGORITHM_EXE_NAME = "KnapsackAlgorithm.exe";

        private Grid grid;
        private Border[,] gridCells;
        private int boardWidth;
        private int boardHeight;
        private int cellSize;
        private int pieceMargin;
        private const int MAX_CELL_SIZE = 50;
        private List<Element> elements;
        private Knapsack knapsack = null;

        public MainWindow()
        {
            InitializeComponent();
            grid = (Grid)FindName("gridBoard");
     
            btnLoad.Click += BtnLoad_Click;
            btnSolve.Click += BtnSolve_Click;
            lvElements.DragEnter += LvElements_DragEnter;
            lvElements.Drop += LvElements_Drop;

            RefreshKnapsackGrid();
        }

        private void LvElements_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length == 1)
                    LoadKnapsackCaseFromFile(files[0]);
            }
        }

        private void LvElements_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Move;
            else
                e.Effects = DragDropEffects.None;
        }

        private void RefreshKnapsackGrid()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (knapsack == null)
                {
                    gridCells = null;
                    grid.RowDefinitions.Clear();
                    grid.ColumnDefinitions.Clear();
                    tbNoKnapsack.Visibility = Visibility.Visible;
                    return;
                }
                boardWidth = knapsack.Width;
                boardHeight = knapsack.Height;
                gridCells = new Border[knapsack.Width, knapsack.Height];
                cellSize = Math.Min((int)Math.Min((SystemParameters.WorkArea.Width - rightPanel.Width) / boardWidth, SystemParameters.WorkArea.Height / boardHeight), MAX_CELL_SIZE);
                pieceMargin = cellSize / 5;
                int borderThickness = cellSize < 10 ? 1 : 2;

                grid.RowDefinitions.Clear();
                grid.ColumnDefinitions.Clear();

                for (int j = 0; j < boardHeight; j++)
                {
                    grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(cellSize) });
                }
                for (int i = 0; i < boardWidth; i++)
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(cellSize) });
                    for (int j = 0; j < boardHeight; j++)
                    {
                        Border border = new Border() { BorderBrush = Brushes.Black, BorderThickness = new Thickness(borderThickness) };
                        Grid.SetColumn(border, i);
                        Grid.SetRow(border, j);
                        grid.Children.Add(border);
                        gridCells[i, j] = border;
                        int idx = knapsack.ElementsId[i, j] - 1;
                        if (idx < 0 || idx >= elements.Count)
                        {
                            gridCells[i, j].Background = Brushes.White;
                        }
                        else
                        {
                            gridCells[i, j].Background = elements.ElementAt(idx).Color;
                        }
                    }
                }
                tbNoKnapsack.Visibility = Visibility.Hidden;
                SizeToContent = SizeToContent.WidthAndHeight;
            }));
        }

        public void LoadKnapsackCaseFromFile(string path)
        {
            using (StreamReader readtext = new StreamReader(path))
            {
                try
                {
                    List<Element> tElements = new List<Element>();
                    string line = readtext.ReadLine(); //wymiary plecaka rozdzielone spacją
                    var p = line.Split(' ');
                    if (p.Length != 2)
                        throw new InvalidDataException("First line should contain knapsack's width and height separated by space.");
                    Knapsack tKnapsack = new Knapsack(int.Parse(p[0]), int.Parse(p[1]));
                    line = readtext.ReadLine(); //liczba elementów
                    if (line.Contains(" "))
                        throw new InvalidDataException("Second line should contain only number of elements.");
                    int n = int.Parse(line);
                    int k = 0;
                    line = readtext.ReadLine(); //szerokość, wysokość i wartość danego elementu rozdzielone spacjami
                    while (!string.IsNullOrEmpty(line))
                    {
                        if (k >= n)
                            throw new InvalidDataException("Too many elements.");
                        var t = line.Split(' ');
                        if (t.Length != 3)
                            throw new InvalidDataException("Line should contain element's width, height and value separated by spaces.");
                        Element elem = new Element(int.Parse(t[0]), int.Parse(t[1]), int.Parse(t[2]), ++k);
                        tElements.Add(elem);
                        line = readtext.ReadLine();
                    }
                    if (k < n)
                        throw new InvalidDataException("Too little elements.");
                    elements = tElements;
                    knapsack = tKnapsack;
                }
                catch (InvalidDataException ex)
                {
                    MessageBox.Show("Bad data in file! " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception)
                {
                    MessageBox.Show("Error occured while reading the file!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            RefreshKnapsackGrid();
            lvElements.ItemsSource = elements;
            lvElements.Items.Refresh();
        }

        private void BtnSolve_Click(object sender, RoutedEventArgs e)
        {
            if (knapsack == null)
            {
                return;
            }
            btnLoad.IsEnabled = false;
            //utworzenie tymczasowego pliku wejściowego do algorytmu
            using (StreamWriter writetext = new StreamWriter(ALGORITHM_INPUT_FILE_NAME))
            {
                writetext.WriteLine($"{knapsack.Width} {knapsack.Height}");
                writetext.WriteLine($"{elements.Count}");
                foreach (Element el in elements)
                {
                    writetext.WriteLine($"{el.Width} {el.Height} {el.Value}");
                }
            }
            //uruchomienie procesu algorytmu
            Process algorithProcess = new Process();
            algorithProcess.StartInfo.FileName = System.IO.Path.Combine(Environment.CurrentDirectory, ALGORITHM_EXE_NAME); //ścieżka do pliku .exe
            algorithProcess.StartInfo.Arguments = ALGORITHM_INPUT_FILE_NAME + " " + ALGORITHM_OUTPUT_FILE_NAME; //argumenty: ścieżka pliku wejściowego i wyjściowego
            algorithProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden; //ukrycie okna procesu
            algorithProcess.EnableRaisingEvents = true;
            algorithProcess.Exited += AlgorithmFinished;
            algorithProcess.Start();
        }

        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog();
            var result = openFileDlg.ShowDialog();
            if (result == true)
                LoadKnapsackCaseFromFile(openFileDlg.FileName);
        }

        private void AlgorithmFinished(object sender, EventArgs e)
        {
            knapsack.LoadFromFile(ALGORITHM_OUTPUT_FILE_NAME);
            //usunięcie plików tymczasowych
            if (File.Exists(ALGORITHM_INPUT_FILE_NAME))
                File.Delete(ALGORITHM_INPUT_FILE_NAME);
            if (File.Exists(ALGORITHM_OUTPUT_FILE_NAME))
                File.Delete(ALGORITHM_OUTPUT_FILE_NAME);
            RefreshKnapsackGrid();
            Dispatcher.BeginInvoke(new Action(() =>
            {
                tbTotalValue.Text = "Total value: " + knapsack.TotalValue;
                btnLoad.IsEnabled = true;
            }));
        }
    }
}
