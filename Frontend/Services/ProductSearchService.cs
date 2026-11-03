using CatalogManager.Application.Interfaces;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using System.Globalization;

namespace CatalogManager.Services;

public sealed record ProductSearchResult(
    int Id,
    string Name,
    string? Description,
    decimal Price,
    string? ImageUrl,
    int CategoryId,
    int StoreId,
    float Score);

public sealed class ProductSearchService(IProductService productService)
{
    private const LuceneVersion Version = LuceneVersion.LUCENE_48;

    public async Task<IReadOnlyList<ProductSearchResult>> SearchAsync(
        string? query,
        int limit = 20)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Array.Empty<ProductSearchResult>();

        query = query.Trim();
        if (query.Length > 100)
            throw new ArgumentException("Search query cannot exceed 100 characters.");

        limit = Math.Clamp(limit, 1, 50);

        // Retrieve the latest products so that MVC/API edits are always visible.
        var products = await productService.GetAllAsync();
        if (products.Count == 0)
            return Array.Empty<ProductSearchResult>();

        // Build a new in-memory search index for each request
        // to ensure that search results reflect current product data.
        using var directory = new RAMDirectory();
        using var analyzer = new StandardAnalyzer(Version);

        var config = new IndexWriterConfig(Version, analyzer);
        using (var writer = new IndexWriter(directory, config))
        {
            foreach (var product in products)
            {
                var document = new Document
                {
                    new StringField("id", product.Id.ToString(CultureInfo.InvariantCulture), Field.Store.YES),
                    new TextField("name", product.Name ?? string.Empty, Field.Store.NO),
                    new TextField("description", product.Description ?? string.Empty, Field.Store.NO)
                };

                writer.AddDocument(document);
            }

            writer.Commit();
        }

        using var reader = DirectoryReader.Open(directory);
        var searcher = new IndexSearcher(reader);

        var parser = new MultiFieldQueryParser(
            Version,
            new[] { "name", "description" },
            analyzer)
        {
            DefaultOperator = Operator.AND
        };

        // Escape syntax metacharacters, so user input is treated as text rather
        // than arbitrary Lucene query syntax.
        var luceneQuery = parser.Parse(QueryParser.Escape(query));
        var matches = searcher.Search(luceneQuery, limit);
        var byId = products.ToDictionary(product => product.Id);
        var results = new List<ProductSearchResult>();

        foreach (var hit in matches.ScoreDocs)
        {
            var document = searcher.Doc(hit.Doc);
            var idText = document.Get("id");

            if (!int.TryParse(idText, NumberStyles.Integer,
                    CultureInfo.InvariantCulture, out var id))
                continue;

            if (!byId.TryGetValue(id, out var product))
                continue;

            results.Add(new ProductSearchResult(
                product.Id,
                product.Name,
                product.Description,
                product.Price,
                product.ImageUrl,
                product.CategoryId,
                product.StoreId,
                hit.Score));
        }

        return results;
    }
}