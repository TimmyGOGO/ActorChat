using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using Akka.Actor;
using ChatMessages;
using Akka.Configuration;

namespace ClientGUI
{
    public partial class Form1 : Form
    {
        bool Reg; 
        bool Login;
        string clientName;
        int clientID;
        
        private IActorRef chatActor;

        public Form1()
        {
            InitializeComponent();

            ////основные настройки содержатся в файле конфигураций "App.config"
            ////адрес (порт) для клиента выбирается произвольно системой из списка доступных адресов
            this.Text = "Client";

            Reg = false;
            Login = false;
            clientName = "-";
            clientID = -1;

            this.AcceptButton = btnSend;


        }

        #region Initialization


        private void Form1_Load(object sender, EventArgs e)
        {
            chatActor = Program.ClientActors.ActorOf(Props.Create(() => new ChatActor(this)), "ChatActor");
            
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //shut down the charting actor
            chatActor.Tell(PoisonPill.Instance);

            //shut down the ActorSystem
            Program.ClientActors.Terminate();
        }

        #endregion

        #region ButtonsRealization

        private void btnReg_Click(object sender, EventArgs e)
        {
            if (Reg == false)
            {
                if (textRegName.Text.ToString().Contains("agent") != true)
                {
                    Reg = true;
                    chatActor.Tell(new NewClientMessage(textRegName.Text.ToString()));
                }
                else
                {
                    MessageBox.Show("Change the name! It mustn't contain word 'agent'!");
                }
            }
            else
            {
                MessageBox.Show("You have already registered!");
            }
        }

        private void btnUnreg_Click(object sender, EventArgs e)
        {
            if (Reg == true)
            {
                Reg = false;
                chatActor.Tell(new RemoveClientMessage(clientName));
            }
            else 
            {
                MessageBox.Show("You haven't registered or logined! Please register!");
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (Login == false)
            {
                Login = true;
                chatActor.Tell(new LoginMessage(clientID, clientName));
            }
            else 
            {
                MessageBox.Show("You have already entered the chat!");
            }
            
        }

        private void btnUnlogin_Click(object sender, EventArgs e)
        {
            if (Login == true)
            {
                Login = false;
                chatActor.Tell(new LogOutMessage(clientID, clientName));
            }
            else 
            {
                MessageBox.Show("You haven't entered the chat!"); 
            } 
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            chatActor.Tell(new WriteMessage(sendMessageText.Text.ToString()));
            sendMessageText.Text = "";
        }

        #endregion



    }
}
