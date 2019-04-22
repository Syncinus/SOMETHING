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
        public Action<Main> startup;
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
            // things
        }
    }

    public class EscapeShip : World
    {
        public override void Setup()
        {
            /*TextSequence startup = new TextSequence(@"
earth was a great planet, once home to many species of creatures
with 3 races of them being sentient;
the humans were a species with no innate magic, though being adaptable
and flexible and able to easily learn magic, aswell as being good at many
different things. the elves were split up into many different species of
their race, each with their own set of magical abilities, though being
less adaptable than humans and having a harder time learning the learned
type of magic that humans did, primarily doing innate magic instead.
then there were the dwarves, who made advanced and intricate technology,
and were rarley seen by anyone other than themselves for living
deep underground. the humans and the elves shared the surface of the earth,
where the dwarves lived under it, with vast tunnel networks and 
advanced technology, rarley ever emerging from the underground. in the year 2043 the united nations
evolved, instead of just being a combined cooperation of many countries with
humans and elves, they all joined together to form one unified group: espilon.
with this event the world started growing at a rate it never did before, the
combined focus of the world improved technology and magic faster than it ever had.
this remained until in 2131, when a 'resistance' against espilon was formed, named
'nexus', which had been growing scince 2116, and finally striked against the 
headquarters of espilon, delivering a huge blow to them as a whole,
despite espilon being bigger than nexus, their focus on science and research
and not weapons technology resulted in nexus having a huge advantage against
them, aswell many scientists of espilon dying in the initial strike on the headquarters.
the war went on for a short period of time, as in 2133 espilon was getting weaker and
nexus was winning against them until one unexpected event happened; aliens.
the exenons were a civilization with more advanced technology then earth,
and known in the universe for taking over worlds with civilizations that were
inferior to them, humans and elves were no exception to that, the exenon motherships
initial strike practically wiped nexus off the map, and our technology wasnt able
to take them down, and our magic was weak against them. espilon didnt have any armed ships
to fight the exenon and all of their existing ships were damaged from the fight with nexus,
the only hope of earth was a project that espilon had been working on scince around 2067 deemed;
necron. project necron was a huge starship build out of our moon, though almost complete, the nexus
war stopped development on it, it was capable of holding a huge amount of people on it,
aswell as being capable of intergalactic travel. necron was powered by a reactor
that generated an energy called multimatter which was a combination of the best technological energies
and magical force, which allowed for it to power its magic and technological features,
and allowed it to have engines powered by it to travel at warp level speeds.
completing necron would take around 2 months, as it was pulled into earth in around 2105,
once the 2 months passed, the population of all earth went down from 10 billion to 1,
cities destroyed, the world was lost, there was nothing humans nor elves could to to
save it now, the last 1 billion was loaded into project necron and then it left,
the exenons had tooken earth, though the dwarves launched a secret weapon of theirs which
rendered the surface of the planet uninhabitable for the exenons, the other races had already
left. 20 years had passed of travel, as espilon built a fleet of ships while traveling through
space and had left necron behind for a command ship called the xenon, when they finally reached
there destination, a planet called galactica around a star named similarly, galacticron, in 2154,
which we grew even faster than we could on earth with the resources of space that were now accessible,
then, in 2194, the exenons found us. we were more prepared for them now, our techonology capable
of fighting theirs, despite our evolved magic having low effect on them, as they created resistances to it,           
we fought them now, we could do it, until in 2195 the final stand happened, we had all our military forces on every one
our planets near us that remained ours gather in the galacticron system, as the exenons took there entire fleet,
virtually there entire force, to fight us, we couldnt win this fight, but we could destroy them once and for all... along
with us. we sent out humans and elves in escape ships to around our galaxy, with most of us remaining on galactica,
defending our weapon, the proteon accelerator, from them, once it triggered it would release a wave
of a deadly but powerful energy called proteon that would render the entire system uninhabitable for centuries,
but we released it, the exenons and most of us destroyed by it. you were our leader, you left,
now you sit in an escape ship waiting to reach somewhere.
            ", true);*/
            TextSequence startup = new TextSequence(@"
<somereallylongtagname> </somereallylongtagname>
", false);
            Location ship = new Location("ship", "a ship", new int2(10, 10), new int2(0, 0));
            Player player = players[0];
            AddLocations(ship);
            player.Move(ship);
            Move(ship);
            this.startup = (main) => {
                main.textsequences.Enqueue(startup);
            };
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
