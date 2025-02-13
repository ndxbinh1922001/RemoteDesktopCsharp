﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.IO;
namespace server
{
    public partial class khungchat_server : Form
    {
        IPEndPoint ip;
        Socket server;
        Thread receiveMess,sendMess;
        public int port;

        public khungchat_server(int port)
        {
            InitializeComponent();
            this.port = port;
            CheckForIllegalCrossThreadCalls = false;
            ip = new IPEndPoint(IPAddress.Any, port+1);
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Bind(ip);
            server.Listen(-1);
            server = server.Accept();
            receiveMess = new Thread(receive);
            receiveMess.Start();
            receiveMess.IsBackground = true;
        }
        void send()
        {
            byte[] data = new byte[1024];
            string plaintext = "Server: " + textBox1.Text;
            string key = "nguyendoanxuanbinh19220011952126";
            string iv = "1234567890abcdef";
            byte[] key_byte = System.Text.Encoding.UTF8.GetBytes(key);
            byte[] iv_byte = System.Text.Encoding.UTF8.GetBytes(iv);
            byte[] encrypted = EncryptStringToBytes_Aes(plaintext, key_byte, iv_byte);
            //data = System.Text.Encoding.UTF8.GetBytes("Server: " + textBox1.Text);                                       
            server.Send(encrypted);
            Addmessager("Server: " + textBox1.Text);
            textBox1.Text = "";
            sendMess.Abort();
        }


        bool check_cmd(string a)
        {
            if (a.Substring(0, 5) == "cmd>>")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        void receive()
        {
            while (server.Connected)
            {
                string plaintext = "";
                string key = "nguyendoanxuanbinh19220011952126";
                string iv = "1234567890abcdef";
                byte[] key_byte = System.Text.Encoding.UTF8.GetBytes(key);
                byte[] iv_byte = System.Text.Encoding.UTF8.GetBytes(iv);
                byte[] data = new byte[1024*1024];
                server.Receive(data);
                byte[] cipher = remove_padding_array(data);
                try
                {
                    plaintext = DecryptStringFromBytes_Aes(cipher, key_byte, iv_byte);
                }
                catch(Exception)
                {
                    textBox1.Text = "Đã có lỗi xảy ra trong quá trình giải mã. Yêu cầu client gửi lại text.";
                    send();
                    textBox1.Text = "";
                }
                if (check_cmd(plaintext))
                {
                    plaintext = plaintext.Substring(5);
                    richTextBox1.Text = plaintext;
                }
                else
                {
                    Addmessager(plaintext);
                }
            }
        }
        void Addmessager(string mess)
        {
            listView1.Items.Add(new ListViewItem() { Text=mess} );
        }
        static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            // Declare the string used to hold the decrypted text.
            string plaintext;
            // Create an Aes object with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;
                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }
        static byte[] remove_padding_array(byte[] a)
        {
            int length = 0;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] == 0&&a[i+1]==0)
                {
                    length = i;
                    break;
                }

            }
            byte[] b = new byte[length];
            for (int i = 0; i < length; i++)
            {
                b[i] = a[i];
            }
            return b;
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }


        private void iconButton1_Click(object sender, EventArgs e)
        {
            sendMess = new Thread(send);
            sendMess.Start();
        }

        private void iconButton2_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = "";
            textBox1.Text = "<<clear>>";
            send();
            textBox1.Text = "";
        }

        static byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }
    }
}
