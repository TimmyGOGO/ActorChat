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
        string clientName;
        int clientID;

        //список других участников чата
        //список сообщений в чате 
        
        public ChatActor()
        {

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

            Receive<string>(msg =>
            {
                //parsing string:
                string[] splits = msg.Split(new Char[] {'#'});

                //РЕГИСТРАЦИЯ: для Клиента в этом случае не нужно создавать отдельный актор.
                //Так как в этот момент он не в чате и не взаимодействует с другими акторами
                if (splits[0] == "reg")
                {
                    ActorSelection linkPoint = Context.ActorSelection("akka.tcp://Agent@localhost:8000/user/AgentActor");
                    linkPoint.Tell(new RegMessage(0, splits[1]), Self);
                    
                    Console.WriteLine("Reg.application has been sent!");
                }
                else
                {
                    Console.WriteLine(msg.ToString());

                }

            });

        }

    }
}
