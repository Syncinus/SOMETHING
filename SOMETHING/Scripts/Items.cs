using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Something
{
    public class ItemPosition
    {
        public Item item;
        public int2 coord;
        public bool randomize;

        public ItemPosition(Item _item, int2? _coord = null)
        {
            item = _item;
            if (_coord != null)
            {
                coord = new int2(_coord.Value.x, _coord.Value.y);
            } else
            {
                coord = new int2(-1, -1);
            }
        }
    }

    public class ItemEffect
    {
        public Entity entity;
        public string data;
        public string text;
        public bool consumed;

        public ItemEffect()
        {

        }

        public ItemEffect(Entity _entity, string _data, string _text = "", bool _consumed = false)
        {
            entity = _entity;
            data = _data;
            text = _text;
            consumed = _consumed;
        }
    }

    public class Item
    {
        public string name;
        public string description;
        public bool useable;
        public bool consumable;

        
        public Item(string _name, string _description, bool _useable = true, bool _consumable = false)
        {
            name = _name;
            description = _description;
            useable = _useable;
            consumable = _consumable;

        }
        

        public virtual ItemEffect Use(Entity target)
        {
            Console.WriteLine("Used: " + name);
            return null;
        }
    }

    class Potion : Item
    {
        public List<Effect> effects = new List<Effect>();

        public Potion(string _name, string _description, bool _useable = true, params Effect[] _effects)
            : base(_name, _description, _useable, true)
        {
            foreach (Effect effect in _effects)
            {
                effects.Add(effect);
            }
        }

        public override ItemEffect Use(Entity target)
        {
            string text = $"Used potion {name}, which:\n";

            //string data = $"[e({effects[0].GetType().ToString()},{effects[0].level},{effects[0].duration},{effects[0].name})]";
            string data = $"[e]";
            foreach (Effect effect in effects)
            {
                //text += $"\nGave level {effect.level} {effect.name} for {effect.duration} turns\n";
                data = data.Insert(2, $"({effect.GetType().ToString()},{effect.level},{effect.duration},{effect.name})");
            }
            return new ItemEffect(target, data, text, consumable);
        }
    }

    class Weapon : Item
    {
        public List<Effect> effects = new List<Effect>();
        int range;
        int damage;
        int modifier;
        string type;

        public Weapon(string _name, string _description, int _damage, int _range, int _modifier, string _type, bool _useable = true, params Effect[] _effects)
                :base(_name, _description, _useable, false)
        {
            damage = _damage;
            range = _range;
            modifier = _modifier;
            type = _type;
            foreach (Effect effect in _effects)
            {
                effects.Add(effect);
            }
        }

        public override ItemEffect Use(Entity target)
        {
            string text = $"Used: {name} on {target.name}";

            string data = $"[e]";
            foreach (Effect effect in effects)
            {
                //text += $"\nGave level {effect.level} {effect.name} for {effect.duration} turns\n";
                data = data.Insert(2, $"({effect.GetType().ToString()},{effect.level},{effect.duration},{effect.name})");
            }
            data += $"[d({damage},{type},{range},{modifier})]";
            //data += $"[r({range}"
            return new ItemEffect(target, data, text, consumable);
        }
    }

    public class Armor : Item
    {
        public int soak;
        public int defense;
        public int attack;
        List<Effect> effects;
        public Creature wearer;

        public Armor(string _name, string _description, bool _useable, int _soak, int _defense, int _attack, params Effect[] _effects)
            : base (_name, _description, _useable, false)
        {
            soak = _soak;
            defense = _defense;
            attack = _attack;
            effects = _effects.ToList();
        }

        public override ItemEffect Use(Entity target)
        {
            string text = $"Equipped armor {name} on {target.name}";

            string data = $"[i({soak},{defense},{attack})]";

            return new ItemEffect(target, data, text, false);
        }
    }
}
