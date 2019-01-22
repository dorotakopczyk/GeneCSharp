using System;

namespace ConsoleApp1
{
    public class GeneProgram
    {
        // java UniqueRegions input.txt output.txt 0.00001 0.0001 500000
        static void Main(string[] args)
        {
            var inputFileLocation = args[0];
            var outputFileLocation = args[1];
            var indexPvalueThreshold = args[2];
            var suggestivePvalueThreshold = args[3];
            var searchSpace = args[4];

            var geneAnalyzer = new GeneAnalyzer(double.Parse(indexPvalueThreshold), double.Parse(suggestivePvalueThreshold), inputFileLocation, int.Parse(searchSpace), outputFileLocation);
            geneAnalyzer.GetMyRegions();
        }
    }
}
