using System;

namespace ConsoleApp1
{
    public class GeneProgram
    {
        // java UniqueRegions input.txt output.txt 0.00001 0.0001 500000
        static void Main(string[] args)
        {
            var inputFileLocation = "C:\\Users\\Dorota Kopczyk\\Downloads\\input.txt";//= args[0];
            var outputFileLocation = "C:\\Users\\Dorota Kopczyk\\Downloads\\outputTry2.txt";//= args[1];
            var indexPvalueThreshold = "0.00001"; //= args[2];
            var suggestivePvalueThreshold = "0.0001";//= args[3];
            var searchSpace = "500000";//args[4];

            var geneAnalyzer = new GeneAnalyzer(double.Parse(indexPvalueThreshold), double.Parse(suggestivePvalueThreshold), inputFileLocation, int.Parse(searchSpace), outputFileLocation);
            geneAnalyzer.GetMyRegions();
        }
    }
}
