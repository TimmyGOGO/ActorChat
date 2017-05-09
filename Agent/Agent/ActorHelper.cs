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
                Console.WriteLine(msg.name + " " + Sender.Path.ToString() + " || " + getNewID() + "/" + N);
                
                agentList.Add(new recordItem(fromId + count, msg.name, Sender));
                count++;
                
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

                //рассылаем список всем помощникам:
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

            //помощник вылетел:
            Receive<HelperFailedMessage>(msg =>
            {
                Console.WriteLine(msg.rItem.ToString());
                for (int i = 0; i < agentList.Count; i++)
                {
                    if (agentList[i].name == msg.rItem.name)
                    {
                        agentList.RemoveAt(i);
                        Console.WriteLine("AAAAAAAAAAAAAAAAAAAAAAAAAARGh!");
                    }
                }
                
                count--;

                //рассылаем плохого помощника остальным агентам (гарантируется, что агентов создается как минимум два):
                Console.WriteLine("The edited agentList:");
                foreach (recordItem i in agentList)
                {
                    i.address.Tell(new HelperFailedMessage(msg.rItem), Self);
                    Console.WriteLine(i.ToString());
                }


            });

            //прием сообщения от главного актора "список для восстановления"
            Receive<ListForRestoringMessage>(msg =>
            {
                foreach (recordItem i in msg.Values)
                {
                    if (i.name.Contains("agent"))
                    {
                        agentList.Add(i);
                        count++;
                        i.address.Tell(new AddressListMessage(msg.Values.ToList<recordItem>()), Self);
                    }
                }
                N = count;
                chiefAgent = Sender; 
                
            });

            Receive<ClientOutMessage>(msg =>
            {
                //рассылаем список всем агентам:
                foreach (recordItem i in agentList)
                {
                    i.address.Tell(new ClientOutMessage(msg.rItem), Self);
                }

            });

            Receive<LogOutClientMessage>(msg =>
            {
                //рассылаем список всем агентам:
                foreach (recordItem i in agentList)
                {
                    i.address.Tell(new LogOutClientMessage(msg.rItem), Self);
                }

            });


            
        }

        //выдать новый ID для помощника:
        public int getNewID()
        {
            int[] arr = new int[N]; //массив ID для помощников
            //посмотрим, какого ID нет, его и выдаем:
            foreach (recordItem i in agentList)
            {
                arr[i.ID] = 1;
            }

            for (int i = 0; i < N; i++)
            {
                if (arr[i] == 0)
                {
                    return i; 
                }
            }

            return count + 1;
        }

    }
}
