using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatMessages;
using Akka.Util;


namespace Client
{

    public class ChatActor : ReceiveActor
    {
        List<recordItem> addressList;
        List<historyItem> historyList;
        string agentAddress = "akka.tcp://Agent@localhost:8000/user/AgentActor";
        String currentMessage; // Сообщение клиентам, для того, чтобы его добавить в историю, необходимо получить отчет о доставке от всех клиентов
        int deliveryReportCount;
        string clientName;
        int clientID;

        
        public ChatActor()
        {
            addressList = new List<recordItem>(0);
            historyList = new List<historyItem>(1);
            currentMessage = "";
            deliveryReportCount = 0;
            clientName = "";
            clientID = -1;

            // Отправляемое сообщение.
            Receive<WriteMessage>(msg =>
            {
                string message = msg.text;

                if (message == "History")
                {
                    var remoteChatActor = Context.ActorSelection(addressList.ElementAt(0).address.ToString());
                    remoteChatActor.Tell(new RequestForHistoryMessage());
                }
                else if (message == "Exit") //Выход?
                {
                    ActorSelection linkPoint = Context.ActorSelection(agentAddress);
                    linkPoint.Tell(new ExitMessage(clientName));

                    // Если это последний клиент в сети, то он отправляет историю агенту.
                    if (addressList.Count == 0)
                    {
                        linkPoint.Tell(new HistoryMessage(getHistoryList()));
                    }
                }
                else
                {
                    foreach (recordItem i in addressList)
                    {
                        i.address.Tell(new ReadMessage(clientName + ':' + message));
                    }

                    currentMessage = message;
                }
            });

            // Полученное сообщение от другого клиента
            Receive<ReadMessage>(msg =>
            {
                Console.WriteLine(msg.text);
                //addToHistory(msg.text);
                //Sender.Tell(new DeliveryReportMessage());
            });

            // Отчет о доставке сообщения.
            Receive<DeliveryReportMessage>(msg =>
            {
                upgradeDeliveryReportCount();
            });

            // Запрос на получение истории сообщений.
            Receive<RequestForHistoryMessage>(msg =>
            {
                Sender.Tell(new HistoryMessage(getHistoryList()));
            });

            // Получение истории сообщений.
            Receive<HistoryMessage>(msg =>
            {
                // выводит полученную историю на экран
                string[] splits = msg.history.Split(new Char[] { '#' });
                for (int i = 0; i < splits.Count(); ++i)
                {
                    Console.WriteLine(splits[i]);
                }
            });

            Receive<ErrorMessage>(msg =>
            {
                Console.WriteLine(msg.title);
                Console.WriteLine(msg.text);
            });

            // Запрос на регистрацию
            Receive<NewClientMessage>(msg =>
            {
                //РЕГИСТРАЦИЯ: для Клиента в этом случае не нужно создавать отдельный актор.
                //Так как в этот момент он не в чате и не взаимодействует с другими акторами
                ActorSelection linkPoint = Context.ActorSelection(agentAddress);
                linkPoint.Tell(new RegMessage(0, msg.name));

                Console.WriteLine("Reg.application has been sent!");
            });

            //Регистрация
            Receive<RegMessage>(msg =>
            {
                if (msg.ID != -1)
                {
                    clientName = msg.name;
                    clientID = msg.ID;

                    Console.WriteLine(msg.ToString());
                    Console.WriteLine("The registration account approved.");
                }
                else
                {
                    Console.WriteLine("Canceled: Change the name!");
                }

                Context.Watch(Sender);
              
                
            });

            Receive<Terminated>(t =>
            {
                Console.WriteLine(t.ToString());
                Console.WriteLine("Main agent has been switched off!");

            });

            //Сообщение входа в чат
            Receive<LoginMessage>(msg =>
            {
                if (clientID != -1) //если клиент зарегистрирован
                {
                    ActorSelection linkPoint = Context.ActorSelection("akka.tcp://Agent@localhost:8000/user/AgentActor");
                    linkPoint.Tell(new LoginMessage(this.clientID, this.clientName), Self);
                    Console.WriteLine("Login message has been sent!");
                }
                else
                {
                    Console.WriteLine("You must be registered in order to enter the chat!");
                }


            });

            //выход клиента из чата
            Receive<LogOutMessage>(msg =>
            {
                if (clientID != -1) //если клиент зарегистрирован
                {
                    ActorSelection linkPoint = Context.ActorSelection("akka.tcp://Agent@localhost:8000/user/AgentActor");
                    linkPoint.Tell(new LogOutMessage(this.clientID, this.clientName), Self);
                    Console.WriteLine("Logout message has been sent!");
                }
                else
                {
                    Console.WriteLine("You must be registered!");
                }

            });

            // Получение списка адресов.
            Receive<AddressListMessage>(msg =>
            {
                addressList.Clear();
                IReadOnlyCollection<recordItem> list = msg.Values;

                foreach (recordItem i in msg.Values)
                {
                    Console.WriteLine(i.ToString());
                    addressList.Add(i);
                    Context.Watch(i.address);
                }
            });

        }

        //дополнительные функции:
        public void addToHistory(String _message)
        {
            int prevNumber = 0;

            if (historyList.Count > 0)
            {
                prevNumber = historyList.Last().number;
            }

            historyList.Add(new historyItem(prevNumber + 1, _message));
        }

        public string getHistoryList()
        {
            string list = "";

            for (int i = 0; i < historyList.Count; i++)
            {
                list += historyList.ElementAt(i).ToString();
                list += "#";
            }

            return list;
        }

        public bool checkDeliveryReportCount()
        {
            return deliveryReportCount == addressList.Count;
        }

        public void upgradeDeliveryReportCount()
        {
            deliveryReportCount++;

            if (checkDeliveryReportCount())
            {
                addToHistory(clientName + ':' + currentMessage);
                currentMessage = "";
                deliveryReportCount = 0;
            }
        }

    }
}
