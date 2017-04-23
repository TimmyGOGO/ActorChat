using Akka.Actor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatMessages;
using System.Diagnostics;
using Akka.Serialization;
using Akka.Actor.Internal;
using Akka.Remote;

namespace Agent
{
    //для работы с помощниками:
    public class ActorHelper: ReceiveActor 
    {
        int N; //изначально всего агентов помощников
        int fromId; //присваиваем ID помощником начиная с fromID
        int count; //подсчет
        IActorRef chiefAgent; //главный агент
        List<recordItem> agentList; //список для агентов
        List<recordItem> fullList; //список всех участников чата 

        public ActorHelper()
        {
            count = 0;
            agentList = new List<recordItem>();
            fullList = new List<recordItem>();

            //получить сообщение на создание помощников:
            Receive<CreateHelpersMessage>(msg =>
            {
                chiefAgent = Sender;
                fromId = msg.fromID;
                N = msg.N;

                for (int i = 0; i < msg.N; i++)
                {
                    Process.Start("C:\\Users\\Artemij\\Source\\Repos\\Client\\AgentHelper\\AgentHelper\\bin\\Debug\\AgentHelper.exe",
                                    "akka.tcp://Agent@localhost:8000/user/AgentActor/ActorHelper" + " " + i);
                }
            
            });

            //получить сообщение идентификации: от нового помощника агента
            Receive<NewAgentHelperMessage>(msg =>
            {
                Console.WriteLine(msg.name + " " + Sender.Path.ToString() + " || " + (count+1) + "/" + N);
                agentList.Add(new recordItem(fromId + count, msg.name, Serialization.SerializedActorPath(Sender)));
                count++;

                //следить за агентом:
                Context.Watch(Sender);
                
                //готовы ли все помощники?
                if (count == N)
                {
                    //отослать сообщение главному актору (список агентов):
                    chiefAgent.Tell(new AddressListMessage(agentList), Self);

                }


            });

            //передача списка агентам-помощникам:
            Receive<AddressListMessage>(msg =>
            {
                Console.WriteLine("All is ready");
                
                
                //скопируем все данные из сообщения:
                fullList = new List<recordItem>();
                List<string> temp = new List<string>();
                foreach (recordItem i in msg.Values)
                {
                    fullList.Add(i);
                }

                //рассылаем список всем агентам:
                foreach (recordItem i in agentList)
                {
                    //i.address.Tell(new AddressListMessage(fullList), Self);
                    Console.WriteLine(i.address);



                    Context.ActorSelection(i.address).Tell(new ZippedAddressListMessage(), Self);
                    //ResolveActorRef(id)i.address.Tell(new NewAgentHelperMessage("Bullshit", ""), Self);
                }
                


            });



            
        }

    }
}
