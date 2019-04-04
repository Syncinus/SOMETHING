using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Something
{
    public class TextSequence
    {
        public string text;
        public bool disablemap;

        public TextSequence(string _text, bool _disablemap)
        {
            text = _text;
            disablemap = _disablemap;
        }
    }

    public class DialougeOption
    {
        public int lead;
        public string option;
        public Action<Dialouge> method;

        public DialougeOption(int _lead, string _option, Action<Dialouge> _method = null)
        {
            lead = _lead;
            option = _option;
            method = _method;
        }
    }

    public class DialougeSelection
    {
        public string text;
        public List<DialougeOption> options;

        public DialougeSelection(string _text, params DialougeOption[] _options)
        {
            text = _text;
            options = _options.ToList();
        }
    }

    public class Dialouge
    {
        public Player player;
        public Communicator creature;
        public bool complete;
        private List<List<DialougeSelection>> rows;
        private List<DialougeSelection> row;
        private DialougeSelection current;
        private int rownumber;
        private int currentindex;
        

        public Dialouge(Player _player, Communicator _creature, params List<DialougeSelection>[] _rows)
        {
            player = _player;
            creature = _creature;
            player.communicator = _creature;
            rows = _rows.ToList();
            if (rows.Count > 0)
            {
                Console.WriteLine("process");
                row = rows.ElementAt(0);
                current = row.ElementAt(0);
                rownumber = 0;
                currentindex = 0;
            }
        }

        public void Process()
        {
            GameVariables.game.TypeLine(current.text);
            for (int i = 0; i < current.options.Count; i++)
            {
                GameVariables.game.TypeLine($"{i + 1}. {current.options[i].option}");
            }
        }

        public void CallOption(int index)
        {
            DialougeOption option = current.options.ElementAt(index);
            GameVariables.game.TypeLine(option.option);
            if (option.method != null)
            {
                option.method.Invoke(this);
            }
            row = rows.ElementAt(rownumber + 1);
            ++rownumber;
            currentindex = option.lead;
            current = row.ElementAt(option.lead);
            if (rownumber + 1 >= rows.Count)
            {
                complete = true;
            }
        }
    }
}
