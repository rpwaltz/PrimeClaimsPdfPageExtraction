using iText.IO.Image;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;

namespace PrimeClaimsPdfPageExtraction
{
    public class PrimeClaimsJpgPdfWriter
    {
        private string inputDirectory;
        private string outputDirectory;

        public string InputDirectory { get => inputDirectory; set => inputDirectory = value; }
        public string OutputDirectory { get => outputDirectory; set => outputDirectory = value; }

        public PrimeClaimsJpgPdfWriter(string inputDirectory, string outputDirectory)
        {
            InputDirectory = inputDirectory;

            OutputDirectory = outputDirectory;


        }

        public void WritePdfToJpgPdf()
        {
            DirectoryInfo pdfTmpDirctoryFileInfo = new DirectoryInfo(inputDirectory);
            FileInfo[] fileInfoArray = pdfTmpDirctoryFileInfo.GetFiles("*.pdf");
            for (int i = 0; i < fileInfoArray.Length; i++)
            {
                
               
                string outputJpgPdfFile = outputDirectory + System.IO.Path.DirectorySeparatorChar + fileInfoArray[i].Name;
                FileInfo fileInfoImageOnlyPdf = new FileInfo(outputJpgPdfFile);
                FileStream fileStreamImageOnlyPdf = fileInfoImageOnlyPdf.Open(FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);

                PdfDocument imageOnlyPdf = new PdfDocument(new PdfWriter(fileStreamImageOnlyPdf));
                Document document = new Document(imageOnlyPdf);

                // The Prime document is printed out A4 sized
                PageSize pageSize = PageSize.A4;

                //List<byte[]> jpegImages = Pdf2Image.PdfSplitter.ExtractJpeg(fileInfoArray[i].FullName);
                List<int> pagenumbers = new List<int>();
                pagenumbers.Add(1);
                List<System.Drawing.Image> listImages = Pdf2Image.PdfSplitter.GetImages(fileInfoArray[i].FullName, Pdf2Image.PdfSplitter.Scale.High, pagenumbers);
                foreach (System.Drawing.Image pageImage in listImages)
                {
                    MemoryStream memoryStream = new MemoryStream();
                    pageImage.Save(memoryStream, ImageFormat.Tiff);
                    
                    ImageData imageData = ImageDataFactory.Create(memoryStream.GetBuffer());
                    Image image = new Image(imageData);
                    image.SetFixedPosition(0, 0);
                    image.SetWidth(pageSize.GetWidth());
                    image.SetHeight(pageSize.GetHeight());
                    document.Add(image);

                }
                document.Close();
               // Console.WriteLine("found images " + jpegImages.Count);
            }
        }

    }
}
