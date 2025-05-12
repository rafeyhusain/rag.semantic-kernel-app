using Rag.SemanticKernel.Core.Sdk.ElasticSearch;
using Rag.SemanticKernel.Core.Sdk.Util;

namespace Rag.SemanticKernel.ConsoleApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var settings = new Settings();
            var logger = new Logger();
            logger.Init();

            try
            {
                logger.Info("Starting ElasticSearch test...");

                var esService = new ElasticSearchService(settings.IndexName);

                // Create index
                var created = await esService.CreateIndexAsync();
                logger.Info($"Index creation status: {created}");

                // Create a sample document
                var doc = new DocumentModel
                {
                    Title = "Test Document",
                    Content = "This is a sample document to test Elasticsearch indexing."
                };

                // Write document
                var written = await esService.WriteDocumentAsync(doc);
                logger.Info($"Document write status: {written}");

                // Read document
                var readDoc = await esService.ReadDocumentAsync(doc.Id);
                logger.Info($"Read document: {readDoc?.Title ?? "Not found"}");

                // Search
                var searchResult = await esService.SearchAsync("sample");
                logger.Info($"Search result count: {searchResult.Total}");

                foreach (var hit in searchResult.Hits)
                {
                    logger.Info($"Hit ID: {hit.Id}, Title: {hit.Source.Title}");
                }

                // Cleanup (optional)
                await esService.DeleteIndexAsync();
                logger.Info("Index deleted.");
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred during Elasticsearch test.", ex);
            }
            finally
            {
                logger.Info("Shutting down logger...");
                logger.Close();
            }

            Console.ReadKey();
        }
    }
}
