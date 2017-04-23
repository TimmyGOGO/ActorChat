﻿using Akka.Actor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatMessages;


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

            //обработка входящих сообщений:
            //сообщение регистрации:
            Receive<RegMessage>(msg =>
            {
                //пользователя с таким именем нет:
                if (isThisObjectRegistered(msg.name) == false)
                {
                    ID++;
                    recordItem curr = new recordItem(ID, msg.name, null);
                    fullList.Add(curr);
                    Sender.Tell(new RegMessage(ID, msg.name), Self);
                }
                //пользователь с таким именем есть:
                else
                {
                    Sender.Tell(new RegMessage(-1, msg.name), Self);
                } 

            });

            //сообщение: создать помощников агента:
            Receive<CreateHelpersMessage>(msg =>
            {
                //создать актора для работы с помощниками:
                Nhelpers = msg.N;
                actorHelper = Context.ActorOf(Props.Create<ActorHelper>(), "ActorHelper");
                actorHelper.Tell(new CreateHelpersMessage(Nhelpers, ID), Self);
                ID += Nhelpers;

            });

            //сообщение "помощники агента":
            Receive<AddressListMessage>(msg =>
            {
                //добавляем данные помощников:
                foreach (recordItem i in msg.Values)
                {
                    fullList.Add(i);
                }

                //отсылаем готовый полный список ActorHelper:
                Sender.Tell(new AddressListMessage(fullList), Self);

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
