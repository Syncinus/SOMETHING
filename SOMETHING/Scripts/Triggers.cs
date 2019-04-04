using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Something
{
    public class Trigger
    {
        public Location location;
        public Action<Entity, Location, Trigger> trigger;

        public Trigger(Action<Entity, Location, Trigger> _trigger, Location _location)
        {
            trigger = _trigger;
            location = _location;
        }

        public virtual void Invoke(Entity triggerer)
        {
            trigger.Invoke(triggerer, location, this);
        }
    }

    public class WorldTrigger : Trigger
    {
        public int2 coord;
        public bool stop = false;

        public WorldTrigger(int2 _coord, Action<Entity, Location, Trigger> _trigger, Location _location, bool _stop = false) : base(_trigger, _location)
        {
            coord = _coord;
            trigger = _trigger;
            location = _location;
            stop = _stop;
        }

        public override void Invoke(Entity triggerer)
        {
            base.Invoke(triggerer);
        }
    }    
}
