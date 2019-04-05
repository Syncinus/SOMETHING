using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using MoreLinq;
using RoyT.AStar;

namespace Something
{
    public class Entity
    {
        #region CREATURE
        // ONLY USED FOR CREATURES
        public List<dynamic> effects = null;
        public List<string> weaknesses = null;
        public List<string> resistances = null;
        #region COMBAT
        public int Attack;
        public int Defense;
        public int Accuracy;
        #endregion
        //
        #endregion
        //private int2[] coordgrid;
        public Color coloring = Color.Yellow;
        public List<Ability> triggered = new List<Ability>();
        public bool dead = false;
        public bool appear = true;
        //public int direction = 1;
        public Location position;
        public int2 coord;
        public int2 size;
        //public int2 center;
        public string name;
        public int movement;
        bool initialhealthset = false;
        public int health {
            get { return realhealth;  }
            set {
                if (initialhealthset == false)
                {
                    maxhealth = value;
                    realhealth = maxhealth;
                }
                realhealth = value;
            }
        }
        public int armor;
        public int speed;

        internal int maxhealth;
        internal int realhealth;

        public void Move(Location newLocation)
        {
            if (position != null)
            {
                position.removeEntity(this);
            }
            position = newLocation;
            position.addEntity(this);
        }

        // Low quality flip system
        public void Flip()
        {
            size = new int2(size.y, size.x);
        }

        public Entity Clone()
        {
            return new Entity
            {
                Attack = this.Attack,
                Defense = this.Defense,
                Accuracy = this.Accuracy,
                dead = this.dead,
                appear = this.appear,
                position = this.position,
                coord = this.coord,
                size = this.size,
                name = this.name,
                movement = this.movement,
                health = this.health,
                armor = this.armor,
                speed = this.speed
            };
        }

        // Experimental center based coordinate rotation, may come back to later if i need to.
        /*
        public void BuildCoordGrid()
        {
            
            List<int2> grid = new List<int2>();
            for (int y = 0; y < size.y; y++)
            {
                for (int x = 0; x < size.x; x++)
                {
                    int relativeX = Math.Abs(center.x - x);
                    if (x < center.x)
                    {
                        relativeX = -relativeX;
                        Console.WriteLine("negative");
                    }
                    int relativeY = Math.Abs(center.y - y);
                    if (y < center.y)
                    {
                        relativeY = -relativeY;
                        Console.WriteLine("negative");
                    }

                    grid.Add(new int2(relativeX, relativeY));
                }
            }
            coordgrid = grid.ToArray();
            foreach (int2 coord in coordgrid)
            {
                Console.WriteLine(coord.ToString());
            }
        }

        public void Flip()
        {
            BuildCoordGrid();

            size = new int2(size.y, size.x);
            int xchange = 0;
            int ychange = 0;

            foreach (int2 relative in coordgrid)
            {
                ychange += relative.x;
                xchange += relative.y;
            }

            Console.WriteLine(xchange);
            Console.WriteLine(ychange);

            coord = new int2(coord.x + xchange, coord.y + ychange);
        }
        */

        public virtual void Update()
        {
            if (health <= 0)
            {
                dead = true;
                Console.WriteLine($"{name} has died");
                //position.removeEntity(this);
            }
            if (triggered.Count > 0)
            {
                for (int i = 0; i < triggered.Count; i++)
                {
                    triggered[i].Trigger(new List<Entity>() { this });
                    triggered[i].remaining--;
                    if (triggered[i].remaining == 0)
                    {

                    }
                }
            }
        }
    }
    
    public class Creature : Entity
    {
        public List<dynamic> effectscopy = new List<dynamic>();
        public List<Ability> abilities = new List<Ability>();
        public List<Item> loot;
        public Armor attachedarmor;
        public string quicktext;
        public int strength;
        public int dexterity;
        public int constitution;
        public int intelligence;
        public int wisdom;
        public int charisma;
        public int experience;

        public Creature()
        {
            effects = new List<dynamic>();
            weaknesses = new List<string>();
            resistances = new List<string>();
        }

        public override void Update()
        {
            base.Update();
            foreach (Effect effect in effects)
            {
                if (!effectscopy.Contains(effect))
                {
                    effectscopy.Add(effect);
                }
                effect.Update();
            }

            bool clear = false;

            foreach (Effect effect in effectscopy)
            {
                if (effect.remaining <= 0)
                {
                    effect.attachment = null;
                    effects.Remove(effect);
                    clear = true;
                }
            }

            if (clear == true) 
                effectscopy.Clear();
        }

        public new Creature Clone()
        {
            return new Creature
            {
                Attack = this.Attack,
                Defense = this.Defense,
                Accuracy = this.Accuracy,
                dead = this.dead,
                appear = this.appear,
                position = this.position,
                coord = this.coord,
                size = this.size,
                name = this.name,
                movement = this.movement,
                health = this.health,
                armor = this.armor,
                speed = this.speed,
                effects = this.effects,
                weaknesses = this.weaknesses,
                resistances = this.resistances,
                attachedarmor = this.attachedarmor,
                strength = this.strength,
                dexterity = this.dexterity,
                constitution = this.constitution,
                intelligence = this.intelligence,
                wisdom = this.wisdom,
                charisma = this.charisma,
                experience = this.experience
            };
        }

        public int modifier(int stat)
        {
            return (stat - 10) / 2;
        }
    }

    public class Communicator : Creature
    {
        public Dialouge dialouge;
        public Player target;

        public void SetDialouge(Dialouge _dialouge)
        {
            dialouge = _dialouge;
        }

        public void InitiateDialouge(Player _target)
        {
            dialouge.player = _target;
            target = _target;
            dialouge.Process();
        }

        public void DialougeOption(int option)
        {
            dialouge.CallOption(option);
            dialouge.Process();
            if (dialouge.complete)
            {
                dialouge = null;
                target.communicator = null;
                target = null;
                GameVariables.game.TypeLine("conversation ended");
            }
        }
    }

    public class Enemy : Creature
    {
        public enum AIMode { None, Agressive, AgressiveUse, AgressiveScare };
        public AIMode ai = AIMode.None;
        public RoyT.AStar.Grid grid;
        public int challengerating;
        public string team;
        public Weapon weapon;
        public List<Item> inventory = new List<Item>();
        public List<int2> blocked = new List<int2>();

        public override void Update()
        {
            base.Update();
            grid = new Grid(position.size.x, position.size.y);

            blocked.Clear();
            foreach (Entity entity in position.entities)
            {
                if (entity != this)
                { 
                    for (int y = 0; y < entity.size.y; y++)
                    {
                        for (int x = 0; x < entity.size.x; x++)
                        {
                            blocked.Add(new int2(entity.coord.x + x, entity.coord.y + y));
                        }
                    }
                }
            }

            foreach (Interactable interactable in position.interactables)
            {
                if (interactable.block == true)
                {
                    for (int y = 0; y < interactable.size.y; y++)
                    {
                        for (int x = 0; x < interactable.size.x; x++)
                        {
                            blocked.Add(new int2(interactable.coord.x + x, interactable.coord.y + y));
                        }
                    }
                }
            }

            

            foreach (int2 occupied in GameVariables.occupiedpaths)
            {
                blocked.Add(occupied);
            }


            foreach (int2 block in blocked)
            {
                grid.BlockCell(new RoyT.AStar.Position(block.x, block.y));
            }

            if (ai != AIMode.None)
            {
                if (ai == AIMode.Agressive)
                {
                    Enemy target = null;
                    Position pos = new Position(-1, -1);
                    foreach (Entity e in position.entities)
                    {
                        if (e is Enemy || e is Player)
                        {
                            if (e is Enemy)
                            {
                                Enemy t = e as Enemy;
                                if (t.team != team)
                                {
                                    target = t;
                                    pos = new Position(target.coord.x, target.coord.y);
                                }
                            }
                            if (e is Player)
                            {
                                Console.WriteLine("target detected");
                                target = e as Enemy;
                                pos = new Position(target.coord.x, target.coord.y);
                                Console.WriteLine($"{pos.X},{pos.Y}");
                            }
                        }
                    }
                    
                    if (target != null && EXT.InRange(target.coord, coord, weapon.range / 2) != true)
                    {                        
                        Position[] positions = grid.GetPath(new Position(coord.x, coord.y), pos);
                        foreach (Position position in positions)
                        {
                            Console.WriteLine($"{position.X},{position.Y}");

                            if (position != new Position(target.coord.x, target.coord.y) && !EXT.InRangeLinearOverlap(target.coord, coord, weapon.range, blocked).Key)
                            {
                                coord = new int2(position.X, position.Y);
                            }
                        }
                        GameVariables.occupiedpaths.Add(coord);                       
                    } else
                    {
                        int2 dir = EXT.InRangeDirectional(target.coord, coord, weapon.range / 2);
                        for (int y = 0; y < dir.y; y++)
                        {
                            coord = EXT.GetDirection(coord, -dir.x);
                        }
                        //coord = EXT.GetDirection(coord, -dir);
                        Console.WriteLine($"moved back {dir}");
                    }
                    

                    if (target != null)
                    {
                        UseItem attack = new UseItem(weapon, target, this);
                        attack.Perform();
                        GameVariables.game.RefreshDrawings();
                    }
                }
                else if (ai == AIMode.AgressiveUse)
                {
                    Enemy target = null;
                    Position pos = new Position(-1, -1);
                    foreach (Entity e in position.entities)
                    {
                        if (e is Player)
                        {
                            Console.WriteLine("target detected");
                            target = e as Enemy;
                            pos = new Position(target.coord.x, target.coord.y);
                            Console.WriteLine($"{pos.X},{pos.Y}");
                        }
                    }

                    if (target != null && EXT.InRange(target.coord, coord, weapon.range / 2) != true)
                    {
                        Position[] positions = grid.GetPath(new Position(coord.x, coord.y), pos);
                        foreach (Position position in positions)
                        {
                            Console.WriteLine($"{position.X},{position.Y}");

                            if (position != new Position(target.coord.x, target.coord.y) && !EXT.InRangeLinearOverlap(target.coord, coord, weapon.range, blocked).Key)
                            {
                                coord = new int2(position.X, position.Y);
                            }
                        }
                        GameVariables.occupiedpaths.Add(coord);
                    }
                    else
                    {
                        int2 dir = EXT.InRangeDirectional(target.coord, coord, weapon.range / 2);
                        for (int y = 0; y < dir.y; y++)
                        {
                            coord = EXT.GetDirection(coord, -dir.x);
                        }
                        //coord = EXT.GetDirection(coord, -dir);
                        Console.WriteLine($"moved back {dir}");
                    }

                    if (target != null)
                    {
                        UseItem attack = new UseItem(weapon, target, this);
                        attack.Perform();
                        GameVariables.game.RefreshDrawings();
                    }

                    if (realhealth <= maxhealth / 2)
                    {
                        foreach (Item item in inventory)
                        {
                            if (item is Potion)
                            {
                                UseItem use = new UseItem(item, this, this);
                                use.Perform();
                            }
                        }
                    }

                    if (inventory.Count > 0)
                    {
                        Random rng = new Random();
                        if (rng.Next(0, 4) == 3)
                        {
                            foreach (Item item in inventory)
                            {
                                if (item is Weapon)
                                {
                                    if (target != null)
                                    {
                                        UseItem use = new UseItem(item, target, this);
                                        use.Perform();
                                    }
                                }
                            }
                        } else
                        {
                            if (target != null)
                            {
                                UseItem attack = new UseItem(weapon, target, this);
                                attack.Perform();
                            }
                        }
                    }
                    GameVariables.game.RefreshDrawings();
                } else if (ai == AIMode.AgressiveScare)
                {
                    Enemy target = null;
                    Position pos = new Position(-1, -1);
                    foreach (Entity e in position.entities)
                    {
                        if (e is Player)
                        {
                            Console.WriteLine("target detected");
                            target = e as Enemy;
                            pos = new Position(target.coord.x, target.coord.y);
                            Console.WriteLine($"{pos.X},{pos.Y}");
                        }
                    }

                    if (target != null && realhealth >= maxhealth / 2)
                    {
                        Position[] positions = grid.GetPath(new Position(coord.x, coord.y), pos);
                        foreach (Position position in positions)
                        {
                            Console.WriteLine($"{position.X},{position.Y}");

                            if (position != new Position(target.coord.x, target.coord.y) && !EXT.InRangeLinearOverlap(target.coord, coord, weapon.range, blocked).Key)
                            {
                                coord = new int2(position.X, position.Y);
                            }
                        }
                        GameVariables.occupiedpaths.Add(coord);
                    }
                    else if (target != null && realhealth < maxhealth / 2)
                    {
                        if (target.coord.x < position.size.x / 2 && target.coord.y > position.size.y / 2)
                        {

                        }
                    }
                }
            }
        }
    }

    public class Player : Creature
    {
        public List<Item> inventory = new List<Item>();
        public Communicator communicator;
        public new int experience;
        public int level;
    }
}
