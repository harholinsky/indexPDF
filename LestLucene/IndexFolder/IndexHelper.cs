using LestLucene.IndexFolder.Models;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LestLucene.IndexFolder
{
    public class IndexHelper
    {

        #region Index writing

        public static IndexWriter CreateWriter(Analyzer analyzer, string pathToWriteIndexes)
        {
            var dir = new DirectoryInfo(pathToWriteIndexes);
            var mmapDir = new MMapDirectory(dir);
            

            return new IndexWriter(mmapDir, analyzer, IndexWriter.MaxFieldLength.UNLIMITED);
        }

        public static void WriteIndex(IIndexable toIdnex, IndexWriter writer)
        {
            writer.AddDocument(toIdnex.ToDocument());
            writer.Commit();
        }

        public static void WriteIndex(IEnumerable<IIndexable> toIdnex, IndexWriter writer)
        {
            foreach (var item in toIdnex)
                writer.AddDocument(item.ToDocument());
        }

        public static void WriteIndex(IIndexable toIdnex, string pathToWriteIndexes)
        {
            using (var writer = CreateWriter(new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30), pathToWriteIndexes))
            {
                writer.AddDocument(toIdnex.ToDocument());
            }
        }

        public static void WriteIndex(IEnumerable<IIndexable> toIdnex, string pathToWriteIndexes)
        {
            using (var writer = CreateWriter(new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30), pathToWriteIndexes))
            {
                foreach (var item in toIdnex)
                    writer.AddDocument(item.ToDocument());
            }
        }

        #endregion

        #region index search

        public static TopDocs Search(string pathToIndexedDir, string searchPattern)
        {
            var directory = FSDirectory.Open(new DirectoryInfo(pathToIndexedDir));
            Analyzer analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);

            var reader = DirectoryReader.Open(directory, readOnly: true);

            var parser = new MultiFieldQueryParser(Lucene.Net.Util.Version.LUCENE_30,
                reader.GetFieldNames(IndexReader.FieldOption.ALL).ToArray(),
                analyzer);

            var searcher = new IndexSearcher(directory);
            Query query = parser.Parse(searchPattern);

            searcher.Dispose();
            directory.Dispose();

            return searcher.Search(query, reader.MaxDoc);
        }

        public static TopDocs Search(string pathToIndexedDir, string searchPattern, out IndexSearcher searcher)
        {
            var directory = FSDirectory.Open(new DirectoryInfo(pathToIndexedDir));
            Analyzer analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);

            var reader = DirectoryReader.Open(directory, readOnly: true);

            var parser = new MultiFieldQueryParser(Lucene.Net.Util.Version.LUCENE_30,
                reader.GetFieldNames(IndexReader.FieldOption.ALL).ToArray(),
                analyzer);

            searcher = new IndexSearcher(directory);
            Query query = parser.Parse(searchPattern);

            return searcher.Search(query, reader.MaxDoc);
        }

        public static TopDocs Search(string pathToIndexedDir, string searchPattern, QueryParser parser)
        {
            var directory = FSDirectory.Open(new DirectoryInfo(pathToIndexedDir));

            var reader = DirectoryReader.Open(directory, readOnly: true);

            var searcher = new IndexSearcher(directory);
            Query query = parser.Parse(searchPattern);

            return searcher.Search(query, reader.MaxDoc);
        }

        #endregion
    }
}
