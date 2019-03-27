using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;
using System.Text.RegularExpressions;

namespace Something
{
    public static class GameVariables
    {
        public static Main game;
        public static int turns;

        public static void Test()
        {
            game.TypeLine("__________");
        }
    }
    public static class EXT
    {
        public static Random rng = new Random();

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

        public static KeyValuePair<bool, int> InRange(int2 target, int2 user, int range)
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
            if (dir == 1)
            {
                return new int2(i2.x, i2.y - range);
            }
            else if (dir == 2)
            {
                return new int2(i2.x + range, i2.y - range);
            }
            else if (dir == 3)
            {
                return new int2(i2.x + range, i2.y);
            }
            else if (dir == 4)
            {
                return new int2(i2.x + range, i2.y + range);
            }
            else if (dir == 5)
            {
                return new int2(i2.x, i2.y + range);
            }
            else if (dir == 6)
            {
                return new int2(i2.x - range, i2.y + range);
            }
            else if (dir == 7)
            {
                return new int2(i2.x - range, i2.y);
            }
            else if (dir == 8)
            {
                return new int2(i2.x - range, i2.y - range);
            }
            else
            {
                throw new InvalidOperationException($"Direction {dir} is invalid!");
            }
        }

        public static int2 RandomPosition(int2 size, List<int2> occupied, int maxtimes = -1)
        {
            int times = 0;
            bool found = false;
            int2 position = new int2(-1, -1);
            while (found == false)
            {
                position = new int2(rng.Next(1, size.x - 1), rng.Next(1, size.y - 1));
                bool located = true;
                foreach (int2 pos in occupied)
                {
                    if (position.Equals(pos))
                    {
                        located = false;
                    }
                }
                found = located;
                ++times;
                if (maxtimes != -1 && times >= maxtimes)
                {
                    break;
                }
            }
            return position;
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

    }
}
