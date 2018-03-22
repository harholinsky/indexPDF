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
            long timeStart = DateTime.Now.Ticks;

            int filesCount = Directory.GetFiles(pathToFolder, "*.pdf", SearchOption.AllDirectories).Length;

            Console.WriteLine(string.Format("Indexing {0} pdf files:", filesCount));

            int indexedfilesCount = 0;
            var progress = new ProgressBar(filesCount);
            progress.Report(0);

            var indexer = new PdfIndexer(pathIndex);
            indexer.FileIndexed += delegate (object sender, IndexedFile file)
            {
                progress.Report(indexedfilesCount++);
            };
            indexer.IndexPdfFilesFromFolder(pathToFolder, PdfIndexTypes.ByPage);

            long timeEnd = DateTime.Now.Ticks;

            Console.WriteLine(string.Format("SUCCESS [Work time is {0} seconds]", (timeEnd - timeStart) / TimeSpan.TicksPerSecond));
        }

        private static void Search(string pathIndex, string searchPattern)
        {
            long timeStart = DateTime.Now.Ticks;

            var indexer = new PdfIndexer(pathIndex);

            var foundDocs = indexer.SearchPdfByText(searchPattern);

            Console.WriteLine(string.Format("Found {0} results", foundDocs.Count()));

            Console.WriteLine("Field name\t|\t\tField value");

            foreach (var doc in foundDocs.Take(10))
            {
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
