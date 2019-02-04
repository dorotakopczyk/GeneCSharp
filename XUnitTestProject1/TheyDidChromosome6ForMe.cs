using System.Linq;
using ConsoleApp1;
using Xunit;

namespace Test
{
    public class TheyDidChromosome6ForMe
    {
        private readonly GeneAnalyzer _geneAnalyzer;

        public TheyDidChromosome6ForMe(GeneAnalyzer geneAnalyzer)
        {
            var inputFileLocation = "C:\\SC\\Repos\\GeneCSharp\\XUnitTestProject1\\inputBig.txt";
            var outputFileLocation = "C:\\SC\\Repos\\GeneCSharp\\XUnitTestProject1\\outputBig.txt";
            _geneAnalyzer = new GeneAnalyzer(0.00001, 0.0001, inputFileLocation, 500000, outputFileLocation);
        }

        [Fact]
        public void ThereIsOnlyOneRegionForChrom6()
        {
            var results = _geneAnalyzer.GetMyRegions();

            Assert.Single(results);
            Assert.Equal(1, results.Single().RegionIndex);
            Assert.Equal("rs2854008", results.Single().MarkerName);
            Assert.Equal(6, results.Single().Chr);
            Assert.Equal(1.802E-06, results.Single().Pvalue);
            Assert.Equal(31378987, results.Single().RegionStart);
            Assert.Equal(32146528, results.Single().RegionStop);
            Assert.Equal(15, results.Single().NumSigMarkers);
            Assert.Equal(44, results.Single().NumSuggestiveMarkers);
            Assert.Equal(253, results.Single().NumTotalMarkers);
            Assert.Equal(767541, results.Single().SizeOfRegion);

            //Region	MarkerName	Chr 	Position	P-value	   RegionStart	RegionStop	NumSigMarkers	NumSuggestiveMarkers	NumTotalMarkers	SizeOfRegion
            //3	         rs2854008	6	  31,420,517	1.802E-06	31,378,987	32,146,528	15	44	235	767,542
        }
    }
}
