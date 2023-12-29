using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace AvaloniaOpenBCI.Attributes;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] [MeansImplicitUse]
[AttributeUsage(AttributeTargets.Class)]
public class TransientAttribute : Attribute
{
    public TransientAttribute()
    {
    }

    public TransientAttribute(Type interfaceType)
    {
        InterfaceType = interfaceType;
    }

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
    public Type? InterfaceType { get; }
}