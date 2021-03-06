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
using System.Drawing.Drawing2D;
using System.Collections;

// Dedicated to jasper the cat, who died
// on feburary 25th, 2019
namespace Something
{
    public partial class Main : Form
    {
        int Line = 0;
        string TextQueue = "";

        public Queue<TextSequence> textsequences = new Queue<TextSequence>();
        public World currentworld;
        public List<int2> blockedcells;
        public RoyT.AStar.Grid grid;
        public Player player;
        public Graphics graphics;
        public int lastturn;
        
        bool gameOver = false;
        bool incrementTurn = false;
        bool playerMovement = false;
        bool textSequence = false;
        bool blockMap = false;

        public List<int2> Path = new List<int2>();
        public List<Rectangle> Rects = new List<Rectangle>();

        public void RefreshDrawings()
        {
            Invoke((MethodInvoker) delegate { this.Invalidate(); panel1.Invalidate(); });
        }

        public void DisableInput()
        {
            Invoke((MethodInvoker)delegate { this.input.ReadOnly = true; });
        }

        public void EnableInput()
        {
            Invoke((MethodInvoker)delegate { this.input.ReadOnly = false; });
        }

        public void InitilizeGame()
        {
            player = new Player()
            {
                name = "player",
                coord = new int2(4, 4),
                armor = 0,
                health = 50,
                speed = 8,
                strength = 10,
                dexterity = 10,
                constitution = 10,
                intelligence = 10,
                wisdom = 10,
                charisma = 10,
                Attack = 10,
                Defense = 10,
                Accuracy = 10,
                movement = 5,
                size = Defaults.Sizes.Normal,
                coloring = Color.Red,
                level = 0
            };
            player.abilities.Add(new BeatdownExplosion(player, "boom", 100, 5));
        }
        
        public void BuildTestMap()
        {
            //TakimaPirateShip start = new TakimaPirateShip();
            //EscapeShip start = new EscapeShip();
            Testing start = new Testing();
            start.AddPlayer(player);
            start.Setup();
            currentworld = start;
        }

        public Main()
        {
            InitilizeGame();
            InitializeComponent();
            Dec1();
            input.Focus();
            LineLOC();
            DoubleBuffered = true;
            blockedcells = new List<int2>();
            
            GameVariables.ChangeTurn += UpdateGame;
            new Thread(() =>
            {
                System.Timers.Timer trigger = new System.Timers.Timer(50);
                trigger.AutoReset = true;
                int level = 0;
                List<string> currentTags = new List<string>();
                Func<int, string, int> FindSecondBracket = (index, expression) =>
                {
                    Stack<int> st = new Stack<int>();

                    int i = 0;
                    for (i = index; i < expression.Length; i++)
                    {
                        if (expression[i] == '<')
                        {
                            st.Push((int)expression[i]);
                        }
                        else if (expression[i] == '>')
                        {
                            st.Pop();
                            if (st.Count <= 0)
                            {
                                return i;
                            }
                        }
                        
                    }
                    // if there is no other bracket return -1 because this is really bad
                    return -1;
                };

                Func<string, string, int2, int2> FindSecondTag = (expression, tag, points) =>
                {
                    Stack<int2> st = new Stack<int2>();
                    st.Push(points);

                    int i = 0;
                    for (i = points.y; i < expression.Length; i++)
                    {
                        if (expression[i] == '<')
                        {
                            int end = FindSecondBracket(i, expression);
                            string area = expression.Substring(i + 1, end - i - 1);
                            string type = area.Split(' ')[0];
                            if (type == "/" + tag)
                            {
                                st.Pop();
                                if (st.Count <= 0)
                                {
                                    return new int2(i, end);
                                }
                            } else if (type == tag)
                            {
                                st.Push(new int2(i, end));
                            }
                            //{
                            //    st.Push(new int2(i, end));
                            //}
                            //string type = expression.Substring(i + 1, end - 2).Split(' ')[0];
                            //return new int2(i, end);
                        }
                    }

                    return new int2(-1, -1);
                };

                trigger.Elapsed += (object source, ElapsedEventArgs e) =>
                {
                    if (TextQueue.Length != 0)
                    {
                        char c = TextQueue[0];
                        bool foundBracket = false;
                        List<int2> tags = new List<int2>();
                        textBox1.Invoke((MethodInvoker)delegate {
                            if (c == '<')
                            {
                                // we want to find the other end of the tag
                                int secondBracket = FindSecondBracket(0, TextQueue);
                                string area = TextQueue.Substring(1, secondBracket - 1); // without the brackets around it
                                foundBracket = true;
                                // now we want to find the other tag and then we put them
                                // together as the int2's they are which is basically the
                                // perfect class for them of course
                                string type = area.Split(' ')[0];
                                int2 openingTag = new int2(0, secondBracket); // the opening tag
                                int2 closingTag = FindSecondTag(TextQueue, type, openingTag);
                                
                                // basically what were going to do with this
                                // is make a tag class that knows what the tag
                                // is supposed to do because of the limited
                                // tag types and then do that to all text within
                                // the range of the two tags
                            }

                            if (foundBracket == false)
                            {
                                textBox1.AppendText(c.ToString());
                            }

                        });

                        if (foundBracket == false)
                        {
                            TextQueue = TextQueue.Remove(0, 1);
                        }
                        Thread.Sleep(45);
                    }
                    if (gameOver == true)
                    {
                        trigger.Stop();
                        trigger.Dispose();
                    }
                };
                trigger.Start();
            }).Start();
            BuildTestMap();
            RegenerateGrid();
            if (currentworld.startup != null)
            {
                currentworld.startup.Invoke(this);
                HandleMaker();
            }
        }

        public async void HandleMaker()
        {
            while (!GameHandleCreated())
            {
                Console.WriteLine("waiting for handle to make");
                await Task.Delay(25);
            }
        }

        public bool GameHandleCreated()
        {
            try
            {
                Invoke((MethodInvoker)delegate { Console.WriteLine(); });
                return true;
            } catch
            {
                return false;
            }
        }

        public void UpdateGame()
        {
            incrementTurn = false;
            
            if (textsequences.Count > 0)
            {
                DisableInput();
                TextSequence sequence = textsequences.Dequeue();
                TypeLine(sequence.text);
                textSequence = true;
                if (sequence.disablemap == true)
                {
                    blockMap = true;
                    RefreshDrawings();
                }
                Task.Delay((sequence.text.Length * 50) + 1000).ContinueWith((continuation) => {
                    EnableInput();
                    textSequence = false;
                    Console.WriteLine("text sequence complete");
                    if (sequence.disablemap == true)
                    {
                        blockMap = false;
                        RefreshDrawings();
                    }
                });
                //Thread.Sleep(1);
                //input.ReadOnly = false;
            }

            if (textSequence == false)
            {
                Console.WriteLine($"turn {GameVariables.turns}");
                lastturn = GameVariables.turns;
                List<Entity> dead = new List<Entity>();

                int level = Convert.ToInt32((double)Math.Floor((Math.Sqrt(((((player.experience * 2) + 25) * 100) + 50) / 100))));
                if (level == 1)
                {
                    player.level = level;
                    TypeLine($"You leveled up! your new level is {level}");
                    player.Attack += (5 * level);
                    player.Defense += (5 * level);
                    player.speed = 8 + Convert.ToInt32(Math.Floor((double)level / 4));
                    player.Accuracy = 10 + Convert.ToInt32(Math.Floor((double)level / 4));
                }

                foreach (Entity entity in player.position.entities)
                {
                    if (entity != null)
                    {
                        entity.Update();
                        if (entity.dead == true)
                        {
                            dead.Add(entity);
                        }
                    }
                }

                foreach (Entity nolongeralive in dead)
                {
                    player.position.removeEntity(nolongeralive);
                    Invoke((MethodInvoker)delegate { TypeLine($"{nolongeralive.name} has died."); });
                    if (nolongeralive is Creature)
                    {
                        Creature deadcreature = nolongeralive as Creature;
                        if (deadcreature.loot != null && deadcreature.loot.Count > 0)
                        {
                            Random rng = new Random();
                            foreach (Item loot in deadcreature.loot)
                            {
                                deadcreature.position.addItem(new ItemPosition(loot, EXT.GetDirection(deadcreature.coord, rng.Next(1, 9), rng.Next(0, 3))));
                            }
                        }

                        deadcreature.position.addTrigger(new WorldTrigger(deadcreature.coord, (triggerer, location, trigg) =>
                        {
                            if (triggerer is Player)
                            {
                                Player p = triggerer as Player;
                                p.experience += deadcreature.experience;
                            }
                            location.removeTrigger((WorldTrigger)trigg);
                        }, deadcreature.position));
                    }
                }

                dead.Clear();
                playerMovement = false;
                Console.WriteLine("game updated");
                foreach (int2 cell in blockedcells)
                {
                    grid.UnblockCell(new RoyT.AStar.Position(cell.x, cell.y));
                }
                blockedcells.Clear();
                List<int2> entitypositions = new List<int2>();
                foreach (Entity entity in player.position.entities)
                {
                    //grid.BlockCell(new RoyT.AStar.Position(entity.coord.x, entity.coord.y));
                    //blockedcells.Add(entity.coord);
                    for (int y = entity.coord.y; y < entity.coord.y + entity.size.y; y++)
                    {
                        for (int x = entity.coord.x; x < entity.coord.x + entity.size.x; x++)
                        {
                            entitypositions.Add(new int2(x, y));
                        }
                    }
                }

                foreach (int2 i in entitypositions)
                {
                    grid.BlockCell(new RoyT.AStar.Position(i.x, i.y));
                    blockedcells.Add(i);
                }

                UpdateMap();
                GameVariables.occupiedpaths.Clear();

                if (player.health == 0)
                {
                    gameOver = true;
                }
            }
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
            if (e.KeyChar == (char)13 && gameOver == false)
            {
                e.Handled = true;
                string command = input.Text;
                DoAction(command);
                if (incrementTurn == true)
                {
                    ++GameVariables.turns;
                    incrementTurn = false;
                    Thread.Sleep(10);
                }
                Console.WriteLine("command was entered");
                input.ResetText();
            }
        }

        public void UpdateMap()
        {
            //for (int y = 0; y < player.position.size.y; y++)
            //{
            //    for (int x = 0; x < player.position.size.x; x++)
            //    {
            foreach (Entity entity in player.position.entities)
            {
                foreach (Entity entity2 in player.position.entities)
                {
                    if (entity2 != entity)
                    {
                        List<int2> pos1 = new List<int2>();
                        List<int2> pos2 = new List<int2>();

                        for (int y = entity.coord.y; y < entity.coord.y + entity.size.y; y++)
                        {
                            for (int x = entity.coord.x; x < entity.coord.x + entity.size.x; x++)
                            {
                                pos1.Add(new int2(x, y));
                            }
                        }

                        for (int y = entity2.coord.y; y < entity2.coord.y + entity2.size.y; y++)
                        {
                            for (int x = entity2.coord.x; x < entity2.coord.x + entity2.size.x; x++)
                            {
                                pos2.Add(new int2(x, y));
                            }
                        }

                        bool touching = false;
                        foreach (int2 position1 in pos1)
                        {
                            foreach (int2 position2 in pos2)
                            {
                                if (position1.Equals(position2))
                                {
                                    touching = true;
                                }
                            }
                        }

                        if (touching == true)
                        {
                            Random rng = new Random();
                            int bump = rng.Next(0, 2);
                            int dir = rng.Next(1, 9);
                            while (touching == true)
                            {
                                if (bump == 0)
                                {
                                    entity.coord = EXT.GetDirection(entity.coord, dir);
                                    for (int i = 0; i < pos1.Count; i++)
                                    {
                                        pos1[i] = EXT.GetDirection(pos1[i], dir);
                                    }
                                }
                                if (bump == 1)
                                {
                                    entity2.coord = EXT.GetDirection(entity2.coord, dir);
                                    for (int i = 0; i < pos2.Count; i++)
                                    {
                                        pos2[i] = EXT.GetDirection(pos2[i], dir);
                                    }
                                }

                                bool stilltouching = false;
                                foreach (int2 position1 in pos1)
                                {
                                    foreach (int2 position2 in pos2)
                                    {
                                        if (position1.Equals(position2))
                                        {
                                            stilltouching = true;
                                        }
                                    }
                                }
                                touching = stilltouching;
                            }
                        }

                        /*
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
                        */
                    }
                }

                foreach (WorldTrigger trigger in player.position.worldtriggers)
                {
                    if (entity.coord == trigger.coord)
                    {
                        trigger.Invoke(entity);
                    }
                }
            }
            //        }
            //    }           

            this.Invoke((MethodInvoker)delegate { this.Invalidate(); panel1.Invalidate(); });
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

            if (command == "test")
            {
                TextSequence sequence = new TextSequence("testing text sequence", true);
                textsequences.Enqueue(sequence);
                incrementTurn = true;
                return;
            }

            if (command.Contains("talk to "))
            {
                string name = command.Substring(8);
                Entity talk = player.position.entities.Find(entity => entity.name == name);
                if (talk is Communicator)
                {
                    Communicator comm = talk as Communicator;
                    if (comm.dialouge != null)
                    {
                        comm.InitiateDialouge(player);
                        player.communicator = comm;
                    } else
                    {
                        throw new NullReferenceException($"dialouge is null");
                    }
                }
                return;
            }

            if (command.Contains("talk "))
            {
                string value = command.Substring(5);
                int number = 0;
                bool parse = int.TryParse(value, out number);
                if (parse == true)
                {
                    if (player.communicator != null)
                    {
                        player.communicator.DialougeOption(number - 1);
                        
                    } else
                    {
                        TypeLine("You aren't talking to anyone");
                    }
                } else
                {
                    TypeLine("That isn't a number");
                }
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
                TypeLine($"Removed armor: {player.attachedarmor.name}");
                int soak = player.attachedarmor.soak;
                int attack = player.attachedarmor.attack;
                int defense = player.attachedarmor.defense;           
                player.inventory.Add(player.attachedarmor);
                player.attachedarmor = null;
                player.armor -= soak;
                player.Attack -= attack;
                player.Defense -= defense;
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
                List<WorldTrigger> triggers = player.position.worldtriggers;
                if (!movement.Contains(','))
                {
                    int amount = int.Parse(command[5].ToString());
                    string m = command.Substring(7);
                    int2 to = new int2();
                    if (amount <= player.movement)
                    {
                        for (int i = 0; i < amount; i++)
                        {
                            if (m == "north" || m == "n" || m == "1")
                            {
                                to = EXT.GetDirection(player.coord, 1);
                            }
                            else if (m == "northeast" || m == "ne" || m == "2")
                            {
                                to = EXT.GetDirection(player.coord, 2);
                            }
                            else if (m == "east" || m == "e" || m == "3")
                            {
                                to = EXT.GetDirection(player.coord, 3);
                            }
                            else if (m == "southeast" || m == "se" || m == "4")
                            {
                                to = EXT.GetDirection(player.coord, 4);
                            }
                            else if (m == "south" || m == "s" || m == "5")
                            {
                                to = EXT.GetDirection(player.coord, 5);
                            }
                            else if (m == "southwest" || m == "sw" || m == "6")
                            {
                                to = EXT.GetDirection(player.coord, 6);
                            }
                            else if (m == "west" || m == "w" || m == "7")
                            {
                                to = EXT.GetDirection(player.coord, 7);
                            }
                            else if (m == "northwest" || m == "nw" || m == "8")
                            {
                                to = EXT.GetDirection(player.coord, 8);
                            }
                            if ((to.x > 0 && to.x < player.position.size.x) && (to.y > 0 && to.y < player.position.size.y) && player.position.entities.Find(location => location.coord == to) == null)
                            {
                                player.coord = to;
                                this.Invalidate();
                                panel1.Invalidate();
                                foreach (WorldTrigger trigger in triggers)
                                {
                                    if (player.coord == trigger.coord)
                                    {
                                        trigger.Invoke(player);
                                        if (trigger.stop == true)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                TypeLine("That position is out of bounds");
                                return;
                            }
                        }
                        TypeLine($"Player moved to: {to.ToString()}");
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
                if (player.attachedarmor != null)
                {
                    TypeLine($"Attached Armor: {player.attachedarmor.name}");
                } else
                {
                    TypeLine($"Attached Armor: None");
                }
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
                    TypeLine($"Position: {e.coord.ToString()}");
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
                TypeLine("All ability caused effects currently on your player:");
                foreach (Ability ability in player.triggered)
                {
                    TypeLine($"{ability.name} which will last for {ability.remaining} more turns");
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

            if (command.Length >= 8 && command.Contains("look at "))
            {
                Item i = null;
                string look = command.ToLower().Substring(8);
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
                    TypeLine("That item does not exist in this location or your inventory.");
                    return;
                }
                return;
            } else if (command == "look at")
            {
                TypeLine("Please specify what you wish to look at");
                return;
            }

            if (command.Contains("trigger ") && command.Contains(" on "))
            {
                string ability = command.Substring(8, command.LastIndexOf(" on ") - 8);
                string names = command.Substring(command.LastIndexOf(" on ") + 4);
                List<Entity> selectedtargets = new List<Entity>();
                string[] targets = names.Split(',');
                if (targets[0] == "all")
                {
                    selectedtargets = player.position.entities;
                }
                else
                {
                    foreach (string target in targets)
                    {
                        Entity e = player.position.entities.Find(item => item.name == target);
                        if (e != null)
                        {
                            selectedtargets.Add(e);
                        }
                        else
                        {
                            TypeLine($"Entity {target} doesn't exist");
                            return;
                        }
                    }
                }
                Ability trigger = player.abilities.Find(item => item.name == ability);
                if (trigger != null)
                {
                    if (selectedtargets.Count <= trigger.maxtargets)
                    {
                        incrementTurn = true;
                        trigger.Trigger(selectedtargets);
                        TypeLine($"Triggered ability {trigger.name} on {names}");
                    }
                    else
                    {
                        TypeLine("You can't select that many targets at once");
                    }
                }
                else
                {
                    TypeLine($"The ability {ability} doesn't exist");
                }
                return;
            }
            else if (command.Contains("trigger ") && !command.Contains(" on "))
            {
                string ability = command.Substring(8);
                Ability trigger = player.abilities.Find(item => item.name == ability);
                if (trigger != null)
                {
                    incrementTurn = true;
                    trigger.Trigger(new List<Entity>() { player });
                    TypeLine($"Used ability {trigger.name} on player");
                    if (trigger.duration > 0)
                    {
                        trigger.remaining = trigger.duration;
                        player.triggered.Add(trigger);
                    }
                } else
                {
                    TypeLine($"The ability {ability} doesn't exist");
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
                    TypeLine("That item doesn't exist");
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
                        TypeLine("You can't put armor on another entity");
                        return;
                    }
                    if (target != null)
                    {
                        UseItem use = new UseItem(item, target, player);
                        use.Perform();
                        if (use.success == false && use.hit == true)
                        {
                            TypeLine("Weapon wasn't in range");
                        }
                        else if (use.success == false && use.hit == false)
                        {
                            TypeLine("You missed");
                        }
                    }
                    else
                    {
                        TypeLine("That entity doesn't exist");
                    }
                }
                else
                {
                    TypeLine("That item doesn't exist");
                }
                return;
            }
            else if (command == "use")
            {
                TypeLine("Please specify which item you want to use");
                return;
            }


            if (command.Length >= 7 && command.Substring(0, 7) == "pick up")
            {
                if (command == "pick up")
                {
                    TypeLine("Please specify what you would like to pick up");
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
                            TypeLine($"You pick up the {command.Substring(8)}");
                            player.position.removeItem(pos);
                            return;
                        } else
                        {
                            TypeLine("You can't reach that item");
                            return;
                        }
                    }
                    this.Invalidate();
                    panel1.Invalidate();
                }
                else
                {
                    TypeLine(command.Substring(8) + " does not exist");
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
                    TypeLine("There is no such direction");
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
                List<int2> obstacles = new List<int2>();

                foreach (Entity entity in player.position.entities)
                {
                    if (entity.name != "player")
                    {
                        for (int y = 0; y < entity.size.y; y++)
                        {
                            for (int x = 0; x < entity.size.y; x++)
                            {
                                obstacles.Add(new int2(entity.coord.x + x, entity.coord.y + y));
                            }
                        }
                    }
                }


                string[] words = interactable.name.Split(' ', '-');
                foreach (string word in words)
                {
                    if (command.Contains(word) && word.Length != 1 && EXT.InRange(interactable.coord, player.coord, 1))
                    {
                        if (command.Length > word.Length + 1)
                        {
                            string type = command.Substring(word.Length + 1);
                            Console.WriteLine(type);
                            player.position.interact(interactable.name, player, type);
                            return true;
                        }
                        else
                        {
                            TypeLine($"Please specifiy what you would like to do to {interactable.name}");
                            TypeLine("Options are");
                            foreach (KeyValuePair<int, InteractableAction> action in interactable.interactableActions)
                            {
                                TypeLine($"{action.Value.name}");
                            }
                            return true;
                        }
                    }
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
                int offsetother = 0;
                string dir = "";
                if (player.position.Equals(exit.room1))
                {
                    dir = exit.getShortDirection(1);
                    offset = exit.attachment2;
                    offsetother = exit.attachment1;
                    leads = exit.room2;
                    room = 1;
                }
                else if (player.position.Equals(exit.room2))
                {
                    dir = exit.getShortDirection(2);
                    offset = exit.attachment1;
                    offsetother = exit.attachment2;
                    leads = exit.room1;
                    room = 2;
                }
                if (command == exit.getDirection(room) || command == dir) //|| command == exit.getShortDirection().ToLower())
                {
                    player.Move(leads);
                    currentworld.Move(leads);
                    //int2 topedge = new int2(offset, 0);
                    //int2 rightedge = new int2(leads.size.x - 1, offset);
                    //int2 bottomedge = new int2(offset, leads.size.y - 1);
                    //int2 leftedge = new int2(0, offset);
                    int2 exitx = new int2();
                    int2 exity = new int2();
                    if (dir == "n")
                    {
                        if (player.coord.x < offsetother)
                        {
                            player.coord = new int2(offset, leads.size.y - 1);
                        }
                        else if (player.coord.x > offsetother + (exit.size - 1))
                        {
                            player.coord = new int2(offset + (exit.size - 1), leads.size.y - 1);
                        }
                        else
                        {
                            for (int i = 0; i < exit.size; i++)
                            {
                                if (player.coord.x == offsetother + i)
                                {
                                    player.coord = new int2(offset + i, leads.size.y - 1);
                                }
                            }
                        }
                    }
                    else if (dir == "e")
                    {
                        if (player.coord.y < offsetother)
                        {
                            player.coord = new int2(0, offset);
                        }
                        else if (player.coord.y > offsetother + (exit.size - 1))
                        {
                            player.coord = new int2(0, offset + (exit.size - 1));
                        }
                        else
                        {
                            for (int i = 0; i < exit.size; i++)
                            {
                                if (player.coord.y == offsetother + i)
                                {
                                    player.coord = new int2(0, offset + i);
                                }
                            }
                        }
                    }
                    else if (dir == "s")
                    {
                        if (player.coord.x < offsetother)
                        {
                            player.coord = new int2(offset, 0);
                        }
                        else if (player.coord.x > offsetother + (exit.size - 1))
                        {
                            player.coord = new int2(offset + (exit.size - 1), 0);
                        }
                        else
                        {
                            for (int i = 0; i < exit.size; i++)
                            {
                                if (player.coord.x == offsetother + i)
                                {
                                    player.coord = new int2(offset + i, 0);
                                }
                            }
                        }
                    }
                    else if (dir == "w")
                    {
                        if (player.coord.y < offsetother)
                        {
                            player.coord = new int2(leads.size.x - 1, offset);
                        }
                        else if (player.coord.y > offsetother + (exit.size - 1))
                        {
                            player.coord = new int2(leads.size.x - 1, offset + (exit.size - 1));
                        }
                        else
                        {
                            for (int i = 0; i < exit.size; i++)
                            {
                                if (player.coord.y == offsetother + i)
                                {
                                    player.coord = new int2(leads.size.x - 1, offset + i);
                                }
                            }
                        }
                    }
                    //else if (dir == "s")
                    //{
                    //    player.coord = topedge;
                    //}
                    //else if (dir == "w")
                    //{
                    //    player.coord = rightedge;
                    //}
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
                TypeLine("Your inventory is empty");
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
                if (player.position.entities[i].appear == true)
                {
                    TypeLine(player.position.entities[i].name);
                }
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

            List<Pixel> pixels = new List<Pixel>()
            {
                new Pixel(Color.Red, new int2(0, 0)), new Pixel(Color.Black, new int2(1, 0)),
                new Pixel(Color.Black, new int2(0, 1)), new Pixel(Color.Red, new int2(1, 1))
            };

            

            if (blockMap == false)
            {
                e.Graphics.TranslateTransform(275 - currentworld.current.offset.x, 175 - currentworld.current.offset.y);
                Pen pen = new Pen(Color.White, 2);
                Pen redpen = new Pen(Color.Red, 2);
                Pen yellowpen = new Pen(Color.Yellow, 2);
                Pen orangepen = new Pen(Color.Orange, 2);
                Pen greenpen = new Pen(Color.Green, 2);
                Pen firebrickpen = new Pen(Color.Firebrick, 2);
                Pen pinkpen = new Pen(Color.HotPink, 2);

                foreach (Location loc in currentworld.locations)
                {
                    List<KeyValuePair<int2, Color>> entitypositions = new List<KeyValuePair<int2, Color>>();
                    List<int2> itempositions = new List<int2>();
                    List<int2> doorsxy = new List<int2>();
                    List<int2> interactablesxy = new List<int2>();


                    foreach (Entity entity in loc.entities)
                    {
                        for (int y = entity.coord.y; y < entity.coord.y + entity.size.y; y++)
                        {
                            for (int x = entity.coord.x; x < entity.coord.x + entity.size.x; x++)
                            {
                                entitypositions.Add(new KeyValuePair<int2, Color>(new int2(x, y), entity.coloring));
                            }
                        }
                    }

                    foreach (ItemPosition item in loc.items)
                    {
                        itempositions.Add(item.coord);
                    }

                    foreach (Exit exit in loc.exits)
                    {
                        Exit.Directions dir = Exit.Directions.Undefined;
                        int attachment = 0;
                        if (loc == exit.room1)
                        {
                            dir = exit.direction1;
                            attachment = exit.attachment1;
                        }
                        else if (loc == exit.room2)
                        {
                            dir = exit.direction2;
                            attachment = exit.attachment2;
                        }

                        if (exit.room1.exits.Contains(exit) && exit.room2.exits.Contains(exit))
                        {
                            if (dir == Exit.Directions.North)
                            {
                                for (int i = 0; i < exit.size; i++)
                                {
                                    doorsxy.Add(new int2(attachment + i, -1));
                                }
                            }
                            else if (dir == Exit.Directions.East)
                            {
                                for (int i = 0; i < exit.size; i++)
                                {
                                    doorsxy.Add(new int2(loc.size.x, attachment + i));
                                }
                            }
                            else if (dir == Exit.Directions.South)
                            {
                                for (int i = 0; i < exit.size; i++)
                                {
                                    doorsxy.Add(new int2(attachment + i, loc.size.y));
                                }
                            }
                            else if (dir == Exit.Directions.West)
                            {
                                for (int i = 0; i < exit.size; i++)
                                {
                                    doorsxy.Add(new int2(-1, attachment + i));
                                }
                            }
                        }
                    }

                    foreach (Interactable interactable in loc.interactables)
                    {
                        for (int y = interactable.coord.y; y < interactable.coord.y + interactable.size.y; y++)
                        {
                            for (int x = interactable.coord.x; x < interactable.coord.x + interactable.size.x; x++)
                            {
                                interactablesxy.Add(new int2(x, y));
                            }
                        }
                    }

                    int2 add = new int2(0, 0);
                    if (loc.size.y % 2 != 0)
                        add.y = 1;
                    if (loc.size.x % 2 != 0)
                        add.x = 1;

                    for (int y = -loc.size.y / 2; y < (loc.size.y / 2) + add.y; y++)
                    {
                        for (int x = -loc.size.x / 2; x < (loc.size.x / 2) + add.x; x++)
                        {
                            int realY = y + loc.size.y / 2;
                            int realX = x + loc.size.x / 2;
                            bool entityposition = false;

                            Rectangle Rect = new Rectangle(new Point(x * 25, y * 25), new Size(20, 20));
                            Rect.Offset(loc.offset.x - (add.x * 25), loc.offset.y - (add.y * 25));

                            /*
                            foreach (int2 position in Path)
                            {
                                if (position.x == realX && position.y == realY)
                                {
                                    e.Graphics.DrawRectangle(orangepen, Rect);
                                    entityposition = true;
                                    break;
                                }
                            }
                            */

                            foreach (int2 position in itempositions)
                            {
                                if (position.x == realX && position.y == realY)
                                {
                                    e.Graphics.DrawRectangle(greenpen, Rect);
                                    entityposition = true;
                                    break;
                                }
                                //Console.WriteLine(position.item.name);
                                //Console.WriteLine(player.position.items.Count);
                            }

                            foreach (KeyValuePair<int2, Color> position in entitypositions)
                            {
                                if (position.Key.x == realX && position.Key.y == realY)
                                {
                                    e.Graphics.DrawRectangle(new Pen(position.Value, 2), Rect);
                                    entityposition = true;
                                    break;
                                }
                            }

                            if (entityposition == false)
                            {
                                e.Graphics.DrawRectangle(pen, Rect);
                            }
                        }
                    }

                    foreach (int2 doorxy in doorsxy)
                    {
                        Point exitpoint = new Point((-(loc.size.x / 2) + doorxy.x) * 25,
                            (-(loc.size.y / 2) + doorxy.y) * 25);
                        Rectangle rect = new Rectangle(exitpoint, new Size(20, 20));
                        rect.Offset(loc.offset.x - (add.x * 25), loc.offset.y - (add.y * 25));
                        e.Graphics.DrawRectangle(firebrickpen, rect);
                    }

                    foreach (int2 interactablexy in interactablesxy)
                    {
                        Point interactablepoint = new Point((-(loc.size.x / 2) + interactablexy.x) * 25,
                            (-(loc.size.y / 2) + interactablexy.y) * 25);
                        Rectangle rect = new Rectangle(interactablepoint, new Size(20, 20));
                        rect.Offset(loc.offset.x - (add.x * 25), loc.offset.y - (add.y * 25));
                        e.Graphics.DrawRectangle(pinkpen, rect);
                    }
                }

                /*
                foreach (Pixel pixel in pixels)
                {
                    int multiplier = 25;
                    Rectangle rect = new Rectangle(new Point(pixel.position.x * multiplier, pixel.position.y * multiplier), new Size(multiplier, multiplier));
                    e.Graphics.FillRectangle(new SolidBrush(pixel.color), rect);
                }
                */
                /*
                e.Graphics.TranslateTransform(275, 175);
                Pen pen = new Pen(Color.White, 2);
                Pen redpen = new Pen(Color.Red, 2);
                Pen yellowpen = new Pen(Color.Yellow, 2);
                Pen orangepen = new Pen(Color.Orange, 2);
                Pen greenpen = new Pen(Color.Green, 2);
                Pen firebrickpen = new Pen(Color.Firebrick, 2);
                List<int2> entitypositions = new List<int2>();
                List<int2> itempositions = new List<int2>();
                List<int2> doorsxy = new List<int2>();
                Rects.Clear();

                foreach (Entity entity in player.position.entities)
                {
                    for (int y = entity.coord.y; y < entity.coord.y + entity.size.y; y++)
                    {
                        for (int x = entity.coord.x; x < entity.coord.x + entity.size.x; x++)
                        {
                            entitypositions.Add(new int2(x, y));
                        }
                    }
                }

                foreach (ItemPosition item in player.position.items)
                {
                    itempositions.Add(item.coord);
                }

                foreach (Exit exit in player.position.exits)
                {
                    Exit.Directions dir = Exit.Directions.Undefined;
                    int attachment = 0;
                    if (player.position == exit.room1)
                    {
                        dir = exit.direction1;
                        attachment = exit.attachment1;
                    }
                    else if (player.position == exit.room2)
                    {
                        dir = exit.direction2;
                        attachment = exit.attachment2;
                    }

                    if (dir == Exit.Directions.North)
                    {
                        for (int i = 0; i < exit.size; i++)
                        {
                            doorsxy.Add(new int2(attachment + i, -1));
                        }
                        Console.WriteLine("north exit");
                    }
                    else if (dir == Exit.Directions.East)
                    {
                        for (int i = 0; i < exit.size; i++)
                        {
                            doorsxy.Add(new int2(player.position.size.x, attachment + i));
                        }
                        Console.WriteLine("east exit");
                    }
                    else if (dir == Exit.Directions.South)
                    {
                        for (int i = 0; i < exit.size; i++)
                        {
                            doorsxy.Add(new int2(attachment + i, player.position.size.y));
                        }
                        Console.WriteLine("south exit");
                    }
                    else if (dir == Exit.Directions.West)
                    {
                        for (int i = 0; i < exit.size; i++)
                        {
                            doorsxy.Add(new int2(-1, attachment + i));
                        }
                        Console.WriteLine("east exit");
                    }
                }

                for (int y = -player.position.size.y / 2; y < player.position.size.y / 2; y++)
                {
                    for (int x = -player.position.size.x / 2; x < player.position.size.x / 2; x++)
                    {                   
                        int realY = y + player.position.size.y / 2;
                        int realX = x + player.position.size.x / 2;
                        bool entityposition = false;

                        Rectangle Rect = new Rectangle(new Point(x * 25, y * 25), new Size(20, 20));

                        foreach (int2 position in Path)
                        {
                            if (position.x == realX && position.y == realY)
                            {
                                e.Graphics.DrawRectangle(orangepen, Rect);
                                entityposition = true;
                                break;
                            }
                        }

                        foreach (int2 position in itempositions)
                        {
                            if (position.x == realX && position.y == realY)
                            {
                                e.Graphics.DrawRectangle(greenpen, Rect);
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
                                    e.Graphics.DrawRectangle(redpen, Rect);
                                    entityposition = true;                               
                                    break;
                                }
                                else
                                {
                                    e.Graphics.DrawRectangle(yellowpen, Rect);
                                    entityposition = true;
                                    break;
                                }
                            }                        
                        }

                        if (entityposition == false)
                        {
                            e.Graphics.DrawRectangle(pen, Rect);                        
                        }
                    }
                }

                foreach (int2 doorxy in doorsxy)
                {
                    Console.WriteLine("exit time");
                    Point exitpoint = new Point((-(player.position.size.x / 2) + doorxy.x) * 25,
                        (-(player.position.size.y / 2) + doorxy.y) * 25);
                    Rectangle rect = new Rectangle(exitpoint, new Size(20, 20));
                    e.Graphics.DrawRectangle(firebrickpen, rect);
                }

                foreach (Location loc in currentworld.locations)
                {
                    if (loc != currentworld.current)
                    {
                        for (int y = -loc.size.y / 2; y < loc.size.y / 2; y++)
                        {
                            for (int x = -loc.size.x / 2; x < loc.size.x / 2; x++)
                            {
                                Rectangle Rect = new Rectangle(new Point(x * 25, y * 25), new Size(20, 20));
                                Rect.Offset(loc.offset.x, loc.offset.y);
                                e.Graphics.DrawRectangle(pen, Rect);
                            }
                        }
                    }
                }

                //e.Graphics.TranslateTransform(e.Graphics.Transform.OffsetX + 150, e.Graphics.Transform.OffsetX + 150);
                return;      
                */
            }
        }

        public Graphics textGraphics;

        private Font drawFont = new Font("Arial", 11);
        private float dx = 0f;
        private float dy = 0f;
        private StringFormat drawFormat = new StringFormat();

        public void DrawText(string text)
        {
            SolidBrush drawBrush = new SolidBrush(Color.White);
            

        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {
            //Font drawFont = new Font("Arial", 12);
            //SolidBrush drawBrush = new SolidBrush(Color.White);
            //float dx = 0f;
            //float dy = 0f;
            //StringFormat drawFormat = new StringFormat();
            //e.Graphics.DrawString("this is a test of the string drawing system", drawFont, drawBrush, dx, dy, drawFormat);
            //panel3.Refresh();
            textGraphics = e.Graphics;
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
