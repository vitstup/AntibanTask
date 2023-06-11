using System;
using System.Collections.Generic;

namespace Antiban
{
    public class Antiban
    {
        private List<EventMessage> messages = new List<EventMessage>();
        private List<AntibanProcess> processes = new List<AntibanProcess>();

        public void PushEventMessage(EventMessage eventMessage)
        {
            messages.Add(eventMessage);
        }

        public List<AntibanResult> GetResult()
        {
            SortMessages();
            ProcessMessages();
            return GetProcessedMessages();
        }

        private void SortMessages()
        {
            messages.Sort((m1, m2) => m1.DateTime.CompareTo(m2.DateTime));
        }

        private void ProcessMessages()
        {
            processes.Clear();

            for (int i = 0; i < messages.Count; i++)
            {
                processes.Add(new AntibanProcess() { message = messages[i], time = messages[i].DateTime });
            }

            DoProcess(true);
            DoProcess(false);
            DoSmallProcess();
            DoProcess(true);
            DoSmallProcess();
        }

        private void DoSmallProcess()
        {
            for (int i = 1; i < processes.Count; i++)
            {
                DateTime newTime = processes[i].time;
                if (processes[i].time < processes[i - 1].time.AddSeconds(10)) newTime = processes[i - 1].time.AddSeconds(10);
                processes[i].time = newTime;
            }
        }

        private void DoProcess(bool lowPriority = false)
        {
            for (int i = 1; i < processes.Count; i++)
            {
                var message = processes[i].message;
                int neededMessage = -1;
                for (int p = i - 1; p >= 0; p--)
                {
                    var curMessage = processes[p].message;
                    if (message.Phone == curMessage.Phone)
                    {
                        if (lowPriority && message.Priority == 1 && curMessage.Priority == 1 && processes[i].time < processes[p].time.AddHours(24))
                        {
                            neededMessage = p;
                            break;
                        }
                        else if (!lowPriority && processes[i].time < processes[p].time.AddMinutes(1))
                        {
                            neededMessage = p;
                            break;
                        }
                    }
                }
                DateTime newTime = processes[i].time;
                if (neededMessage >= 0)
                {
                    if (lowPriority) newTime = processes[neededMessage].time.AddHours(24);
                    else newTime = processes[neededMessage].time.AddMinutes(1);
                }
                processes[i].time = newTime;
            }

            processes.Sort((m1, m2) => m1.time.CompareTo(m2.time));
        }

        private List<AntibanResult> GetProcessedMessages()
        {
            List<AntibanResult> results = new List<AntibanResult>();
            for (int i = 0; i < processes.Count; i++)
            {
                results.Add(new AntibanResult() { EventMessageId = processes[i].message.Id, SentDateTime = processes[i].time });
            }
            return results;
        }

        private class AntibanProcess
        {
            public EventMessage message { get; set; }
            public DateTime time { get; set; }
        }
    }
}