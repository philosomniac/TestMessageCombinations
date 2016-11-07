using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestMessageCombinations
{
    class Message
    {
        public string MessageArea;
        public string Goal;

        public Message(string messagearea, string goal)
        {
            Goal = goal;
            MessageArea = messagearea;
        }

        public override string ToString()
        {
            return MessageArea + " - " + Goal;
        }
    }
}
