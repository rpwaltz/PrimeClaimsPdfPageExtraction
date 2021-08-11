using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Layout;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PrimeClaimsPdfPageExtraction
{
    public class PrimeClaimsPdfExtractor
    {
        private string inputDirectory;
        private string tmpDirectory;
        private string outputDirectory;
        private string primePdfFilename;

        public string InputDirectory { get => inputDirectory; set => inputDirectory = value; }
        public string PrimePdfFilename { get => primePdfFilename; set => primePdfFilename = value; }
        public string TmpDirectory { get => tmpDirectory; set => tmpDirectory = value; }
        public string OutputDirectory { get => outputDirectory; set => outputDirectory = value; }

        public PrimeClaimsPdfExtractor(string inputDirectory, string primePdfFilename, string outputDirectory = null, string tmpDirectory = null)
        {
            InputDirectory = inputDirectory;
            PrimePdfFilename = primePdfFilename;
            if (String.IsNullOrEmpty(tmpDirectory))
            {
                TmpDirectory = inputDirectory + System.IO.Path.DirectorySeparatorChar + "tmp";
            }
            else
            {
                TmpDirectory = tmpDirectory;
            }
            if (String.IsNullOrEmpty(tmpDirectory))
            {
                OutputDirectory = inputDirectory + System.IO.Path.DirectorySeparatorChar + "output";
            }
            else
            {
                OutputDirectory = outputDirectory;
            }
            if (InputDirectory.Equals(OutputDirectory) || InputDirectory.Equals(TmpDirectory) || TmpDirectory.Equals(OutputDirectory))
            {
                throw new Exception("Input, Output and Tmp directories must be different");
            }
            if (!(Directory.Exists(inputDirectory) && Directory.Exists(TmpDirectory) && Directory.Exists(OutputDirectory)))
            {
                throw new Exception("A directory needs to be created");
            }

        }
        public void SplitIntoSinglePages()
        {
            string sourcePDFFilename = InputDirectory + System.IO.Path.DirectorySeparatorChar + PrimePdfFilename;
            FileInfo fileInfo = new FileInfo(sourcePDFFilename);
            FileStream fileStream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            PdfDocument pdfDocument = new PdfDocument(new PdfReader(fileStream));

            // iText.Kernel.Pdf.Canvas.Parser.Listener.SimpleTextExtractionStrategy
            SplitPdfDocument(pdfDocument);
            pdfDocument.Close();
            DirectoryInfo pdfTmpDirctoryFileInfo = new DirectoryInfo(tmpDirectory);
            FileInfo[] fileInfoArray = pdfTmpDirctoryFileInfo.GetFiles("*.pdf");

            for (int i = 0; i < fileInfoArray.Length; i++)
            {
                BuildCSVData(fileInfoArray[i]);
            }
        }
        private void SplitPdfDocument(PdfDocument pdfDocument)
        {
            System.Console.WriteLine("FOUND Number of Pages: " + pdfDocument.GetNumberOfPages());
            for (int i = 1; i <= pdfDocument.GetNumberOfPages(); ++i)
            {
                WriterProperties writerProperties = new WriterProperties();
                FileInfo fileInfo = new FileInfo(String.Format(@"{0}{1}Pg_{2}.pdf", TmpDirectory, System.IO.Path.DirectorySeparatorChar, i));
                System.Console.WriteLine("CREATE : " + fileInfo.FullName);
                FileStream fileStream = fileInfo.Open(FileMode.Create, FileAccess.Write, FileShare.ReadWrite);

                PdfWriter pdfWriter = new PdfWriter(fileStream);

                PdfDocument newPdf = new PdfDocument(pdfWriter);

                pdfDocument.CopyPagesTo(i, i, newPdf);
                newPdf.Close();

            }

        }

        private void BuildCSVData(FileInfo fileInfo)
        {
            FileStream fileStream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.Delete);
            PdfDocument pdfDocument = new PdfDocument(new PdfReader(fileStream));

            var strategy = new SimpleTextExtractionStrategy();

            // System.Console.WriteLine("\n\n\n\n Find : " + pdfDocument.GetDocumentInfo().GetTitle() + " pages " + pdfDocument.GetNumberOfPages());
            string billId = "";
            string claimAmount = "";
            string claimNumber = "";
            for (int i = 1; i <= pdfDocument.GetNumberOfPages(); ++i)
            {

                PdfPage page = pdfDocument.GetPage(i);

                string text = PdfTextExtractor.GetTextFromPage(page, strategy);
                // System.Console.WriteLine(text);
                Regex regex = new Regex(@"Bill\s+ID\s+(?<billid>\S+)", RegexOptions.ExplicitCapture);
                MatchCollection matches = regex.Matches(text);
                foreach (Match match in matches)
                {
                    if (match.Success)
                    {
                        GroupCollection groups = match.Groups;
                        System.Console.WriteLine("FOUND BILL ID: " + groups["billid"]);
                        billId = groups["billid"].ToString();
                    }
                }
                
                //           Totals: $1,131.00 $714.70 $83.26 $333.04
                regex = new Regex(@"Totals:(?:\s+\$[\d,\.]+){3}\s+\$(?<claimamount>[\d,\.]+)", RegexOptions.ExplicitCapture);
                matches = regex.Matches(text);
                foreach (Match match in matches)
                {
                    if (match.Success)
                    {
                        GroupCollection groups = match.Groups;
                        System.Console.WriteLine("FOUND CLAIM AMOUNT: " + groups["claimamount"]);
                        claimAmount = groups["claimamount"].ToString();

                    }
                }

                regex = new Regex(@"Claim Number\s+(?<claimNumber>[\d\w]+)", RegexOptions.ExplicitCapture);
                matches = regex.Matches(text);
                foreach (Match match in matches)
                {
                    if (match.Success)
                    {
                        GroupCollection groups = match.Groups;
                        System.Console.WriteLine("FOUND CLAIM NUMBER: " + groups["claimNumber"]);
                        claimNumber = groups["claimNumber"].ToString();
                    }
                }
                
                string destTextFilename = TmpDirectory + System.IO.Path.DirectorySeparatorChar + billId + ".pdf";
                FileInfo fileInfoTextOnlyPdf = new FileInfo(destTextFilename);
                FileStream fileStreamImageOnlyPdf = fileInfoTextOnlyPdf.Open(FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);

                PdfDocument textOnlyPdf = new PdfDocument(new PdfWriter(fileStreamImageOnlyPdf));

                textOnlyPdf.AddPage(page.CopyTo(textOnlyPdf));
                Document pdfTextDocument = new Document(textOnlyPdf);

                pdfTextDocument.Close();
            }
            fileStream.Close();
            
            fileInfo.Delete();
        }
    }
}
