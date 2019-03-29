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

        public static int2 operator +(int2 one, int two)
        {
            int2 i2 = new int2();
            i2.x = one.x + two;
            i2.y = one.y + two;
            return i2;
        }

        public static int2 operator -(int2 one, int two)
        {
            int2 i2 = new int2();
            i2.x = one.x - two;
            i2.y = one.y - two;
            return i2;
        }

        public static int2 operator *(int2 one, int two)
        {
            int2 i2 = new int2();
            i2.x = one.x * two;
            i2.y = one.y * two;
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

        /*
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
        */

        public override bool Equals(object obj)
        {
            return obj is int2 @int &&
                   x == @int.x &&
                   y == @int.y;
        }

        public override int GetHashCode()
        {
            var hashCode = 1502939027;
            hashCode = hashCode * -1521134295 + x.GetHashCode();
            hashCode = hashCode * -1521134295 + y.GetHashCode();
            return hashCode;
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
        public List<ItemPosition> items;
        Random rng = new Random();

        public override string ToString()
        {
            return title;
        }

        public Location(string _title, string _description, int2 _size, List<Exit> _exits = null, List<ItemPosition> _items = null, List<Interactable> _interactables = null, List<Entity> _entities = null)
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
                items = new List<ItemPosition>();

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

        public void addItem(ItemPosition item)
        {
            if (item.coord.Equals(new int2(-1, -1)))
            {
                Console.WriteLine("Initilizing randomizer...");
                List<int2> occupied = new List<int2>();
                foreach (Entity e in entities)
                {
                    occupied.Add(e.coord);
                }
                foreach (ItemPosition itempos in items)
                {
                    occupied.Add(itempos.coord);
                }
                items.Add(new ItemPosition(item.item, EXT.RandomPosition(size, occupied)));
            }
            else
            {
                items.Add(item);
            }           
        }

        public void removeItem(ItemPosition item)
        {
            if (items.Contains(item))
            {
                items.Remove(item);
            }
        }

        public Item takeItem(string name)
        {
            foreach (ItemPosition _item in items)
            {
                if (_item.item.name.ToLower() == name)
                {
                    ItemPosition temp = _item;
                    items.Remove(temp);
                    return temp.item;
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

        public override bool Equals(object obj)
        {
            return obj is Location location &&
                   title == location.title &&
                   description == location.description &&
                   EqualityComparer<int2>.Default.Equals(size, location.size);
        }

        public override int GetHashCode()
        {
            var hashCode = -1545337786;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(title);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(description);
            hashCode = hashCode * -1521134295 + EqualityComparer<int2>.Default.GetHashCode(size);
            return hashCode;
        }
    }
    public class Exit
    {
        public enum Directions
        {
            Undefined, North, East, South, West, Up, Down, NorthEast, NorthWest, SouthEast, SouthWest, In, Out
        }

        public static string[] shortDirections = { "Null", "N", "E", "S", "W", "U", "D", "NE", "NW", "SE", "SW", "I", "O" };

        public int attachment1;
        public int attachment2;
        public Location room1;
        public Location room2;
        public Directions direction1;
        public Directions direction2;

        public override string ToString()
        {
            return $"{direction1.ToString()},{direction2.ToString()}";
        }

        public Exit(Directions _direction1 = Directions.Undefined, Directions _direction2 = Directions.Undefined, Location _room1 = null, Location _room2 = null)
        {
            direction1 = _direction1;
            direction2 = _direction2;
            room1 = _room1;
            room2 = _room2;
        }

        public void setAttachments(int set1, int set2)
        {
            attachment1 = set1;
            attachment2 = set2;
        }

        public void setAttachment(int room, int set)
        {
            if (room == 1)
            {
                attachment1 = set;
            } else if (room == 2)
            {
                attachment2 = set;
            } else
            {
                throw new IndexOutOfRangeException($"Invalid room value {room}, room value must be 1 or 2");
            }
        }

        public string getDirection(int room)
        {
            if (room == 1)
            {
                return direction1.ToString("G");
            }
            else if (room == 2)
            {
                return direction2.ToString("G");
            }
            else
            {
                throw new IndexOutOfRangeException($"Invalid room value {room}, room value must be 1 or 2");
            }
        }

        public string getShortDirection(int room)
        {
            if (room == 1)
            {
                return shortDirections[(int)direction1].ToLower();
            }
            else if (room == 2)
            {
                return shortDirections[(int)direction2].ToLower();
            }
            else
            {
                throw new IndexOutOfRangeException($"Invalid room value {room}, room value must be 1 or 2");
            }
        }
    }
}
