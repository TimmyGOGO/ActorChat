using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using System.Configuration;
using ChatMessages;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            //основные настройки содержатся в файле конфигураций "App.config"
            //адрес (порт) для клиента выбирается произвольно системой из списка доступных адресов
            
            string actorSystemName = ConfigurationManager.AppSettings["actorSystemName"];
            Console.Title = actorSystemName;

            try
            {
                using (var actorSystem = ActorSystem.Create("Client"))
                {

                    var localChatActor = actorSystem.ActorOf(Props.Create<ChatActor>(), "ChatActor");


                    bool Reg = false, Login = false;
                    if (localChatActor != null)
                    {
                        string line = string.Empty;
                        while (line != null)
                        {
                            line = Console.ReadLine();

                            //проверка сообщения:
                            //1.проверка имени:
                            string[] splits = line.Split(new Char[] { '#' });

                            if (splits[0] == "reg") //зарегистрироваться
                            {
                                if (Reg == false)
                                {
                                    Reg = true; 
                                    if (splits[1].Contains("agent") != true)
                                    {
                                        localChatActor.Tell(new NewClientMessage(splits[1]));
                                    }
                                    else
                                    {
                                        Console.WriteLine("Change the name! It mustn't contain word 'agent'!");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("You have already registered!");
                                } 
                                
                            }
                            else if (splits[0] == "logout") //выход из чата
                            {
                                if (Login == true)
                                {
                                    Login = false;
                                    localChatActor.Tell(new LogOutMessage(0, splits[0]));
                                }
                                else { Console.WriteLine("You haven't entered the chat!"); } 
                                
                            }
                            else if (splits[0] == "login") //вход в чат
                            {
                                if(Login == false)
                                {
                                    Login = true;
                                    localChatActor.Tell(new LoginMessage(0, splits[0]));
                                }
                                else { Console.WriteLine("You have already entered the chat!"); }

                            } 
                            else if (splits[0] == "unreg")
                            {
                                if (Reg == true)
                                {
                                    Reg = false;
                                    localChatActor.Tell(new RemoveClientMessage(splits[0]));
                                }
                                else { Console.WriteLine("You haven't registered or logined! Please register!"); }
                            }
                            else //написать сообщение в чат:
                            {
                                localChatActor.Tell(new WriteMessage(line)); 
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Could not get remote actor ref");
                        Console.ReadLine();
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }

        }
    }
}
