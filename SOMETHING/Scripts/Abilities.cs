using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Something
{
    public class Ability
    {
        public Creature user;
        public string name;

        public virtual void Trigger(List<Entity> targets)
        {
            // Do something to the targets
        }


    }
    
    public class BeatdownExplosion : Ability
    {
        public BeatdownExplosion(Creature _user, string _name)
        {
            user = _user;
            name = _name;
        }

        public override void Trigger(List<Entity> targets)
        {
            foreach (Entity e in targets)
            {
                e.health = 1;
            }
        }
    }
}
