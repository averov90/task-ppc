using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Net;

namespace task1_ppc
{
    class Program
    {
        volatile static ushort conncount = 0;
        volatile static ushort wins = 0;

        static void Main()
        {
            Console.WriteLine("Welcome to PPC-TASK\nServer was started! Port: 24\nWait for connections...");
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse("0.0.0.0"), 24);
            Socket sck = new Socket(ep.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            sck.Bind(ep);
            sck.Listen(10);
            TaskFactory tf = new TaskFactory();
            while (true)
            {
                Socket conn = sck.Accept();
                tf.StartNew(() => Connection_th(conn));
                Console.WriteLine("Connections count: " + ++conncount);
            }
        }

        static void Connection_th(object con)
        {
            try
            {
                Socket conn = (Socket)con;
                System.Diagnostics.Stopwatch stw = new System.Diagnostics.Stopwatch();
                Task ts;
                byte[] buffer = new byte[512];
                int byte_count = 0, rand1, rand2, tint;
                string cinp;
                conn.Send(Encoding.UTF8.GetBytes("Congratulations! You have solved this task... in a parallel universe. Not in this one yet.\n"));
                byte wc = 0;
                stw.Start();
                pos1:
                ts = Task.Run(() => { byte_count = conn.Receive(buffer); });
                while (!ts.IsCompleted)
                {
                    Thread.Sleep(100);
                    if (stw.Elapsed.Minutes >= 2)
                    {
                        conn.Send(Encoding.UTF8.GetBytes("Time is up!\n"));
                        conn.Shutdown(SocketShutdown.Both);
                        conn.Disconnect(false);
                        throw new Exception();
                    }
                }
                cinp = Encoding.UTF8.GetString(buffer, 0, byte_count - 1);
                if (cinp != "play-me")
                {
                    if (++wc == 15)
                    {
                        conn.Send(Encoding.UTF8.GetBytes("I think there was some tension between us...\n"));
                        conn.Shutdown(SocketShutdown.Both);
                        conn.Disconnect(false);
                        throw new Exception();
                    }
                    conn.Send(Encoding.UTF8.GetBytes("Let's play a game. (Type 'play-me')\n"));
                    goto pos1;
                }
                stw.Reset();
                conn.Send(Encoding.UTF8.GetBytes("My game is very simple!\nJust turn into a calculator and count everything I send you. (Round to zero)\n"));
                string ask;
                string myexp;
                ushort all_c = GRandom(100, 1000);
                for (ushort i2 = 0; i2 != all_c; i2++)
                {
                    ask = "";
                    myexp = "";
                    rand1 = GRandom(4, 40);
                    for (ushort i = 0; i < rand1; i++)
                    {
                        rand2 = GRandom(0, 4);
                        switch (rand2)
                        {
                            case 0:
                                tint = GRandom(0, 1000);
                                ask += tint + " + ";
                                myexp += tint + "+";
                                break;
                            case 1:
                                tint = GRandom(0, 1000);
                                ask += tint + " - ";
                                myexp += tint + "-";
                                break;
                            case 2:
                                tint = GRandom(0, 1000);
                                ask += tint + " * ";
                                myexp += tint + "*";
                                break;
                            case 3:
                                tint = GRandom(0, 1000);
                                ask += tint + " / ";
                                myexp += tint + "/";
                                break;
                        }
                    }
                    int ransw = (int)Get_answer(myexp.Substring(0, myexp.Length - 1));
                    conn.Send(Encoding.UTF8.GetBytes(ask.Substring(0, ask.Length - 3) + "\n"));
                    ts = Task.Run(() => { byte_count = conn.Receive(buffer); });
                    stw.Start();
                    while (!ts.IsCompleted)
                    {
                        Thread.Sleep(100);
                        if (stw.Elapsed.Seconds >= 5)
                        {
                            conn.Send(Encoding.UTF8.GetBytes("Time is up!\n"));
                            conn.Shutdown(SocketShutdown.Both);
                            conn.Disconnect(false);
                            throw new Exception();
                        }
                    }
                    stw.Reset();
                    cinp = Encoding.UTF8.GetString(buffer, 0, byte_count - 1);
                    if (cinp == ransw.ToString())
                    {
                        conn.Send(Encoding.UTF8.GetBytes("Correct answer!\n"));
                    }
                    else
                    {
                        conn.Send(Encoding.UTF8.GetBytes("Uncorrect answer! Good bye!\n"));
                        conn.Shutdown(SocketShutdown.Both);
                        conn.Disconnect(false);
                        throw new Exception();
                    }
                }
                stw.Stop();
                conn.Send(Encoding.UTF8.GetBytes("Well, it was simple, agree...\nYour flag: CTF{it_remains_t0_learn_how_to_turn_back}\n"));
                Console.WriteLine("Connections count: " + --conncount + " Flag given: " + ++wins + " times");
            }
            catch
            {
                Console.WriteLine("Connections count: " + --conncount);
            }
        }
        static double Get_answer(string str)
        {
            double summ = 0;
            foreach (string item in str.Split('+'))
            {
                if (int.TryParse(item, out int temp))
                    summ += temp;
                else
                    summ += Get_minus(item);
            }
            return summ;
        }
        static double Get_minus(string str)
        {
            string[] strings = str.Split('-');
            double sum;
            if (int.TryParse(strings[0], out int temp))
                sum = temp;
            else
                sum = Get_multi(strings[0]);
            for (ushort i = 1; i != strings.Length; i++)
            {
                if (int.TryParse(strings[i], out temp))
                    sum -= temp;
                else
                    sum -= Get_multi(strings[i]);
            }
            return sum;
        }
        static double Get_multi(string str)
        {
            string[] strings = str.Split('*');
            double prew;
            if (int.TryParse(strings[0], out int temp))
                prew = temp;
            else
                prew = Get_div(strings[0]);
            for (ushort i = 1; i != strings.Length; i++)
            {
                if (int.TryParse(strings[i], out temp))
                    prew *= temp;
                else
                    prew *= Get_div(strings[i]);
            }
            return prew;
        }
        static double Get_div(string str)
        {
            string[] strings = str.Split('/');
            double prew;
            prew = int.Parse(strings[0]);
            for (ushort i = 1; i != strings.Length; i++)
            {
                prew /= int.Parse(strings[i]);
            }
            return prew;
        }
        static System.Security.Cryptography.RNGCryptoServiceProvider rand = new System.Security.Cryptography.RNGCryptoServiceProvider();
        static ushort GRandom(ushort s, ushort d)
        {
            byte[] size = new byte[2];
            rand.GetBytes(size);
            ushort num = (ushort)(BitConverter.ToUInt16(size, 0) * (d - s) / 65535 + s);
            return (ushort)(num == d ? num - 1 : num);
        }
    }
}
