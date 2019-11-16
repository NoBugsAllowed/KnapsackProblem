using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnapsackGUI.Models
{
    class Knapsack
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int[,] ElementsId { get; private set; }
        public int TotalValue { get; private set; }

        public double Time { get; private set; }

        public Knapsack(int width, int height, double Time = 0.0)
        {
            Width = width;
            Height = height;
            this.Time = Time;
            ElementsId = new int[width, height];
        }

        public Knapsack(int[,] ids)
        {
            Width = ids.GetLength(0);
            Height = ids.GetLength(1);
            ElementsId = new int[Width, Height];
            for(int i=0;i<Width;i++)
            {
                for(int j=0;j<Height;j++)
                {
                    ElementsId[i, j] = ids[i, j];
                }
            }
        }

        public bool LoadFromFile(string path)
        {
            using (StreamReader readtext = new StreamReader(path))
            {
                try
                {
                    string time = readtext.ReadLine(); //czas trwania algorytmu
                    double Time = -1;
                    double.TryParse(time, out Time);
                    this.Time = Time;
                    string line = readtext.ReadLine();
                    TotalValue = int.Parse(line);
                    int h = 0;
                    line = readtext.ReadLine();
                    while (!string.IsNullOrEmpty(line))
                    {
                        var t = line.Split(' ');
                        if (t.Length != Width)
                            throw new InvalidDataException();
                        for (int i = 0; i < Width; i++)
                            ElementsId[i, h] = int.Parse(t[i]);
                        h++;
                        line = readtext.ReadLine();
                    }
                    if (h != Height)
                        throw new InvalidDataException();
                }
                catch (Exception ex)
                {
                    for (int i = 0; i < Width; i++)
                    {
                        for (int j = 0; j < Height; j++)
                        {
                            ElementsId[i, j] = 0;
                        }
                    }
                    return false;
                }
            }
            return true;
        }
    }
}
