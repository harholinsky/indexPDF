using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using LestLucene.PdfHelper.Models;

namespace LestLucene.PdfHelper
{
    public class PdfToTextParser
    {
        private static readonly string[] lineSeparators = new string[] { Environment.NewLine, "\n" };


        public static PdfText ExtractTextFromPdf(string path)
        {
            using (PdfReader reader = new PdfReader(path))
            {
                StringBuilder text = new StringBuilder();

                for (int i = 1; i <= reader.NumberOfPages; i++)
                {
                    text.Append(PdfTextExtractor.GetTextFromPage(reader, i));
                }

                return new PdfText
                {
                    PdfPath = path,
                    Text = text.ToString()
                };
            }
        }


        public static IEnumerable<PdfLineText> ExtractTextLinesFromPdf(string path)
        {
            PdfReader reader = null;

            try
            {
                reader = new PdfReader(path);
            }
            catch
            {
                yield break;
            }

            for (int i = 1; i <= reader.NumberOfPages; i++)
            {
                string pageText = PdfTextExtractor.GetTextFromPage(reader, i);

                string[] spilittedLines = pageText
                    .Split(lineSeparators, StringSplitOptions.RemoveEmptyEntries);

                for (int j = 0; j < spilittedLines.Length; j++)
                {
                    yield return new PdfLineText
                    {
                        LineNumber = j,
                        PageNumber = i,
                        Text = spilittedLines[j],
                        PdfPath = path
                    };
                }
            }

            reader.Close();
        }

        public static IEnumerable<PdfPageText> ExtractTextPagesFromPdf(string path)
        {
            PdfReader reader = null;

            try
            {
                reader = new PdfReader(path);
            }
            catch
            {
                yield break;
            }
            
            for (int i = 1; i <= reader.NumberOfPages; i++)
            {
                yield return new PdfPageText
                {
                    PageNumber = i,
                    Text = PdfTextExtractor.GetTextFromPage(reader, i),
                    PdfPath = path
                };
            }
        }
    }
}
