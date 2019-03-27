using System;
using System.Collections.Generic;
using System.Text;

namespace Something
{
    public class Effect
    {
        public Creature attachment;
        public int remaining;
        public int level;
        public int duration;
        public string name;

        public virtual void Update()
        {
            --remaining;
            //if (remaining <= 0)
            //{
            //    if (attachment.effects.Contains(this))
            //    {
            //        attachment.effects.Remove(this);
            //        attachment = null;
            //    }
            //}
            Console.WriteLine($"updated {name}");
            // Do something to the attached entity scaled by level
        }
    }
    
    class Healing : Effect // ID is 1
    {
        public Healing(int _level, int _duration, string _name, Creature _attachment)
        {
            level = _level;
            duration = _duration;
            remaining = duration;
            name = _name;
            attachment = _attachment;
        }
    }

    class Existing : Effect // ID is 2
    {
        public Existing(int _level, int _duration, string _name, Creature _attachment)
        {
            level = _level;
            duration = _duration;
            remaining = duration;
            name = _name;
            attachment = _attachment;
        }
    }

    class Beating : Effect
    {
        public Beating(int _level, int _duration, string _name, Creature _attachment)
        {
            level = _level;
            duration = _duration;
            remaining = duration;
            name = _name;
            attachment = _attachment;
        }

        public override void Update()
        {
            attachment.health = int.MinValue;
            base.Update();
        }
    }
}
