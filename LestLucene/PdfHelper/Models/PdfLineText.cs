using LestLucene.IndexFolder.Models;
using Lucene.Net.Documents;
using System.IO;

namespace LestLucene.PdfHelper.Models
{
    public class PdfLineText : IIndexable
    {
        public int LineNumber { get; set; }
        public int PageNumber { get; set; }
        public string Text { get; set; }
        public string PdfPath { get; set; }

        public Document ToDocument()
        {
            var result = new Document();

            result.Add(new Field("id", 
                $"{Path.GetFileNameWithoutExtension(PdfPath)}-{PageNumber}-{LineNumber}",
                Field.Store.YES,
                Field.Index.NO));

            result.Add(new Field("t", $"{PdfPath}|{PageNumber}", Field.Store.YES, Field.Index.NO));

            result.Add(new Field("PageNum", PageNumber + "", Field.Store.YES, Field.Index.NO));
            result.Add(new Field("LineNum", LineNumber + "", Field.Store.YES, Field.Index.NO));

            result.Add(new Field("v", Text, Field.Store.YES, Field.Index.ANALYZED));


            return result;
        }
    }
}
