using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Something
{
    public class World
    {
        public List<Player> players;
        public List<Location> locations;
        public Location current;

        internal Main main;

        public World()
        {
            main = GameVariables.game;
            players = new List<Player>();
            locations = new List<Location>();
        }

        public void Move(Location newlocation)
        {
            current = newlocation;
            if (!locations.Contains(newlocation))
            {
                throw new IndexOutOfRangeException($"Location {newlocation.title} doesn't exist in world!");
            }
        }

        public void AddLocations(params Location[] _locations)
        {
            foreach (Location location in _locations)
            {
                locations.Add(location);
            }
        }

        public void RemoveLocations(params Location[] _locations)
        {
            foreach (Location location in _locations)
            {
                if (locations.Contains(location))
                {
                    locations.Remove(location);
                }
            }
        }

        public void AddPlayer(Player player)
        {
            players.Add(player);
        }

        public void RemovePlayer(Player player)
        {
            if (players.Contains(player))
            {
                players.Remove(player);
            }
        }

        public virtual void Setup()
        {
            // 
        }
    }


    public class TakimaPirateShip : World
    {
        public override void Setup()
        {
            Player player = players[0];
            player.coord = new int2(2, 2);
            Location storage = new Location("storage room", "a room filled with around 17 different boxes, all in stacks, each looking openable", new int2(5, 5), new int2(0, 0));
            Item junk = new Item("junk", "some junk from other planets", false);
            Item containmentsystem = new Item("small jerry containment system", "", false);
            Weapon lasergun = new Weapon("basic laser gun", "a rusty little laser gun that looks functional", 5, 3, 2, "laser", true);
            Armor basicsuit = new Armor("basic armor", "some old armor, looks capable though", true, 5, 3, 3);
            List<int2> occupied = new List<int2>();
            occupied.Add(player.coord);
            occupied.Add(new int2(2, 0));
            Random rng = new Random();
            int count = 0;
            int max = 17;

            int armorcount = rng.Next(0, 18);
            int containmentcount = rng.Next(0, 18);
            while (containmentcount == armorcount)
            {
                containmentcount = rng.Next(0, 18);
            }
            while (count <= max)
            {
                int amount = rng.Next(1, 4);
                List<Item> items = new List<Item>();
                for (int i = 0; i < amount; i++)
                {
                    if (count == armorcount)
                    {
                        items.Add(basicsuit);
                    }
                    else if (count == containmentcount)
                    {
                        items.Add(containmentsystem);
                    }
                    else
                    {
                        items.Add(junk);
                    }
                    count++;
                }

                int2 coord = EXT.RandomPosition(new int2(5, 5), occupied);
                BoxStack stack = new BoxStack(coord, items);
                storage.addInteractable(stack);
                stack.location = storage;
                occupied.Add(coord);
            }

            AddLocations(storage);
            player.position = storage;
            player.Move(storage);
            Move(storage);

            BoxStack laserstack = new BoxStack(new int2(2, 0), new List<Item>() { junk, lasergun });
            storage.addInteractable(laserstack);
            laserstack.location = storage;
        }
    }

    public class Testing : World
    {
        public override void Setup()
        {
            Console.WriteLine(players.Count);
            Player player = players[0];
            Location l1 = new Location("Entrance to hall", "You stand at the entrance of a long hallway. The hallway gets darker\nand darker, and you cannot see what lies beyond. To the east\nis an old oak door, which looks locked but openable.", new int2(10, 10), new int2(0, 0));
            Item rock = new Item("rock", "A rather jagged rock, slightly smaller than a fist.", false);
            Potion testpotion = new Potion("potion of existing", "An intricately designed bottle containing some kind of fluid", true,
                new Existing(4000, 10, "existingness", player));
            Weapon beatingstick = new Weapon("beating stick", "a stick of wood around twenty centimeters thick and one meter long\nthat weighs a very large amount, good for throwing at people", 25120, 5, 10000000, "beating", true, new Beating(1, 5, "beatdown retribution", player));
            Weapon weapon = new Weapon("weapon", "a weapon", 10, 2, 3, "b");
            
            WorldTrigger trigger = new WorldTrigger(new int2(9, 9), (triggerer, location, trigg) =>
            {
                Console.WriteLine("far out dude, far out");
            }, l1);

            Enemy uglywugly = new Enemy()
            {
                name = "enemy",
                armor = 0,
                health = 200,
                speed = 15,
                coord = new int2(4, 4),
                Attack = 15,
                Defense = 5,
                size = Defaults.Sizes.Normal,
                coloring = Color.Aqua,
                weapon = weapon,
                ai = Enemy.AIMode.Agressive,
                movement = 1,
            };
          
            Entity wall = new Entity()
            {
                name = "wall",
                appear = false,
                armor = 3000,
                health = 1000,
                speed = 0,
                Attack = 0,
                Defense = 50,
                size = new int2(1, 5),
                coloring = Color.Yellow
            };
            Entity wall2 = (Entity)EXT.DeepCopy(wall);
            Entity wall3 = new Entity()
            {
                name = "wall",
                appear = false,
                armor = 3000,
                health = 1000,
                speed = 10,
                Attack = 0,
                Defense = 0,
                size = new int2(6, 1),
            };
            Entity wall4 = wall3.Clone();
            wall.coord = new int2(3, 2);
            wall2.coord = new int2(5, 2);
            wall3.coord = new int2(2, 7);
            wall4.coord = new int2(2, 0);
            //wall2.coord = new int2(2, 7);

            //wall.Flip();
            //this.Invalidate();
            //panel1.Invalidate();
            //wall.Move(l1);
            //wall2.Move(l1);
            //wall3.Move(l1);
            //wall4.Move(l1);
            Armor armor = new Armor("armor of existing", "The uncomprehensibly complicated armor which's power is exponential", true, 10, 0, 0);
            player.inventory.Add(armor);
            uglywugly.Move(l1);
            player.inventory.Add(testpotion);
            player.inventory.Add(beatingstick);
            player.inventory.Add(weapon);
            player.Move(l1);
            l1.addItem(new ItemPosition(rock));
            l1.addItem(new ItemPosition(testpotion));

            Location l2 = new Location("End of hall", "You have reached the end of a long dark hallway. You can\nsee nowhere to go but back.", new int2(10, 10), new int2(0, -275));
            Item window = new Item("window", "A single sheet of glass. It seems sealed up.", false);
            l2.addItem(new ItemPosition(window));

            Location l3 = new Location("Small study", "This is a small and cluttered study, containing a desk covered with\npapers. Though they no doubt are of some importance,\nyou cannot read their writing", new int2(10, 10), new int2(275, -75));

            Exit mrclean = new Exit(Exit.Directions.East, Exit.Directions.West, 2, l1, l3);
            mrclean.setAttachments(2, 5);
            LockedDoor door = new LockedDoor("locked door", mrclean, new int2(l1.size.x, 2), new int2(1, 2), false, new InteractableAction("lockpick", 10), new InteractableAction("break", 15));
            l3.addExit(mrclean);
            door.location = l1;
            l1.addInteractable(door);
            Exit through = new Exit(Exit.Directions.North, Exit.Directions.South, 2, l1, l2);
            through.setAttachments(3, 3);
            l1.addExit(through);
            l2.addExit(through);
            player.position = l1;
            player.coord = new int2(2, 2);
            AddLocations(l1, l2, l3);
            Move(player.position);
        }
    }
}
