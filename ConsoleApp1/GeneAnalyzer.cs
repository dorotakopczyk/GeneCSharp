﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace ConsoleApp1
{

    public class GeneAnalyzer
    {
        readonly double _indexPvalueThreshold;
        readonly double _suggestivePvalueThreshold; 
        readonly string _inputFileLocation; 
        readonly int _searchSpace;
        readonly string _outputFileLocation; 

        List<Region> _resultSet = new List<Region>();


        public GeneAnalyzer(double indexPvalueThreshold, double suggestivePvalueThreshold, string inputFileLocation, int searchSpace, string outputFileLocation)
        {
            _indexPvalueThreshold = indexPvalueThreshold;
            _suggestivePvalueThreshold = suggestivePvalueThreshold;
            _inputFileLocation = inputFileLocation;
            _searchSpace = searchSpace;
            _outputFileLocation = outputFileLocation;
        }

        public List<Region> GetMyRegions()
        {
            var dataset = File.ReadLines(_inputFileLocation).Skip(1); //Assuming row 1 is headers 
            var markers = TransformInputFileToListOfObjects(dataset);
 
            var chromosomeSets = markers.GroupBy(c => c.Chromosome).Select(c => c.ToList()).ToList();

            // All the work is done for a set of chromosomes,
            // where first the chromosomes are in order and then so are positions. (genomic order)
            foreach (var chromosomeSet in chromosomeSets)
            {
                BuildResultSetForChromosome(chromosomeSet);
            }
            
            // However, there are several regions with many markers below this threshold that are not unique. These markers are correlated due to the underlying
            // structure of the genome (linkage disequilibrium), and we want to identify and summarize all of the UNIQUE regions.
            //ConsolidateResultSet(); 
            BuildResultFile(_outputFileLocation);
            
            return _resultSet;
        }

        private void BuildResultSetForChromosome(List<Marker> chromosomeSet)
        {
            //Verify position
            var workingChromosome = VerifyChromosonalOrder(chromosomeSet);

            var stepOneCandidates = GetRecordsExceedingIndexThreshold(workingChromosome);

            if (stepOneCandidates.Any())
            {
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
                            var expandedResults = GetExpandedSearchSpace(workingChromosome, regionCandidate.Position)
                                .ToList();

                            // We will define the start and stop positions of the region as the positions of the first and last marker
                            // in the region that meet the SUGGESTIVE THRESHOLD.
                            var newRegion = BuildRegion(expandedResults.ToList(), regionCandidate);
                            if (IsNewMarker(newRegion.MarkerName) && !OverlapsWithPreviousRegion(newRegion))
                            {
                                newRegion.RegionIndex = _resultSet.Count + 1;
                                _resultSet.Add(newRegion);
                            }
                            else
                            {
                                FixUpRegion(newRegion, expandedResults.ToList());
                            }
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine(
                    $"For chromosome {workingChromosome.First().Chromosome}, no markers found with an index p-value threshold exceeding {_indexPvalueThreshold}");
            }
        }

        private bool OverlapsWithPreviousRegion(Region newRegion)
        {
            if (_resultSet.Count < 1)
            {
                return false;
            }
            var previousRegion = _resultSet.Last();
            if ((newRegion.RegionStart - previousRegion.RegionStart < 500000) && (newRegion.RegionStart - previousRegion.RegionStart > 0))
            {
                return true;
            }
            return false; 
        }

        private void FixUpRegion(Region newRegion, List<Marker> chromosomeSet)
        {
            var regionNeedingFixup = _resultSet.FirstOrDefault(x => (newRegion.RegionStart - x.RegionStart) < _searchSpace &&
                                                           (newRegion.RegionStart - x.RegionStart) >= 0 &&
                                                           x.Chr == newRegion.Chr);

            if (regionNeedingFixup == null)
            {
                return;
            }

            if (regionNeedingFixup.RegionStart > newRegion.RegionStart)
            {
                regionNeedingFixup.RegionStart = newRegion.RegionStart;
            }
            if (regionNeedingFixup.RegionStop < newRegion.RegionStop)
            {
                regionNeedingFixup.RegionStop = newRegion.RegionStop;
            }

            var region = chromosomeSet.Where(x => x.Pvalue < _suggestivePvalueThreshold).ToList();

            regionNeedingFixup.NumSigMarkers = region.Count(x => x.Pvalue < _indexPvalueThreshold);
            regionNeedingFixup.NumSuggestiveMarkers = region.Count(x => x.Pvalue < _suggestivePvalueThreshold);
            regionNeedingFixup.NumTotalMarkers = chromosomeSet.Count(x => x.Position >= regionNeedingFixup.RegionStart && x.Position <= regionNeedingFixup.RegionStop);
            regionNeedingFixup.SizeOfRegion = regionNeedingFixup.RegionStop - regionNeedingFixup.RegionStart + 1;
        }

        private static List<Marker> VerifyChromosonalOrder(List<Marker> chromosomeSet)
        {
            var isSorted = IsSorted(chromosomeSet);

            if (!isSorted) //Be nice and resort if data got messed up
            {
                Console.WriteLine($"Markers for chromosome {chromosomeSet.First().Chromosome} are not in order. Reshuffling...");
                chromosomeSet = chromosomeSet.OrderBy(x => x.Position).ToList();
            }

            return chromosomeSet;
        }

        private bool IsNewMarker(string newRegionMarkerName)
        {
            if (_resultSet.Any(x => x.MarkerName == newRegionMarkerName))
            {
                return false;
            }

            return true;
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

        public Region BuildRegion(List<Marker> chromosomeSet, Marker regionCandidate)
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
                Position = region.OrderBy(p => p.Pvalue).First().Position,

                // The p-value of the marker with the minimum p-value
                Pvalue = region.OrderBy(p => p.Pvalue).First().Pvalue,

                RegionStart = region.OrderBy(p => p.Position).First().Position,
                RegionStop = region.OrderByDescending(p => p.Position).First().Position,
                // The number of markers in the region with a p-value less than the index p-value threshold
                NumSigMarkers = region.Count(x => x.Pvalue < _indexPvalueThreshold),
                NumSuggestiveMarkers = region.Count(x => x.Pvalue < _suggestivePvalueThreshold),

            };

            newRegion.NumTotalMarkers = chromosomeSet.Count(x => x.Position >= newRegion.RegionStart && x.Position <= newRegion.RegionStop);
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



        private void BuildResultFile(string resultsFileLocation)
        {
            // WriteAllLines creates a file, writes a collection of strings to the file,
            // and then closes the file.  You do NOT need to call Flush() or Close().
            using (StreamWriter file = new System.IO.StreamWriter(resultsFileLocation))
            {
                var header = new[] { "Region", "MarkerName", "Chr", "Position", "P-value", "RegionStart",
                    "RegionStop", "NumSigMarkers", "NumSuggestiveMarkers",  "NumTotalMarkers", "SizeOfRegion" };
                file.WriteLine(string.Join("\t", header));

                foreach (var result in _resultSet)
                {
                    var arrayResult = new[]
                    {
                        result.RegionIndex.ToString(), result.MarkerName, result.Chr.ToString(),
                        result.Position.ToString(),
                        result.Pvalue.ToString(), result.RegionStart.ToString(), result.RegionStop.ToString(),
                        result.NumSigMarkers.ToString(),
                        result.NumSuggestiveMarkers.ToString(), result.NumTotalMarkers.ToString(),
                        result.SizeOfRegion.ToString()
                    };


                    file.WriteLine(string.Join("\t", arrayResult));
                }
            }
        }
    }
}
