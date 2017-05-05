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


                    if (localChatActor != null)
                    {
                        string line = string.Empty;
                        while (line != null)
                        {
                            line = Console.ReadLine();

                            //проверка сообщения:
                            //1.проверка имени:
                            string[] splits = line.Split(new Char[] { '#' });

                            if (splits[0] == "reg")
                            {
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
