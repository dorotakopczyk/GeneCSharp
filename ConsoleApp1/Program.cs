using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            try
            {
                var dataset = File.ReadLines("C:\\Users\\Dorota Kopczyk\\Downloads\\input.txt").Skip(1);
                var markers =  TransformInputFileToListOfObjects(dataset);
                var chromosomeSets = markers.GroupBy(c => c.Chromosome).Select(c => c.ToList()).ToList();

                // All the work is done for a set of chromosomes,
                // where first the chromosomes are in order and then so are positions. (genomic order)
                foreach (var chromosomeSet in chromosomeSets)
                {
                    //Verify position
                    var isSorted = IsSorted(chromosomeSet);
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
