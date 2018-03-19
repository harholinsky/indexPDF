using System;
using System.Collections.Generic;
using System.Text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using LestLucene.PdfHelper.Models;

namespace LestLucene.PdfHelper
{
    public class PdfToTextParser
    {
        private static readonly string[] lineSeparators = new string[] { Environment.NewLine, "\n" };


        public static string ExtractTextFromPdf(string path)
        {
            using (PdfReader reader = new PdfReader(path))
            {
                StringBuilder text = new StringBuilder();

                for (int i = 1; i <= reader.NumberOfPages; i++)
                {
                    text.Append(PdfTextExtractor.GetTextFromPage(reader, i));
                }

                return text.ToString();
            }
        }


        public static List<PdfLineText> ExtractTextLinesFromPdf(string path)
        {
            List<PdfLineText> lines = new List<PdfLineText>();

            using (PdfReader reader = new PdfReader(path))
            {

                for (int i = 1; i <= reader.NumberOfPages; i++)
                {
                    string[] spilittedLines = PdfTextExtractor.GetTextFromPage(reader, i)
                        .Split(lineSeparators, StringSplitOptions.RemoveEmptyEntries);

                    for (int j = 0; j < spilittedLines.Length; j++)
                    {
                        lines.Add(new PdfLineText
                        {
                            LineNumber = j,
                            PageNumber = i,
                            Text = spilittedLines[j]
                        });
                    }
                }

                return lines;
            }
        }

        public static List<PdfPageText> ExtractTextPagesFromPdf(string path)
        {
            List<PdfPageText> lines = new List<PdfPageText>();

            using (PdfReader reader = new PdfReader(path))
            {
                for (int i = 1; i <= reader.NumberOfPages; i++)
                {
                    lines.Add(new PdfPageText
                    {
                        PageNumber = i,
                        Text = PdfTextExtractor.GetTextFromPage(reader, i)
                    });
                }

                return lines;
            }
        }
    }
}
