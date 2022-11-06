﻿using BlazorStudio.ClassLib.FileSystem.Interfaces;
using BlazorStudio.ClassLib.Namespaces;

namespace BlazorStudio.ClassLib.Menu;

public interface ICommonMenuOptionsFactory
{
    public MenuOptionRecord NewEmptyFile(
        IAbsoluteFilePath parentDirectory,
        Func<Task> onAfterCompletion);
    
    public MenuOptionRecord NewTemplatedFile(
        NamespacePath parentDirectory,
        Func<Task> onAfterCompletion);
    
    public MenuOptionRecord NewDirectory(
        IAbsoluteFilePath parentDirectory,
        Func<Task> onAfterCompletion);
    
    public MenuOptionRecord DeleteFile(
        IAbsoluteFilePath absoluteFilePath,
        Func<Task> onAfterCompletion);
    
    public MenuOptionRecord CopyFile(
        IAbsoluteFilePath absoluteFilePath,
        Func<Task> onAfterCompletion);
    
    public MenuOptionRecord CutFile(
        IAbsoluteFilePath absoluteFilePath,
        Func<Task> onAfterCompletion);
    
    public MenuOptionRecord RenameFile(
        IAbsoluteFilePath sourceAbsoluteFilePath,
        Func<Task> onAfterCompletion);
    
    public MenuOptionRecord PasteClipboard(
        IAbsoluteFilePath directoryAbsoluteFilePath,
        Func<Task> onAfterCompletion);
}