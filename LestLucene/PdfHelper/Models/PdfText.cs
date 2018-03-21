using LestLucene.IndexFolder.Models;
using Lucene.Net.Documents;
using System.IO;

namespace LestLucene.PdfHelper.Models
{
    public class PdfText : IIndexable
    {
        public string Text { get; set; }
        public string PdfPath { get; set; }

        public Document ToDocument()
        {
            var result = new Document();

            result.Add(new Field("id",
                string.Format("{0}", Path.GetFileNameWithoutExtension(PdfPath)),
                Field.Store.YES,
                Field.Index.NO));

            result.Add(new Field("t", string.Format("{0}", PdfPath), Field.Store.YES, Field.Index.NO));
            result.Add(new Field("v", Text, Field.Store.YES, Field.Index.ANALYZED));
            
            return result;
        }
    }
}
