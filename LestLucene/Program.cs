using iTextSharp.text;
using LestLucene.IndexFolder;
using LestLucene.IndexFolder.Models;
using LestLucene.PdfHelper;
using LestLucene.PdfHelper.Models;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Search;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LestLucene
{
    class Program
    {
        private static readonly int MAX_PARALLEL_TASKS_COUNT = Environment.ProcessorCount * 5;

        static void Main(string[] args)
        {
            #region data parsing/validation
            //parse values
            string dirToIndex = null;
            string indexedDir = null;
            string searchPattern = null;

            try
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == "-dirToIndex")
                    {
                        dirToIndex = args[i + 1].Trim('<', '>', '"', '\'');
                    }
                    else if (args[i] == "-indexedDir")
                    {
                        indexedDir = args[i + 1].Trim('<', '>', '"', '\'');
                    }
                    else if (args[i] == "-search")
                    {
                        searchPattern = args[i + 1].Trim('<', '>', '"', '\'');
                    }
                }
            }
            catch { }

            if (args.Length == 0 || args.Contains("-help"))
            {
                Usage();
                return;
            }
            else if (!args.Contains("-indexedDir") || string.IsNullOrWhiteSpace(indexedDir))
            {
                Console.WriteLine("-indexedDir \"Path\" is required");
                Usage();
                return;
            }
            else if (args.Contains("-index")
                && (!args.Contains("-dirToIndex") || string.IsNullOrWhiteSpace(dirToIndex)))
            {
                Console.WriteLine("-dirToIndex is missed");
                Usage();
                return;
            }
            if (args.Contains("-search") && string.IsNullOrEmpty(searchPattern))
            {
                Console.WriteLine("search pattern is missed");
                Usage();
                return;
            }
            #endregion


            if (args.Contains("-index"))
            {
                IndexFolder(dirToIndex, indexedDir);
            }
            else if (args.Contains("-search"))
            {
                Search(indexedDir, searchPattern);
            }
        }


        private static void IndexFolder(string pathToFolder, string pathIndex)
        {
            int indexedCount = 0;
            long timeStart = DateTime.Now.Ticks;
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            string[] pdfFiles = Directory.GetFiles(pathToFolder, "*.pdf", SearchOption.AllDirectories);
            if (pdfFiles.Length == 0)
            {
                Console.WriteLine(string.Format("Cant find any *.pdf file in folder \"{0}\"", pathToFolder));
                return;
            }

            Console.WriteLine(string.Format("Indexing {0} pdf files:", pdfFiles.Length));
            var progress = new ProgressBar(pdfFiles.Length);
            progress.Report(0);

            using (var pdfIndexer = new PdfIndexer(pathIndex, true))
            {
                var tasks = new List<Task>();

                foreach (var file in pdfFiles)
                {
                    try
                    {
                        tasks.Add(Task.Factory.StartNew(() =>
                        {
                            pdfIndexer.IndexPdfFile(file, PdfIndexTypes.ByLine);
                        }));
                    }
                    finally
                    {
                        progress.Report(indexedCount++);
                    }

                }
                Task.WaitAll(tasks.ToArray());
                tokenSource.Cancel(true);
            }


            long timeEnd = DateTime.Now.Ticks;

            Console.WriteLine(string.Format("SUCCESS [Work time is {0} seconds]", (timeEnd - timeStart) / TimeSpan.TicksPerSecond));
        }

        private static void Search(string pathIndex, string searchPattern)
        {
            long timeStart = DateTime.Now.Ticks;

            IndexSearcher searcher = null;

            var topDocs = IndexHelper.Search(pathIndex, searchPattern, out searcher);

            Console.WriteLine(string.Format("Found {0} results", topDocs.ScoreDocs.Length));

            Console.WriteLine("Field name\t|\t\tField value");

            foreach (var scoreDoc in topDocs.ScoreDocs)
            {
                var doc = searcher.Doc(scoreDoc.Doc);
                var fields = doc.GetFields();

                if (fields.Count > 0)
                    Console.WriteLine("----------------------");

                foreach (var field in fields)
                {
                    Console.WriteLine(string.Format("{0}\t\t|\t\t{1}", field.Name, field.StringValue));
                }
            }

            long timeEnd = DateTime.Now.Ticks;

            Console.WriteLine(string.Format("SUCCESS [Work time is {0} seconds]", (timeEnd - timeStart) / TimeSpan.TicksPerSecond));
        }

        private static void Usage()
        {
            Console.WriteLine("Usage [this app works only with pdf files]:");
            Console.WriteLine("-search \"search pattern\"      - search in indexed dir");
            Console.WriteLine("-index                         - search in indexed dir");
            Console.WriteLine("-indexedDir \"Path\"            - path to index derictory");
            Console.WriteLine("-dirToIndex \"Path\"            - path to derictory which should be indexed");
        }

    }
}
