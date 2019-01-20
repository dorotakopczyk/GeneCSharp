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
                List<Marker> filedata = new List<Marker>();
                
                foreach (var line in File.ReadLines("C:\\Users\\Dorota Kopczyk\\Downloads\\input.txt").Skip(1))
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
                        Console.WriteLine("Could not parse pvalue for " + tempLine[3]);
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
