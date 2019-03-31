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
        public bool hit = true;

        public UseItem(Item _item, Entity _entity, Entity _user)
        {
            item = _item;
            entity = _entity;
            user = _user;
        }

        public override void Perform()
        {
            if (item.useable == false)
            {
                GameVariables.game.TypeLine("You cant use that.");
                return;
            }
            ItemEffect effect = item.Use(entity);
            string data = effect.data;
            string text = effect.text;
            List<List<string>> effects;
            List<List<string>> damage;
            List<dynamic> effectsToAdd = new List<dynamic>();
            Regex dbregex = new Regex(@"(?<=\[).*?(?=\])");
            MatchCollection datablocks = dbregex.Matches(data);           

            bool inrange = false;
            int distance = 0;
            if (user == null)
            {
                inrange = true;
                success = true;
            }

            foreach (Match datablock in datablocks)
            {
                // Effects
                string dbvalue = datablock.Value;
                if (dbvalue[0] == 'e')
                {
                    effects = EXT.Process(dbvalue);
                    foreach (List<string> values in effects)
                    {
                        if (entity.effects != null)
                        {
                            //GameVariables.game.TypeLine($"Game level {values[1]} {values[3]} to {entity.name} for {values[2]} turns.");
                            dynamic geneffect = EXT.CreateInstance(values[0], int.Parse(values[1]), int.Parse(values[2]), values[3], entity);
                            effectsToAdd.Add(geneffect);
                        }
                    }
                }

                // Damage
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
                            success = true;
                        }
                        else
                        {
                            Console.WriteLine("checking if target is in range");
                            List<int2> blocked = new List<int2>();
                            foreach (Entity e in entity.position.entities)
                            {
                                if (e.name != entity.name && e.name != user.name)
                                {
                                    for (int y = 0; y < e.size.y; y++)
                                    {
                                        for (int x = 0; x < e.size.x; x++)
                                        {
                                            blocked.Add(e.coord + new int2(x, y));
                                        }
                                    }
                                }
                            }

                            foreach (Interactable i in entity.position.interactables)
                            {
                                if (i.block == true)
                                {
                                    for (int y = 0; y < i.size.y; y++)
                                    {
                                        for (int x = 0; x < i.size.x; x++)
                                        {
                                            blocked.Add(i.coord + new int2(x, y));
                                        }
                                    }
                                }
                            }

                            KeyValuePair<bool, int> returnrange = EXT.InRangeLinearOverlap(entity.coord, user.coord, 4, blocked);
                            inrange = returnrange.Key;
                            distance = returnrange.Value;
                            if (inrange == true)
                            {
                                Console.WriteLine("target is in range");
                            }
                        }

                        if (inrange == true)
                        {
                            // The extremley overcomplicated ultra efficent advanced attack system                            Random rng = new Random();
                            Random rng = new Random();
                            int TargetDefense = entity.Defense;
                            int TargetSpeed = entity.speed;
                            int AttackerAttack = user.Attack;
                            int AttackerSpeed = user.speed;
                            int Accuracy = user.Accuracy;

                            int AttSpeed;
                            int DefSpeed;

                            float TargetSpeedDivision = TargetSpeed / 3;
                            float AttackerSpeedDivision = AttackerSpeed / 3;
                            float TargetBattleSpeed = TargetSpeed - TargetSpeedDivision;
                            float AttackerBattleSpeed = AttackerSpeed - AttackerSpeedDivision;
                            float AttackerIncreaseCalculation = 10 - AttackerBattleSpeed / 2;
                            float TargetIncreaseCalculation = 10 - TargetBattleSpeed / 2;
                            AttSpeed = Convert.ToInt32(Math.Round(AttackerBattleSpeed + AttackerIncreaseCalculation)) + int.Parse(values[3]);
                            DefSpeed = Convert.ToInt32(Math.Round(TargetBattleSpeed + TargetIncreaseCalculation)); // Factor in armor to this later (maybe)
                            int AccuracyIncrease = rng.Next(1, 12);
                            int CombinedAccuracy = Accuracy + AccuracyIncrease;
                            int CombinedAttSpeed = AttSpeed + rng.Next(1, 20);
                            int CombinedDefSpeed = DefSpeed + rng.Next(1, 20) + distance;
                            Console.WriteLine("Combined attack speed: " + CombinedAttSpeed);
                            Console.WriteLine("Combined defense speed: " + CombinedDefSpeed);
                            hit = CombinedAttSpeed >= CombinedDefSpeed;
                            Console.WriteLine("Attack hit: " + hit);

                            if (hit == true)
                            {
                                int DamageCalculation = Convert.ToInt32(Math.Round((double)(((AttackerAttack * (AttackerAttack + int.Parse(values[0])) / (AttackerAttack + TargetDefense)) - entity.armor) * ((10 + user.speed) / 10)) / (rng.Next(220, 275) / 100)));
                                int Damage = DamageCalculation + 1;
                                Console.WriteLine("Base damage value: " + Damage);
                                if (CombinedAccuracy <= 4)
                                    Damage = Damage / 4;
                                else if (CombinedAccuracy >= 5 && CombinedAccuracy <= 8)
                                    Damage = Damage / 2;
                                else if (CombinedAccuracy >= 9 && CombinedAccuracy <= 11)
                                    Damage = (Damage / 4) * 3;
                                else if (CombinedAccuracy >= 12 && CombinedAccuracy <= 15)
                                    Damage = Damage * 1;
                                else if (CombinedAccuracy == 16 || CombinedAccuracy == 17)
                                    Damage = Damage + (Damage / 4);
                                else if (CombinedAccuracy >= 18 && CombinedAccuracy <= 20)
                                    Damage = Damage + (Damage / 2);
                                else if (CombinedAccuracy == 21 || CombinedAccuracy == 22)
                                    Damage = Damage * 2;
                                else if (CombinedAccuracy > 22)
                                    Damage = Damage * 3;

                                Console.WriteLine("Damage after accuracy: " + Damage);
                                if (entity.resistances != null && entity.weaknesses != null)
                                {
                                    if (entity.resistances.Contains(values[1]))
                                        Damage /= Convert.ToInt32(rng.Next(11, 20) / 10);
                                    else if (entity.weaknesses.Contains(values[1]))
                                        Damage *= Convert.ToInt32(rng.Next(11, 20) / 10);
                                }
                                Console.WriteLine("Damage after weakness application: " + Damage);
                                entity.health -= EXT.Clamp(Damage, 1, int.MaxValue);
                                success = true;
                            } else
                            {
                                success = false;

                            }
                        }
                        else
                        {
                            success = false;
                        }
                    }
                }

                // Improvements
                if (dbvalue[0] == 'i')
                {

                }
            }

            if (inrange == true && success == true)
            {
                GameVariables.game.TypeLine(effect.text);
            }

            if (inrange == true)
            {
                foreach (dynamic eff in effectsToAdd)
                {
                    GameVariables.game.TypeLine($"Gave level {eff.level} {eff.name} to {entity.name} for {eff.duration} turns.");
                    entity.effects.Add(eff);
                }
            }

            if (success == true)
            {
                base.Perform();
            }
        }
    }

    class UseArmor : Action
    {
        public Creature user;
        public Armor armor;

        public UseArmor(Creature _user, Armor _armor)
        {
            user = _user;
            armor = _armor;
        }

        public override void Perform()
        {
            ItemEffect use = armor.Use(user);
            string data = use.data;
            Regex dbregex = new Regex(@"(?<=\[).*?(?=\])");
            MatchCollection datablocks = dbregex.Matches(data);

            foreach (Match datablock in datablocks)
            {
                string dbvalue = datablock.Value;
                if (dbvalue[0] == 'i')
                {
                    List<List<string>> values = EXT.Process(dbvalue);
                    foreach (List<string> improvements in values)
                    {
                        int soak = int.Parse(improvements[0]);
                        int defense = int.Parse(improvements[1]);
                        int attack = int.Parse(improvements[2]);
                        user.armor += soak;
                        user.Defense += defense;
                        user.Attack += attack;
                        
                    }
                }
            }

            GameVariables.game.TypeLine(use.text);
            user.attachedarmor = armor;
            armor.wearer = user;
            base.Perform();
        }
    }
}
