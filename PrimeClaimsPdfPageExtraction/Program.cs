using CommandLine;
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


        static public string FileToProcess;

        static public string OutputDirectory;

        static public bool DoWriteJpgsToPDF;

        static void Main(string[] args)
            {
            Options options = new Options();

            Parser commandLineParser = new Parser();
            commandLineParser.FormatCommandLine(options);
            /* The static function RunOptions is called after parsing the command line arguments */
            ParserResult<Options> parserResult = commandLineParser.ParseArguments<Options>(args).WithParsed(RunOptions);

 
            string tmpDirectory = OutputDirectory + System.IO.Path.DirectorySeparatorChar + "tmp";
            if (!Directory.Exists(OutputDirectory))
                {
                throw new Exception(OutputDirectory + " does not exist");
                }
            if (! File.Exists(FileToProcess))
                {
                throw new Exception(FileToProcess + " does not exist");
                }

            if (! Directory.Exists(tmpDirectory) )
                {
                Directory.CreateDirectory(tmpDirectory);
                }
            if (DoWriteJpgsToPDF)
                {
                PrimeClaimsPdfExtractor extractor = new PrimeClaimsPdfExtractor(FileToProcess, tmpDirectory, tmpDirectory);
                extractor.SplitIntoSinglePages();

                PrimeClaimsJpgPdfWriter writer = new PrimeClaimsJpgPdfWriter(OutputDirectory);
                writer.WritePdfToJpgPdf();
                }
            else
                {
                PrimeClaimsPdfExtractor extractor = new PrimeClaimsPdfExtractor(FileToProcess, OutputDirectory, tmpDirectory);
                extractor.SplitIntoSinglePages();
                }
            }
        static public void RunOptions(Options options)
            {

            if (options.doWriteJpgsToPDF)
                {
                Program.DoWriteJpgsToPDF = true;
                }

            if (!String.IsNullOrEmpty(options.fileToProcess))
                {
                Program.FileToProcess = options.fileToProcess;
                }

            if (!String.IsNullOrEmpty(options.outputDirectory))
                {
                Program.OutputDirectory = options.outputDirectory;
                }

            }
        }
    class Options
        {

        [Option('f', "FileToProcess", Required = true,
          HelpText = "Prime PDF to split")]
        public string fileToProcess { get; set; }

        [Option('o', "OutputDirectory", Required = true,
          HelpText = "Directory to output all pages as pdf files.")]
        public string outputDirectory { get; set; }


        [Option('j', "DoWriteJpgsToPDF", Required = false,
          HelpText = "Create PDFs with JPG images embedded, instead of PDF txt content files.")]
        public bool doWriteJpgsToPDF { get; set; }

        }


    }
