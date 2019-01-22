using System;

namespace ConsoleApp1
{
    public class GeneProgram
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            double _indexPvalueThreshold = 0.00001;
            double _suggestivePvalueThreshold = 0.0001;
            string _inputFileLocation = "C:\\Users\\Dorota Kopczyk\\Downloads\\input.txt";
            int searchSpace = 500000;

            var geneAnalyzer = new GeneAnalyzer(_indexPvalueThreshold, _suggestivePvalueThreshold, _inputFileLocation, searchSpace);
            geneAnalyzer.RunProgram();

        }
    }
}
