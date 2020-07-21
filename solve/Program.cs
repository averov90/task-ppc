using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Net;

namespace solve
{
    class Program
    {
        static volatile Queue<string> input = new Queue<string>(); static volatile Socket sck;
        static void Main(string[] args)
        {
            Console.Write("Enter IP: ");
            string ip = Console.ReadLine();
            Console.Write("Enter port: ");
            ushort port = ushort.Parse(Console.ReadLine());
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(ip), port);
            sck = new Socket(ep.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            sck.Connect(ep);
            Thread th = new Thread(new ThreadStart(getter));
            th.Start();
            while (input.Count == 0) { Thread.Sleep(50); }
            Console.WriteLine(input.Dequeue());
            sck.Send(Encoding.UTF8.GetBytes("play-me\n"));
            while (input.Count < 2) { Thread.Sleep(50); }
            Console.WriteLine(input.Dequeue()); Console.WriteLine(input.Dequeue());
            bool nw = true;
            string s="";
            try
            {
                while (nw)
                {
                    while (input.Count == 0) { Thread.Sleep(50); }
                    s = input.Dequeue();
                    sck.Send(Encoding.UTF8.GetBytes(Get_expression_solved(s.Replace(" ", "")).ToString() + "\n"));
                    while (input.Count == 0) { Thread.Sleep(50); }
                    input.Dequeue();
                }
            }
            catch
            {
                Thread.Sleep(50);
                Console.WriteLine(s);
                while (input.Count != 0) { Thread.Sleep(50); s = input.Dequeue(); Console.WriteLine(s);  }
            }
            
        }

        static int Get_expression_solved(string str)
        {
            double summ = 0;
            foreach (string item in str.Split('+'))
            {
                if (int.TryParse(item, out int temp))
                    summ += temp;
                else
                    summ += Get_minus(item);
            }
            summ = (int)summ;
            Console.WriteLine(str + "= " + summ);
            return (int)summ;
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

        static void getter()
        {
            byte[] buffer = new byte[512];
            ushort rec = 0;
            while (sck.Connected)
            {
                rec = (ushort)sck.Receive(buffer);
                foreach (string item in Encoding.UTF8.GetString(buffer, 0, rec).Split('\n'))
                {
                    if (item != "") input.Enqueue(item);
                }
            }
        }
    }
}
