﻿using Akka.Actor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatMessages;
using Akka.Event;
using Akka.Configuration;


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
                

            });

            //прием сообщения "уничтожить всех"
            Receive<DestroyAllMessage>(msg =>
            {
                Become(Bad);
                Self.Tell(new DestroyAllMessage("Order66"));

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

            //выход клиента из чата!!!!неправильно: разослать список!!!
            Receive<LogOutMessage>(msg =>
            {
                //нужно изменить статус клиента и разослать список

                if (isThisObjectRegistered(msg.name) == true)
                {
                    Sender.Tell(new LogOutMessage(ID, msg.name), Self);
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

       
    }
}
