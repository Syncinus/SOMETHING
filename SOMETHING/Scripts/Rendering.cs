using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Numerics;

namespace Something
{
    public class Pixel
    {
        public int2 position;
        public Color color;
        
        public Pixel(Color _color, int2 _position)
        {
            color = _color;
            position = _position;
        }
    }

    public class PixelMap
    {
        public string name;
        public int2 center;
        public KeyValuePair<int2, int2> extents;
        public List<Pixel> pixels;

        public PixelMap(string _name, int2 _center, List<Pixel> _pixels)
        {
            name = _name;
            center = _center;
            pixels = _pixels;
            CalculateExtents();
        }

        public List<Pixel> RelativeCalculator()
        {
            List<Pixel> relative = new List<Pixel>();
            foreach (Pixel pixel in pixels)
            {
                int relativex = Difference(center.x, pixel.position.x);
                int relativey = Difference(center.y, pixel.position.y);
                relative.Add(new Pixel(pixel.color, new int2(relativex, relativey)));
            }

            return relative;
        }

        public void CalculateExtents()
        {
            int2 xextents;
            int2 yextents;

            List<Pixel> relative = RelativeCalculator();
            int lowestx = relative.Min(p => p.position.x);
            int highestx = relative.Max(p => p.position.x);
            int lowesty = relative.Min(p => p.position.y);
            int highesty = relative.Max(p => p.position.y);
            xextents.x = lowestx; xextents.y = highestx;
            yextents.x = lowesty; yextents.y = highesty;
            extents = new KeyValuePair<int2, int2>(xextents, yextents);
        }

        public void SetCenter(int2 _center)
        {
            center = _center;
        }

        public int Difference(int a, int b)
        {
            int total = Math.Abs(a - b);
            if (b < a)
            {
                total = -total;
            }
            return total;
        }

        public float Difference(float a, float b)
        {
            float total = Math.Abs(a - b);
            if (b < a)
            {
                total = -total;
            }
            return total;
        }

        public void Render(Point position, Graphics g, int size, int roommultiplier = 20)
        {
            List<Pixel> relative = RelativeCalculator();

            foreach (Pixel p in relative)
            {
                PointF point = new PointF(position.X + (p.position.x * size) + Difference(size, roommultiplier) / 2f, (position.Y + (p.position.y * size) + Difference(size, roommultiplier) / 2f) + -extents.Value.x * roommultiplier);
                RectangleF rect = new RectangleF(point, new SizeF(size, size));
                g.FillRectangle(new SolidBrush(p.color), rect);
            }
        }
    }

    public class Sprite
    {
        public List<List<PixelMap>> pixelmaps = new List<List<PixelMap>>();        
        public Graphics graphics;
        public Point position;
        public int direction;
        public int size;

        public Sprite(Point _position, Graphics _graphics, int _direction, int _size, params List<PixelMap>[] _pixelmaps)
        {
            position = _position;
            graphics = _graphics;
            direction = _direction;
            size = _size;
            pixelmaps = _pixelmaps.ToList();
        }

        public void Move(int2 move, int multiplier = 25)
        {
            position.X = move.x * multiplier;
            position.Y = move.y * multiplier;
        }

        public void Rotate(int newdirection)
        {
            direction = newdirection;
        }

        public void Scale(int newsize)
        {
            size = newsize;
        }

        public void Render(Graphics g, string name = "default")
        {
            PixelMap map = pixelmaps[direction].Where(i => i.name == name).FirstOrDefault();
            if (map != null)
            {
                map.Render(position, g, size);
            } else
            {
                throw new NullReferenceException($"PixelMap {name} is null!");
            }
            //Console.WriteLine(map);
            //pixelmaps[0][0].Render(new Point(0, 0), g, 10);
        }
    }
}
