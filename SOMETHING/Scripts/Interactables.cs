using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Something
{
    public class InteractableAction
    {
        public string name;
        public int value;
        
        public InteractableAction(string _name, int _value = 0)
        {
            name = _name;
            value = _value;
        }
    }

    public class Interactable
    {
        public string name;
        public Location location;
        public Dictionary<int, InteractableAction> interactableActions = new Dictionary<int, InteractableAction>();
        public int2 coord;
        public int2 size;
        public bool block;

        public virtual void Interact(string action, Creature user)
        {
            
        }
    }

    public class BoxStack : Interactable
    {
        List<Item> items;
        public int height;
        public BoxStack(int2 _coord, List<Item> _items)
        {
            items = _items;
            height = items.Count;
            coord = _coord;
            name = $"{height} box stack";
            size = new int2(1, 1);
            block = true;
            interactableActions.Add(0, new InteractableAction("open"));
        }

        public override void Interact(string action, Creature user)
        {
            if (action == interactableActions[0].name)
            {
                if (user is Player)
                {
                    foreach (Item item in items)
                    {
                        GameVariables.game.player.inventory.Add(item);
                        GameVariables.game.TypeLine($"Found {item.name}");
                    }
                }
            }
            location.removeInteractable(this);
            GameVariables.game.RefreshDrawings();
        }
    }

    class LockedDoor : Interactable
    {
        Exit exit;

        public LockedDoor(string _name, Exit _exit, int2 _coord, int2 _size, bool _block, params InteractableAction[] _interactableActions)
        {
            name = _name;
            exit = _exit;
            coord = _coord;
            size = _size;
            block = _block;
            if (_interactableActions[0] != null)
                interactableActions.Add(1, _interactableActions[0]);
            if (_interactableActions[1] != null)
                interactableActions.Add(2, _interactableActions[1]);
        }

        public override void Interact(string action, Creature user)
        {
            if (interactableActions.ContainsKey(1))
            {
                if (action == interactableActions[1].name)
                {
                    GameVariables.game.TypeLine($"Attempting to pick lock, which is a difficulty {interactableActions[1].value} dexterity check.");
                    SkillCheck pick = new SkillCheck(user.modifier(user.dexterity), interactableActions[1].value);
                    pick.Perform();
                    if (pick.success == true)
                    {
                        GameVariables.game.TypeLine("You successfully picked the lock.");
                        location.addExit(exit);
                        location.removeInteractable(this);
                    } else
                    {
                        GameVariables.game.TypeLine("You can't seem to pick the lock right now.");
                    }
                    return;
                }
            }
            if (interactableActions.ContainsKey(2))
            {
                if (action == interactableActions[2].name)
                {
                    GameVariables.game.TypeLine($"Attempting to break the door, which is a difficulty {interactableActions[2].value} strength check.");
                    SkillCheck destroy = new SkillCheck(user.modifier(user.strength), interactableActions[2].value);
                    destroy.Perform();
                    if (destroy.success == true)
                    {
                        GameVariables.game.TypeLine("You successfully broke down the door.");
                        location.addExit(exit);
                        location.removeInteractable(this);
                    } else
                    {
                        GameVariables.game.TypeLine("You are not able to break the door at the moment.");
                    }
                    return;
                }
            }
            GameVariables.game.TypeLine("Invalid way to interact with the " + name); 
        }
    }
    
    class Triggerable : Interactable
    {
        Trigger trigger;
    }
}
