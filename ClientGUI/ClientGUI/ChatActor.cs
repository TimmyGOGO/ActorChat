using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatMessages;
using Akka.Util;
using System.Windows.Forms;
using Akka.Event;


namespace ClientGUI
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

        //ссылка на визуальное отображение данных:
        Form1 formExplorer;

        
        public ChatActor(Form1 form)
        {
            formExplorer = form;

            addressList = new List<recordItem>(0);
            historyList = new List<historyItem>(1);
            currentMessage = "";
            deliveryReportCount = 0;
            clientName = "";
            clientID = -1;

            //РЕГИСТРАЦИЯ:
            //запрос главному агенту
            Receive<NewClientMessage>(msg =>
            {
                //РЕГИСТРАЦИЯ: для Клиента в этом случае не нужно создавать отдельный актор.
                //Так как в этот момент он не в чате и не взаимодействует с другими акторами
                ActorSelection linkPoint = Context.ActorSelection(agentAddress);
                linkPoint.Tell(new RegMessage(0, msg.name));

                MessageBox.Show("Reg.application has been sent!");
            });

            //прием сообщения от главного агента:
            Receive<RegMessage>(msg =>
            {
                if (msg.ID != -1)
                {
                    clientName = msg.name;
                    clientID = msg.ID;

                    //пришло сообщение:
                    formExplorer.chatWindow.Text = "";
                    formExplorer.textRegName.Text = msg.name.ToString();
                    formExplorer.textRegName.Enabled = false;
                    formExplorer.chatWindow.Text += "You have registered: " + msg.ID + " " + msg.name + "\n";

                }
                else
                {
                    MessageBox.Show("Canceled: Change the name!");
                }
                
            });

            //ВХОД В ЧАТ:
            //запрос на вход в чат
            Receive<LoginMessage>(msg =>
            {
                if (clientID != -1) //если клиент зарегистрирован
                {
                    ActorSelection linkPoint = Context.ActorSelection(agentAddress);
                    linkPoint.Tell(new LoginMessage(this.clientID, this.clientName), Self);
                    MessageBox.Show("Login message has been sent!");
                }
                else
                {
                    MessageBox.Show("You must be registered in order to enter the chat!");
                }


            });

            //после входа в чат (получение списка от Агента):
            Receive<AddressListMessage>(msg =>
            {
                addressList.Clear();
                formExplorer.chatWindow.Text = "";
                formExplorer.listOfClients.Text = "";
                IReadOnlyCollection<recordItem> list = msg.Values;

                foreach (recordItem i in msg.Values)
                {

                    //добавление клиентов в список, если:
                    //есть адрес - т.е. клиент онлайн
                    if (i.address != null)
                    {
                        if (!i.name.Contains("agent"))
                        {
                            formExplorer.listOfClients.Text += i.name.ToString() + "\n";
                        }
                        
                        addressList.Add(i);

                        //рассылаем другим агентам свои данные:
                        if (i.ID != this.clientID && i.name != this.clientName)
                        {
                            i.address.Tell(new NewClientEnterMessage(new recordItem(this.clientID, this.clientName, Self)));
                        } 
                        
                    }

                }

                //после того, как все получили: берем историю:
                Self.Tell(new WriteMessage("history"));

            });

            //РАБОТА В ЧАТЕ:
            //сообщение в чат: история/обычное сообщение клиентам
            Receive<WriteMessage>(msg =>
            {
                string message = msg.text;

                if (message == "history") //запрос истории у первого попавшегося клиента
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
                else //сообщение клиента другим клиентам:
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

            //прием сообщения от другого клиента:
            Receive<ReadMessage>(msg =>
            {
                formExplorer.chatWindow.Text += msg.text.ToString() + "\n";
                addToHistory(msg.text);

            });

            //Запрос на получение истории сообщений.
            Receive<RequestForHistoryMessage>(msg =>
            {
                Sender.Tell(new HistoryMessage(getHistoryList()));

            });

            //Получение истории сообщений.
            Receive<HistoryMessage>(msg =>
            {
                // выводит полученную историю на экран
                string[] splits = msg.history.Split(new Char[] { '#' });
                for (int i = 0; i < splits.Count(); i++)
                {
                    formExplorer.chatWindow.Text += splits[i] + "\n";
                }
            });

            Receive<InfoMessage>(msg =>
            {
                Console.WriteLine(msg.text);
            });

            //ПОЯВЛЕНИЕ НОВОГО КЛИЕНТА В ЧАТЕ:
            //получение сообщения, что новый клиент вошел в чат:
            Receive<NewClientEnterMessage>(msg =>
            { 
                addressList.Add(msg.rItem);

                formExplorer.chatWindow.Text += msg.rItem.name.ToString() + " entered the chat!\n";
                formExplorer.chatWindow.Text += "List of Clients updated!\n";
                formExplorer.listOfClients.Text = "";
                foreach (recordItem i in addressList)
                {
                    if (!i.name.Contains("agent"))
                    {
                        formExplorer.listOfClients.Text += i.name.ToString() + "\n";
                    }
                    
                }

            });

            //ВЫХОД ИЗ ЧАТА:
            //выход клиента из чата
            Receive<LogOutMessage>(msg =>
            {
                if (clientID != -1) //если клиент зарегистрирован
                {
                    ActorSelection linkPoint = Context.ActorSelection("akka.tcp://Agent@localhost:8000/user/AgentActor");
                    linkPoint.Tell(new LogOutMessage(this.clientID, this.clientName), Self);
                    MessageBox.Show("Logout message has been sent!");
                    formExplorer.listOfClients.Text = "";
                }
                else
                {
                    MessageBox.Show("You must be registered!");
                }

            });

            //уведомление о выходе клиента из чата:
            Receive<LogOutClientMessage>(msg =>
            {
                Console.WriteLine("I've got: " + msg.rItem.ToString());
                for (int i = 0; i < addressList.Count; i++)
                {
                    if (addressList[i].name == msg.rItem.name && addressList[i].ID == msg.rItem.ID)
                    {
                        //удалить из списка данный элемент:
                        addressList.RemoveAt(i);
                    }

                }
                //addressList.Remove(msg.rItem);
                //MessageBox.Show(msg.rItem.ToString());
                formExplorer.chatWindow.Text += msg.rItem.name + " left the chat!\n";
                formExplorer.chatWindow.Text += "List of Clients updated!\n";
                formExplorer.listOfClients.Text = "";

                foreach (recordItem i in addressList)
                {
                    if(!i.name.Contains("agent") && i.address != null)
                    {
                        formExplorer.listOfClients.Text += i.name.ToString() + "\n";
                    }
                    
                }

            });

            //обновление списка для клиентов:
            Receive<LogOutClientAddressListMessage>(msg =>
            {
                addressList.Clear();
                addressList = msg.Values.ToList<recordItem>();
                
                foreach (recordItem i in msg.Values)
                {
                    //рассылаем другим измененный список:
                    if (!isMySelf(i))
                    {
                        i.address.Tell(new LogOutClientMessage(new recordItem(this.clientID, this.clientName, Self)));
                    }
                }

            });

            //ВЫЛЕТ КЛИЕНТА ИЗ ЧАТА:
            //поймать мертвое сообщение:
            Receive<Debug>(msg =>
            {
                string[] splits = msg.ToString().Split(new Char[] { ' ' });
                if (splits[3].Contains("Disassociated") && splits[6].Contains("Client")) //проверяем, что вылетел клиент!
                {
                    Console.WriteLine("{0} is Dead!", splits[6]);
                    for (int i = 0; i < addressList.Count; i++)
                    {
                        if (addressList[i].address.ToString().Contains(splits[6])) //найдем мертвого клиента
                        {
                            formExplorer.chatWindow.Text += addressList[i].name + " left the chat!\n";
                            formExplorer.chatWindow.Text += "List of Clients updated!\n";
                            formExplorer.listOfClients.Text = "";
                            
                            //удаляем клиента:
                            addressList.RemoveAt(i);

                            foreach (recordItem cl in addressList)
                            {
                                if (!cl.name.Contains("agent") && cl.address != null)
                                {
                                    formExplorer.listOfClients.Text += cl.name.ToString() + "\n";
                                }

                            }
                            break;

                        }

                    }
                }

            });

            //РАЗРЕГИСТРАЦИЯ:
            //запрос на разрегистрацию:
            Receive<RemoveClientMessage>(msg =>
            {
                ActorSelection linkPoint = Context.ActorSelection(agentAddress);
                linkPoint.Tell(new ClientOutMessage(new recordItem(this.clientID, this.clientName, Self)));

                MessageBox.Show("Unreg.application has been sent!");

                addressList.Clear();
                historyList.Clear();

                formExplorer.listOfClients.Text = "";
                formExplorer.chatWindow.Text = "";
                formExplorer.textRegName.Text = "";
                formExplorer.textRegName.Enabled = true;

            });

            //УНИЧТОЖЕНИЕ:
            //прием сообщения "уничтожить всех"
            Receive<DestroyAllMessage>(msg =>
            {
                Context.System.Terminate();
                Console.WriteLine("Destroyed");
                formExplorer.Close();
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
