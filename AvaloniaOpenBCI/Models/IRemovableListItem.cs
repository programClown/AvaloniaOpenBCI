using System;

namespace AvaloniaOpenBCI.Models;

public interface IRemovableListItem
{
    public event EventHandler ParentListRemoveRequested;
}