namespace LeXtudio.DevFlow.Agent.Core;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class DevFlowActionAttribute : Attribute
{
    public DevFlowActionAttribute(string name)
    {
        Name = name;
    }

    public string Name { get; }
    public string? Description { get; set; }
}
