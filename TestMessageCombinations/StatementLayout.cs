using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestMessageCombinations
{
    class StatementLayout
    {
        public List<string> MessageAreas = new List<string>() { "D4", "D8", "D9" };

        public Message D4Message;
        public Message D8Message;
        public Message D9Message;

        public StatementLayout(Message d4, Message d8, Message d9)
        {
            D4Message = d4;
            D8Message = d8;
            D9Message = d9;
        }

        public override string ToString()
        {
            return D4Message.ToString() + "|" + D8Message.ToString() + "|" + D9Message.ToString();
        }
    }
}
