using Microsoft.SemanticKernel;

namespace Rag.SemanticKernel.Llm.Core.Plugins;

public interface IKernelPluginInjector
{
    void InjectPlugins(Kernel kernel);
}
