using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp1
{
    public class Region
    {
        public int RegionIndex { get; set; }
        public string MarkerName { get; set; }
        public int Chr { get; set; }
        public double Pvalue { get; set; }
        public int RegionStart { get; set; }
        public int RegionStop { get; set; }
        public int NumSigMarkers { get; set; }
        public int NumSuggestiveMarkers { get; set; }
        public int NumTotalMarkers { get; set; }
        public int SizeOfRegion { get; set; }
    }
}
