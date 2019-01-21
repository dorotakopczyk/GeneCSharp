﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var indexPvalueThreshold = 0.00001;
            var suggestivePvalueThreshold = 0.0001;
            var resultSet = new List<Region>();

            try
            {
                var dataset = File.ReadLines("C:\\Users\\Dorota Kopczyk\\Downloads\\input.txt").Skip(1);
                var markers =  TransformInputFileToListOfObjects(dataset);

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

                    var stepOneCandidates = SnpExceedingIndexThreshold(indexPvalueThreshold, workingChromosome);

                    if (!stepOneCandidates.Any())
                    {
                        Console.WriteLine($"For chromosome {workingChromosome.First().Chromosome}, no markers found with an index p-value threshold exceeding {indexPvalueThreshold}");
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
                            List<Marker> regions = GetRegionCandidates(suggestivePvalueThreshold, stepTwoCandidates);
                            if (regions.Any())
                            {
                                foreach (var regionCandidate in regions)
                                {
                                    
                                    var newRegion = BuildRegion(indexPvalueThreshold, suggestivePvalueThreshold, resultSet, chromosomeSet, regionCandidate);

                                    resultSet.Add(newRegion);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            finally
            {
                Console.WriteLine("Executing finally block.");
            }
        }

        private static Region BuildRegion(double indexPvalueThreshold, double suggestivePvalueThreshold, List<Region> resultSet, List<Marker> chromosomeSet, Marker regionCandidate)
        {
            //First, define the region by expanding the search results +/- 500k again
            var startingPositionReg = regionCandidate.Position - 500000;
            var endingPositionReg = regionCandidate.Position + 500000;
            var region = chromosomeSet.Where(x =>
                x.Position >= startingPositionReg && x.Position <= endingPositionReg).ToList();

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

        private static List<Marker> SnpExceedingIndexThreshold(double indexPvalueThreshold, List<Marker> workingSet)
        {
            return workingSet.Where(x => x.Pvalue < indexPvalueThreshold).ToList();
        }

        private static List<Marker> TransformInputFileToListOfObjects(IEnumerable<string> dataset)
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

        /// <summary>
        /// Determines if int list (this can be an array too) is sorted from 0 -> Max
        /// </summary>
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
