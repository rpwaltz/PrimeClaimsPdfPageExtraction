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

        private string tmpDirectory;
        private string outputDirectory;
        private string primePdfFilename;

        string PrimePdfFilename { get => primePdfFilename; set => primePdfFilename = value; }
        string TmpDirectory { get => tmpDirectory; set => tmpDirectory = value; }
        string OutputDirectory { get => outputDirectory; set => outputDirectory = value; }

        public PrimeClaimsPdfExtractor(string primePdfFilename, string outputDirectory, string tmpDirectory)
        {
            OutputDirectory = outputDirectory;
            PrimePdfFilename = primePdfFilename;

            if (String.IsNullOrEmpty(tmpDirectory))
            {
                TmpDirectory = outputDirectory + System.IO.Path.DirectorySeparatorChar + "tmp";
            }
            else
            {
                TmpDirectory = tmpDirectory;
            }

            if (TmpDirectory.Equals(OutputDirectory))
            {
                throw new Exception("Output and Tmp directories must be different");
            }
            if (!Directory.Exists(TmpDirectory))
                {
                Directory.CreateDirectory(TmpDirectory);
                }
            }
        public void SplitIntoSinglePages()
        {
            FileInfo fileInfo = new FileInfo(PrimePdfFilename);
            FileStream fileStream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            PdfDocument pdfDocument = new PdfDocument(new PdfReader(fileStream));

            // iText.Kernel.Pdf.Canvas.Parser.Listener.SimpleTextExtractionStrategy
            SplitPdfDocument(pdfDocument);
            pdfDocument.Close();
            DirectoryInfo pdfTmpDirctoryFileInfo = new DirectoryInfo(tmpDirectory);
            FileInfo[] fileInfoArray = pdfTmpDirctoryFileInfo.GetFiles("*PG_?????.pdf");

            for (int i = 0; i < fileInfoArray.Length; i++)
            {
                BuildCSVDataAsync(fileInfoArray[i]);
            }
        }
        private void SplitPdfDocument(PdfDocument pdfDocument)
        {
            for (int i = 1; i <= pdfDocument.GetNumberOfPages(); ++i)
            {
                WriterProperties writerProperties = new WriterProperties();
                FileInfo fileInfo = new FileInfo(String.Format(@"{0}{1}PG_{2}.pdf", TmpDirectory, System.IO.Path.DirectorySeparatorChar, i));
                FileStream fileStream = fileInfo.Open(FileMode.Create, FileAccess.Write, FileShare.ReadWrite);

                PdfWriter pdfWriter = new PdfWriter(fileStream);

                PdfDocument newPdf = new PdfDocument(pdfWriter);

                pdfDocument.CopyPagesTo(i, i, newPdf);
                newPdf.Close();

            }

        }

        private async Task BuildCSVDataAsync(FileInfo fileInfo)
        {
            FileStream fileStream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.Delete);
            PdfDocument pdfDocument = new PdfDocument(new PdfReader(fileStream));

            var strategy = new SimpleTextExtractionStrategy();


            string billId = "";
            string claimAmount = "";
            string claimNumber = "";
            for (int i = 1; i <= pdfDocument.GetNumberOfPages(); ++i)
            {

                PdfPage page = pdfDocument.GetPage(i);

                string text = PdfTextExtractor.GetTextFromPage(page, strategy);
                
                Regex regex = new Regex(@"Bill\s+ID\s+(?<billid>.*)(\n|\r|\r\n)", RegexOptions.ExplicitCapture);
                MatchCollection matches = regex.Matches(text);
                foreach (Match match in matches)
                {
                    if (match.Success)
                    {
                        GroupCollection groups = match.Groups;

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

                        claimNumber = groups["claimNumber"].ToString();
                    }
                }

//               string destTextFilename = OutputDirectory + System.IO.Path.DirectorySeparatorChar + billId + ".txt";
//                File.WriteAllText(destTextFilename, text);

                string destTextPDFFilename = OutputDirectory + System.IO.Path.DirectorySeparatorChar + billId + ".pdf";
                FileInfo fileInfoTextOnlyPdf = new FileInfo(destTextPDFFilename);
                FileStream fileStreamTextOnly = fileInfoTextOnlyPdf.Open(FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);

                PdfDocument textOnlyPdf = new PdfDocument(new PdfWriter(fileStreamTextOnly));

                textOnlyPdf.AddPage(page.CopyTo(textOnlyPdf));
                Document pdfTextDocument = new Document(textOnlyPdf);

                pdfTextDocument.Close();
                System.Console.WriteLine("Created File for PDF Page named " + destTextPDFFilename);
                    
            }
            fileStream.Close();
            
            fileInfo.Delete();
        }
    }
}
