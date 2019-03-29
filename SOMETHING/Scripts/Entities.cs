﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

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
        public int health;
        public int armor;
        public int speed;

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
        public Armor attachedarmor;
        public int strength;
        public int dexterity;
        public int constitution;
        public int intelligence;
        public int wisdom;
        public int charisma;

        public Creature() : base()
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
                charisma = this.charisma
            };
        }

        public int modifier(int stat)
        {
            return (stat - 10) / 2;
        }
    }

    public class Player : Creature
    {
        public List<Item> inventory = new List<Item>();
    }
}
