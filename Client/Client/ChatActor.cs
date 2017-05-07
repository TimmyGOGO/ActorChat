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

                if (message == "history")
                {
                    foreach (recordItem i in addressList)
                    {
                        if (!i.name.Contains("agent") && !isMySelf(i))
                        {
                            i.address.Tell(new RequestForHistoryMessage());
                            break;
                        }
                    }
                }
                else
                {
                    foreach (recordItem i in addressList)
                    {
                        if (!i.name.Contains("agent"))
                        {
                            i.address.Tell(new ReadMessage(clientName + ':' + message));
                        }
                        
                    }

                    currentMessage = message;
                }
            });

            // Полученное сообщение от другого клиента
            Receive<ReadMessage>(msg =>
            {
                Console.WriteLine(msg.text);
                addToHistory(msg.text);

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
                for (int i = 0; i < splits.Count(); i++)
                {
                    Console.WriteLine(splits[i]);
                }
            });

            Receive<InfoMessage>(msg =>
            {
                Console.WriteLine(msg.text);
            });

            //получение запроса на разрегистрацию:
            Receive<RemoveClientMessage>(msg =>
            {
                ActorSelection linkPoint = Context.ActorSelection(agentAddress);
                linkPoint.Tell(new ClientOutMessage(new recordItem(this.clientID, this.clientName, Self)));

                Console.WriteLine("Unreg.application has been sent!");

                addressList.Clear();
                historyList.Clear();
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
                    ActorSelection linkPoint = Context.ActorSelection(agentAddress);
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

            Receive<LogOutClientMessage>(msg =>
            {
                Console.WriteLine("I've got: " + msg.rItem.ToString());
                for (int i = 0; i < addressList.Count; i++)
                {
                    if (addressList[i].name == msg.rItem.name && addressList[i].ID == msg.rItem.ID)
                    {
                        //удалить из списка данный элемент:
                        addressList.Remove(addressList[i]);
                    }

                }
                //addressList.Remove(msg.rItem);
                Console.WriteLine(msg.rItem);
                Console.WriteLine("Updated addressList:");

                Console.WriteLine("Пользователь " + msg.rItem.name + " покинул чат");
                foreach (recordItem i in addressList)
                {
                    Console.WriteLine(i.ToString());
                }

            });

            //обновление списка для клиента:
            Receive<LogOutClientAddressListMessage>(msg =>
            {
                addressList.Clear();
                addressList = msg.Values.ToList<recordItem>();
                
                foreach (recordItem i in msg.Values)
                {
                    //рассылаем другим агентам измененный список:
                    if (!isMySelf(i))
                    {
                        i.address.Tell(new LogOutClientMessage(new recordItem(this.clientID, this.clientName, Self)));
                    }
                }

            });

            // Получение списка адресов (после входа в чат)
            Receive<AddressListMessage>(msg =>
            {
                addressList.Clear();
                IReadOnlyCollection<recordItem> list = msg.Values;

                foreach (recordItem i in msg.Values)
                {

                    //добавление клиентов в список, если:
                    //есть адрес - т.е. клиент онлайн
                    if (i.address != null)
                    {
                        Console.WriteLine(i.ToString());
                        addressList.Add(i);

                        //рассылаем другим агентам свои данные:
                        if (i.ID != this.clientID && i.name != this.clientName)
                        {
                            i.address.Tell(new NewClientEnterMessage(new recordItem(this.clientID, this.clientName, Self)));
                        } 
                        
                    }

                }
            });

            //получение сообщения, что новый клиент вошел в чат:
            Receive<NewClientEnterMessage>(msg =>
            {
                Console.WriteLine("I've got: " + msg.rItem.ToString());
                addressList.Add(msg.rItem);
                Console.WriteLine("Updated addressList:");
                foreach (recordItem i in addressList)
                {
                    Console.WriteLine(i.ToString());
                }

            });

            //прием сообщения "уничтожить всех"
            Receive<DestroyAllMessage>(msg =>
            {
                Context.System.Terminate();
                Console.WriteLine("Destroyed");
                Environment.Exit(0);
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

        public bool isMySelf(recordItem item)
        {
            if (item.ID == clientID && item.name == clientName)
            {
                return true;
            }

            return false;
        }



    }
}
