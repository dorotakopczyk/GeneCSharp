using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ConsoleApp1
{

    public class GeneAnalyzer
    {
        readonly double _indexPvalueThreshold; // = 0.00001;
        readonly double _suggestivePvalueThreshold; // = 0.0001;
        readonly string _inputFileLocation; // = "C:\\Users\\Dorota Kopczyk\\Downloads\\input.txt";
        private readonly int _searchSpace;
        List<Region> resultSet = new List<Region>();

        public GeneAnalyzer(double indexPvalueThreshold, double suggestivePvalueThreshold, string inputFileLocation, int searchSpace)
        {
            _indexPvalueThreshold = indexPvalueThreshold;
            _suggestivePvalueThreshold = suggestivePvalueThreshold;
            _inputFileLocation = inputFileLocation;
            _searchSpace = searchSpace;
        }

        public List<Region> GetMyRegions()
        {
            var dataset = File.ReadLines(_inputFileLocation).Skip(1); //Assuming row 1 is headers 
            var markers = TransformInputFileToListOfObjects(dataset);

            //TODO: Verify order of chromosomes too 
            var chromosomeSets = markers.GroupBy(c => c.Chromosome).Select(c => c.ToList()).ToList();

            // All the work is done for a set of chromosomes,
            // where first the chromosomes are in order and then so are positions. (genomic order)
            foreach (var chromosomeSet in chromosomeSets)
            {
                // ChromosomeSet cant be assigned to bc its a foreach iteration variable
                // thus it's immutable. It needs to be assignable. This is important in case we need to 
                // manipulate our list 
                var workingChromosome = chromosomeSet;

                //Verify position
                var isSorted = IsSorted(workingChromosome);
                if (!isSorted) //Be nice and resort if data got messed up
                {
                    Console.WriteLine($"Markers for chromosome {workingChromosome.First().Chromosome} are not in order. Reshuffling...");
                    workingChromosome = chromosomeSet.OrderBy(x => x.Position).ToList();
                }

                var stepOneCandidates = GetRecordsExceedingIndexThreshold(workingChromosome);

                if (!stepOneCandidates.Any())
                {
                    Console.WriteLine($"For chromosome {workingChromosome.First().Chromosome}, no markers found with an index p-value threshold exceeding {_indexPvalueThreshold}");
                }
                else
                {
                    // We will then search 500,000 base pairs in both directions 
                    // We can now begin defining a region. Expand the search +/- 500k (position) 
                    foreach (var candidate in stepOneCandidates)
                    {
                       var stepTwoCandidates = GetExpandedSearchSpace(workingChromosome, candidate.Position);

                        // We can now build our regions and add them to the result set.
                        var regionCandidates = GetRecordsExceedingSuggestiveThreshold(stepTwoCandidates);

                        // If we find any marker meeting these criteria, 
                        if (regionCandidates.Any())
                        {
                            foreach (var regionCandidate in regionCandidates)
                            {
                                // Then we will extend the window for another 500,000 base pairs beyond that and continue searching.
                                // First, define the region by expanding the search results +/- 500k again
                                var expandedResults = GetExpandedSearchSpace(workingChromosome, regionCandidate.Position);

                                // We will define the start and stop positions of the region as the positions of the first and last marker
                                // in the region that meet the SUGGESTIVE THRESHOLD.
                                var newRegion = BuildRegion(resultSet, expandedResults.ToList(), regionCandidate);

                                if (IsDistinct(newRegion))
                                {
                                    newRegion.RegionIndex = resultSet.Count + 1;
                                    resultSet.Add(newRegion);
                                }
                            }
                        }
                    }
                }
            }

            // However, there are several regions with many markers below this threshold that are not unique. These markers are correlated due to the underlying
            // structure of the genome (linkage disequilibrium), and we want to identify and summarize all of the UNIQUE regions.
            return resultSet;
        }

        private bool IsDistinct(Region newRegion)
        {
            var dupe = resultSet.Where(x => x.Pvalue == newRegion.Pvalue && 
                                                  x.Chr == newRegion.Chr &&
                                                  x.MarkerName == newRegion.MarkerName &&
                                                  x.NumSigMarkers == newRegion.NumSigMarkers &&
                                                  x.NumSuggestiveMarkers == newRegion.NumSuggestiveMarkers &&
                                                  x.NumTotalMarkers == newRegion.NumTotalMarkers &&
                                                  x.SizeOfRegion == newRegion.SizeOfRegion &&
                                                  x.RegionStart == newRegion.RegionStart &&
                                                  x.RegionStop == newRegion.RegionStop).ToList();

            if (dupe.Count > 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public IEnumerable<Marker> GetExpandedSearchSpace(List<Marker> workingChromosome, int candidatePosition)
        {
            var startingPosition = candidatePosition - _searchSpace;
            var endingPosition = candidatePosition + _searchSpace;
            var stepTwoCandidates = workingChromosome  // On that chromosome.
                .Where(x => x.Position >= startingPosition &&
                            x.Position <= endingPosition);
            return stepTwoCandidates;
        }

        public Region BuildRegion( List<Region> resultSet, List<Marker> chromosomeSet, Marker regionCandidate)
        {
            // We will define the start and stop positions of the region as the positions of the first and last marker
            // in the region that meet the SUGGESTIVE THRESHOLD. 
            // TODO: I was not 100% if that caps locked above meant that the region was also defined as ONLY markers that met the suggestive threshold

            var region = chromosomeSet.Where(x => x.Pvalue < _suggestivePvalueThreshold).ToList();

            var newRegion = new Region()
            {
                Chr = int.Parse(region.First().Chromosome),

                // The name of the marker with the minimum p-value in that region
                MarkerName = region.OrderBy(p => p.Pvalue).First().Name,

                // The p-value of the marker with the minimum p-value
                Pvalue = region.OrderBy(p => p.Pvalue).First().Pvalue,

                RegionStart = region.OrderBy(p => p.Position).First().Position,
                RegionStop = region.OrderByDescending(p => p.Position).First().Position,
                // The number of markers in the region with a p-value less than the index p-value threshold
                NumSigMarkers = region.Count(x => x.Pvalue < _indexPvalueThreshold),
                NumSuggestiveMarkers = region.Count(x => x.Pvalue < _suggestivePvalueThreshold),
                NumTotalMarkers = region.Count
            };

            newRegion.SizeOfRegion = newRegion.RegionStop - newRegion.RegionStart;
            return newRegion;
        }

        public List<Marker> GetRecordsExceedingSuggestiveThreshold(IEnumerable<Marker> stepTwoCandidates)
        {
            // For any SNP on the same chromosome with a p-value that exceeds the suggestive p-value threshold (p<0.0001)
            return stepTwoCandidates.Where(x => x.Pvalue < _suggestivePvalueThreshold).ToList();
        }


        public List<Marker> GetRecordsExceedingIndexThreshold( List<Marker> workingSet)
        {
            // First we search for an index SNP exceeding the index SNP threshold (p<0.00001)
            return workingSet.Where(x => x.Pvalue < _indexPvalueThreshold).ToList();
        }

        public List<Marker> TransformInputFileToListOfObjects(IEnumerable<string> dataset)
        {
            List<Marker> filedata = new List<Marker>();

            foreach (var line in dataset)
            {
                var record = new Marker();
                var tempLine = line.Split('\t');
                record.Name = tempLine[0];
                record.Chromosome = tempLine[1];
                record.Position = Int32.Parse(tempLine[2]);

                try
                {
                    record.Pvalue = double.Parse(tempLine[3]);
                    filedata.Add(record);
                }
                catch(FormatException) //We only want to swallow format exceptions, otherwise it's scary
                {
                    // Assuming we can skip any line with an unavailable/ unparsable pvalue
                    Console.WriteLine("Could not parse pvalue for " + tempLine[3]);
                }
            }

            return filedata;
        }


        public static bool IsSorted(List<Marker> listMarkers)
        {
            for (int i = 1; i < listMarkers.Count; i++)
            {
                var previousPosition = listMarkers[i - 1].Position;
                var currentPosition = listMarkers[i].Position;
                if (previousPosition > currentPosition)
                {
                    return false;
                }
            }
            return true;
        }

        

    }
}
