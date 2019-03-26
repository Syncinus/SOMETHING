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

        public static bool InRange(int2 target, int2 user, int range)
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < range; j++)
                {
                    int r = j + 1;
                    if (user == GetDirection(target, i, r))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static int2 GetDirection(int2 i2, int dir, int range = 1)
        {
            if (dir == 1)
            {
                return new int2(i2.x, i2.y + range);
            }
            else if (dir == 2)
            {
                return new int2(i2.x + range, i2.y);
            }
            else if (dir == 3)
            {
                return new int2(i2.x, i2.y - range);
            }
            else if (dir == 4)
            {
                return new int2(i2.x - range, i2.y);
            }
            else
            {
                throw new InvalidOperationException($"Direction {dir} is invalid!");
            }
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
