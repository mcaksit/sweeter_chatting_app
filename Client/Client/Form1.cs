using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace client
{
    public partial class Form1 : Form
    {

        bool terminating = false;
        bool connected = false;
        Socket clientSocket;
        int packet_size = 512;

        public Form1()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            InitializeComponent();
        }

        private void button_connect_Click(object sender, EventArgs e)
        {
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            string IP = textBox_ip.Text;

            int portNum;
            if(Int32.TryParse(textBox_port.Text, out portNum))
            {
                try
                {
                    Byte[] buffer = new Byte[packet_size];

                    string username = textBox_username.Text;
                    if (username == "" || username == null)
                    {
                        logs.AppendText("Please enter a username!\n");
                    }
                    else
                    {
                        clientSocket.Connect(IP, portNum);
                        button_connect.Enabled = false;
                        button_disconnect.Enabled = true;
                        textBox_message.Enabled = true;
                        textBox_username.Enabled = false;
                        button_send.Enabled = true;
                        button_loadfeeds.Enabled = true;
                        button_followedfeed.Enabled = true;
                        button_listusers.Enabled = true;
                        block_button.Enabled = true;
                        connected = true;
                        logs.AppendText("Connecting to the server!\n");

                        buffer = Encoding.Default.GetBytes(username);
                        clientSocket.Send(buffer);

                        Byte[] buffer2 = new Byte[packet_size];
                        clientSocket.Receive(buffer2);

                        string incomingMessage = Encoding.Default.GetString(buffer2).Trim('\0');

                        if (incomingMessage == "yes")
                        {
                            logs.AppendText("Connected to the server!\n");
                            button_connect.Enabled = false;
                            button_disconnect.Enabled = true;
                            textBox_message.Enabled = true;
                            button_send.Enabled = true;
                            button_loadfeeds.Enabled = true;
                            button_followedfeed.Enabled = true;
                            button_listusers.Enabled = true;
                            follow_button.Enabled = true;
                            unfollow_button.Enabled = true;
                            follower_text_box.Enabled = true;
                            button_see_sweets.Enabled = true;
                            button_current_followed_users.Enabled = true;
                            button_current_followers.Enabled = true;
                            textBox_sweet_id.Enabled = true;
                            button_delete_sweet.Enabled = true;

                            connected = true;
                            terminating = false;

                            Thread receiveThread = new Thread(Receive);
                            receiveThread.Start();
                        }
                        if (incomingMessage == "no")
                        {
                            button_connect.Enabled = true;
                            button_disconnect.Enabled = false;
                            textBox_message.Enabled = false;
                            textBox_username.Enabled = true;
                            button_send.Enabled = false;
                            button_loadfeeds.Enabled = false;
                            button_followedfeed.Enabled = false;
                            button_listusers.Enabled = false;
                            connected = false;
                            button_see_sweets.Enabled = false;
                            button_current_followed_users.Enabled = false;
                            button_current_followers.Enabled = false;
                            textBox_sweet_id.Enabled = false;
                            button_delete_sweet.Enabled = false;

                            logs.AppendText("Username Check Failed!\n");

                        }
                    }

                }
                catch
                {
                    logs.AppendText("Could not connect to the server!\n");
                }
            }
            else
            {
                logs.AppendText("Check the port\n");
            }

        }

        private void Receive()
        {
            while(connected)
            {
                try
                {
                    Byte[] buffer = new Byte[packet_size];
                    clientSocket.Receive(buffer);

                    string incomingMessage = Encoding.Default.GetString(buffer);
                    incomingMessage = incomingMessage.Substring(0, incomingMessage.IndexOf("\0"));

                    if (incomingMessage == "") {
                        
                        logs.AppendText("The server has disconnected\n");
                        logs.AppendText("ilki");
                        button_connect.Enabled = true;
                        button_disconnect.Enabled = false;
                        textBox_message.Enabled = false;
                        button_send.Enabled = false;
                        button_loadfeeds.Enabled = false;
                        button_followedfeed.Enabled = false;
                        button_listusers.Enabled = false;
                        textBox_username.Enabled = true;
                        follow_button.Enabled = false;
                        unfollow_button.Enabled = false;
                        follower_text_box.Enabled = false;
                        block_button.Enabled = false;
                        clientSocket.Close();
                        connected = false;
                        button_see_sweets.Enabled = false;
                        button_current_followed_users.Enabled = false;
                        button_current_followers.Enabled = false;
                        textBox_sweet_id.Enabled = false;
                        button_delete_sweet.Enabled = false;

                    }
                    else
                    {
                        logs.AppendText(incomingMessage);
                    }
                        
                }
                catch
                {
                    if (!terminating)
                    {
                        logs.AppendText("The server has disconnected\n");
                        button_connect.Enabled = true;
                        button_disconnect.Enabled = false;
                        textBox_message.Enabled = false;
                        button_send.Enabled = false;
                        button_loadfeeds.Enabled = false;
                        button_followedfeed.Enabled = false;
                        button_listusers.Enabled = false;
                        textBox_username.Enabled = true;
                        follow_button.Enabled = false;
                        unfollow_button.Enabled = false;
                        follower_text_box.Enabled = false;
                        block_button.Enabled = false;
                        button_see_sweets.Enabled = false;
                        button_current_followed_users.Enabled = false;
                        button_current_followers.Enabled = false;
                        textBox_sweet_id.Enabled = false;
                        button_delete_sweet.Enabled = false;
                    }

                    clientSocket.Close();
                    connected = false;
                }

            }
        }

        private void Form1_FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            connected = false;
            terminating = true;
            Environment.Exit(0);
        }

        private void button_send_Click(object sender, EventArgs e)
        {
            string message = "sendmessage***" + textBox_message.Text;

            if (message != "sendmessage***" && message.Length <= packet_size)
            {
                logs.AppendText("Message sent to the server: " + message.Substring(14) + "\n");
                Byte[] buffer = new byte[packet_size];
                buffer = Encoding.Default.GetBytes(message);
                clientSocket.Send(buffer);
            }
        }

        private void button_loadfeeds_Click(object sender, EventArgs e)
        {
            string message = "loadfeeds***";
            logs.AppendText("\nLoaded Feeds: \n");
            Byte[] buffer = Encoding.Default.GetBytes(message);
            clientSocket.Send(buffer);
        }

        private void button_listusers_Click(object sender, EventArgs e)
        {
            string message = "listusers***";
            logs.AppendText("\nUser List: \n");
            Byte[] buffer = Encoding.Default.GetBytes(message);
            clientSocket.Send(buffer);
        }

        private void textBox_port_TextChanged(object sender, EventArgs e)
        {

        }

        private void button_disconnect_Click(object sender, EventArgs e)
        {
            connected = false;
            button_connect.Enabled = true;
            button_disconnect.Enabled = false;
            textBox_message.Enabled = false;
            button_send.Enabled = false;
            textBox_ip.Enabled = true;
            textBox_port.Enabled = true;
            textBox_username.Enabled = true;
            button_loadfeeds.Enabled = false;
            button_followedfeed.Enabled = false;
            button_listusers.Enabled = false;
            follow_button.Enabled = false;
            unfollow_button.Enabled = false;
            follower_text_box.Enabled = false;
            block_button.Enabled = false;
            button_see_sweets.Enabled = false;
            button_current_followed_users.Enabled = false;
            button_current_followers.Enabled = false;
            textBox_sweet_id.Enabled = false;
            button_delete_sweet.Enabled = false;

            terminating = true;
            clientSocket.Close();

            logs.AppendText("Disconnected from server!\n");
        }

        private void follow_button_Click(object sender, EventArgs e)
        {
            string message = "follow***" + follower_text_box.Text;

            if (textBox_username.Text != follower_text_box.Text)
            {
                if (message != "follow***" && message.Length <= packet_size)
                {
                    Byte[] buffer = new byte[packet_size];
                    buffer = Encoding.Default.GetBytes(message);
                    clientSocket.Send(buffer);
                }
            }

            else
            {
                logs.AppendText("You can't follow yourself!\n");
            }
        }

        private void unfollow_button_Click(object sender, EventArgs e)
        {
            string message = "unfollow***" + follower_text_box.Text;

            if (textBox_username.Text != follower_text_box.Text)
            {
                if (message != "unfollow***" && message.Length <= packet_size)
                {
                    Byte[] buffer = new byte[packet_size];
                    buffer = Encoding.Default.GetBytes(message);
                    clientSocket.Send(buffer);
                }
            }

            else
            {
                logs.AppendText("You can't unfollow yourself!\n");
            }
        }

        private void button_followedfeed_Click(object sender, EventArgs e)
        {
            string message = "followedfeed***";
            logs.AppendText("\nLoaded feeds from followed users: \n");
            Byte[] buffer = Encoding.Default.GetBytes(message);
            clientSocket.Send(buffer);
        }

        private void textBox_username_TextChanged(object sender, EventArgs e)
        {

        }

        private void block_button_Click(object sender, EventArgs e)
        {
            
            string blocked_user = follower_text_box.Text;
            if (blocked_user != textBox_username.Text)
            {
                string message = "blockuser***";
                Byte[] buffer = Encoding.Default.GetBytes(message + blocked_user);
                clientSocket.Send(buffer);
            }
            else
            {
                textBox_message.AppendText("You can not follow yourself!\n");
            }
        }

        private void button_current_followed_users_Click(object sender, EventArgs e)
        {
            string message = "currentfollowedusers***";
            logs.AppendText("\nCurrent followed users list: \n");
            Byte[] buffer = Encoding.Default.GetBytes(message);
            clientSocket.Send(buffer);
        }

        private void button_current_followers_Click(object sender, EventArgs e)
        {
            string message = "currentfollowers***";
            logs.AppendText("\nCurrent followers list: \n");
            Byte[] buffer = Encoding.Default.GetBytes(message);
            clientSocket.Send(buffer);
        }

        private void button_delete_sweet_Click(object sender, EventArgs e)
        {
            string message = "deletesweet***";
            string sweet_id = textBox_sweet_id.Text;
            Byte[] buffer = Encoding.Default.GetBytes(message + sweet_id);
            clientSocket.Send(buffer);
        }

        private void button_see_sweets_Click(object sender, EventArgs e)
        {
            string message = "selfsweets***";
            Byte[] buffer = Encoding.Default.GetBytes(message);
            clientSocket.Send(buffer);
        }
    }
}
