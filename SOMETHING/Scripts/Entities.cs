﻿using System;
using System.Collections.Generic;
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
        public bool dead = false;
        public Location position;
        public int2 coord;
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

        public virtual void Update()
        {
            if (health <= 0)
            {
                dead = true;
                Console.WriteLine($"{name} has died");
                //position.removeEntity(this);
            }
        }
    }

    public class Creature : Entity
    {
        public List<dynamic> effectscopy = new List<dynamic>();
        public Armor attachedarmor;
        public int strength;
        public int dexterity;
        public int constitution;
        public int intelligence;
        public int wisdom;
        public int charisma;

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