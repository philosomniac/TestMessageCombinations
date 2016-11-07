using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace TestMessageCombinations
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> MessageAreas = new List<string>() {"D4", "D8", "D9" };
            List<string> Goals = new List<string>() { "PayOnline", "PaymentPlans", "AvoidCollections", "RecurringPayments", "CharityCare", "PatientPortal", "Default" };
            List<Message> D8Overrides = new List<Message>()
            {
                new Message("D8", "PaymentPlanQR"),
                new Message("D8", "PaymentPlanQRCycle2"),
                new Message("D8", "PaymentPlanQRCycle3"),
                new Message("D8", "PaymentPlanNewCharges"),
                new Message("D8", "PaymentPlanNewChargesCycle2"),
                new Message("D8", "PaymentPlanNewChargesCycle3")
            };

            List<Message> Messages = new List<Message>();

            foreach (string goal in Goals)
            {
                foreach (string area in MessageAreas)
                {
                    Messages.Add(new Message(area,goal));
                }
            }

            foreach(Message m in D8Overrides)
            {
                Messages.Add(m);
            }

            foreach(Message message in Messages)
            {
                Console.WriteLine(message);
                
            }
            Console.WriteLine("Total number of messages: " + Messages.Count);
            Console.ReadLine();

            List<StatementLayout> PossibleLayouts = new List<StatementLayout>();

            List<Message> D4Messages = new List<Message>();
            D4Messages.AddRange(Messages.Where(x => x.MessageArea == "D4"));

            List<Message> D8Messages = new List<Message>();
            D8Messages.AddRange(Messages.Where(x => x.MessageArea == "D8"));

            List<Message> D9Messages = new List<Message>();
            D9Messages.AddRange(Messages.Where(x => x.MessageArea == "D9"));

            foreach(Message d4 in D4Messages)
            {
                foreach(Message d8 in D8Messages)
                {
                    foreach(Message d9 in D9Messages)
                    {
                        StatementLayout currentlayout = new StatementLayout(d4, d8, d9);

                        if(!LayoutAlreadyExists(currentlayout, PossibleLayouts))
                        {
                            PossibleLayouts.Add(currentlayout);
                        }
                    }
                }
            }
            foreach(StatementLayout s in PossibleLayouts)
            {
                Console.WriteLine(s);

            }
            Console.WriteLine("Total number of statement layouts: " + PossibleLayouts.Count());
            Console.ReadLine();

            WriteToCsv(PossibleLayouts);

        }

        private static void WriteToCsv(List<StatementLayout> possibleLayouts)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\PossibleMessagesReport.csv";
            StreamWriter report = new StreamWriter(path, false);
            int counter = 1;
            report.WriteLine("LayoutNumber,D4Message,D8Message,D9Message");
            foreach(StatementLayout layout in possibleLayouts)
            {
                report.WriteLine(counter.ToString() + "," + layout.D4Message.Goal + "," + layout.D8Message.Goal + "," + layout.D9Message.Goal);
                counter++;
            }
            report.Close();

            //System.Diagnostics.Process.Start("CMD.exe", $"start {path}");
        }

        private static bool LayoutAlreadyExists(StatementLayout currentlayout, List<StatementLayout> layouts)
        {
            var messageQuery = from s in layouts
                               where s.D4Message == currentlayout.D4Message
                               && s.D8Message == currentlayout.D8Message
                               && s.D9Message == currentlayout.D9Message
                               select s;

            if (messageQuery.Count() > 0)
                return true;
            else
                return false;
        }
    }
}
