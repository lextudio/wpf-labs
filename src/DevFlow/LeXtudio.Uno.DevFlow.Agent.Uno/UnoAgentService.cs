using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using LeXtudio.Wpf.DevFlow.Agent.Core;
using Microsoft.Maui.DevFlow.Agent.Core;

namespace LeXtudio.Uno.DevFlow.Agent.Uno;

public sealed class UnoAgentService : DevFlowAgentServiceBase
{
    private readonly UnoVisualTreeWalker _treeWalker = new();

    public UnoAgentService(AgentOptions? options = null)
        : base(options)
    {
    }

    protected override string AgentId => "LeXtudio.Uno.DevFlow.Agent";
    protected override string AgentName => "LeXtudio.Uno.DevFlow.Agent";
    protected override string FrameworkName => "uno";

    protected override Task<string?> GetApplicationNameAsync()
    {
        var appType = FindType(
            "Microsoft.UI.Xaml.Application",
            "Windows.UI.Xaml.Application");

        if (appType == null)
            return Task.FromResult<string?>("UnoApplication");

        var current = appType.GetProperty("Current", BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
        return Task.FromResult<string?>(current?.GetType().FullName ?? "UnoApplication");
    }

    private static Type? FindType(params string[] typeNames)
    {
        foreach (var typeName in typeNames)
        {
            var type = Type.GetType(typeName, false, true);
            if (type != null)
                return type;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(typeName, false, true);
                if (type != null)
                    return type;
            }
        }

        return null;
    }

    protected override Task<List<ElementInfo>> BuildTreeAsync()
    {
        return Task.FromResult(_treeWalker.WalkTree());
    }

    protected override Task<ElementInfo?> FindElementAsync(string id)
    {
        return Task.FromResult(_treeWalker.FindElementById(id));
    }

    protected override Task<byte[]?> CaptureScreenshotAsync()
    {
        return Task.FromResult<byte[]?>(null);
    }

    protected override Task<bool> TryTapAsync(string elementId)
    {
        var target = _treeWalker.FindElementObjectById(elementId);
        if (target == null)
            return Task.FromResult(false);

        if (TryExecuteCommand(target))
            return Task.FromResult(true);

        if (TryInvokeOnClick(target))
            return Task.FromResult(true);

        return Task.FromResult(false);
    }

    protected override Task<bool> TryScrollAsync(string elementId, double deltaX, double deltaY)
    {
        var target = _treeWalker.FindElementObjectById(elementId);
        if (target == null)
            return Task.FromResult(false);

        var scrollViewer = FindScrollViewer(target);
        if (scrollViewer == null)
            return Task.FromResult(false);

        if (TryScroll(scrollViewer, deltaX, deltaY))
            return Task.FromResult(true);

        return Task.FromResult(false);
    }

    private static object? FindScrollViewer(object element)
    {
        var current = element;
        while (current != null)
        {
            if (IsScrollViewer(current))
                return current;

            current = GetParent(current);
        }

        return null;
    }

    private static bool IsScrollViewer(object element)
    {
        var type = element.GetType();
        return string.Equals(type.Name, "ScrollViewer", StringComparison.OrdinalIgnoreCase)
            || (type.FullName?.EndsWith("ScrollViewer", StringComparison.OrdinalIgnoreCase) ?? false);
    }

    private static object? GetParent(object element)
    {
        var parentProp = element.GetType().GetProperty("Parent", BindingFlags.Public | BindingFlags.Instance);
        if (parentProp != null)
            return parentProp.GetValue(element);

        var visualTreeHelperType = FindType(
            "Microsoft.UI.Xaml.Media.VisualTreeHelper",
            "Windows.UI.Xaml.Media.VisualTreeHelper");

        if (visualTreeHelperType != null)
        {
            var getParent = visualTreeHelperType.GetMethod("GetParent", BindingFlags.Public | BindingFlags.Static);
            if (getParent != null)
                return getParent.Invoke(null, new[] { element });
        }

        return null;
    }

    private static bool TryScroll(object scrollViewer, double deltaX, double deltaY)
    {
        var currentHorizontal = GetDoubleProperty(scrollViewer, "HorizontalOffset");
        var currentVertical = GetDoubleProperty(scrollViewer, "VerticalOffset");
        var horizontal = currentHorizontal ?? 0.0;
        var vertical = currentVertical ?? 0.0;
        var targetHorizontal = Math.Max(0, horizontal + deltaX);
        var targetVertical = Math.Max(0, vertical + deltaY);

        if (TryInvokeChangeView(scrollViewer, targetHorizontal, targetVertical))
            return true;

        if (deltaX != 0 && TryInvokeMethod(scrollViewer, "ScrollToHorizontalOffset", targetHorizontal))
            return true;

        if (deltaY != 0 && TryInvokeMethod(scrollViewer, "ScrollToVerticalOffset", targetVertical))
            return true;

        return false;
    }

    private static bool TryInvokeChangeView(object target, double horizontalOffset, double verticalOffset)
    {
        foreach (var method in target.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(m => m.Name == "ChangeView"))
        {
            var parameters = method.GetParameters();
            if (parameters.Length == 3)
            {
                try
                {
                    var args = new object?[3];
                    args[0] = ConvertToParameterType(horizontalOffset, parameters[0].ParameterType);
                    args[1] = ConvertToParameterType(verticalOffset, parameters[1].ParameterType);
                    args[2] = null;
                    method.Invoke(target, args);
                    return true;
                }
                catch
                {
                }
            }
            else if (parameters.Length == 4)
            {
                try
                {
                    var args = new object?[4];
                    args[0] = ConvertToParameterType(horizontalOffset, parameters[0].ParameterType);
                    args[1] = ConvertToParameterType(verticalOffset, parameters[1].ParameterType);
                    args[2] = null;
                    args[3] = ConvertToParameterType(true, parameters[3].ParameterType);
                    method.Invoke(target, args);
                    return true;
                }
                catch
                {
                }
            }
        }

        return false;
    }

    private static bool TryInvokeMethod(object target, string methodName, double value)
    {
        foreach (var method in target.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(m => m.Name == methodName && m.GetParameters().Length == 1))
        {
            var parameterType = method.GetParameters()[0].ParameterType;
            try
            {
                var convertedValue = ConvertToParameterType(value, parameterType);
                method.Invoke(target, new[] { convertedValue! });
                return true;
            }
            catch
            {
            }
        }

        return false;
    }

    private static object? ConvertToParameterType(object value, Type targetType)
    {
        var effectiveType = Nullable.GetUnderlyingType(targetType) ?? targetType;
        return Convert.ChangeType(value, effectiveType);
    }

    private static bool TryExecuteCommand(object element)
    {
        var command = GetPropertyValue(element, "Command");
        if (command == null)
            return false;

        var canExecuteMethod = command.GetType().GetMethod("CanExecute", new[] { typeof(object) });
        var executeMethod = command.GetType().GetMethod("Execute", new[] { typeof(object) });
        if (canExecuteMethod == null || executeMethod == null)
            return false;

        var canExecute = canExecuteMethod.Invoke(command, new object?[] { null });
        if (canExecute is bool can && can)
        {
            executeMethod.Invoke(command, new object?[] { null });
            return true;
        }

        return false;
    }

    private static bool TryInvokeOnClick(object element)
    {
        var onClick = element.GetType().GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic);
        if (onClick != null)
        {
            onClick.Invoke(element, Array.Empty<object>());
            return true;
        }

        return false;
    }

    private static object? GetPropertyValue(object target, string propertyName)
    {
        if (target == null)
            return null;

        var property = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        return property?.GetValue(target);
    }

    private static double? GetDoubleProperty(object target, string propertyName)
    {
        var value = GetPropertyValue(target, propertyName);
        if (value is double d)
            return d;

        if (value is float f)
            return f;

        if (value is int i)
            return i;

        if (value is long l)
            return l;

        if (value is string s && double.TryParse(s, out var parsed))
            return parsed;

        return null;
    }
}
