using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Something
{
    public enum RollType { None, Advantage, Disadvantage }
    class Check
    {
        public Random random;
        public int difficulty;

        public Check(int _difficulty)
        {
            random = new Random();
            difficulty = _difficulty;
        }

        public bool Roll(int modifier)
        {
            int check = random.Next(1, 20);
            check += modifier;
            return check >= difficulty;
        }
    }

    class Action
    {
        public bool success;

        public virtual void Perform()
        {
            ++GameVariables.turns;
        }
    }

    class SkillCheck : Action {
        public int modifier;
        public int difficulty;

        public SkillCheck(int _modifier, int _difficulty)
        {
            modifier = _modifier;
            difficulty = _difficulty;
        }

        public override void Perform()
        {
            Check check = new Check(difficulty);
            success = check.Roll(modifier);
            base.Perform();
        }
    }

    class UseItem : Action
    {
        public Item item;
        public Entity entity;
        public Entity user;

        public UseItem(Item _item, Entity _entity, Entity _user)
        {
            item = _item;
            entity = _entity;
            user = _user;
        }

        public override void Perform()
        {
            ItemEffect effect = item.Use(entity);
            string data = effect.data;
            List<List<string>> effects;
            List<List<string>> damage;
            Regex dbregex = new Regex(@"(?<=\[).*?(?=\])");
            MatchCollection datablocks = dbregex.Matches(data);

            bool inrange = false;
            if (user == null)
            {
                inrange = true;
            }

            foreach (Match datablock in datablocks)
            {
                string dbvalue = datablock.Value;
                if (dbvalue[0] == 'e')
                {
                    effects = EXT.Process(dbvalue);
                    foreach (List<string> values in effects)
                    {
                        if (entity.effects != null)
                        {
                            dynamic geneffect = EXT.CreateInstance(values[0], int.Parse(values[1]), int.Parse(values[2]), values[3], entity);
                            entity.effects.Add(geneffect);
                        }
                    }
                }

                if (dbvalue[0] == 'd')
                {
                    damage = EXT.Process(dbvalue);
                    foreach (List<string> values in damage)
                    {
                        int range = int.Parse(values[2]);
                        if (user == null)
                        {
                            Console.WriteLine("user null, damage works");
                            inrange = true;
                        }
                        else
                        {
                            Console.WriteLine("checking if target is in range");
                            if (EXT.InRange(entity.coord, user.coord, range))
                            {
                                Console.WriteLine("target is in range");
                                inrange = true;
                            }
                        }

                        if (inrange == true)
                        {
                            int weapondamage;
                            weapondamage = int.Parse(values[0]) - entity.armor;
                            if (entity.resistances != null && entity.weaknesses != null)
                            {
                                if (entity.resistances.Contains(values[1]))
                                    weapondamage /= 2;
                                else if (entity.weaknesses.Contains(values[1]))
                                    weapondamage *= 2;
                            }
                            entity.health -= EXT.Clamp(weapondamage, 1, 1000);
                            success = true;
                        }
                        else
                        {
                            success = false;
                        }
                    }
                }
            }

            if (inrange == true)
            {
                GameVariables.game.TypeLine(effect.text);
            }

            if (success == true)
            {
                base.Perform();
            }
        }
    }
}
