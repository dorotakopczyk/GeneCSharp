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

            var result = geneAnalyzer.GetRecordsExceedingIndexThreshold(0.00001, markers);

            Assert.Equal(3, result.Count);
            Assert.Equal("rs11910404", result[0].Name);
            Assert.Equal("rs16991720", result[1].Name);
            Assert.Equal("rs16991721", result[2].Name);
        }
    }
}
