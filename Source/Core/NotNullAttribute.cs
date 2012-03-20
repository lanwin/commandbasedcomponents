using System;

namespace CommandBasedComponents.Core
{
    [AttributeUsage(AttributeTargets.Event | AttributeTargets.Method | AttributeTargets.Property)]
    public class NotNullAttribute : Attribute
    {
    }
}