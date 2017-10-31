//(C) Abhishek Sathiabalan. 2017-10-30. MIT Liscence

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    class Analyzer
    {
        private string[] Text;
        private string InputURI;
        private string OutputURI;
        private string CurrentSubroutine;
        private string HTML;

        public Dictionary<(string From,string To), int> Flow= new Dictionary<(string From, string To),int>();

        public void Open(string URI)
        {
            if (string.IsNullOrWhiteSpace(URI) == true)
            {
                throw new ArgumentNullException(URI);
            }
           this.OutputURI = System.IO.Path.GetDirectoryName(URI) + "\\" + System.IO.Path.GetFileNameWithoutExtension(URI) + " Code Analysis.html";
           this.InputURI = URI;
           this.Text = System.IO.File.ReadAllLines(URI);
        }

        public void CreateFlowChart()
        {
            CurrentSubroutine = "Main";

            for (int i = 0; i < Text.Length; i++)
            {
                string CL = Text[i].Trim();
                StringComparison Comparison = StringComparison.InvariantCultureIgnoreCase;
                Console.Write("{0} ", i);
                //Ignore Cases
                if (CL.StartsWith("'", Comparison) || string.IsNullOrWhiteSpace(CL) == true)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(CL);
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else if (CL.StartsWith("sub", Comparison))
                {
                    CurrentSubroutine = CL.Remove(0, 3).Trim();
                    int IndexSpace = CurrentSubroutine.IndexOf(" ");
                    int IndexComment = CurrentSubroutine.IndexOf("'");

                    if (IndexSpace >= IndexComment && IndexComment != -1)
                    {
                        CurrentSubroutine = CurrentSubroutine.Substring(0, IndexComment);
                    }
                    else if (IndexSpace < IndexComment)
                    {
                        CurrentSubroutine = CurrentSubroutine.Substring(0, IndexSpace);
                    }

                    if (Flow.ContainsKey(("Main", CurrentSubroutine)) == false)
                    {
                        Flow.Add(("Main", CurrentSubroutine), 0);
                    }
                    Console.WriteLine("Start of {0}", CurrentSubroutine);

                }
                else if (CL.StartsWith("endsub", Comparison))
                {
                    Console.WriteLine("End of {0}", CurrentSubroutine);
                }
                //Function call
                else if (CL.IndexOf("(") != -1 && CL.IndexOf(")") != -1)
                {

                    if (CL.IndexOf(".") == -1) //Subroutine call detected :)
                    {
                        int Start = CL.IndexOf("(");
                        int End = CL.IndexOf(")");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("{0} ({1},{2})",CL,Start,End);
                        Console.ForegroundColor = ConsoleColor.White;
                        string Called = CL.Substring(0, Start); ;
                        if (End == (Start + 1))
                        {
                            if (Flow.ContainsKey((CurrentSubroutine, Called)) == true)
                            {
                                Flow[(CurrentSubroutine, Called)] = Flow[(CurrentSubroutine, Called)] + 1;
                            }
                            else
                            {
                                this.Flow.Add((CurrentSubroutine, Called), 1);
                            }
                            Console.WriteLine("{0} called {1}", CurrentSubroutine, Called);
                        }
                    }
                    //TODO LDCALL.Function support
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(CL);
                        Console.ForegroundColor = ConsoleColor.White;
                    }

                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(CL);
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
        }

        public string Export()
        {
            StringBuilder SB = new StringBuilder();
            SB.AppendLine("<html>");
            SB.AppendLine("\t<head>");
            SB.AppendLine("\t<title>Code Analaysis</title>");
            SB.AppendLine("\t\t<script type=\"text/javascript\" src=\"https://www.gstatic.com/charts/loader.js\"></script>");
            SB.AppendLine("\t\t<script type=\"text/javascript\">");
            SB.AppendLine("\t\t\tgoogle.charts.load('current',{'packages':['sankey']});");
            SB.AppendLine("\t\t\tgoogle.charts.setOnLoadCallback(drawChart);");
            SB.AppendLine();
            SB.AppendLine("\t\t\tfunction drawChart() {");
            SB.AppendLine("var data = new google.visualization.DataTable();");
            SB.AppendLine("data.addColumn('string','From');");
            SB.AppendLine("data.addColumn('string','To');");
            SB.AppendLine("data.addColumn('number','Calls');");

            SB.Append("data.addRows([");
            int Count = 0;
            foreach (KeyValuePair<(string From, string To), int> entry in Flow)
            {

                if (entry.Key.From == entry.Key.To)
                {
                    Console.WriteLine("WARN {0}", Count);
                }
                else
                {
                    if (entry.Value != 0)
                    {
                        SB.AppendFormat("['{0}','{1}',{2}]", entry.Key.From, entry.Key.To, entry.Value);
                        Count = Count + 1;
                        if (Count < Flow.Count)
                        {
                            SB.AppendLine(",");
                        }
                    }
                    else
                    {
                        Count = Count + 1;
                        /*
                        SB.AppendFormat("['{0}','{1}',{2}]", entry.Key.From ?? "Unused", entry.Key.To, entry.Value + 1);
                        if (Count < Flow.Count)
                        {
                            SB.AppendLine(",");
                        }
                        */
                    }
                }
            }

            SB.Append("]);");

            SB.AppendLine("var options = {width :600,};");
            SB.AppendLine("var chart = new google.visualization.Sankey(document.getElementById('sankey_basic'));\nchart.draw(data, options);");
            SB.AppendLine("}");
            SB.AppendLine("</script>");
            SB.AppendLine("</head>");
            SB.AppendLine("<body>");
            SB.AppendLine("<div id=\"sankey_basic\" style=\"width: 95%; height: 95%; \"></div>");
            SB.AppendLine("<p>Recursive and self referencing subroutines have been omitted due to a limitation on the part of Google Chart's API</p>");
            SB.AppendLine("</body></html>");

            this.HTML = SB.ToString();
            return HTML;
        }

        public void Write()
        {
            System.IO.File.WriteAllText(OutputURI, HTML);
        }
    }
}
