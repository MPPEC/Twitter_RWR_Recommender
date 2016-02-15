using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Experiment
{
    class Program
    {
        static void Main(string[] args)
        {
            int number0 = 8191;
            int number1 = 1024;

            Console.WriteLine("{0}", (int)Math.Ceiling(Math.Log(number0, 2.0)));
            Console.WriteLine("{0}", (int)Math.Ceiling(Math.Log(number1, 2.0)));
        }
    }
}
