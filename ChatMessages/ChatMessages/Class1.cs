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

        public override string ToString()
        {
            return String.Format("{0} {1} {2}", ID, name, address.Path.ToString());
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

    //сообщение: сжатый список адресов
    public class ZippedAddressListMessage
    {
        public ZippedAddressListMessage(byte[] array)
        {
            values = new byte[array.Length];
            array.CopyTo(values, 0);
        
        }

        public byte[] values;

    }

    //-----------------------------------
    //Юля Журавлева
    //-----------------------------------


}
