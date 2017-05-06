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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Akka.Actor.Dsl;

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

        public ActorHelper()
        {
            count = 0;
            agentList = new List<recordItem>();

            //получить сообщение на создание помощников:
            Receive<CreateHelpersMessage>(msg =>
            {

                chiefAgent = Sender;
                fromId = msg.fromID;
                N = msg.N;

                for (int i = fromId; i < msg.N; i++)
                {
                    Process.Start("C:\\Users\\Artemij\\Source\\Repos\\ActorChat\\AgentHelper\\AgentHelper\\bin\\Debug\\AgentHelper.exe",
                                    "akka.tcp://Agent@localhost:8000/user/AgentActor/ActorHelper" + " " + i);
                }
            
            });

            //получить сообщение идентификации: от нового помощника агента
            Receive<NewAgentHelperMessage>(msg =>
            {
                Console.WriteLine("Received message from agentHelper!");
                Console.WriteLine(msg.name + " " + Sender.Path.ToString() + " || " + (count+1) + "/" + N);
                agentList.Add(new recordItem(fromId + count, msg.name, Sender));
                count++;

                //следить за помощником: (нужен другой способ!)
                Context.Watch(Sender);
                
                //готовы ли все помощники?
                if (count == N)
                {
                    //отослать сообщение главному актору (список агентов):
                    chiefAgent.Tell(new AddressListMessage(agentList), Self);

                }


            });

            
            //Receive<Terminated>(msg =>
            //{
            //    int tempID = 0;
            //    foreach(recordItem i in agentList){
            //        if(i.address == msg.ActorRef){
            //            tempID = i.ID;
            //            fromId = msg.fromID;
            //            N = msg.N;

            //            for (int i = 0; i < msg.N; i++)
            //            {
            //                Process.Start("C:\\Users\\Artemij\\Source\\Repos\\ActorChat\\AgentHelper\\AgentHelper\\bin\\Debug\\AgentHelper.exe",
            //                                "akka.tcp://Agent@localhost:8000/user/AgentActor/ActorHelper" + " " + i);
            //            }
            //        }
            //    }


                

            //});

            //передача списка агентам-помощникам:
            Receive<AddressListMessage>(msg =>
            {
                Console.WriteLine("All is ready");

                //рассылаем список всем агентам:
                foreach (recordItem i in agentList)
                {
                    i.address.Tell(new AddressListMessage(msg.Values.ToList<recordItem>()), Self);
                    Console.WriteLine(i.address);
                    
                }

            });

            //передача новой информации о клиенте помощникам:
            Receive<NewClientEnterMessage>(msg =>
            {
                //рассылаем список всем агентам:
                foreach (recordItem i in agentList)
                {
                    i.address.Tell(new NewClientEnterMessage(msg.rItem), Self);
                }

            });



            
        }

    }
}
