using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using ChatMessages;
using System.Diagnostics;

namespace AgentHelper
{

    //ГЛАВНЫЙ АКТОР агента-помощника:
    class AgentHelperActor: ReceiveActor
    {
        int priority;
        ActorSelection seniorAgentActor;       
        List<recordItem> fullList;
        

        public AgentHelperActor(string addressSenior, string rank)
        {
            //создание пустого списка:
            fullList = new List<recordItem>();
            
            //обработка аргументов:
            seniorAgentActor = Context.ActorSelection(addressSenior);
            Int32.TryParse(rank, out priority);
            
            //сообщаем о себе актору главного агента:
            seniorAgentActor.Tell(new NewAgentHelperMessage("agent"+priority, ""), Self);
            
            //получаем сообщение-список от главного агента:
            Receive<ZippedAddressListMessage>(msg =>
            {
                Console.WriteLine("The entire list");
                //обновляем список:
                //fullList.Clear();
                //foreach (recordItem i in msg.Values)
                //{
                //    Console.WriteLine(i.ToString());
                //    //fullList.Add(i);
                //}
               
                /*
                foreach (recordItem i in fullList)
                {
                    Console.WriteLine(i.ToString());
                }
                */
                //следим за основным актором:
                //Context.Watch(Sender);

            });

            //главный агент рухнул:
            Receive<Terminated>(t =>
            {
                //подождать других! (иначе будет создано N главных агентов!
                //Process.Start("C:\\Users\\Artemij\\Source\\Repos\\Client\\Agent\\Agent\\bin\\Debug\\Agent.exe");
                Console.WriteLine("Chief Agent has been terminated!");

            });

            //тест:
            Receive<NewAgentHelperMessage>(msg =>
            {
                Console.WriteLine(msg.name);

            });


        }


        public void updateList(IReadOnlyCollection<recordItem> list)
        {
            fullList = new List<recordItem>(list);
            foreach (recordItem i in fullList)
            {
                Console.WriteLine(i.ToString());
            }
        }

        
    }
}
