using DynamiteAndy.DynamiteToolbar.Models;
using DynamiteAndy.DynamiteToolbar.Models.Qt;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace DynamiteAndy.DynamiteToolbar.Factories;

public class QtFactory() : IQtFactory
{
    public T Create<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>()
        where T : QObject
        => ActivatorUtilities.CreateInstance<T>(ModuleHost.Services);

    public TButton CreateButton<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TButton>()
        where TButton : QAbstractButton
    {
        var button = ActivatorUtilities.CreateInstance<TButton>(ModuleHost.Services);
        button.RegisterCallback();
        return button;
    }

    public TLabelGroup CreateLabelGroup<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TLabelGroup>()
        where TLabelGroup : LabelGroup
        => ActivatorUtilities.CreateInstance<TLabelGroup>(ModuleHost.Services);
}