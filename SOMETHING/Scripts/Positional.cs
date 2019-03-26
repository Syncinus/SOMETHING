using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace Something
{
    public struct int2
    {
        public int x;
        public int y;

        public int2(int _x, int _y)
        {
            x = _x;
            y = _y;
        }

        public static int2 operator +(int2 one, int2 two)
        {
            int2 i2 = new int2();
            i2.x = one.x + two.x;
            i2.y = one.y + two.y;
            return i2;
        }

        public static int2 operator -(int2 one, int two)
        {
            int2 i2 = new int2();
            i2.x = one.x - two;
            i2.y = one.y - two;
            return i2;
        }

        public static int2 operator +(int2 one, int two)
        {
            int2 i2 = new int2();
            i2.x = one.x + two;
            i2.y = one.y + two;
            return i2;
        }

        public static int2 operator -(int2 one, int2 two)
        {
            int2 i2 = new int2();
            i2.x = one.x - two.x;
            i2.y = one.y - two.y;
            return i2;
        }

        public static int2 operator *(int2 one, int2 two)
        {
            int2 i2 = new int2();
            i2.x = one.x * two.x;
            i2.y = one.y * two.y;
            return i2;
        }

        public static int2 operator /(int2 one, int2 two)
        {
            int2 i2 = new int2();
            i2.x = one.x / two.x;
            i2.y = one.y / two.y;
            return i2;
        }

        public static bool operator !=(int2 one, int2 two)
        {
            return (!one.Equals(two));
        }

        public static bool operator ==(int2 one, int2 two)
        {
            return one.Equals(two);
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !GetType().Equals(obj.GetType()))
            {
                return false;
            } else
            {
                int2 i = (int2)obj;
                return (x == i.x) && (y == i.y);
            }
        }

        public override string ToString()
        {
            return $"x:{x},y:{y}";
        }

        public Point ToPoint()
        {
            return new Point(x, y);
        }

        public Vector2 ToVector2()
        {
            return new Vector2(x, y);
        }
    }

    public class Location
    {
        public string title;
        public string description;
        public int2 size;
        public List<Entity> entities;
        public List<Interactable> interactables;
        public List<Exit> exits;
        public List<Item> items;

        public override string ToString()
        {
            return title;
        }

        public Location(string _title, string _description, int2 _size, List<Exit> _exits = null, List<Item> _items = null, List<Interactable> _interactables = null, List<Entity> _entities = null)
        {
            title = _title;
            description = _description;
            size = _size;
            if (_exits != null)
                exits = _exits;
            else
                exits = new List<Exit>();

            if (_items != null)
                items = _items;
            else
                items = new List<Item>();

            if (_interactables != null)
                interactables = _interactables;
            else
                interactables = new List<Interactable>();

            if (_entities != null)
                entities = _entities;
            else
                entities = new List<Entity>();
        }

        public void addEntity(Entity entity)
        {
            entities.Add(entity);
        }

        public void removeEntity(Entity entity)
        {
            if (entities.Contains(entity))
            {
                entities.Remove(entity);
            }
        }

        public void addExit(Exit exit)
        {
            exits.Add(exit);
        }

        public void removeExit(Exit exit)
        {
            if (exits.Contains(exit))
            {
                exits.Remove(exit);
            }
        }

        public void addItem(Item item)
        {
            items.Add(item);
        }

        public void removeItem(Item item)
        {
            if (items.Contains(item))
            {
                items.Remove(item);
            }
        }

        public Item takeItem(string name)
        {
            foreach (Item _item in items)
            {
                if (_item.name.ToLower() == name)
                {
                    Item temp = _item;
                    items.Remove(temp);
                    return temp;
                }
            }

            return null;
        }

        public void addInteractable(Interactable interactable)
        {
            interactables.Add(interactable);
        }

        public void removeInteractable(Interactable interactable)
        {
            if (interactables.Contains(interactable))
            {
                interactables.Remove(interactable);
            }
        }

        public void interact(string name, Creature user, string way)
        {
            Interactable interactable = interactables.Find(item => item.name == name);
            if (interactable != null)
            {
                /*
                GameVariables.game.TypeLine("These are the actions you can do on the " + interactable.name);
                Console.WriteLine();
                foreach (KeyValuePair<int, InteractableAction> intaction in interactable.interactableActions)
                {
                    GameVariables.game.TypeLine(intaction.Value.name);
                    Console.WriteLine();
                }
                GameVariables.game.TypeLine("What would you like to do to the " + interactable.name);
                string input = Console.ReadLine();
                */
                interactable.Interact(way, user);
                //Game.TypeLine("You attempt to interact with " + interactable.name);
                //interactable.Interact();
            }
        }
    }
    public class Exit
    {
        public enum Directions
        {
            Undefined, North, East, South, West, Up, Down, NorthEast, NorthWest, SouthEast, SouthWest, In, Out
        }

        public static string[] shortDirections = { "Null", "N", "E", "S", "W", "U", "D", "NE", "NW", "SE", "SW", "I", "O" };

        public Location leads;
        public Directions direction;

        public override string ToString()
        {
            return direction.ToString();
        }

        public Exit(Directions _direction = Directions.Undefined, Location _leads = null)
        {
            direction = _direction;
            leads = _leads;
        }

        public string getShortDirection()
        {
            return shortDirections[(int)direction].ToLower();
        }
    }
}
