using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Event;

namespace Agent
{
    class GrimmWatcherActor: ReceiveActor
    {

        string chiefAddress = "akka.tcp://Agent@localhost:8000/user/AgentActor";

        public GrimmWatcherActor()
        {

            Receive<DeadLetter>(msg =>
            {
                

            });

        }

    }
}
