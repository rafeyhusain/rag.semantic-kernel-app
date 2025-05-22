using Azure.Core;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;

namespace Rag.SemanticKernel.Llm.Core.Plugins;

public class KernelPluginInjector<TRecord> : IKernelPluginInjector
    where TRecord : class
{
    private readonly VectorStoreTextSearch<TRecord> _searchService;

    public KernelPluginInjector(VectorStoreTextSearch<TRecord> searchService)
    {
        _searchService = searchService;
    }

    public void InjectPlugins(Kernel kernel)
    {
        var plugin = _searchService.CreateWithGetTextSearchResults("SearchPlugin");
        kernel.Plugins.Add(plugin);

        // Add more plugins if needed here
    }

    public static void InjectPlugins(Kernel kernel, VectorStoreTextSearch<TRecord> searchService)
    {
        var plugin = searchService.CreateWithGetTextSearchResults("SearchPlugin");
        kernel.Plugins.Add(plugin);
    }
}
