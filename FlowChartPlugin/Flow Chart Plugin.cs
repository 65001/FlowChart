using System;
using FlowChart.Core;
using System.Drawing;
using SB_IDE;

namespace FlowChartPlugin
{
    [SBplugin]
    public static class Program
    {
        public static string GetName()
        {
            return "Flow Chart";
        }

        // Set a bitmap image for the plugin
        public static Bitmap GetBitmap()
        {
            
            return FlowChart.Core.Icon.GetBitmap();
        }

        // Set a tooltip description for the plugin
        public static string GetToolTip()
        {
            return "Documents the interactions between subroutines of the current program";
        }

        public static bool Run(string Text)
        {
            string TempPath = System.IO.Path.GetTempFileName();
            System.IO.File.WriteAllText(TempPath, Text);
            //string[] Data = System.IO.File.ReadAllLines(TempPath);
            Analyzer AZ = new Analyzer();
            AZ.Open(TempPath);
            AZ.CreateFlowChart();
            TempPath = TempPath.Replace(".tmp", ".html");
            System.IO.File.WriteAllText(TempPath, AZ.Export());
            System.Diagnostics.Process.Start(TempPath);
            return true;
        }
    }
}
