﻿using Akka.Actor;
using Akka.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using ChatMessages;

namespace Agent
{
    class Program
    {
        
        static void Main(string[] args)
        {
            string currAddress = @"
        akka {{  
            actor {{
                provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
            }}
            remote {{
                helios.tcp
                        {{
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
                    var localChatActor = actorSystem.ActorOf(Props.Create<ChatActor>(), "AgentActor");
                    
                    string line = string.Empty;
                    while (line != "exit")
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