using Akka.Actor;
using Akka.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using ChatMessages;
using Akka.Event;

namespace Agent
{
    class Program
    {
        
        static void Main(string[] args)
        {
            string currAddress = @"
        akka {{
  
            loglevel = DEBUG        
    
            actor {{
                provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
                serializers {{
                    hyperion = ""Akka.Serialization.HyperionSerializer, Akka.Serialization.Hyperion""
                }}
                serialization-bindings {{
                    ""System.Object"" = hyperion
                }}

            }}
            remote {{
                helios.tcp
                        {{
                            transport-class = ""Akka.Remote.Transport.Helios.HeliosTcpTransport, Akka.Remote""
                            transport-protocol = tcp
                            port = {0:D4}
                            hostname = localhost
                        }}
              
            }}
        }}
        ";
            //простой способ регистрации:
            //инициализация текущего актора:
            var config = ConfigurationFactory.ParseString(String.Format(currAddress, 8000));

            Console.Title = "Agent";

            try
            {
                using (var actorSystem = ActorSystem.Create("Agent", config))
                {
                    ////АКТОР, КОТОРЫЙ СЛЕДИТ ЗА МЕРТВЫМИ СООБЩЕНИЯМИ:
                    //var grimmWatcher = actorSystem.ActorOf(Props.Create<GrimmWatcherActor>(), "GrimmWatcher");
                    //actorSystem.EventStream.Subscribe(grimmWatcher, typeof(DeadLetter));

                    //ОСНОВНОЙ АКТОР ДЛЯ РАБОТЫ АГЕНТА
                    var localChatActor = actorSystem.ActorOf(Props.Create<ChatActor>(), "AgentActor");
                    actorSystem.EventStream.Subscribe(localChatActor, typeof(Debug));
                    //actorSystem.EventStream.Subscribe(localChatActor, typeof());

                    string line = string.Empty;
                    while (line != "e")
                    {
                        line = Console.ReadLine();
                        //parsing string:
                        string[] splits = line.Split(new Char[] { '#' });
                        if (splits[0] == "help")
                        {
                            int N = 0;
                            Int32.TryParse(splits[1], out N);
                            localChatActor.Tell(new CreateHelpersMessage(N, 0));
                        }
                        else if (splits[0] == "kill")
                        {
                            localChatActor.Tell(new DestroyAllMessage("Order66"));
                        }
                        
                    }

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            /*//Заметка для парсинга строк:
            recordItem cl = new recordItem();
            string[] splits = cInfo.Split(new Char[] { '\n', '\r', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            //должно быть 4 куска: в порядке: ID name address approved
            Int32.TryParse(splits[0], out cl.ID);
            cl.name = splits[1];
            cl.address = splits[2];
            Boolean.TryParse(splits[3], out cl.approved);
            */
        }
    }
}
