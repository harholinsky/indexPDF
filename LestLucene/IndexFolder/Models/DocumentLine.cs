using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lucene.Net.Documents;

namespace LestLucene.IndexFolder.Models
{
    public class DocumentLine : IIndexable
    {
        public DocumentLine(string filePath, string fileName, int pageNumber, int valueLine, string text)
        {
            Id = $"{fileName}-{pageNumber}-{valueLine}";
            Title = $"{filePath}|{pageNumber}";
            Value = text;
        }


        /// <summary>
        /// identifier (id = {file name}-{page number}-{line})
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Title = {file path}|{page number}
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Text to indexing
        /// </summary>
        public string Value { get; set; }

        public Document ToDocument()
        {
            var result = new Document();

            result.Add(new Field("id", Id, Field.Store.YES, Field.Index.NO));
            result.Add(new Field("t", Title, Field.Store.YES, Field.Index.NO));
            result.Add(new Field("v", Value, Field.Store.YES, Field.Index.ANALYZED));

            return result;
        }
    }
}
