﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private const int MAX_CELL_SIZE = 40;
        private List<Element> elements;
        private Knapsack knapsack = null;
        private Process algorithProcess = null;
        private bool KilledLastProcess = false;
        private Stopwatch sw = new Stopwatch();

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
            if (btnLoad.IsEnabled && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length == 1)
                    LoadKnapsackCaseFromFile(files[0]);
            }
        }

        private void LvElements_DragEnter(object sender, DragEventArgs e)
        {
            if (btnLoad.IsEnabled && e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Move;
            else
                e.Effects = DragDropEffects.None;
        }

        private void RefreshKnapsackGrid(bool afterSolove = false)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                List<int> UsedElements = new List<int>();
                if (knapsack == null)
                {
                    gridCells = null;
                    grid.RowDefinitions.Clear();
                    grid.ColumnDefinitions.Clear();
                    tbNoKnapsack.Visibility = Visibility.Visible;
                    return;
                }
                //wyznaczenie wysokości paska zadań
                double psh = SystemParameters.PrimaryScreenHeight;
                double psbh = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
                double ratio = psh / psbh;
                int taskBarHeight = (int)(psbh - System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height);
                taskBarHeight = (int)(taskBarHeight * ratio);

                boardWidth = knapsack.Width;
                boardHeight = knapsack.Height;
                gridCells = new Border[knapsack.Width, knapsack.Height];
                
                cellSize = Math.Min((int)Math.Min((this.Width - rightPanel.Width) / boardWidth, (this.Height - taskBarHeight) / boardHeight), MAX_CELL_SIZE);

                double borderThickness =  1;

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
                        double bottom = borderThickness;
                        double left = borderThickness;
                        double right = borderThickness;
                        double top = borderThickness;
                        if (i != 0)
                        {
                            left = 0;
                        }
                        if (j != 0)
                        {
                            top = 0;
                        }
                        Border border = new Border() { BorderBrush = Brushes.Black, BorderThickness = new Thickness(left,top,right,bottom) };
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
                            if (!UsedElements.Exists(el => el == idx))
                            {
                                elements[i].Color = Element.GetColorForId(elements[i].Id);
                                gridCells[i, j].Background = elements.ElementAt(idx).Color;
                                UsedElements.Add(idx);
                            }
                        }
                    }
                }
                tbNoKnapsack.Visibility = Visibility.Hidden;
                SizeToContent = SizeToContent.WidthAndHeight;
                if (afterSolove)
                {
                    for (int i = 0; i < elements.Count; i++)
                    {
                        if (!UsedElements.Exists(el => el == i))
                        {
                            elements[i].Color = new SolidColorBrush(Color.FromArgb(200, 200, 200, 200));
                        }
                    }
                }

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
                    var t = line.Split(' ');
                    var t2 = t.ToList();
                    t2.RemoveAll(str => string.IsNullOrEmpty(str));
                    if (t2.Count != 2)
                        throw new InvalidDataException("Bad first line format");

                    Knapsack tKnapsack = new Knapsack(int.Parse(t2[0]), int.Parse(t2[1]));
                    line = readtext.ReadLine(); //liczba elementów


                    t = line.Split(' ');
                    t2 = t.ToList();
                    t2.RemoveAll(str => string.IsNullOrEmpty(str));

                    if (t2.Count != 1)
                        throw new InvalidDataException("Bad Second line format");
                    int n = int.Parse(t2[0]);

                    int k = 0;
                    line = readtext.ReadLine(); //szerokość, wysokość i wartość danego elementu rozdzielone spacjami
                    while (!string.IsNullOrEmpty(line))
                    {
                        if (k >= n)
                            throw new InvalidDataException("Too many elements.");
 
                        t = line.Split(' ');
                        t2 = t.ToList();
                        t2.RemoveAll(str => string.IsNullOrEmpty(str));
                        if (t2.Count != 3)
                            throw new InvalidOperationException($"Bad {k + 1} elements line format");

                        int width = int.Parse(t2[0]);
                        int height = int.Parse(t2[1]);
                        if (width < height)
                        {
                            int tmp = width;
                            width = height;
                            height = tmp;
                        }

                        Element elem = new Element(width, height, int.Parse(t2[2]), ++k);
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
                catch (Exception e)
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
            if (knapsack == null || algorithProcess != null)
            {
                return;
            }
            btnLoad.IsEnabled = false;
            btnSolve.IsEnabled = false;
            lvElements.AllowDrop = false;
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
            algorithProcess = new Process();
            KilledLastProcess = false;
            algorithProcess.StartInfo.FileName = System.IO.Path.Combine(Environment.CurrentDirectory, ALGORITHM_EXE_NAME); //ścieżka do pliku .exe
            algorithProcess.StartInfo.Arguments = ALGORITHM_INPUT_FILE_NAME + " " + ALGORITHM_OUTPUT_FILE_NAME; //argumenty: ścieżka pliku wejściowego i wyjściowego
            algorithProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden; //ukrycie okna procesu
            algorithProcess.EnableRaisingEvents = true;
            algorithProcess.Exited += AlgorithmFinished;
            sw = new Stopwatch();
            sw.Start();
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
            sw.Stop();
            if (!KilledLastProcess)
            {
                knapsack.LoadFromFile(ALGORITHM_OUTPUT_FILE_NAME);
                //usunięcie plików tymczasowych
                //if (File.Exists(ALGORITHM_INPUT_FILE_NAME))
                //    File.Delete(ALGORITHM_INPUT_FILE_NAME);
                //if (File.Exists(ALGORITHM_OUTPUT_FILE_NAME))
                //    File.Delete(ALGORITHM_OUTPUT_FILE_NAME);
                RefreshKnapsackGrid(true);
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    tbTotalValue.Text = "Total value: " + knapsack.TotalValue;
                    tbTime.Text = "Time: " + (long)sw.Elapsed.TotalMilliseconds + " ms";
                    btnLoad.IsEnabled = true;
                    btnSolve.IsEnabled = true;
                    lvElements.AllowDrop = true;
                    lvElements.Items.Refresh();
                }));
                algorithProcess = null;
            }
        }

        private void DataWindow_Closing(object sender, CancelEventArgs e)
        {
            if (algorithProcess != null)
            {
                algorithProcess.Kill();
                KilledLastProcess = true;
            }
        }
        
    }
}
