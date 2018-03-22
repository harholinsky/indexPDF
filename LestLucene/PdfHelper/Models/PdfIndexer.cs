using iTextSharp.text;
using LestLucene.IndexFolder;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LestLucene.PdfHelper.Models
{
    public class PdfIndexer : IDisposable
    {
        private IndexWriter writer = null;

        public PdfIndexer(string indexedDir, bool removeAllIndexedData)
        {
            writer = IndexHelper.CreateWriter(new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30), indexedDir);
            writer.DeleteAll();
        }

        public PdfIndexer(string indexedDir)
        {
            writer = IndexHelper.CreateWriter(new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30), indexedDir);
        }

        #region Index file


        public void IndexPdfFile(string file, PdfIndexTypes indexType)
        {
            //Index md5
            string MD5 = MD5Helper.CalculateMD5ForFile(file);

            var fileInfo = new Lucene.Net.Documents.Document();
            fileInfo.Add(new Field("FilePath", file, Field.Store.NO, Field.Index.ANALYZED));
            fileInfo.Add(new Field("MD5", MD5, Field.Store.YES, Field.Index.NO));
            writer.AddDocument(fileInfo);

            switch (indexType)
            {
                case PdfIndexTypes.ByPage:
                    IndexPdfFileAllPages(file);
                    break;
                case PdfIndexTypes.ByLine:
                    IndexPdfFileAllLines(file);
                    break;
                case PdfIndexTypes.ByAllText:
                default:
                    IndexPdfFileAll(file);
                    break;
            }
        }

        private void IndexPdfFileAllLines(string file)
        {
            var items = PdfToTextParser.ExtractTextLinesFromPdf(file);

            foreach (var item in items)
            {
                writer.AddDocument(item.ToDocument());
            }
        }

        private void IndexPdfFileAllPages(string file)
        {
            var items = PdfToTextParser.ExtractTextPagesFromPdf(file);
            var tasks = new List<Task>();

            foreach (var item in items)
            {
                writer.AddDocument(item.ToDocument());
            }
        }

        private void IndexPdfFileAll(string file)
        {
            var item = PdfToTextParser.ExtractTextFromPdf(file);

            writer.AddDocument(item.ToDocument());
        }
        
        #endregion


        public void Dispose()
        {
            if (writer != null)
                writer.Dispose();
        }
    }

    public enum PdfIndexTypes
    {
        /// <summary>
        /// Generate indexed document which include path + text
        /// </summary>
        ByAllText,
        /// <summary>
        /// Generate indexed documents for each page which includes path + text + page
        /// </summary>
        ByPage,
        /// <summary>
        /// Generate indexed documents for each line which includes path + text + page + line
        /// </summary>
        ByLine
    }
}
