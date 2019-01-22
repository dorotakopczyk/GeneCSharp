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

        public GeneAnalyzer(double indexPvalueThreshold, double suggestivePvalueThreshold, string inputFileLocation)
        {
            _indexPvalueThreshold = indexPvalueThreshold;
            _suggestivePvalueThreshold = suggestivePvalueThreshold;
            _inputFileLocation = inputFileLocation;
        }

        public void RunProgram()
        {
            var resultSet = new List<Region>();

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

                var stepOneCandidates = GetRecordsExceedingIndexThreshold(_indexPvalueThreshold, workingChromosome);

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
                        var startingPosition = candidate.Position - 500000;
                        var endingPosition = candidate.Position + 500000;
                        var stepTwoCandidates = workingChromosome  // On that chromosome.
                            .Where(x => x.Position >= startingPosition &&
                                        x.Position <= endingPosition);

                        // We can now build our regions and add them to the result set.
                        List<Marker> regions = GetRegionCandidates(_suggestivePvalueThreshold, stepTwoCandidates);

                        // If we find any marker meeting these criteria, 
                        if (regions.Any())
                        {
                            foreach (var regionCandidate in regions)
                            {

                                // Then we will extend the window for another 500,000 base pairs beyond that and continue searching.
                                // We will define the start and stop positions of the region as the positions of the first and last marker
                                // in the region that meet the SUGGESTIVE THRESHOLD.
                                var newRegion = BuildRegion(_indexPvalueThreshold, _suggestivePvalueThreshold, resultSet, chromosomeSet, regionCandidate);

                                resultSet.Add(newRegion);
                            }
                        }
                    }
                }
            }
        }

        private static Region BuildRegion(double indexPvalueThreshold, double suggestivePvalueThreshold, List<Region> resultSet, List<Marker> chromosomeSet, Marker regionCandidate)
        {
            //First, define the region by expanding the search results +/- 500k again
            var startingPositionReg = regionCandidate.Position - 500000;
            var endingPositionReg = regionCandidate.Position + 500000;

            // We will define the start and stop positions of the region as the positions of the first and last marker
            // in the region that meet the SUGGESTIVE THRESHOLD. 
            // TODO: I was unsure if that meant that the region was also defined as ONLY markers that met the suggestive threshold
            var region = chromosomeSet.Where(x =>
                x.Position >= startingPositionReg && x.Position <= endingPositionReg && x.Pvalue < suggestivePvalueThreshold).ToList();

            var newRegion = new Region()
            {
                RegionIndex = resultSet.Count + 1,
                Chr = Int32.Parse(region.First().Chromosome),
                // The name of the marker with the minimum p-value in that region
                MarkerName = region.OrderByDescending(p => p.Pvalue).First().Name,
                // The p-value of the marker with the minimum p-valu
                Pvalue = region.OrderByDescending(p => p.Pvalue).First().Pvalue,
                RegionStart = region.OrderBy(p => p.Position).First().Position,
                RegionStop = region.OrderByDescending(p => p.Position).First().Position,
                // The number of markers in the region with a p-value less than the index p-value threshold
                NumSigMarkers = region.Count(x => x.Pvalue < indexPvalueThreshold),
                NumSuggestiveMarkers = region.Count(x => x.Pvalue < suggestivePvalueThreshold),
                NumTotalMarkers = region.Count
            };

            newRegion.SizeOfRegion = newRegion.RegionStop - newRegion.RegionStart;
            return newRegion;
        }

        private static List<Marker> GetRegionCandidates(double suggestivePvalueThreshold, IEnumerable<Marker> stepTwoCandidates)
        {

            // For any SNP on the same chromosome with a p-value that exceeds the suggestive p-value threshold (p<0.0001)
            return stepTwoCandidates.Where(x => x.Pvalue < suggestivePvalueThreshold).ToList();
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
                    record.Pvalue = Double.Parse(tempLine[3]);
                    filedata.Add(record);
                }
                catch (Exception e)
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

        public double ParsePValue(string value)
        {
            double pvalue;
            try
            {
                pvalue = Double.Parse(value);
                return pvalue;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

    }
}
