using System.IO;
using System.Linq;
using ConsoleApp1;
using Xunit;

namespace Test
{
    public class DidChromosome21ByHand
    {
        private readonly GeneAnalyzer _geneAnalyzer;

        public DidChromosome21ByHand()
        {
            var inputFileLocation = "C:\\SC\\Repos\\GeneCSharp\\XUnitTestProject1\\input.txt";
            var outputFileLocation = "C:\\SC\\Repos\\GeneCSharp\\XUnitTestProject1\\output.txt";
            _geneAnalyzer = new GeneAnalyzer(0.00001, 0.0001, inputFileLocation, 500000, outputFileLocation);
        }

        [Fact]
        public void ThreeRecordsReturnWithIndexThreshold()
        {
            var dataset = File.ReadLines("C:\\SC\\Repos\\GeneCSharp\\XUnitTestProject1\\input.txt").Skip(1); //Assuming row 1 is headers 

            var markers = _geneAnalyzer.TransformInputFileToListOfObjects(dataset);

            var result = _geneAnalyzer.GetRecordsExceedingIndexThreshold(markers);

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
            var dataset = File.ReadLines("C:\\SC\\Repos\\GeneCSharp\\XUnitTestProject1\\input.txt").Skip(1); //Assuming row 1 is headers 

            var markers21 = _geneAnalyzer.TransformInputFileToListOfObjects(dataset);

            var results = _geneAnalyzer.GetExpandedSearchSpace(markers21, markerPosition);

            Assert.Equal(expectedResultCount, results.Count());
        }

        [Theory]
        [InlineData("rs11910404", 34775341, 3)]
        [InlineData("rs16991720", 34778632, 3)]
        [InlineData("rs16991721", 34779464, 3)]
        public void DefiningPotentialRegionsYieldsExpectedResults(string markerName, int markerPosition, int expectedResultCount)
        {
            var dataset = File.ReadLines("C:\\SC\\Repos\\GeneCSharp\\XUnitTestProject1\\input.txt").Skip(1);
            var markers21 = _geneAnalyzer.TransformInputFileToListOfObjects(dataset);

            var searchSet = _geneAnalyzer.GetExpandedSearchSpace(markers21, markerPosition);

            var results = _geneAnalyzer.GetRecordsExceedingSuggestiveThreshold(searchSet);

            Assert.Equal(expectedResultCount, results.Count());
        }

        [Fact]
        public void ThereIsJustOneRegion()
        {
            var results = _geneAnalyzer.GetMyRegions(); 

            Assert.Single(results);
            Assert.Equal(1, results.Single().RegionIndex);
            Assert.Equal("rs16991721", results.Single().MarkerName);
            Assert.Equal(21, results.Single().Chr);
            Assert.Equal(0.0000007890, results.Single().Pvalue);
            Assert.Equal(34775341, results.Single().RegionStart);
            Assert.Equal(34779464, results.Single().RegionStop);
            Assert.Equal(3, results.Single().NumSigMarkers);
            Assert.Equal(3, results.Single().NumSuggestiveMarkers);
            Assert.Equal(3, results.Single().NumTotalMarkers);
            Assert.Equal(4124, results.Single().SizeOfRegion);
        }
       
    }
}
