using Microsoft.SemanticKernel;

namespace Rag.Connector.Core.Plugins;

public interface IKernelPluginInjector
{
    void InjectPlugins(Kernel kernel);
}
