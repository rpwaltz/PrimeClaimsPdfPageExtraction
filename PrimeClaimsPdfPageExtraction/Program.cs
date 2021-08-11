using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimeClaimsPdfPageExtraction
{
    class Program
    {
        static private string inputDirectory = @"C:\Tmp\PrimeClaims";

        static private string primePdfFilename = "BigBatch.pdf";
        

        static void Main(string[] args)
        {
            string tmpDirectory = inputDirectory + System.IO.Path.DirectorySeparatorChar + "tmp";
            string outputDirectory = inputDirectory + System.IO.Path.DirectorySeparatorChar + "output";
            PrimeClaimsPdfExtractor extractor = new PrimeClaimsPdfExtractor(inputDirectory, primePdfFilename, outputDirectory, tmpDirectory);
            extractor.SplitIntoSinglePages();
            PrimeClaimsJpgPdfWriter writer = new PrimeClaimsJpgPdfWriter(tmpDirectory, outputDirectory);
            writer.WritePdfToJpgPdf();
        }
    }
}
