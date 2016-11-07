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

            // contains all possible messages (Goal + message area)
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
            Console.WriteLine("Total number of possible messages: " + Messages.Count);
            Console.ReadLine();

            // contains all possible statement layouts--impossible layouts will be removed later based on rules
            List<StatementLayout> PossibleLayouts = new List<StatementLayout>();

            
            List<Message> D4Messages = new List<Message>();
            D4Messages.AddRange(Messages.Where(x => x.MessageArea == "D4"));

            List<Message> D8Messages = new List<Message>();
            D8Messages.AddRange(Messages.Where(x => x.MessageArea == "D8"));

            List<Message> D9Messages = new List<Message>();
            D9Messages.AddRange(Messages.Where(x => x.MessageArea == "D9"));


            //TODO: figure out a better way to come up with all possible layouts
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

            // get rid of any layouts that are impossible
            RemoveImpossibleLayouts(PossibleLayouts);

            foreach(StatementLayout s in PossibleLayouts)
            {
                Console.WriteLine(s);
            }
            Console.WriteLine("Total number of statement layouts: " + PossibleLayouts.Count());
            Console.ReadLine();

            // output the layout report
            WriteToCsv(PossibleLayouts);

        }

        private static void RemoveImpossibleLayouts(List<StatementLayout> possibleLayouts)
        {
            Dictionary<string, int> GoalRankings = new Dictionary<string, int>()
            {
                { "PayOnline", 1 },
                { "PaymentPlans", 2 },
                { "AvoidCollections", 3 },
                { "RecurringPayments", 4 },
                { "CharityCare", 5 },
                { "PatientPortal", 6 },
                { "Default", 7 },
                { "PaymentPlanQR", 0 },
                { "PaymentPlanQRCycle2", 0 },
                { "PaymentPlanQRCycle3", 0 },
                { "PaymentPlanNewCharges", 0 },
                { "PaymentPlanNewChargesCycle2", 0 },
                { "PaymentPlanNewChargesCycle3", 0 }
                //"Default"
            };

            Dictionary<string, int> MessageAreaRankings = new Dictionary<string, int>()
            {
                {"D8", 1 },
                {"D4", 2 },
                {"D9", 3 }

            };


            // Check for repeated messages (except for AvoidCollections and Default--those can have repeats)
            for (int index = 0; index < possibleLayouts.Count; index++)
            {
                StatementLayout s = possibleLayouts[index];
                List<Message> messageList = s.GetMessages();
                var duplicateMessageQuery = messageList.GroupBy(x => x.Goal)
                    .Select(n => new { MessageName = n.Key, MessageCount = n.Count() })
                    .Where(y => y.MessageCount > 1 /*&& y.MessageName != "AvoidCollections"*/ && y.MessageName != "Default");
                
                if(duplicateMessageQuery.Count() > 0)
                {
                    possibleLayouts.RemoveAt(index);
                    index--;
                }
            }

            // Check for mismatched rankings -- the messages in each area have to be ranked appropriately.
            
            for (int index = 0; index < possibleLayouts.Count; index++)
            {
                bool isBadLayout = false;

                StatementLayout currentLayout = possibleLayouts[index];

                // get rankings of messages in current layout
                string d9MessageGoal = currentLayout.D9Message.Goal;
                int d9MessageRank = GoalRankings[d9MessageGoal];

                string d8MessageGoal = currentLayout.D8Message.Goal;
                int d8MessageRank = GoalRankings[d8MessageGoal];

                string d4MessageGoal = currentLayout.D4Message.Goal;
                int d4MessageRank = GoalRankings[d4MessageGoal];

                //check if other message areas have a lower-ranked message in their areas
                // D8 is highest rank, then D4, then D9. 
                if (d4MessageRank < d8MessageRank || d9MessageRank < d4MessageRank || d9MessageRank < d8MessageRank)
                {
                    isBadLayout = true;
                }

                //check for avoidcollections inconsistencies
                if (d4MessageGoal == "AvoidCollections" || d8MessageGoal == "AvoidCollections" || d9MessageGoal == "AvoidCollections")
                {
                    ////if avoidcollections is in D8, then everything else has to be AvoidCollections (repeated messages)
                    //if (d8MessageGoal == "AvoidCollections" && (d4MessageGoal != "AvoidCollections" || d9MessageGoal != "AvoidCollections"))
                    //{
                    //    isBadLayout = true;
                    //}

                    ////if avoidcollections is in D4, then D9 has to be avoidcollections (repeated messages)
                    //else if(d4MessageGoal == "AvoidCollections" && d9MessageGoal != "AvoidCollections")
                    //{
                    //    isBadLayout = true;
                    //}

                    // can't have cycle 1 or 2 D8 ads at the same time as avoidcollections
                    List<string> Cycle1And2Goals = new List<string>() { "PaymentPlanQR", "PaymentPlanQRCycle2", "PaymentPlanNewCharges", "PaymentPlanNewChargesCycle2" };
                    if (Cycle1And2Goals.Contains(d8MessageGoal))
                    {
                        isBadLayout = true;
                    }
                }

                if (isBadLayout)
                {
                    possibleLayouts.RemoveAt(index);
                    index--;
                }

            }

            
        }

        // Throw a report on the desktop with all possible statement layouts
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
            var layoutQuery = from s in layouts
                               where s.D4Message == currentlayout.D4Message
                               && s.D8Message == currentlayout.D8Message
                               && s.D9Message == currentlayout.D9Message
                               select s;

            if (layoutQuery.Count() > 0)
                return true;
            else
                return false;
        }
    }
}
