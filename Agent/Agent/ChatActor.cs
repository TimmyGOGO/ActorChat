using Akka.Actor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatMessages;
using Akka.Event;
using Akka.Configuration;
using System.IO;


namespace Agent
{
   
    //основной класс:
    public class ChatActor : ReceiveActor
    {
        int ID;
        List<recordItem> fullList;
        const int MAX_CAPACITY = 100;

        int Nhelpers; //кол-во помощников
        IActorRef actorHelper; //актор для создания помощников

        public ChatActor()
        {
            //инициализироваться:
            ID = 0;
            Nhelpers = 0;
            fullList = new List<recordItem>(MAX_CAPACITY);

            Good();
        }

        void Good(){
            //РЕГИСТРАЦИЯ:
            //сообщение регистрации:
            Receive<RegMessage>(msg =>
            {
                //пользователя с таким именем нет:
                if (isThisObjectRegistered(msg.name) == false)
                {
                    ID++;
                    recordItem curr = new recordItem(ID, msg.name, null);
                    fullList.Add(curr);
                    //добавили нового клиента (гарантируется, что помощники уже созданы):
                    Sender.Tell(new RegMessage(ID, msg.name), Self);
                    //отправляем нового клиента помощникам (передаем задачу актору для работы с помощниками):
                    actorHelper.Tell(new NewClientEnterMessage(curr));
                }
                //пользователь с таким именем есть:
                else
                {
                    Sender.Tell(new RegMessage(-1, msg.name), Self);
                } 

            });

            //РАБОТА С ПОМОЩНИКАМИ:
            //сообщение: создать помощников агента (создаются до добавления клиентов):
            Receive<CreateHelpersMessage>(msg =>
            {
                //создать актора для работы с помощниками:
                Nhelpers = msg.N;
                actorHelper = Context.ActorOf(Props.Create<ActorHelper>(), "ActorHelper");
                actorHelper.Tell(new CreateHelpersMessage(Nhelpers, ID), Self);
                ID += Nhelpers; //освобождает ID для помощников

            });

            //сообщение от "помощников агента":
            Receive<AddressListMessage>(msg =>
            {
                //добавляем данные помощников (если их нет еще в списке):
                foreach (recordItem i in msg.Values)
                {
                    if (!fullList.Contains(i))
                    {
                        fullList.Add(i);
                    }
                }

                //отсылаем готовый полный список ActorHelper:
                Sender.Tell(new AddressListMessage(fullList), Self);

            });

            //поймать мертвое сообщение:
            Receive <Debug> (msg =>
            {
                string[] splits = msg.ToString().Split(new Char[] { ' ' });
                if (splits[3].Contains("Disassociated") && splits[6].Contains("AgentHelper")) //проверяем, что вылетел помощник!
                {
                    Console.WriteLine("{0} is Dead!", splits[6]);
                    for(int i = 0; i < fullList.Count; i++)
                    {
                        if (fullList[i].address.ToString().Contains(splits[6])) //найдем мертвого помощника:
                        {
                            //удаляем из списка и говорим удалить его остальным:
                            recordItem curr = new recordItem(fullList[i]);
                            fullList.RemoveAt(i);

                            //распечатать список после вылета помощника:
                            Console.WriteLine("After fall of the helper:");
                            foreach (recordItem f in fullList)
                            {
                                Console.WriteLine(f.ToString());
                            }

                            actorHelper.Tell(new HelperFailedMessage(curr));
                            break;

                        }

                    }
                }
                else if (splits[3].Contains("Disassociated") && splits[6].Contains("Client")) //проверяем, что вылетел клиент!
                {
                    Console.WriteLine("{0} is Dead!", splits[6]);
                    for (int i = 0; i < fullList.Count; i++)
                    {
                        if (fullList[i].address.ToString().Contains(splits[6])) //найдем мертвого клиента:
                        {
                            //удалить из списка данный элемент:
                            recordItem curr = new recordItem(fullList[i].ID, fullList[i].name, null);
                            fullList.RemoveAt(i);
                            //вставить новый с такими же ID и name, и новым адресом:
                            fullList.Add(curr);

                            //Изменения в списке:
                            Console.WriteLine("List's been changed!");
                            foreach (recordItem f in fullList)
                            {
                                Console.WriteLine(f.ToString());
                            }

                            //отправляем нового клиента помощникам (передаем задачу актору для работы с помощниками):
                            actorHelper.Tell(new LogOutClientMessage(curr));
                            break;

                        }

                    }
                }

            });

            //прием сообщения "уничтожить всех"
            Receive<DestroyAllMessage>(msg =>
            {
                Become(Bad);
                Self.Tell(new DestroyAllMessage("Order66"));

            });

            //прием собщения "восстановить"
            Receive<RestoreMessage>(msg =>
            {
                //связаться с помощником и запросить восстановление:
                var helper = Context.ActorSelection(msg.helper);
                helper.Tell(new RestoreMessage(msg.helper));

            });

            //прием сообщения от помощника "список для восстановления"
            Receive<ListForRestoringMessage>(msg =>
            {
                //восстанавливаем главный актор:
                //общий список:
                fullList = msg.Values.ToList<recordItem>();
               
                //наибольший ID, кол-во помощников:
                ID = 0;
                Nhelpers = 0;
                foreach (recordItem i in fullList)
                {
                    if (ID < i.ID)
                    {
                        ID = i.ID;
                    }
                    if (i.name.Contains("agent"))
                    {
                        Nhelpers++;
                    }
                }
                //актор для создания помощников:
                actorHelper = Context.ActorOf(Props.Create<ActorHelper>(), "ActorHelper");
                actorHelper.Tell(new ListForRestoringMessage(fullList));

            });

            //РАЗРЕГИСТРАЦИЯ:
            //сообщение разрегистрации:
            Receive<ClientOutMessage>(msg =>
            {
                for (int i = 0; i < fullList.Count; i++)
                {
                    recordItem curr = fullList[i];
                    if (curr.name == msg.rItem.name && curr.ID == msg.rItem.ID)
                    {
                        fullList.RemoveAt(i);

                        //отправляем список чтобы разослать всем онлайн клиентам
                        Sender.Tell(new UnregClientAddressListMessage(fullList), Self);
                        //уведомляем помощников:
                        actorHelper.Tell(new ClientOutMessage(curr));
                        break;
                    }
                }

                //рассылаем остальным клиентам обновленный список:
                //foreach (recordItem i in fullList)
                //{
                //    //рассылаем другим измененный список:
                //    if (!i.name.Contains("agent"))
                //    {
                //        i.address.Tell(new LogOutClientMessage(new recordItem(msg.rItem.ID, msg.rItem.name, Self)));
                //    }
                //}

                //Изменения в списке:
                Console.WriteLine("List's been changed!");
                foreach (recordItem f in fullList)
                {
                    Console.WriteLine(f.ToString());
                }

                Sender.Tell(new InfoMessage("Done, your account has been deleted!"), Self);
            });

            //ВХОД И ВЫХОД ИЗ ЧАТА:
            //Обработка сообщения входа в чат:
            Receive<LoginMessage>(msg =>
            {
                //нужно изменить статус клиента и разослать список
                //проверяем что такой клиент зарегистрирован по его ID и имени:
                //гарантировано, что такой клиент в списке только один
                Console.WriteLine("I've got LoginMessage!");
                for (int i = 0; i < fullList.Count; i++)
                {
                    if (fullList[i].name == msg.name && fullList[i].ID == msg.ID)
                    {
                        //отправить историю, если он единственный онлайн
                        if (isFirstOnlineClient(fullList[i]))
                        {
                            StreamReader f = new StreamReader("History.txt");
                            Console.WriteLine("Read history from file");
                            string historyString = "";
                            string currentString = f.ReadLine();

                            while ((currentString != null && currentString != "\n"))
                            {
                                historyString += currentString;
                                historyString += "#";
                                currentString = f.ReadLine();
                            }

                            f.Close();

                            Sender.Tell(new HistoryMessage(historyString));
                        }

                        //удалить из списка данный элемент:
                        fullList.Remove(fullList[i]);
                        //вставить новый с такими же ID и name, и новым адресом:
                        recordItem curr = new recordItem(msg.ID, msg.name, Sender);
                        fullList.Add(curr);

                        //Изменения в списке:
                        Console.WriteLine("List's been changed!");
                        foreach (recordItem f in fullList)
                        {
                            Console.WriteLine(f.ToString());
                        }
                        //отправляем список клиенту
                        Sender.Tell(new AddressListMessage(fullList), Self);
                        
                        //отправляем нового клиента помощникам (передаем задачу актору для работы с помощниками):
                        actorHelper.Tell(new NewClientEnterMessage(curr));
                        break;
                    }

                }


            });

            //прием истории сообщений от последнего клиента онлайн
            Receive<HistoryMessage>(msg =>
            {
                string[] splits = msg.history.Split(new Char[] { '#' });

                try
                {
                    StreamWriter f = new StreamWriter("History.txt");

                    for (int i = 0; i < splits.Count(); i++)
                    {
                        f.WriteLine(splits[i]);
                    }

                    f.Close();

                    Console.WriteLine("I'm wrote history");

                }
                catch (Exception e)
                {
                    Console.WriteLine("Error StreamWriter: " + e.Message);
                    return;
                }
            });

            //ВЫХОД КЛИЕНТА из чата
            Receive<LogOutMessage>(msg =>
            {
                //нужно изменить статус клиента и разослать список

                Console.WriteLine("I've got LogOutMessage!");
                for (int i = 0; i < fullList.Count; i++)
                {
                    if (fullList[i].name == msg.name && fullList[i].ID == msg.ID)
                    {
                        //удалить из списка данный элемент:
                        fullList.RemoveAt(i);
                        //вставить новый с такими же ID и name, и новым адресом:
                        recordItem curr = new recordItem(msg.ID, msg.name, null);
                        fullList.Add(curr);

                        //Изменения в списке:
                        Console.WriteLine("List's been changed!");
                        foreach (recordItem f in fullList)
                        {
                            Console.WriteLine(f.ToString());
                        }

                        //отправляем список чтобы разослать всем онлайн клиентам
                        Sender.Tell(new LogOutClientAddressListMessage(fullList), Self);

                        //отправляем нового клиента помощникам (передаем задачу актору для работы с помощниками):
                        actorHelper.Tell(new LogOutClientMessage(curr));
                        break;
                    }

                }
            });


            
        }

        void Bad()
        {

            Receive<RegMessage>(msg => { Console.WriteLine("Back off!"); });
            Receive<CreateHelpersMessage>(msg => { Console.WriteLine("Back off!"); });
            Receive<AddressListMessage>(msg => { Console.WriteLine("Back off!"); });
            Receive<Debug>(msg =>  {  });
            Receive<LoginMessage>(msg => { Console.WriteLine("Back off!"); });
            Receive<LogOutMessage>(msg => { Console.WriteLine("Back off!"); }); 

            //прием сообщения "уничтожить всех"
            Receive<DestroyAllMessage>(msg =>
            {
                foreach (recordItem i in fullList)
                {
                    i.address.Tell(new DestroyAllMessage("Order66"));
                }
            });

            

        }

        //РАБОТА СО СПИСКОМ:
        //найти запись по ID:
        public recordItem findByIdinFullList(int id)
        {
            for (int i = 0; i < fullList.Count; i++)
            {
                if (fullList[i].ID == id)
                {
                    return fullList[i];
                }
            }

            return null;
        }

        //есть ли запись с таким именем в списке:
        public bool isThisObjectRegistered(string _name)
        {
            for (int i = 0; i < fullList.Count; i++)
            {
                if (fullList[i].name == _name)
                {
                    return true;
                }
            }

            return false;
        }

        // проверка на первого клиента-онлайн
        public bool isFirstOnlineClient(recordItem client)
        {
            bool flag = true;

            foreach (recordItem f in fullList)
            {
                // Если есть еще клиенты онлайн
                if (!f.name.Contains("agent") && f.address != null && client.ID != f.ID && client.name != f.name)
                {
                    flag = false;
                }
            }

            return flag;
        }

       
    }
}
