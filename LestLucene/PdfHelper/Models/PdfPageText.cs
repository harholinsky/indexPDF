﻿using LestLucene.IndexFolder.Models;
using Lucene.Net.Documents;
using System.IO;

namespace LestLucene.PdfHelper.Models
{
    public class PdfPageText : IIndexable
    {
        public int PageNumber { get; set; }
        public string Text { get; set; }
        public string PdfPath { get; set; }

        public Document ToDocument()
        {
            var result = new Document();

            result.Add(new Field("PdtPath", PdfPath, Field.Store.YES, Field.Index.NO));
            result.Add(new Field("PageNum", PageNumber + "", Field.Store.YES, Field.Index.NO));
            result.Add(new Field("Text", Text, Field.Store.NO, Field.Index.ANALYZED, Field.TermVector.WITH_POSITIONS_OFFSETS));
            
            return result;
        }
    }
}
