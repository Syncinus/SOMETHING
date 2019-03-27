﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Timers;
using RoyT.AStar;

namespace Something
{
    public partial class Main : Form
    {
        int Line = 0;
        string TextQueue = "";

        public List<int2> blockedcells;
        public RoyT.AStar.Grid grid;
        public Player player;
        public Graphics graphics;
        public int lastturn;
        bool gameOver = false;
        bool incrementTurn = false;
        bool playerMovement = false;

        public List<int2> Path = new List<int2>();
        public List<Rectangle> Rects = new List<Rectangle>();

        public void InitilizeGame()
        {
            player = new Player()
            {
                name = "player",
                coord = new int2(5, 5),
                armor = 0,
                health = 400,
                speed = 8,
                strength = 10,
                dexterity = 30,
                constitution = 10,
                intelligence = 10,
                wisdom = 10,
                charisma = 10,
                Attack = 17,
                Defense = 3,
                Accuracy = 10,
                movement = 5
            };
        }

        public void BuildTestMap()
        {
            Location l1 = new Location("Entrance to hall", "You stand at the entrance of a long hallway. The hallway gets darker\nand darker, and you cannot see what lies beyond. To the east\nis an old oak door, which looks locked but openable.", new int2(10, 10));
            Item rock = new Item("rock", "A rather jagged rock, slightly smaller than a fist.", false);
            Potion testpotion = new Potion("potion of existing", "An intricately designed bottle containing some kind of fluid", true,
                new Existing(4000, 10, "existingness", player));
            Melee beatingstick = new Melee("beating stick", "a stick of wood around twenty centimeters thick and one meter long\nthat is good for giving beatings", 25120, 1, 10000000, "beating", true, new Beating(1, 5, "beatdown retribution", player));
            Entity uglywugly = new Entity()
            {
                name = "enemy",
                armor = 0,
                health = 200,
                speed = 15,
                coord = new int2(4, 4),
                Attack = 15,
                Defense = 5
            };
            Armor armor = new Armor("armor of existing", "The uncomprehensibly complicated armor which's power is exponential", true, 10);
            player.inventory.Add(armor);
            uglywugly.Move(l1);
            player.inventory.Add(testpotion);
            player.inventory.Add(beatingstick);
            player.Move(l1);
            l1.addItem(new ItemPosition(rock));
            l1.addItem(new ItemPosition(testpotion));

            Location l2 = new Location("End of hall", "You have reached the end of a long dark hallway. You can\nsee nowhere to go but back.", new int2(10, 10));
            Item window = new Item("window", "A single sheet of glass. It seems sealed up.", false);
            l2.addItem(new ItemPosition(window));

            Location l3 = new Location("Small study", "This is a small and cluttered study, containing a desk covered with\npapers. Though they no doubt are of some importance,\nyou cannot read their writing", new int2(10, 10));

            Exit mrclean = new Exit(Exit.Directions.East, Exit.Directions.West, l1, l3);
            mrclean.setAttachments(2, 5);
            LockedDoor door = new LockedDoor("locked door", mrclean, new InteractableAction("lockpick", 10), new InteractableAction("break", 15));
            l3.addExit(mrclean);
            door.location = l1;
            l1.addInteractable(door);
            Exit through = new Exit(Exit.Directions.North, Exit.Directions.South, l1, l2);
            through.setAttachments(5, 5);
            l1.addExit(through);
            l2.addExit(through);
            //l1.addExit(new Exit(Exit.Directions.North, l2));
            //l1.addExit(new Exit(Exit.Directions.East, l3));

            //l2.addExit(new Exit(Exit.Directions.South, l1));

            //l3.addExit(new Exit(Exit.Directions.West, l1));

            player.position = l1;
        }

        public Main()
        {
            InitilizeGame();
            BuildTestMap();
            InitializeComponent();
            Dec1();
            input.Focus();
            LineLOC();
            blockedcells = new List<int2>();
            Thread loop = new Thread(() =>
            {
                while (true)
                {
                    if (TextQueue.Length != 0)
                    {
                        char c = TextQueue[0];
                        textBox1.Invoke((MethodInvoker)delegate { textBox1.AppendText(c.ToString()); });
                        TextQueue = TextQueue.Remove(0, 1);
                        Thread.Sleep(45);
                    }
                    if (lastturn != GameVariables.turns)
                    {
                        lastturn = GameVariables.turns;
                        List<Entity> dead = new List<Entity>();
                        foreach (Entity e in player.position.entities)
                        {
                            e.Update();
                            if (e.dead == true)
                            {
                                dead.Add(e);
                            }
                        }
                        foreach (Entity nolongeralive in dead)
                        {
                            player.position.removeEntity(nolongeralive);
                            Invoke((MethodInvoker) delegate { TypeLine($"{nolongeralive.name} has died."); });
                        }
                        dead.Clear();
                        playerMovement = false;
                        Console.WriteLine("game updated");
                        UpdateMap();
                        foreach (int2 cell in blockedcells)
                        {
                            grid.UnblockCell(new RoyT.AStar.Position(cell.x, cell.y));
                        }
                        blockedcells.Clear();
                        foreach (Entity entity in player.position.entities)
                        {
                            grid.BlockCell(new RoyT.AStar.Position(entity.coord.x, entity.coord.y));
                            blockedcells.Add(entity.coord);
                        }
                    }
                    if (player.health == 0)
                    {
                        gameOver = true;
                        break;
                    }
                }
            });
            loop.Start();
            //grid = new RoyT.AStar.Grid(player.position.size.x, player.position.size.y, 1.0f);
            RegenerateGrid();
        }

        public void RegenerateGrid()
        {
            grid = new RoyT.AStar.Grid(player.position.size.x, player.position.size.y, 1.0f);
            foreach (Entity entity in player.position.entities)
            {
                grid.BlockCell(new RoyT.AStar.Position(entity.coord.x, entity.coord.y));
                blockedcells.Add(entity.coord);
            }
        }

        public void TypeLine(object input)
        {
            string text = input.ToString();
            TextQueue += text + Environment.NewLine;
            int numLines = text.Split('\n').Length;
            for (int q = 0; q < numLines; q++)
            {
                LineLOC();
            }
        }

        public void Write(string text)
        {
            textBox1.AppendText(text);
        }

        public void WriteLine()
        {
            textBox1.Text += Environment.NewLine;
            LineLOC();
        }

        public void WriteLine(string text)
        {
            textBox1.AppendText(text);
            textBox1.Text += Environment.NewLine;
            LineLOC();
        }

        private void Input_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                e.Handled = true;
                string command = input.Text;
                DoAction(command);
                if (incrementTurn == true)
                {
                    ++GameVariables.turns;
                }
                Console.WriteLine("command was entered");
                input.ResetText();
            }
        }

        public void UpdateMap()
        {
            for (int y = 0; y < player.position.size.y; y++)
            {
                for (int x = 0; x < player.position.size.x; x++)
                {
                    foreach (Entity entity in player.position.entities)
                    {
                        foreach (Entity entity2 in player.position.entities)
                        {
                            if (entity2 != entity)
                            {
                                if (entity.coord.Equals(entity2.coord))
                                {
                                    Random rng = new Random();
                                    int bump = rng.Next(0, 2);
                                    int dir = rng.Next(1, 5);

                                    if (bump == 0)
                                    {
                                        entity.coord = EXT.GetDirection(entity.coord, dir);
                                    }
                                    else if (bump == 1)
                                    {
                                        entity2.coord = EXT.GetDirection(entity2.coord, dir);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            this.Invoke((MethodInvoker)delegate { this.Invalidate(); panel1.Invalidate(); });
            //panel1.Invalidate();
            Console.WriteLine("map was updated");
        }

        public void DoAction(string command)
        {
            incrementTurn = false;
            if (command == "help" || command == "h")
            {
                TypeLine("Welcome to SOMETHING!");
                TypeLine("Im still making the commands so im not making this until thats done.");
                return;
            }

            if (command == "look" || command == "l")
            {
                TypeLine("Positions of items in the room:");
                foreach (ItemPosition position in player.position.items)
                {
                    TypeLine($"{position.item.name} at {position.coord.ToString()}");
                }
                return;
            }

            if (command == "remove armor")
            {
                TypeLine($"Removed armor: {player.attachedarmor.name}.");
                player.inventory.Add(player.attachedarmor);
                player.attachedarmor = null;
                return;
            }

            if (command.Contains("moveto ") && playerMovement == false)
            {
                string itemname = command.Substring(7);
                Console.WriteLine(itemname);
                ItemPosition i = null;
                foreach (ItemPosition itempos in player.position.items)
                {
                    if (itempos.item.name.ToLower() == itemname)
                    {
                        i = itempos;
                    }
                }

                if (i != null)
                {
                    Path.Clear();
                    RoyT.AStar.Position[] path = grid.GetPath(new RoyT.AStar.Position(player.coord.x, player.coord.y),
                        new RoyT.AStar.Position(i.coord.x, i.coord.y));
                    foreach (RoyT.AStar.Position position in path)
                    {
                        Path.Add(new int2(position.X, position.Y));
                    }
                    int length = path.Length - 1;

                    if (length < player.movement)
                    {
                        player.coord = i.coord;
                        incrementTurn = true;
                        player.inventory.Add(player.position.takeItem(itemname));
                        TypeLine("You pick up the " + itemname + ".");
                        player.position.removeItem(i);
                    } else
                    {
                        TypeLine("You can't move that far.");
                        Path.Clear();
                        return;
                    }
                } else
                {
                    TypeLine("That item doesn't exist");
                }
                return;
            }

            if (command.Contains("move ") && playerMovement == false)
            {
                string movement = command.Substring(5);
                string[] split = movement.Split(',');
                if (!movement.Contains(','))
                {
                    int amount = int.Parse(command[5].ToString());
                    string m = command.Substring(7);
                    if (amount <= player.movement)
                    {
                        if (m == "north" || m == "n" || m == "1")
                        {
                            player.coord = EXT.GetDirection(player.coord, 1, amount);
                        }
                        else if (m == "northeast" || m == "ne" || m == "2")
                        {
                            player.coord = EXT.GetDirection(player.coord, 2, amount);
                        }
                        else if (m == "east" || m == "e" || m == "3")
                        {
                            player.coord = EXT.GetDirection(player.coord, 3, amount);
                        }
                        else if (m == "southeast" || m == "se" || m == "4")
                        {
                            player.coord = EXT.GetDirection(player.coord, 4, amount);
                        }
                        else if (m == "south" || m == "s" || m == "5")
                        {
                            player.coord = EXT.GetDirection(player.coord, 5, amount);
                        }
                        else if (m == "southwest" || m == "sw" || m == "6")
                        {
                            player.coord = EXT.GetDirection(player.coord, 6, amount);
                        }
                        else if (m == "west" || m == "w" || m == "7")
                        {
                            player.coord = EXT.GetDirection(player.coord, 7, amount);
                        }
                        else if (m == "northwest" || m == "nw" || m == "8")
                        {
                            player.coord = EXT.GetDirection(player.coord, 8, amount);
                        }

                        TypeLine("Player moved successfully");
                        this.Invalidate();
                        panel1.Invalidate();
                        return;
                    } else {
                        TypeLine("You can't move that far");
                        return;
                    }
                }
                else
                {
                    int2 newposition = new int2(int.Parse(split[0]), int.Parse(split[1]));
                    Path.Clear();
                    RoyT.AStar.Position[] path = grid.GetPath(new RoyT.AStar.Position(player.coord.x, player.coord.y),
                        new RoyT.AStar.Position(newposition.x, newposition.y));

                    foreach (RoyT.AStar.Position position in path)
                    {
                        Path.Add(new int2(position.X, position.Y));
                    }

                    int length = path.Length - 1;
                    if (length > player.movement)
                    {
                        TypeLine("You can't move that far.");
                        Path.Clear();
                        return;
                    }

                    /*
                    Position current = null;
                    Position start = new Position { X = player.coord.x + 1, Y = player.coord.y + 1 };
                    Position target = new Position { X = newposition.x + 1, Y = newposition.y + 1 };
                    string[] map = StringifyMap(new int2(start.X, start.Y), new int2(target.X, target.Y));
                    foreach (var line in map)
                    {
                        Console.WriteLine(line);
                    }
                    var openlist = new List<Position>();
                    var closedlist = new List<Position>();
                    int g = 0;
                    openlist.Add(start);

                    while (openlist.Count > 0)
                    {
                        var lowest = openlist.Min(l => l.F);
                        current = openlist.First(l => l.F == lowest);

                        closedlist.Add(current);
                        Console.SetCursorPosition(current.X, current.Y);
                        Console.Write('.');
                        Console.SetCursorPosition(current.X, current.Y);

                        openlist.Remove(current);

                        if (closedlist.FirstOrDefault(l => l.X == target.X && l.Y == target.Y) != null)
                            break;

                        var adjacentSquares = GetWalkableAdjacentSquares(current.X, current.Y, map, openlist);
                        g = current.G + 1;

                        foreach (var adjacentSquare in adjacentSquares)
                        {
                            // if this adjacent square is already in the closed list, ignore it  
                            if (closedlist.FirstOrDefault(l => l.X == adjacentSquare.X
                                && l.Y == adjacentSquare.Y) != null)
                                continue;

                            // if it's not in the open list...  
                            if (openlist.FirstOrDefault(l => l.X == adjacentSquare.X
                                && l.Y == adjacentSquare.Y) == null)
                            {
                                // compute its score, set the parent  
                                adjacentSquare.G = g;
                                adjacentSquare.H = ComputeHScore(adjacentSquare.X, adjacentSquare.Y, target.X, target.Y);
                                adjacentSquare.F = adjacentSquare.G + adjacentSquare.H;
                                adjacentSquare.parent = current;

                                // and add it to the open list  
                                openlist.Insert(0, adjacentSquare);
                            }
                            else
                            {
                                // test if using the current G score makes the adjacent square's F score  
                                // lower, if yes update the parent because it means it's a better path  
                                if (g + adjacentSquare.H < adjacentSquare.F)
                                {
                                    adjacentSquare.G = g;
                                    adjacentSquare.F = adjacentSquare.G + adjacentSquare.H;
                                    adjacentSquare.parent = current;
                                }
                            }
                        }
                    }

                    Position end = current;

                    while (current != null)
                    {
                        Console.SetCursorPosition(current.X, current.Y);
                        Console.Write('_');
                        Console.SetCursorPosition(current.X, current.Y);
                        Path.Add(new int2(current.X - 1, current.Y - 1));
                        current = current.parent;
                    }

                    if (end != null)
                    {
                        Console.SetCursorPosition(0, 20);
                        Console.WriteLine($"Path: {end.G}");
                    }
                    */

                    if (newposition.x > player.position.size.x - 1 || newposition.x < 0 || newposition.y > player.position.size.y - 1 || newposition.y < 0)
                    {
                        TypeLine("That position is invalid!");
                        return;
                    }
                    else
                    {
                        foreach (Entity e in player.position.entities)
                        {
                            if (e.name != "player")
                            {
                                if (e.coord == newposition)
                                {
                                    TypeLine("You can't move ontop of another entity!");
                                    return;
                                }
                            }
                        }
                        TypeLine("Moved to: " + newposition.ToString());
                        player.coord = newposition;
                    }
                    this.Invalidate();
                    panel1.Invalidate();
                    playerMovement = true;
                    Console.WriteLine("player moved to a new position");
                    return;
                }
            } else if (command.Contains("move ") && playerMovement == true)
            {
                TypeLine("You already moved this turn!");
                return;
            } else if (command == "move")
            {
                TypeLine("Please specify where you would like to move.");
            }

            if (command == "draw")
            {
                List<int2> entitypositions = new List<int2>();

                foreach (Entity entity in player.position.entities)
                {
                    entitypositions.Add(entity.coord);
                }

                for (int y = 0; y < player.position.size.y; y++)
                {
                    string line = "";
                    for (int x = 0; x < player.position.size.x; x++)
                    {
                        bool entityposition = false;
                        foreach (int2 position in entitypositions)
                        {
                            if (position.x == x && position.y == y)
                            {
                                if (position.x == player.coord.x && position.y == player.coord.y)
                                {
                                    line += "P";
                                    entityposition = true;
                                    break;
                                }
                                else
                                {
                                    line += "O";
                                    entityposition = true;
                                    break;
                                }
                            }
                        }

                        if (entityposition == false)
                        {
                            line += "B";
                        }
                    }
                    WriteLine(line);
                }
                return;
            }

            if (command == "update")
            {
                incrementTurn = true;
                TypeLine("Incremented game");
                return;
            }

            if (command == "stats" || command == "s")
            {
                TypeLine("Current player stats:");
                TypeLine($"Attached Armor: {player.attachedarmor.name}");
                TypeLine($"Health: {player.health}");
                TypeLine($"Armor: {player.armor}");
                TypeLine($"Speed: {player.movement}");
                return;
            }

            if (command == "allstats")
            {
                TypeLine("Stats for all entities in the room:");

                foreach (Entity e in player.position.entities)
                {
                    TypeLine($"Stats for {e.name}:");
                    TypeLine($"Health: {e.health}");
                    TypeLine($"Armor: {e.armor}");
                    TypeLine($"Speed: {e.movement}");
                }

                return;
            }

            if (command == "179324865")
            {
                TypeLine("Obtained 1x GUCCI ACCELERATOR");
                return;
            }

            if (command == "effects" || command == "e")
            {
                TypeLine("All effects currently on your player:");
                foreach (dynamic effect in player.effects)
                {
                    TypeLine($"Level {effect.level} {effect.name} which will last for {effect.remaining} more turns");
                }
                return;
            }

            if (command == "inventory" || command == "i")
            {
                ShowInventory();
                WriteLine();
                return;
            }

            if (command == "description" || command == "d")
            {
                ShowLocation();
                WriteLine();
                WriteLine();
                return;
            }

            /*
            if (command == "look" || command == "l")
            {
                ShowLocation();
                if (player.position.items.Count == 0)
                {
                    TypeLine("There are no items in this room.");
                }
                return;
            }
            */

            if (command.Length >= 7 && command.Substring(0, 7) == "look at")
            {
                Item i = null;
                string look = command.ToLower().Substring(8);
                if (command == "look at")
                {
                    TypeLine("Please specify what you wish to look at.");
                    return;
                }
                if (player.position.items.Exists(x => x.item.name == look))
                {
                    ItemPosition ip;
                    ip = player.position.items.Find(item => item.item.name == look);
                    TypeLine($"{ip.item.name} at {ip.coord}: {ip.item.description}");
                }
                else if (player.inventory.Exists(x => x.name == look)) {
                    i = player.inventory.Find(item => item.name == look);
                    TypeLine($"{i.name}: {i.description}");
                }
                else
                {
                    Console.WriteLine("That item does not exist in this location or your inventory.");
                    return;
                }
                return;
            }

            if (command.Contains("use ") && !command.Contains(" on "))
            {
                string itemname = command.Substring(4);
                Item item = player.inventory.Find(find => find.name == itemname);
                if (item != null)
                {
                    if (item is Armor)
                    {
                        UseArmor use = new UseArmor(player, item as Armor);
                        use.Perform();
                        player.inventory.Remove(item);
                    }
                    else
                    {
                        UseItem use = new UseItem(item, player, null);
                        use.Perform();
                    }
                }
                else
                {
                    TypeLine("That item doesn't exist.");
                }
                return;
            }
            else if (command.Contains("use ") && command.Contains(" on "))
            {
                string itemname = command.Substring(4, command.LastIndexOf(" on ") - 4);
                string entityname = command.Substring(command.LastIndexOf(" on ") + 4);
                Item item = player.inventory.Find(find => find.name == itemname);
                Entity target = player.position.entities.Find(entity => entity.name == entityname);
                if (item != null)
                {
                    if (item is Armor)
                    {
                        TypeLine("You can't put armor on another entity.");
                        return;
                    }
                    if (target != null)
                    {
                        UseItem use = new UseItem(item, target, player);
                        use.Perform();
                        if (use.success == false && use.hit == true)
                        {
                            TypeLine("Weapon wasn't in range.");
                        }
                        else if (use.success == false && use.hit == false)
                        {
                            TypeLine("You missed.");
                        }
                    }
                    else
                    {
                        TypeLine("That entity doesn't exist.");
                    }
                }
                else
                {
                    TypeLine("That item doesn't exist.");
                }
                return;
            }
            else if (command == "use")
            {
                TypeLine("Please specify which item you want to use.");
                return;
            }


            if (command.Length >= 7 && command.Substring(0, 7) == "pick up")
            {
                if (command == "pick up")
                {
                    TypeLine("Please specify what you would like to pick up.");
                    return;
                }
                else if (player.position.items.Exists(x => x.item.name == command.Substring(8)))
                {
                    ItemPosition pos = player.position.items.Find(item => item.item.name == command.Substring(8));
                    if (pos != null)
                    {
                        if (EXT.InRange(pos.coord, player.coord, 2, -1) == true)
                        {
                            incrementTurn = true;
                            player.inventory.Add(player.position.takeItem(command.Substring(8)));
                            TypeLine("You pick up the " + command.Substring(8) + ".");
                            player.position.removeItem(pos);
                            return;
                        } else
                        {
                            TypeLine("You can't reach that item.");
                            return;
                        }
                    }
                    this.Invalidate();
                    panel1.Invalidate();
                }
                else
                {
                    TypeLine(command.Substring(8) + " does not exist.");
                    return;
                }
            }

            if (command.Contains("go "))
            {
                if (MoveRoom(command.Substring(3)))
                {
                    incrementTurn = true;
                    this.Invalidate();
                    panel1.Invalidate();
                    return;
                }
                else
                {
                    TypeLine("There is no such direction.");
                    return;
                }
            }

            if (Interact(command))
            {
                return;
            }

            TypeLine("Invalid command, are you confused?");
        }

        /*
        class Position
        {
            public int X;
            public int Y;
            public int F;
            public int G;
            public int H;
            public Position parent;
        }
        

        static List<Position> GetWalkableAdjacentSquares(int x, int y, string[] map, List<Position> openList)
        {
            List<Position> list = new List<Position>();

            if (map[y - 1][x] == ' ' || map[y - 1][x] == 'B')
            {
                Position node = openList.Find(l => l.X == x && l.Y == y - 1);
                if (node == null) list.Add(new Position() { X = x, Y = y - 1 });
                else list.Add(node);
            }

            if (map[y + 1][x] == ' ' || map[y + 1][x] == 'B')
            {
                Position node = openList.Find(l => l.X == x && l.Y == y + 1);
                if (node == null) list.Add(new Position() { X = x, Y = y + 1 });
                else list.Add(node);
            }

            if (map[y][x - 1] == ' ' || map[y][x - 1] == 'B')
            {
                Position node = openList.Find(l => l.X == x - 1 && l.Y == y);
                if (node == null) list.Add(new Position() { X = x - 1, Y = y });
                else list.Add(node);
            }

            if (map[y][x + 1] == ' ' || map[y][x + 1] == 'B')
            {
                Position node = openList.Find(l => l.X == x + 1 && l.Y == y);
                if (node == null) list.Add(new Position() { X = x + 1, Y = y });
                else list.Add(node);
            }

            return list;
        }
        */

        static int ComputeHScore(int x, int y, int targetX, int targetY)
        {
            return Math.Abs(targetX - x) + Math.Abs(targetY - y);
        }

        public string[] StringifyMap(int2 one, int2 two)
        {
            int2 playerposition;
            List<int2> obstaclepositions = new List<int2>();
            List<string> map = new List<string>();
            foreach (Entity entity in player.position.entities)
            {
                if (entity.name != "player")
                {
                    obstaclepositions.Add(entity.coord);
                }
            }
            playerposition = player.coord;
            // Add a top border
            string topborder = "+";
            for (int t = 0; t < player.position.size.x; t++)
            {
                topborder += "-";
            }
            topborder += "+";
            map.Add(topborder);

            for (int y = 0; y < player.position.size.y; y++)
            {
                string row = "|";
                for (int x = 0; x < player.position.size.x; x++)
                {
                    bool obstacleposition = false;
                    foreach (int2 pos in obstaclepositions)
                    {
                        if (pos.Equals(new int2(x, y)))
                        {
                            obstacleposition = true;
                        }
                    }

                    if (playerposition.Equals(new int2(x, y))) {
                        row += "P";
                    } else if (obstacleposition == true)
                    {
                        row += "X";
                    } else if (one.Equals(new int2(x, y)))
                    {
                        row += "A";
                    } else if (two.Equals(new int2(x, y)))
                    {
                        row += "B";
                    }else
                    {
                        row += " ";
                    }
                }
                row += "|";
                map.Add(row);
            }

            // Add a bottom border
            string bottomborder = "+";
            for (int t = 0; t < player.position.size.x; t++)
            {
                bottomborder += "-";
            }
            bottomborder += "+";
            map.Add(bottomborder);
            string[] astarmap = map.ToArray();
            return astarmap;
        }

        public bool Interact(string command)
        {
            foreach (Interactable interactable in player.position.interactables)
            {
                if (command.Contains(interactable.name.ToLower()))
                {
                    string type = command.Substring(interactable.name.Length + 1);
                    Console.WriteLine(type);
                    player.position.interact(interactable.name, player, type);
                    return true;
                }
            }
            return false;
        }
        public bool MoveRoom(string command)
        {
            foreach (Exit exit in player.position.exits)
            {
                Location leads = null;
                int room = 0;
                int offset = 0;
                string dir = "";
                if (player.position.Equals(exit.room1))
                {
                    dir = exit.getShortDirection(1);
                    offset = exit.attachment2;
                    leads = exit.room2;
                    room = 1;
                }
                else if (player.position.Equals(exit.room2))
                {
                    dir = exit.getShortDirection(2);
                    offset = exit.attachment1;
                    leads = exit.room1;
                    room = 2;
                }
                if (command == exit.getDirection(room) || command == dir) //|| command == exit.getShortDirection().ToLower())
                {
                    player.Move(leads);
                    int2 topedge = new int2(offset, 0);
                    int2 rightedge = new int2(leads.size.x - 1, offset);
                    int2 bottomedge = new int2(offset, leads.size.y - 1);
                    int2 leftedge = new int2(0, offset);
                    if (dir == "n")
                    {
                        player.coord = bottomedge;
                    }
                    else if (dir == "e")
                    {
                        player.coord = leftedge;
                    }
                    else if (dir == "s")
                    {
                        player.coord = topedge;
                    }
                    else if (dir == "w")
                    {
                        player.coord = rightedge;
                    }
                    TypeLine($"You move {exit.getDirection(room)} to the: {leads.title}");
                    /*
                    player.Move(exit.leads);
                    player.position = exit.leads;
                    TypeLine("You move " + exit.ToString() + " to the:");
                    ShowLocation();
                    int2 topedge = new int2(exit.leads.size.x / 2, 0);
                    int2 rightedge = new int2(exit.leads.size.x - 1, exit.leads.size.y / 2);
                    int2 bottomedge = new int2(exit.leads.size.x / 2, exit.leads.size.y - 1);
                    int2 leftedge = new int2(0, exit.leads.size.y / 2);
                    if (command == "n" || command == "north")
                    {
                        player.coord = bottomedge;
                    } else if (command == "e" || command == "east")
                    {
                        player.coord = leftedge;
                    } else if (command == "s" || command == "south")
                    {
                        player.coord = topedge;
                    } else if (command == "w" || command == "west")
                    {
                        player.coord = rightedge;
                    }
                    return true;
                    */
                    return true;
                }
            }
            return false;
        }

        public void ShowInventory()
        {
            if (player.inventory.Count > 0)
            {
                TypeLine("A look in your inventory reveals the following:");

                foreach (Item item in player.inventory)
                {
                    TypeLine(item.name);
                }
            }
            else
            {
                TypeLine("Your inventory is empty.");
            }
        }

        public void ShowLocation()
        {
            TypeLine(player.position.title);
            TypeLine(player.position.description);

            if (player.position.items.Count > 0)
            {
                TypeLine("The room contains the following:");

                for (int i = 0; i < player.position.items.Count; i++)
                {
                    TypeLine(player.position.items[i].item.name);
                }
            }

            TypeLine("Available exits:");

            for (int i = 0; i < player.position.exits.Count; i++)
            {
                if (player.position.Equals(player.position.exits[i].room1))
                {
                    TypeLine(player.position.exits[i].getDirection(1));
                }
                else if (player.position.Equals(player.position.exits[i].room1))
                {
                    TypeLine(player.position.exits[i].getDirection(2));
                }
            }

            TypeLine("Objects of interest:");

            for (int i = 0; i < player.position.interactables.Count; i++)
            {
                TypeLine(player.position.interactables[i].name);
            }

            TypeLine("Entities in the room:");

            for (int i = 0; i < player.position.entities.Count; i++)
            {
                TypeLine(player.position.entities[i].name);
            }
        }

        /*
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13) // if Enter is pressed
            {
                // exec the command
                try
                {
                    if (textBox1.Lines[Line] == "" & Line == 0)
                    {
                        WriteLine("Empty Line!"); // Just for signalling
                    }
                    else
                    {
                        // Line = 0 (for the first time)
                        if (textBox1.Lines[Line] == "Help" || textBox1.Lines[Line] == "help" || textBox1.Lines[Line] == "HELP")
                        {
                            // Exec the help command
                            LineLOC(); // Add a line for the Showing Text
                            // Add a text into the textbox1
                            textBox1.Text = textBox1.Text + Environment.NewLine + "CLOSE -- Closing the app!";
                            LineLOC(); // Add a line for the next user input
                            // ---- Set the cursor at the last letter ----
                            textBox1.SelectionStart = textBox1.Text.Length;
                            textBox1.SelectionLength = 0;
                            // -------------------------------------------
                        }
                        else
                        {
                            if (textBox1.Lines[Line] == "close" || textBox1.Lines[Line] == "close")
                            {
                                //Exec the close command
                                Close();
                            }
                            else
                            {
                                // here another command
                                // WARNING: You must call LineLOC method in the last function (ex. close) in the else brackets
                                LineLOC();
                            }
                        }
                    }
                }
                catch
                {
                    // There will be error if you dont fill the first command line with any text
                    // So I put here a "catch"
                    LineLOC();
                }
            }
        }
        */

        private void LineLOC() // Line Location Method
        {
            Line++;
            if (Line < 17)
            {
                // Sets up the location of the prompt (">")
                // You can change the label1 Text when you are in different folders
                label1.Location = new Point(label1.Location.X, label1.Location.Y + 16);
                input.Location = new Point(input.Location.X, input.Location.Y + 16);
            }
        }

        /*
        private void textBox1_MouseHover(object sender, EventArgs e)
        {
            // THERE IS SOME TIME DELAY IN WHICH YOU CAN TRY TO CHANGE SOMETHING
            // IF YOU KNOW HOW TO FIX IT OR YOU HAVE ALTERNATIVE WAY OF PREVENTING
            // YOU SHOULD WRITE TO MY EMAIL : mitkonikov01@gmail.com
            textBox1.Enabled = false; // To prevent changing the words
            // ---- Set the cursor at the last letter ----
            textBox1.SelectionStart = textBox1.Text.Length;
            textBox1.SelectionLength = 0;
            // -------------------------------------------
            label1.Text = ""; //It hides the label text (instead of label1.Hide();)
        }
        */

        /*
        private void pictureBox1_MouseHover(object sender, EventArgs e)
        {
            textBox1.Enabled = true;
            textBox1.Focus();
            label1.Text = ">";
            textBox1.SelectionStart = textBox1.Text.Length;
            textBox1.SelectionLength = 0;
        }
        */

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up) //Keys.Up is the Upper Arrow
            {
                UpTimer1.Start();
            }
        }

        private void UpTimer1_Tick(object sender, EventArgs e)
        {
            // THERE MUST BE SOME DELAY BECAUSE IT WILL RUN THE "SET UP CURSOR"
            // FIRST BEFORE THE UPPER KEY IS REGISTERED IN THE TEXTBOX
            // ---- Set the cursor at the last letter ----
            textBox1.SelectionStart = textBox1.Text.Length;
            textBox1.SelectionLength = 0;
            // -------------------------------------------
            UpTimer1.Stop();
        }

        private void Dec1()
        {
            // JUST FOR DECORATION
            Bitmap b1;
            Graphics g1;
            b1 = new Bitmap(102, 102);
            g1 = Graphics.FromImage(b1);
            g1.DrawEllipse(new Pen(Color.FromArgb(107, 173, 246), 2f), 0, 0, 100, 100);//-------------------
            g1.DrawEllipse(new Pen(Color.FromArgb(107, 173, 246), 3f), 20, 20, 60, 60);//Draws some eclipses
            g1.DrawEllipse(new Pen(Color.FromArgb(107, 173, 246), 4f), 40, 40, 20, 20);//-------------------
            pictureBox1.Image = b1;
            g1.Dispose();
        }

        private void Main_Load(object sender, EventArgs e)
        {

        }

        private void Input_TextChanged(object sender, EventArgs e)
        {

        }

        private void Panel1_MouseMove(object sender, MouseEventArgs e)
        {
            using (System.Drawing.Drawing2D.GraphicsPath gPath = new System.Drawing.Drawing2D.GraphicsPath())
            {
                

            }
        }

        private void Panel1_Paint(object sender, PaintEventArgs e)
        {
            /*
            SolidBrush brush = new SolidBrush(Color.Red);
            Pen pen = new Pen(Color.Black, 1f);

            // Create array of points that define lines to draw.
            Point[] points = {
                 new Point(300, 300),
                 new Point(300, 250),
                 new Point(400, 200),
                 new Point(400, 250),

                 new Point(300, 300),
                 new Point(300, 250),
                 new Point(200, 200),
                 new Point(200, 250),

                 new Point(300, 300),
                 new Point(300, 250),
                 new Point(400, 200),
                 new Point(300, 150),

                 new Point(200, 200),
                 new Point(300, 250)
            };

            //Draw lines to screen.
            //e.Graphics.FillPolygon(brush, points);
            //e.Graphics.DrawLines(pen, points);
            */
            e.Graphics.TranslateTransform(275, 175);            
            Pen pen = new Pen(Color.White, 2);
            Pen redpen = new Pen(Color.Red, 2);
            Pen yellowpen = new Pen(Color.Yellow, 2);
            Pen orangepen = new Pen(Color.Orange, 2);
            Pen greenpen = new Pen(Color.Green, 2);
            List<int2> entitypositions = new List<int2>();
            List<int2> itempositions = new List<int2>();
            
            foreach (Entity entity in player.position.entities)
            {
                entitypositions.Add(entity.coord);
            }
            foreach (ItemPosition item in player.position.items)
            {
                itempositions.Add(item.coord);
            }

            for (int y = -player.position.size.y / 2; y < player.position.size.y / 2; y++)
            {
                for (int x = -player.position.size.x / 2; x < player.position.size.x / 2; x++)
                {
                    int realY = y + player.position.size.y / 2;
                    int realX = x + player.position.size.x / 2;
                    bool entityposition = false;

                    foreach (int2 position in Path)
                    {
                        if (position.x == realX && position.y == realY)
                        {
                            e.Graphics.DrawRectangle(orangepen, new Rectangle(new Point(x * 25, y * 25), new Size(20, 20)));
                            entityposition = true;
                            break;
                        }
                    }

                    foreach (int2 position in itempositions)
                    {
                        if (position.x == realX && position.y == realY)
                        {
                            e.Graphics.DrawRectangle(greenpen, new Rectangle(new Point(x * 25, y * 25), new Size(20, 20)));
                            entityposition = true;
                            break;
                        }
                        //Console.WriteLine(position.item.name);
                        //Console.WriteLine(player.position.items.Count);
                    }

                    foreach (int2 position in entitypositions)
                    {
                        if (position.x == realX && position.y == realY)
                        {
                            if (position.x == player.coord.x && position.y == player.coord.y)
                            {
                                e.Graphics.DrawRectangle(redpen, new Rectangle(new Point(x * 25, y * 25), new Size(20, 20)));
                                entityposition = true;
                                break;
                            }
                            else
                            {
                                e.Graphics.DrawRectangle(yellowpen, new Rectangle(new Point(x * 25, y * 25), new Size(20, 20)));
                                entityposition = true;
                                break;
                            }
                        }
                    }

                    if (entityposition == false)
                    {
                        e.Graphics.DrawRectangle(pen, new Rectangle(new Point(x * 25, y * 25), new Size(20, 20)));
                    }
                }
            }

            return;
            
        }

        /*
        private void Write_Tick(object sender, EventArgs e)
        {
            if (i < write.Length)
            {
                textBox1.Text += write[i];
                i++;
            } else
            {
                typeout.Enabled = false;
            }
        }
        */
    }
}
