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
            var geneAnalyzer = new GeneAnalyzer(0.00001, 0.0001, inputFileLocation, 500000);

            var dataset = File.ReadLines(inputFileLocation).Skip(1); //Assuming row 1 is headers 

            var markers = geneAnalyzer.TransformInputFileToListOfObjects(dataset);

            var result = geneAnalyzer.GetRecordsExceedingIndexThreshold(markers);

            Assert.Equal(3, result.Count);
            Assert.Equal("rs11910404", result[0].Name);
            Assert.Equal("rs16991720", result[1].Name);
            Assert.Equal("rs16991721", result[2].Name);
        }

        [Theory]
        [InlineData("rs11910404", 34775341, 9)]
        [InlineData("rs16991720", 34778632, 9)]
        [InlineData("rs16991721", 34779464, 9)]
        // I know I don't need marker name, I'm just leaving it in here bc its easier to make sense of the data
        public void FirstExpansionOfSearchSpaceYieldsExpectedCountOfResults(string markerName, int markerPosition, int expectedResultCount)
        {
            var inputFileLocation = "C:\\SC\\Repos\\GeneCSharp\\XUnitTestProject1\\input.txt";
            var geneAnalyzer = new GeneAnalyzer(0.00001, 0.0001, inputFileLocation, 500000);
            var dataset = File.ReadLines(inputFileLocation).Skip(1); //Assuming row 1 is headers 

            var markers21 = geneAnalyzer.TransformInputFileToListOfObjects(dataset);

            var results = geneAnalyzer.GetExpandedSearchSpace(markers21, markerPosition);

            Assert.Equal(expectedResultCount, results.Count());
        }

        [Theory]
        [InlineData("rs11910404", 34775341, 3)]
        [InlineData("rs16991720", 34778632, 3)]
        [InlineData("rs16991721", 34779464, 3)]
        public void DefiningPotentialRegionsYieldsExpectedResults(string markerName, int markerPosition, int expectedResultCount)
        {
            var inputFileLocation = "C:\\SC\\Repos\\GeneCSharp\\XUnitTestProject1\\input.txt";
            var geneAnalyzer = new GeneAnalyzer(0.00001, 0.0001, inputFileLocation, 500000);
            var dataset = File.ReadLines(inputFileLocation).Skip(1); //Assuming row 1 is headers 

            var markers21 = geneAnalyzer.TransformInputFileToListOfObjects(dataset);

            var searchSet = geneAnalyzer.GetExpandedSearchSpace(markers21, markerPosition);

            var results = geneAnalyzer.GetRecordsExceedingSuggestiveThreshold(searchSet);

            Assert.Equal(expectedResultCount, results.Count());
        }

        /*
        [Theory]
        [InlineData("rs11910404", 34775341, 9)]
        [InlineData("rs16991720", 34778632, 9)]
        [InlineData("rs16991721", 34779464, 9)]
        [InlineData("rs11910404", 34775341, 9)]
        [InlineData("rs16991720", 34778632, 9)]
        [InlineData("rs16991721", 34779464, 9)]
        [InlineData("rs11910404", 34775341, 9)]
        [InlineData("rs16991720", 34778632, 9)]
        [InlineData("rs16991721", 34779464, 9)]
        // I know I don't need marker name, I'm just leaving it in here bc its easier to make sense of the data
        public void SecondExpansionOfSearchSpaceYieldsExpectedCountOfResults(string markerName, int markerPosition, int expectedResultCount)
        {
            var inputFileLocation = "C:\\SC\\Repos\\GeneCSharp\\XUnitTestProject1\\input.txt";
            var geneAnalyzer = new GeneAnalyzer(0.00001, 0.0001, inputFileLocation, 500000);
            var dataset = File.ReadLines(inputFileLocation).Skip(1); //Assuming row 1 is headers 

            var markers21 = geneAnalyzer.TransformInputFileToListOfObjects(dataset);

            var results = geneAnalyzer.GetExpandedSearchSpace(markers21, markerPosition);

            Assert.Equal(expectedResultCount, results.Count());
        }

        */

    }
}
