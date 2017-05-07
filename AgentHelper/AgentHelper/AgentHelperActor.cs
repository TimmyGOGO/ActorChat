using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using ChatMessages;
using System.Diagnostics;
using Akka.Event;

namespace AgentHelper
{

    //ГЛАВНЫЙ АКТОР агента-помощника:
    class AgentHelperActor: ReceiveActor
    {
        int priority;
        ActorSelection seniorAgentActor;       
        List<recordItem> fullList;
        

        public AgentHelperActor(string addressSenior, string rank)
        {
            //создание пустого списка:
            fullList = new List<recordItem>();

            Console.WriteLine("Tell the chief about yourself.");

            try
            {
                //обработка аргументов:
                seniorAgentActor = Context.ActorSelection(addressSenior);
                Int32.TryParse(rank, out priority);

                //сообщаем о себе актору главного агента:
                seniorAgentActor.Tell(new NewAgentHelperMessage("agent" + priority, ""), Self);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            //прием общего списка:
            Receive<AddressListMessage>(msg =>
            {
                //полная передача списка:
                fullList = msg.Values.ToList<recordItem>();


            });

            //получаем сообщение "я в чате" от главного агента:
            Receive<NewClientEnterMessage>(msg =>
            {
                bool isElementFound = false;
                //заменяем значение адреса:
                for (int i = 0; i < fullList.Count; i++)
                {
                    if (fullList[i].name == msg.rItem.name && fullList[i].ID == msg.rItem.ID)
                    {
                        //нашли элемент в списке:
                        isElementFound = true;
                        //удалить из списка данный элемент:
                        fullList.Remove(fullList[i]);
                        //вставить новый с такими же ID и name, и новым адресом:
                        recordItem curr = new recordItem(msg.rItem.ID, msg.rItem.name, msg.rItem.address);
                        fullList.Add(curr);
                        break;
                    }

                }

                if (isElementFound == false) //если клиента в списке нет, то добавляем:
                {
                    recordItem curr = new recordItem(msg.rItem.ID, msg.rItem.name, msg.rItem.address);
                    fullList.Add(curr);
                }

                //Изменения в списке:
                Console.WriteLine("List's been changed!");
                foreach (recordItem f in fullList)
                {
                    Console.WriteLine(f.ToString());
                }

            });

            //главный агент рухнул:
            Receive<Terminated>(t =>
            {
                //подождать других! (иначе будет создано N главных агентов!
                //Process.Start("C:\\Users\\Artemij\\Source\\Repos\\Client\\Agent\\Agent\\bin\\Debug\\Agent.exe");
                Console.WriteLine("Chief Agent has been terminated!");

            });

            Receive<HelperFailedMessage>(msg =>
            {
                Console.WriteLine("AgentHelperActor:failed");
                if (amIcapableToRepair(msg.rItem)) //если он способен отремонтировать этого помощника
                {
                    Process.Start("C:\\Users\\Artemij\\Source\\Repos\\ActorChat\\AgentHelper\\AgentHelper\\bin\\Debug\\AgentHelper.exe",
                                        "akka.tcp://Agent@localhost:8000/user/AgentActor/ActorHelper" + " " + getNextPriority());
                }

            });

            //прием сообщения "уничтожить всех"
            Receive<DestroyAllMessage>(msg =>
            {
                Context.System.Terminate();
                Console.WriteLine("Destroyed");
                Environment.Exit(0);
            });

        }


        //ДОПОЛНИТЕЛЬНЫЕ МЕТОДЫ:
        //Я ПОДХОЖУ ДЛЯ ВОССТАНОВИТЕЛЬНОЙ ОПЕРАЦИИ?
        public bool amIcapableToRepair(recordItem failed)
        {
            
            foreach (recordItem i in fullList)
            {
                if (i.name.Contains("agent") && i.ID != failed.ID && i.name != failed.name) //просматриваю помощников, кроме падшего
                {
                    //беру приоритет из имени помощников
                    int newP = 0;
                    Int32.TryParse(i.name.Substring(5), out newP);
                    
                    //если меньше моего, то я не подхожу
                    if (newP < priority)
                    {
                        return false;
                    }
                }
            }

            //у меня самый высокий (самое низкое значение)
            Console.WriteLine("I will save my friend!");
            return true;

        }

        //ВЕРНУТЬ СЛЕДУЮЩИЙ ВЫСКОИЙ ПРИОРИТЕТ СРЕДИ ПОМОЩНИКОВ:
        public int getNextPriority()
        {
            int hPriority = 0;
            foreach (recordItem i in fullList)
            {
                if (i.name.Contains("agent"))
                {
                    //беру приоритет из имени помощников
                    int newP = 0;
                    Int32.TryParse(i.name.Substring(5), out newP);

                    //если меньше моего, то я не подхожу
                    if (newP > hPriority)
                    {
                        hPriority = newP;
                    }
                }
                
            }

            return hPriority + 1;
        }

        public void updateList(IReadOnlyCollection<recordItem> list)
        {
            fullList = new List<recordItem>(list);
            foreach (recordItem i in fullList)
            {
                Console.WriteLine(i.ToString());
            }
        }

        
    }
}
