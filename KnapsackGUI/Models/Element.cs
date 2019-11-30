using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace KnapsackGUI.Models
{
    public class Element
    {
        public int Height { get; }
        public int Width { get; }
        public double Value { get; }
        public int Id { get; }
        public string Description { get => $"{Width} x {Height} - {Value}"; }
        public double DrawWidth
        {
            get
            {
                return Width * drawMultiplier;
            }
        }
        public double DrawHeight
        {
            get
            {
                return Height * drawMultiplier;
            }
        }
        private int drawMultiplier = 20;
        public SolidColorBrush Color { get; set; }
        private static Random rnd = new Random(2137);

        public Element() { }

        public Element(int width, int height, double value, int id)
        {
            Height = height;
            Width = width;
            Value = value;
            Id = id;
            Color = GetColorForId(Id);
            Color.Opacity = 1.0;
        }

        public static SolidColorBrush GetColorForId(int id)
        {
            switch(id)
            {
                case 1: return new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 204, 153, 0));
                case 2: return new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 102, 153));
                case 3: return new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 204, 0, 204));
                case 4: return new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 153, 51));
                case 5: return new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 51, 51, 255));
                case 6: return new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 204, 0, 0));
                case 7: return new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 255, 0));
                case 8: return new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 51, 204, 204));
                case 9: return new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 0, 255));
                case 10: return new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 102, 255, 102));
                case 11: return new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 102, 153, 255));
                case 12: return new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 102, 102));
                case 13: return new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 255, 153));
                case 14: return new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 102, 255, 255));
                case 15: return new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 153, 255));
                case 16: return new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 128, 128, 128));
                default: return new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, (byte)(rnd.Next() % 256), (byte)(rnd.Next() % 256), (byte)(rnd.Next() % 256)));
            }
        }
    }
}
