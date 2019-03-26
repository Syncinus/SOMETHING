using System;
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

namespace Something
{
    public partial class Main : Form
    {
        int Line = 0;
        string TextQueue = "SOMETHING - DEVELOPMENT VERSION\r\n";

        public Player player;
        public Graphics graphics;
        public int lastturn;
        bool gameOver = false;
        bool incrementTurn = false;
        bool playerMovement = false;

        public void InitilizeGame()
        {
            player = new Player()
            {
                name = "player",
                coord = new int2(5, 5),
                armor = 0,
                health = 400,
                speed = 30,
                strength = 10,
                dexterity = 10,
                constitution = 10,
                intelligence = 10,
                wisdom = 10,
                charisma = 10
            };
        }

        public void BuildTestMap()
        {
            Location l1 = new Location("Entrance to hall", "You stand at the entrance of a long hallway. The hallway gets darker\nand darker, and you cannot see what lies beyond. To the east\nis an old oak door, which looks locked but openable.", new int2(10, 10));
            Item rock = new Item("rock", "A rather jagged rock, slightly smaller than a fist.", true);
            Potion testpotion = new Potion("potion of existing", "An intricately designed bottle containing some kind of fluid", true,
                new Existing(4000, 10, "existingness", player));
            Melee beatingstick = new Melee("beating stick", "a stick of wood around twenty centimeters thick and one meter long\nthat is good for giving beatings", 100, 1, "beating", true, new Beating(1, 5, "beatdown retribution", player));
            Entity uglywugly = new Entity()
            {
                name = "ugly wugly",
                armor = 0,
                health = 200,
                speed = 0,
                coord = new int2(4, 4)
            };
            uglywugly.Move(l1);
            player.inventory.Add(testpotion);
            player.inventory.Add(beatingstick);
            player.Move(l1);
            l1.addItem(rock);
            l1.addItem(testpotion);

            Location l2 = new Location("End of hall", "You have reached the end of a long dark hallway. You can\nsee nowhere to go but back.", new int2(5, 5));
            Item window = new Item("window", "A single sheet of glass. It seems sealed up.", false);
            l2.addItem(window);

            Location l3 = new Location("Small study", "This is a small and cluttered study, containing a desk covered with\npapers. Though they no doubt are of some importance,\nyou cannot read their writing", new int2(10, 10));

            LockedDoor door = new LockedDoor("locked door", new Exit(Exit.Directions.East, l3), new InteractableAction("lockpick", 10), new InteractableAction("break", 15));
            door.location = l1;
            l1.addInteractable(door);
            l1.addExit(new Exit(Exit.Directions.North, l2));
            //l1.addExit(new Exit(Exit.Directions.East, l3));

            l2.addExit(new Exit(Exit.Directions.South, l1));

            l3.addExit(new Exit(Exit.Directions.West, l1));

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
                        foreach (Entity e in player.position.entities)
                        {
                            e.Update();
                        }
                        playerMovement = false;
                        Console.WriteLine("game updated");
                        UpdateMap();
                    }
                    if (player.health == 0)
                    {
                        gameOver = true;
                        break;
                    }
                }
            });
            loop.Start();
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

            if (command.Contains("move ") && playerMovement == false)
            {
                string movement = command.Substring(5);
                string[] split = movement.Split(',');
                int2 newposition = new int2(int.Parse(split[0]), int.Parse(split[1]));
                if (newposition.x > player.position.size.x - 1 || newposition.x < 0 || newposition.y > player.position.size.y - 1 || newposition.y < 0)
                {
                    TypeLine("That position is invalid!");
                    return;
                } else
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
                TypeLine($"Health: {player.health}");
                TypeLine($"Armor: {player.armor}");
                TypeLine($"Speed: {player.speed}");
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
                    TypeLine($"Speed: {e.speed}");
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

            if (command == "look" || command == "l")
            {
                ShowLocation();
                if (player.position.items.Count == 0)
                {
                    TypeLine("There are no items in this room.");
                }
                return;
            }

            if (command.Contains("use ") && !command.Contains(" on "))
            {
                string itemname = command.Substring(4);
                Item item = player.inventory.Find(find => find.name == itemname);
                if (item != null)
                {
                    UseItem use = new UseItem(item, player, null);
                    use.Perform();
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
                    if (target != null)
                    {
                        UseItem use = new UseItem(item, target, player);
                        use.Perform();
                        if (use.success == false)
                        {
                            TypeLine("Weapon wasn't in range.");
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
                else if (player.position.items.Exists(x => x.name == command.Substring(8)))
                {
                    incrementTurn = true;
                    player.inventory.Add(player.position.takeItem(command.Substring(8)));
                    TypeLine("You pick up the " + command.Substring(8) + ".");
                    return;
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
                    this.Refresh();
                    panel1.Refresh();
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
                if (command == exit.ToString().ToLower() || command == exit.getShortDirection().ToLower())
                {
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
                    TypeLine(player.position.items[i].name);
                }
            }

            TypeLine("Available exits:");

            for (int i = 0; i < player.position.exits.Count; i++)
            {
                TypeLine(player.position.exits[i].direction);
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
            e.Graphics.FillPolygon(brush, points);
            e.Graphics.DrawLines(pen, points);
            */
            Pen pen = new Pen(Color.Black, 3);
            Pen redpen = new Pen(Color.Red, 3);
            Pen yellowpen = new Pen(Color.Yellow, 3);
            Pen orangepen = new Pen(Color.Orange, 3);
            List<int2> entitypositions = new List<int2>();

            foreach (Entity entity in player.position.entities)
            {
                entitypositions.Add(entity.coord);
            }

            for (int y = 0; y < player.position.size.y; y++)
            {
                for (int x = 0; x < player.position.size.x; x++)
                {
                    bool entityposition = false;
                    foreach (int2 position in entitypositions)
                    {
                        if (position.x == x && position.y == y)
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
