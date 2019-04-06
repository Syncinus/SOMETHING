using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using RoyT.AStar;
using System.Drawing;
using System.Xml.Linq;

namespace Something
{
    public static class GameVariables
    {
        public delegate void OnTurnChanged();
        public static event OnTurnChanged ChangeTurn;

        private static int _turns;
        public static int turns
        {
            get
            {
                return _turns;
            }
            set
            {
                _turns = value;
                ChangeTurn();
            }
        }

        public static World world;
        public static Main game;
        public static List<int2> occupiedpaths = new List<int2>();

        public static void MoveWorld(World _world)
        {
            world = _world;
            game.currentworld = world;
        }
    }
    public static class EXT
    {
        public static Random rng = new Random();
        //public static RoyT.AStar.Grid grid = new RoyT.AStar.Grid(10, 10);


        public static List<List<string>> Process(string datablock)
        {
            List<List<string>> processed = new List<List<string>>();
            Regex vbregex = new Regex(@"(?<=\().*?(?=\))");
            MatchCollection valueblocks = vbregex.Matches(datablock);

            foreach (Match valueblock in valueblocks)
            {
                string vbdata = valueblock.Value;
                string[] split = vbdata.Split(',');

                processed.Add(split.ToList());
            }

            return processed;
        }

        public static dynamic CreateInstance(string name, params object[] args)
        {
            Type t = Type.GetType(name);
            return Activator.CreateInstance(t, args);
        }

        public static bool InRange(int2 target, int2 user, int range, int? _ = null)
        {
            List<int2> positions = new List<int2>();
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < range; j++)
                {
                    int r = j + 1;
                    int2 getdir = GetDirection(target, i + 1, r);
                    if (i + 1 == 1 || i + 1 == 5)
                    {
                        for (int k = 0; k < range; k++)
                        {
                            positions.Add(getdir + new int2(k, 0));
                            positions.Add(getdir - new int2(k, 0));
                        }
                    }
                    else if (i + 1 == 3 || i + 1 == 7)
                    {
                        for (int k = 0; k < range; k++)
                        {
                            positions.Add(getdir + new int2(0, k));
                            positions.Add(getdir - new int2(0, k));
                        }
                    }
                    positions.Add(getdir);
                }
            }

            foreach (int2 position in positions)
            {
                if (user == position)
                {
                    return true;
                }
            }

            return false;
        }

        public static int2 InRangeDirectional(int2 target, int2 user, int range)
        {
            List<KeyValuePair<int2, int2>> positions = new List<KeyValuePair<int2, int2>>();
            for (int r = 1; r <= 2; r++)
            {
                for (int i = 1; i <= 8; i++)
                {
                    int2 getdir = GetDirection(user, i, r);
                    if (i + 1 == 1 || i + 1 == 5)
                    {
                        for (int k = 0; k < range; k++)
                        {
                            positions.Add(new KeyValuePair<int2, int2>(getdir + new int2(k, 0), new int2(i, r)));
                            positions.Add(new KeyValuePair<int2, int2>(getdir - new int2(k, 0), new int2(i, r)));
                        }
                    }
                    else if (i + 1 == 3 || i + 1 == 7)
                    {
                        for (int k = 0; k < range; k++)
                        {
                            positions.Add(new KeyValuePair<int2, int2>(getdir + new int2(0, k), new int2(i, r)));
                            positions.Add(new KeyValuePair<int2, int2>(getdir - new int2(0, k), new int2(i, r)));
                        }
                    }
                    positions.Add(new KeyValuePair<int2, int2>(getdir, new int2(i, r)));
                }
            }

            foreach (KeyValuePair<int2, int2> position in positions)
            {
                if (target == position.Key)
                {
                    return position.Value;
                }
            }

            return new int2(0, 0);
        }



        public static bool LineIntersectsRect(Point p1, Point p2, Rectangle r)
        {
            return LineIntersectsLine(p1, p2, new Point(r.X, r.Y), new Point(r.X + r.Width, r.Y)) ||
                   LineIntersectsLine(p1, p2, new Point(r.X + r.Width, r.Y), new Point(r.X + r.Width, r.Y + r.Height)) ||
                   LineIntersectsLine(p1, p2, new Point(r.X + r.Width, r.Y + r.Height), new Point(r.X, r.Y + r.Height)) ||
                   LineIntersectsLine(p1, p2, new Point(r.X, r.Y + r.Height), new Point(r.X, r.Y)) ||
                   (r.Contains(p1) && r.Contains(p2));
        }

        private static bool LineIntersectsLine(Point l1p1, Point l1p2, Point l2p1, Point l2p2)
        {
            float q = (l1p1.Y - l2p1.Y) * (l2p2.X - l2p1.X) - (l1p1.X - l2p1.X) * (l2p2.Y - l2p1.Y);
            float d = (l1p2.X - l1p1.X) * (l2p2.Y - l2p1.Y) - (l1p2.Y - l1p1.Y) * (l2p2.X - l2p1.X);

            if (d == 0)
            {
                return false;
            }

            float r = q / d;

            q = (l1p1.Y - l2p1.Y) * (l1p2.X - l1p1.X) - (l1p1.X - l2p1.X) * (l1p2.Y - l1p1.Y);
            float s = q / d;

            if (r < 0 || r > 1 || s < 0 || s > 1)
            {
                return false;
            }

            return true;
        }

        public static KeyValuePair<bool, int> InRangeLinearOverlap(int2 target, int2 user, int range, List<int2> blocked)
        {
            Point targetpoint = new Point(target.x * 25, target.y * 25);
            Point userpoint = new Point(user.x * 25, user.y * 25);

            foreach (int2 i in blocked)
            {
                Rectangle rect = new Rectangle(new Point(i.x * 25, i.y * 25), new Size(17, 17));
                if (LineIntersectsRect(targetpoint, userpoint, rect))
                {
                    return new KeyValuePair<bool, int>(false, 0);
                }
            }

            List<KeyValuePair<int2, int>> positions = new List<KeyValuePair<int2, int>>();
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < range; j++)
                {
                    int r = j + 1;
                    int2 getdir = GetDirection(target, i + 1, r);
                    if (i + 1 == 1 || i + 1 == 5)
                    {
                        for (int k = 0; k < range; k++)
                        {
                            positions.Add(new KeyValuePair<int2, int>(getdir + new int2(k, 0), r));
                            positions.Add(new KeyValuePair<int2, int>(getdir - new int2(k, 0), r));
                        }
                    }
                    else if (i + 1 == 3 || i + 1 == 7)
                    {
                        for (int k = 0; k < range; k++)
                        {
                            positions.Add(new KeyValuePair<int2, int>(getdir + new int2(0, k), r));
                            positions.Add(new KeyValuePair<int2, int>(getdir - new int2(0, k), r));
                        }
                    }
                    positions.Add(new KeyValuePair<int2, int>(getdir, r));
                }
            }

            foreach (KeyValuePair<int2, int> position in positions)
            {
                if (user == position.Key)
                {
                    return new KeyValuePair<bool, int>(true, position.Value);
                }
            }

            return new KeyValuePair<bool, int>(false, 0);
        }

        public static KeyValuePair<bool, int> InRangeExperimental(int2 target, int2 user, int range, List<List<int2>> blocked)
        {
            /*
            grid = new RoyT.AStar.Grid(size.x, size.y, 1.0f);
            foreach (int2 block in blocked)
            {
                grid.BlockCell(new Position(block.x, block.y));
            }

            RoyT.AStar.Position[] path = grid.GetPath(new RoyT.AStar.Position(target.x, target.y), new RoyT.AStar.Position(user.x, user.y), 
                MovementPatterns.Full);
            foreach (RoyT.AStar.Position position in path)
            {

            }
            */
            
            int size = range;
            for (int y = target.y - size; y <= target.y + range; y++)
            {
                for (int x = target.x - size; x <= target.x + range; x++)
                {
                    int2 pos = new int2(x, y);
                    /*
                    bool prevent = false;
                    foreach (int2 block in blocked)
                    {
                        if (pos.y == block.y)
                        {
                            if (pos.x == block.x || pos.x == block.x + 1 || pos.x == block.x + 2)
                            {
                                Console.WriteLine(pos.ToString());
                                Console.WriteLine("Bad position");
                                prevent = true;
                            }
                        }
                    }
                    */
                    bool prevent = false;
                    foreach (List<int2> block in blocked)
                    {
                        // Basic checking
                        /*
                        if ((pos.x <= block.x && pos.y == block.y) || (pos.x >= block.x && pos.y == block.y)
                            || (pos.y <= block.y && pos.x == block.x) || (pos.y >= block.y && pos.x == block.x))
                        {
                            prevent = true;
                        }
                        */
                        for (int i = 1; i <= range; i++)
                        {
                            // Basic checks
                            bool prioritizeX = false;
                            if (block[1].y > block[0].y)
                            {
                                prioritizeX = true;
                            }

                            if (prioritizeX == false)
                            {
                                if (block[0].y < target.y)
                                {
                                    foreach (int2 blocker in block)
                                    {
                                        if (pos.y <= blocker.y && pos.x == blocker.x)
                                        {
                                            prevent = true;
                                        }
                                        if (pos.y == blocker.y - i && pos.x == blocker.x - i)
                                        {
                                            prevent = true;
                                        }
                                        if (pos.y == blocker.y - i && pos.x == blocker.x + i) {
                                            prevent = true;
                                        }
                                    }
                                }

                                if (block[0].y > target.y)
                                {
                                    foreach (int2 blocker in block)
                                    {
                                        if (pos.y >= blocker.y && pos.x == blocker.x)
                                        {
                                            prevent = true;
                                        }
                                        if (pos.y == blocker.y + i && pos.x == blocker.x - i)
                                        {
                                            prevent = true;
                                        }
                                        if (pos.y == blocker.y + i && pos.x == blocker.x + i)
                                        {
                                            prevent = true;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (block[0].x < target.x)
                                {
                                    foreach (int2 blocker in block)
                                    {
                                        if (pos.x <= blocker.x && pos.y == blocker.y)
                                        {
                                            prevent = true;
                                        }
                                        if (pos.x == blocker.x - i && pos.y == blocker.y + i)
                                        {
                                            prevent = true;
                                        }
                                        if (pos.x == blocker.x - i && pos.y == blocker.y - i)
                                        {
                                            prevent = true;
                                        }
                                        if (block[0].x == target.x - 1)
                                        {
                                            if (pos.x == blocker.x - (i - 1) && (pos.y == blocker.y + 1 || pos.y == blocker.y + 2))
                                            {
                                                prevent = true;
                                            }
                                            if (pos.x == blocker.x - (i - 1) && (pos.y == blocker.y - 1 || pos.y == blocker.y - 2))
                                            {
                                                prevent = true;
                                            }
                                            if (pos.x == blocker.x - i && pos.y == blocker.y + (i + 1))
                                            {
                                                prevent = true;
                                            }
                                            if (pos.x == blocker.x - i && pos.y == blocker.y - (i + 1))
                                            {
                                                prevent = true;
                                            }
                                        }
                                    }
                                }

                                if (block[0].x > target.x)
                                {
                                    foreach (int2 blocker in block)
                                    {
                                        if (pos.x >= blocker.x && pos.y == blocker.y)
                                        {
                                            prevent = true;
                                        }
                                        if (pos.x == blocker.x + i && pos.y == blocker.y + i)
                                        {
                                            prevent = true;
                                        }
                                        if (pos.x == blocker.x + i && pos.y == blocker.y - i)
                                        {
                                            prevent = true;
                                        }
                                        if (block[0].x == target.x + 1)
                                        {
                                            if (pos.x == blocker.x + (i - 1) && (pos.y == blocker.y + 1 || pos.y == blocker.y + 2))
                                            {
                                                prevent = true;
                                            }
                                            if (pos.x == blocker.x + (i - 1) && (pos.y == blocker.y - 1 || pos.y == blocker.y - 2))
                                            {
                                                prevent = true;
                                            }
                                            if (pos.x == blocker.x + i && pos.y == blocker.y + (i + 1))
                                            {
                                                prevent = true;
                                            }
                                            if (pos.x == blocker.x + i && pos.y == blocker.y - (i + 1))
                                            {
                                                prevent = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        /*
                        // Diagonal check loop
                        for (int i = 0; i < range; i++)
                        {
                            // Lower than target's y value
                            if (block.y > target.y)
                            {
                                if (pos.x == block.x + i && pos.y == block.y + i)
                                {
                                    prevent = true;
                                    break;
                                }
                                if (pos.x == block.x - i && pos.y == block.y + i)
                                {
                                    prevent = true;
                                    break;
                                }
                            }
                            /*
                            // Higher that target's y value
                            if (block.y < target.y)
                            {
                                if (pos.x == block.x - (i + 1) && pos.y == block.y - (i + 1))
                                {
                                    prevent = true;
                                    break;
                                }
                                if (pos.x == block.x + (i + 1) && pos.y == block.y - (i + 1))
                                {
                                    prevent = true;
                                    break;
                                }
                            }
                            
                        }
                        */
                    }

                    if (prevent == false)
                    {
                        if (pos == user)
                        {
                            return new KeyValuePair<bool, int>(true, 2);
                        }
                        GameVariables.game.Path.Add(pos);
                        Console.WriteLine(pos.ToString());
                    } else
                    {
                        Console.WriteLine(pos.ToString());
                    }
                }
            }
            return new KeyValuePair<bool, int>(false, 0);
        }

        public static KeyValuePair<bool, int> InRange(int2 target, int2 user, int range, List<int2> blocked)
        {
            List<KeyValuePair<int2, int>> positions = new List<KeyValuePair<int2, int>>();
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < range; j++)
                {
                    int r = j + 1;
                    int2 getdir = GetDirection(target, i + 1, r);
                    if (i + 1 == 1 || i + 1 == 5)
                    {
                        for (int k = 0; k < range; k++)
                        {                           
                            positions.Add(new KeyValuePair<int2, int>(getdir + new int2(k, 0), r));
                            positions.Add(new KeyValuePair<int2, int>(getdir - new int2(k, 0), r));
                        }
                    } else if (i + 1 == 3 || i + 1 == 7)
                    {
                        for (int k = 0; k < range; k++)
                        {
                            positions.Add(new KeyValuePair<int2, int>(getdir + new int2(0, k), r));
                            positions.Add(new KeyValuePair<int2, int>(getdir - new int2(0, k), r));
                        }
                    }
                    positions.Add(new KeyValuePair<int2, int>(getdir, r));
                }
            }
            
            foreach (KeyValuePair<int2, int> position in positions)
            {
                if (user == position.Key)
                {
                    return new KeyValuePair<bool, int>(true, position.Value);
                }
            }

            return new KeyValuePair<bool, int>(false, 0);
        }

        public static int2 GetDirection(int2 i2, int dir, int range = 1, int add = 0)
        {
            if (dir == 1 || dir == -5)
            {
                return new int2(i2.x, i2.y - range);
            }
            else if (dir == 2 || dir == -6)
            {
                return new int2(i2.x + range, i2.y - range);
            }
            else if (dir == 3 || dir == -7)
            {
                return new int2(i2.x + range, i2.y);
            }
            else if (dir == 4 || dir == -8)
            {
                return new int2(i2.x + range, i2.y + range);
            }
            else if (dir == 5 || dir == -1)
            {
                return new int2(i2.x, i2.y + range);
            }
            else if (dir == 6 || dir == -2)
            {
                return new int2(i2.x - range, i2.y + range);
            }
            else if (dir == 7 || dir == -3)
            {
                return new int2(i2.x - range, i2.y);
            }
            else if (dir == 8 || dir == -4)
            {
                return new int2(i2.x - range, i2.y - range);
            }
            else
            {
                throw new InvalidOperationException($"Direction {dir} is invalid!");
            }
        }

        public static int2 RandomPosition(int2 size, List<int2> occupied)
        {
            int times = 0;
            bool found = false;
            int2 position = new int2(0, 0);
            while (found == false)
            {
                position = new int2(rng.Next(0, size.x - 1), rng.Next(0, size.y - 1));
                bool located = true;
                foreach (int2 pos in occupied)
                {
                    if (position == pos)
                    {
                        located = false;
                    }
                }
                found = located;
            }
            return position;
        }

        public static PixelMap PixelGen(string name, Bitmap bitmap)
        {
            List<Pixel> pixels = new List<Pixel>();

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Color pixelcolor = bitmap.GetPixel(x, y);
                    if (pixelcolor.A != 0)
                    {
                        pixels.Add(new Pixel(pixelcolor, new int2(x, y)));  
                    }
                }
            }

            PixelMap map = new PixelMap(name, new int2(bitmap.Width / 2, bitmap.Height / 2), pixels);
            return map;
        }

        public static T Clamp<T>(T aValue, T aMin, T aMax) where T : IComparable<T>
        {
            var _Result = aValue;
            if (aValue.CompareTo(aMax) > 0)
                _Result = aMax;
            else if (aValue.CompareTo(aMin) < 0)
                _Result = aMin;
            return _Result;
        }

        public static T DeepCopy<T>(T obj)
        {
            if (obj == null)
                throw new ArgumentNullException("Object cannot be null");
            return (T)Process(obj);
        }

        private static object Process(object obj)
        {
            if (obj == null)
                return null;
            Type type = obj.GetType();

            if (type.IsValueType || type == typeof(string))
            {
                return obj;
            }
            else if (type.IsArray)
            {
                Type elementType = type.GetElementType();
                var array = obj as Array;
                Array copied = Array.CreateInstance(elementType, array.Length);
                for (int i = 0; i < array.Length; i++)
                {
                    copied.SetValue(DeepCopy(array.GetValue(i)), i);
                }
                return Convert.ChangeType(copied, obj.GetType());
            }
            else if (type.IsClass)
            {

                object toret = Activator.CreateInstance(obj.GetType());
                FieldInfo[] fields = type.GetFields(BindingFlags.Public |
                            BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (FieldInfo field in fields)
                {
                    object fieldValue = field.GetValue(obj);
                    if (fieldValue == null)
                        continue;
                    field.SetValue(toret, DeepCopy(fieldValue));
                }
                return toret;
            }
            else
                throw new ArgumentException($"Unknown type {type.FullName}");
        }

    }
}
