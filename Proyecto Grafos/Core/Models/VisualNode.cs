using System.Drawing;

namespace Proyecto_Grafos.Core.Models
{
    public class VisualNode
    {
        public string Name { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public Color Color { get; set; }
        public Size Size { get; set; }
        public Image CachedImage { get; set; } 

        public VisualNode(string name, int x, int y)
        {
            Name = name;
            X = x;
            Y = y;
            Color = Color.LightBlue;
            Size = new Size(120, 140); 
        }

        public Rectangle GetBounds()
        {
            return new Rectangle(X, Y, Size.Width, Size.Height);
        }

        public Point GetCenter()
        {
            return new Point(X + Size.Width / 2, Y + Size.Height / 2);
        }

        public void DisposeImage()
        {
            CachedImage?.Dispose();
            CachedImage = null;
        }
    }
}