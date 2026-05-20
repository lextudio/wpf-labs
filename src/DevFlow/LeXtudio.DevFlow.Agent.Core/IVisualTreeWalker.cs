using Microsoft.Maui.DevFlow.Agent.Core;

namespace LeXtudio.DevFlow.Agent.Core;

public interface IVisualTreeWalker
{
    List<ElementInfo> WalkTree();
    ElementInfo? FindElementById(string id);
}
