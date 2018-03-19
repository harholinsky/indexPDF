using Lucene.Net.Documents;

namespace LestLucene.IndexFolder.Models
{
    public interface IIndexable
    {
        Document ToDocument();
    }
}
