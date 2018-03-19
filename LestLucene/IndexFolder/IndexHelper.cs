using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LestLucene.IndexFolder
{
    public class IndexHelper
    {
        public static IndexWriter CreateWriter(Analyzer analyzer, string path)
        {
            var dir = new DirectoryInfo(path);
            var mmapDir = new MMapDirectory(dir);

            return new IndexWriter(mmapDir, analyzer, IndexWriter.MaxFieldLength.UNLIMITED);
        }
    }
}
