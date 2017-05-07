using Akka.Actor;
using Akka.Configuration;
using Akka.Event;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AgentHelper
{
    class Program
    {
        static void Main(string[] args)
        {
            //основные настройки содержатся в файле конфигураций "App.config"
            //адрес (порт) для клиента выбирается произвольно системой из списка доступных адресов

            string actorSystemName = ConfigurationManager.AppSettings["actorSystemName"];
            Console.Title = actorSystemName + args[1];

            try
            {
                using (var actorSystem = ActorSystem.Create(actorSystemName))
                {


                    //ВАЖНО!
                    //актор-помощник создается программно и берет аргументы из командной строки:
                    //адрес актора "Главного агента" (arg[0]) и имя (arg[1]):

                    var localChatActor = actorSystem.ActorOf(Props.Create<AgentHelperActor>(args[0], args[1]), "AgentHelperActor");
                    //var localChatActor = actorSystem.ActorOf(Props.Create<AgentHelperActor>("akka.tcp://Agent@localhost:8000/user/AgentActor/ActorHelper", 2+""), "AgentHelperActor");

                    string line = string.Empty;
                    while (line != null)
                    {
                        line = Console.ReadLine();

                        //проверка сообщения:
                        //1.проверка имени:
                        //string[] splits = line.Split(new Char[] { '#' });
                        //if (splits[1].Contains("agent") != true)
                        //{
                        //    localChatActor.Tell(line);

                        //}
                        //else
                        //{
                        //    Console.WriteLine("Change the name! It mustn't contain word 'agent'!");
                        //}

                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }
    }
}
