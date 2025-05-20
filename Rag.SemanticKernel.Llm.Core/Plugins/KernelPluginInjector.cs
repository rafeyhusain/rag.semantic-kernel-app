using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;
using Rag.SemanticKernel.Model.Vector;

namespace Rag.SemanticKernel.Llm.Core.Plugins;

public class KernelPluginInjector<T> : IKernelPluginInjector
    where T : class, IDocument, new()
{
    private readonly VectorStoreTextSearch<T> _searchService;

    public KernelPluginInjector(VectorStoreTextSearch<T> searchService)
    {
        _searchService = searchService;
    }

    public void InjectPlugins(Kernel kernel)
    {
        var plugin = _searchService.CreateWithGetTextSearchResults("SearchPlugin");
        kernel.Plugins.Add(plugin);

        // Add more plugins if needed here
    }
}
