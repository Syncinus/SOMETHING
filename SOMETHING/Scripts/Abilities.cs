using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Something
{
    public class Ability
    {
        public Creature originaluser;
        public Creature user;
        public string name;
        public int maxtargets;
        public int duration;
        public int remaining;
        public bool restore;

        public virtual void Trigger(List<Entity> targets)
        {
            // Do something to the targets
        }

        public virtual void Remove()
        {
            if (user.triggered.Contains(this))
            {
                user.triggered.Remove(this);
            }
            if (restore == true)
            {
                List<dynamic> ceffects = user.effects;
                user = originaluser;
                user.effects = ceffects;
            }
            user = null;
        }
    }
    
    public class BeatdownExplosion : Ability
    {
        public BeatdownExplosion(Creature _user, string _name, int _maxtargets, int _duration)
        {
            originaluser = (Creature)EXT.DeepCopy(_user);
            user = _user;
            name = _name;
            maxtargets = _maxtargets;
            duration = _duration;
            restore = false;
        }

        public override void Trigger(List<Entity> targets)
        {
            foreach (Entity e in targets)
            {
                e.health -= 10;
            }
        }
    }
}

