using System;
using System.IO;
using System.Linq;
using ConsoleApp1;
using Xunit;

namespace XUnitTestProject1
{
    public class IDidChromosome21ByHand
    {
        [Fact]
        public void ThreeRecordsReturnWithIndexThreshold()
        {
            var inputFileLocation = "C:\\SC\\Repos\\GeneCSharp\\XUnitTestProject1\\input.txt";
            var geneAnalyzer = new GeneAnalyzer(0.00001, 0.0001, inputFileLocation);

            var dataset = File.ReadLines(inputFileLocation).Skip(1); //Assuming row 1 is headers 

            var markers = geneAnalyzer.TransformInputFileToListOfObjects(dataset);
        }
    }
}
