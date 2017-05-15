using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using System.Collections;
using System.Runtime.Serialization;

namespace ChatMessages
{
    //сообщение регистрации:
    public class RegMessage
    {
        public RegMessage(int _ID, string _name)
        {
            this.ID = _ID;
            this.name = _name;
        }
        public int ID { get; private set; }
        public string name { get; private set; }

        public override string ToString()
        {
            return this.ID + " " + this.name;
        }
    } 

    //сообщение: создай агентов-помощников
    public class CreateHelpersMessage
    {
        public CreateHelpersMessage(int number, int _fromID)
        {
            N = number;
            fromID = _fromID;
        }
        public int N { get; private set; }
        public int fromID { get; private set; }
    
    }

    //сообщение: новый агент-помощник:
    public class NewAgentHelperMessage
    {
        public NewAgentHelperMessage(string _name, string _address)
        {
            name = _name;
            address = _address;
        }
        public string name { get; private set; }
        public  string address { get; private set; }
    
    }

    //класс для строчки общего списка адресов:
    [Serializable()]
    public class recordItem: ISerializable
    {
        public int ID { get; private set; }
        public string name { get; private set; }
        public IActorRef address { get; private set; }

        public recordItem(int _id, string _name, IActorRef _addr)
        {
            ID = _id;
            name = _name;
            address = _addr;
        }

        public recordItem(recordItem item)
        {
            ID = item.ID;
            name = item.name;
            address = item.address;
        }

        public override string ToString()
        {
            string _addr = (this.address == null) ? "none" : this.address.Path.ToString();
            return String.Format("{0} {1} {2}", ID, name, _addr);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("recordID", this.ID);
            info.AddValue("recordName", this.name);
            info.AddValue("recordAddress", this.address.ToString());
            
        }
    }


    //сообщение: список адресов
    
    public class AddressListMessage
    {
        public AddressListMessage(List<recordItem> list)
        {
            this.Values = list.AsReadOnly();
        }

        public IReadOnlyCollection<recordItem> Values { get; private set; }

    }

    //Сообщение "помощник вылетел"
    public class HelperFailedMessage
    {
        public HelperFailedMessage(recordItem item)
        {
            this.rItem = new recordItem(item.ID, item.name, item.address);
        }

        public recordItem rItem { get; private set; }

        public override string ToString()
        {
            return this.rItem.ToString();
        }
    }

    //Сообщение: "уничтожить всех"
    public class DestroyAllMessage
    {
        public DestroyAllMessage(string _text)
        {
            text = _text;
        }
        
        public string text { get; private set; }
    }

    //Сообщение: "восстановить"
    public class RestoreMessage
    {
        public RestoreMessage(string _addr)
        {
            helper = _addr;
        }

        public string helper { get; private set; }
    }

    //Сообщение от помощника "список для восстановления":
    public class ListForRestoringMessage
    {
        public ListForRestoringMessage(List<recordItem> list)
        {
            this.Values = list.AsReadOnly();
        }

        public IReadOnlyCollection<recordItem> Values { get; private set; }

    }

    //-----------------------------------
    //Юлия Журавлева
    //-----------------------------------

    // Класс представления строки истории сообщений
    // В дальнейшем должен появиться класс-контейнер (будет доступ к последнему используемому номеру сообщения)
    public class historyItem
    {
        public int number;
        public string text;

        public historyItem(int _number, string _text)
        {
            this.number = _number;
            this.text = _text;
        }

        public historyItem()
        {
            this.number = -1;
            this.text = "Text";
        }

        public override string ToString()
        {
            return String.Format("{0}${1}", this.number, this.text);
        }

    }



    public class NewClientMessage
    {
        public NewClientMessage(string _name)
        {
            name = _name;
        }

        public string name { get; private set; }
    }

    public class RemoveClientMessage
    {
        public RemoveClientMessage(string _name)
        {
            name = _name;
        }

        public string name { get; private set; }
    }

    //Сообщение "клиент разрегистрировался" (данные клиента, который удаляет аккуант)
    public class ClientOutMessage
    {
        public ClientOutMessage(recordItem item)
        {
            this.rItem = new recordItem(item.ID, item.name, item.address);
        }

        public recordItem rItem { get; private set; }

        public override string ToString()
        {
            return this.rItem.ToString();
        }
    }


    // Сообщение из Program актору.
    public class WriteMessage
    {
        public WriteMessage(string _text)
        {
            text = _text;
        }

        public string text { get; private set; }
    }

    // Сообщение от клиента клиентам.
    public class ReadMessage
    {
        public ReadMessage(string _text)
        {
            text = _text;
        }

        public string text { get; private set; }
    }

    // Отчет о доставке.
    public class DeliveryReportMessage
    {
        public DeliveryReportMessage()
        {
            message = "Done!";
        }

        string message;
    }
    // Запрос истории сообщений.
    public class RequestForHistoryMessage
    {
        public RequestForHistoryMessage()
        {
            message = "History";
        }

        string message;
    }

    public class HistoryMessage
    {
        public HistoryMessage(string _history)
        {
            history = _history;
        }

        public string history { get; private set; }
    }

    public class UnregMessage
    {
        public UnregMessage(string _name)
        {
            name = _name;
        }

        public string name { get; private set; }
    }

    // сообщение разрегистрации клиента.
    public class UnregClientMessage
    {
        public UnregClientMessage(recordItem item)
        {
            this.rItem = new recordItem(item.ID, item.name, item.address);
        }

        public recordItem rItem { get; private set; }

        public override string ToString()
        {
            return this.rItem.ToString();
        }
    }

    public class UnregClientAddressListMessage
    {
        public UnregClientAddressListMessage(List<recordItem> list)
        {
            this.Values = list.AsReadOnly();
        }

        public IReadOnlyCollection<recordItem> Values { get; private set; }
    }

    // Информационное сообщение.
    public class InfoMessage
    {
        public InfoMessage(string _text)
        {
            text = _text;
        }

        public string text { get; private set; }
    }

    //-----------------------------------
    //Настя Боброва
    //-----------------------------------
    //Сообщение входа в чат
    public class LoginMessage
    {
        public LoginMessage(int _ID, string _name)
        {
            this.ID = _ID;
            this.name = _name;
        }
        public int ID { get; private set; }
        public string name { get; private set; }

        public override string ToString()
        {
            return this.ID + " " + this.name;
        }
    }

    //сообщение выхода из чата
    public class LogOutMessage
    {
        public LogOutMessage(int _ID, string _name)
        {
            this.ID = _ID;
            this.name = _name;
        }
        public int ID { get; private set; }
        public string name { get; private set; }

        public override string ToString()
        {
            return this.ID + " " + this.name;
        }
    }

    //Сообщение "я в чате" (данные клиента, который вошел в чат)
    public class NewClientEnterMessage
    {
        public NewClientEnterMessage(recordItem item)
        {
            this.rItem = new recordItem(item.ID, item.name, item.address);
        }

        public recordItem rItem { get; private set; }

        public override string ToString()
        {
            return this.rItem.ToString();
        }
    }

    //
    public class LogOutClientAddressListMessage
    {
        public LogOutClientAddressListMessage(List<recordItem> list)
        {
            this.Values = list.AsReadOnly();
        }

        public IReadOnlyCollection<recordItem> Values { get; private set; }
    }

    //
    public class LogOutClientMessage
    {
        public LogOutClientMessage(recordItem item)
        {
            this.rItem = new recordItem(item.ID, item.name, item.address);
        }

        public recordItem rItem { get; private set; }

        public override string ToString()
        {
            return this.rItem.ToString();
        }
    }

}
