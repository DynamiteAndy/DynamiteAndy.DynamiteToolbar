using DynamiteAndy.DynamiteToolbar.Models;
using DynamiteAndy.DynamiteToolbar.Models.Qt;
using System.Diagnostics.CodeAnalysis;

namespace DynamiteAndy.DynamiteToolbar.Factories;

public interface IQtFactory
{
    T Create<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>()
        where T : QObject;

    TButton CreateButton<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TButton>()
        where TButton : QAbstractButton;

    TLabelGroup CreateLabelGroup<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TLabelGroup>()
        where TLabelGroup : LabelGroup;
}