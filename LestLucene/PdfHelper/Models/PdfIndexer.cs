using LestLucene.IndexFolder;
using LestLucene.IndexFolder.Models;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace LestLucene.PdfHelper.Models
{
    public class PdfIndexer : IDisposable
    {
        private static readonly int MAX_PARALLEL_TASKS_COUNT = Environment.ProcessorCount * 5;

        private IndexWriter writer = null;
        private IndexSearcher searcher = null;
        private FSDirectory directory = null;
        private Analyzer analyzer;

        public event EventHandler<IndexedFile> FileIndexed;

        public PdfIndexer(string indexedDir, bool removeAllIndexedData)
        {
            analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);
            writer = IndexHelper.CreateWriter(analyzer, indexedDir);

            directory = FSDirectory.Open(new DirectoryInfo(indexedDir));
            searcher = new IndexSearcher(directory);

            if (removeAllIndexedData)
                writer.DeleteAll();
        }

        public PdfIndexer(string indexedDir)
        {
            analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);
            writer = IndexHelper.CreateWriter(analyzer, indexedDir);

            directory = FSDirectory.Open(new DirectoryInfo(indexedDir));
            searcher = new IndexSearcher(directory);

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

            writer.Optimize();
            writer.Flush(true, true, true);
        }

        public void IndexPdfFilesFromFolder(string folderPath, PdfIndexTypes indexType)
        {
            string[] pdfFiles = System.IO.Directory.GetFiles(folderPath, "*.pdf", SearchOption.AllDirectories);
            if (pdfFiles.Length == 0)
            {
                Console.WriteLine(string.Format("Cant find any *.pdf file in folder \"{0}\"", folderPath));
                return;
            }

            var tasks = new List<Task>();

            foreach (var file in pdfFiles)
            {
                try
                {
                    //Index md5
                    string MD5 = MD5Helper.CalculateMD5ForFile(file);

                    var fileInfo = new Lucene.Net.Documents.Document();
                    fileInfo.Add(new Field("FilePath", file, Field.Store.NO, Field.Index.ANALYZED));
                    fileInfo.Add(new Field("MD5", MD5, Field.Store.YES, Field.Index.NO));
                    writer.AddDocument(fileInfo);

                    //index file
                    if (tasks.Count >= MAX_PARALLEL_TASKS_COUNT)
                    {
                        int finishedIndex = Task.WaitAny(tasks.ToArray());
                        tasks.RemoveAt(finishedIndex);
                    }

                    tasks.Add(Task.Factory.StartNew(() =>
                    {
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
                    }));

                }
                catch
                {
                    this.Dispose();
                    throw;
                }
            }
            Task.WaitAll(tasks.ToArray());

            this.Dispose();
        }

        private void IndexPdfFileAllLines(string file)
        {
            var items = PdfToTextParser.ExtractTextLinesFromPdf(file);

            foreach (var item in items)
            {
                writer.AddDocument(item.ToDocument());
            }

            if (FileIndexed != null)
                FileIndexed(this, new IndexedFile { Path = file, Message = string.Empty });
        }

        private void IndexPdfFileAllPages(string file)
        {
            var items = PdfToTextParser.ExtractTextPagesFromPdf(file);
            var tasks = new List<Task>();

            foreach (var item in items)
            {
                writer.AddDocument(item.ToDocument());
            }

            if (FileIndexed != null)
                FileIndexed(this, new IndexedFile { Path = file, Message = string.Empty });
        }

        private void IndexPdfFileAll(string file)
        {
            var item = PdfToTextParser.ExtractTextFromPdf(file);

            writer.AddDocument(item.ToDocument());

            if (FileIndexed != null)
                FileIndexed(this, new IndexedFile { Path = file, Message = string.Empty });
        }

        #endregion


        #region search

        public IEnumerable<Document> SearchPdfByText(string searchPattern)
        {
            var reader = DirectoryReader.Open(directory, readOnly: true);

            var parser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30,
                "Text",
                analyzer);

            parser.DefaultOperator = QueryParser.Operator.AND;

            Query query = parser.Parse(searchPattern);

            var topDocs = searcher.Search(query, reader.MaxDoc);

            foreach (var scoreDoc in topDocs.ScoreDocs)
            {
                yield return searcher.Doc(scoreDoc.Doc);
            }
        }

        #endregion

        public void Dispose()
        {
            if (writer != null)
            {
                writer.Optimize();
                writer.Flush(true, true, true);
                writer.Dispose();
            }
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

    public class IndexedFile : EventArgs
    {
        public string Path { get; set; }
        public string Message { get; set; }
    }
}
