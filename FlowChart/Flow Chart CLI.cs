//(C) Abhishek Sathiabalan. 2017-10-30. MIT Liscence

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowChart.Core;

namespace FlowChart
{
    class Program
    {
        static void Main(string[] args)
        {
            Analyzer AZ = new Analyzer();
            AZ.Open(args[0]);
            AZ.CreateFlowChart();

            int Count = 0;
            foreach (KeyValuePair<(string From, string To), int> entry in AZ.Flow)
            {
                Console.WriteLine("{3} {0} -> {1} : {2}", entry.Key.From, entry.Key.To, entry.Value,Count);
                Count = Count + 1;
            }

            Console.WriteLine(AZ.Export());
            AZ.Write();
            Console.Read();
        }
    }
}
