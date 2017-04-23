using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;

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
    public class recordItem
    {
        public int ID { get; private set; }
        public string name { get; private set; }
        public string address { get; private set; }

        public recordItem(int _id, string _name, string _addr)
        {
            ID = _id;
            name = _name;
            address = _addr;
        }


        //public static ZippedAddressListMessage zipToMessage(List<recordItem> l)
        //{
        //    List<string> temp = new List<string>();
            
        //}

        public override string ToString()
        {
            return String.Format("{0} {1} {2}", ID, name, address);
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
        public ZippedAddressListMessage(List<string> list)
        {
            this.Values = list.AsReadOnly();
        }

        public IReadOnlyCollection<string> Values { get; private set; }

    }

    //-----------------------------------
    //Юля Журавлева
    //-----------------------------------


}
