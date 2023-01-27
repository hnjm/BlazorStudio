﻿using BlazorALaCarte.TreeView;
using BlazorALaCarte.TreeView.BaseTypes;
using BlazorStudio.ClassLib.FileConstants;
using BlazorStudio.ClassLib.FileSystem.Classes;
using BlazorStudio.ClassLib.FileSystem.Interfaces;
using BlazorStudio.ClassLib.Namespaces;

namespace BlazorStudio.ClassLib.TreeViewImplementations.Helper;

public partial class TreeViewHelper
{
    public static Task<List<TreeViewNoType>> LoadChildrenForCSharpProjectAsync(
        TreeViewNamespacePath cSharpProjectTreeView)
    {
        if (cSharpProjectTreeView.Item is null)
            return Task.FromResult<List<TreeViewNoType>>(new());
        
        var parentDirectoryOfCSharpProject = (IAbsoluteFilePath)
            cSharpProjectTreeView.Item.AbsoluteFilePath.Directories
                .Last();

        var parentAbsoluteFilePathString = parentDirectoryOfCSharpProject
            .GetAbsoluteFilePathString();
        
        var hiddenFiles = HiddenFileFacts
            .GetHiddenFilesByContainerFileExtension(ExtensionNoPeriodFacts.C_SHARP_PROJECT);
        
        var childDirectoryTreeViewModels = Directory
            .GetDirectories(parentAbsoluteFilePathString)
            .OrderBy(filePathString => filePathString)
            .Where(x => hiddenFiles.All(hidden => !x.EndsWith(hidden)))
            .Select(x =>
            {
                var absoluteFilePath = new AbsoluteFilePath(x, true);

                var namespaceString = cSharpProjectTreeView.Item.Namespace +
                                      NAMESPACE_DELIMITER +
                                      absoluteFilePath.FileNameNoExtension;
                
                return new TreeViewNamespacePath(
                    new NamespacePath(
                        namespaceString,
                        absoluteFilePath),
                    cSharpProjectTreeView.CommonComponentRenderers,
                    cSharpProjectTreeView.SolutionExplorerStateWrap,
                    true,
                    false)
                {
                    TreeViewChangedKey = TreeViewChangedKey.NewTreeViewChangedKey()
                };
            });
        
        var uniqueDirectories = UniqueFileFacts
            .GetUniqueFilesByContainerFileExtension(
                ExtensionNoPeriodFacts.C_SHARP_PROJECT);
        
        var foundUniqueDirectories = new List<TreeViewNamespacePath>();
        var foundDefaultDirectories = new List<TreeViewNamespacePath>();

        foreach (var directoryTreeViewModel in childDirectoryTreeViewModels)
        {
            if (directoryTreeViewModel.Item is null)
                continue;

            if (uniqueDirectories.Any(unique => directoryTreeViewModel
                    .Item.AbsoluteFilePath.FileNameNoExtension == unique))
            {
                foundUniqueDirectories.Add(directoryTreeViewModel);
            }
            else
            {
                foundDefaultDirectories.Add(directoryTreeViewModel);
            }
        }
        
        foundUniqueDirectories = foundUniqueDirectories
            .OrderBy(x => x.Item?.AbsoluteFilePath.FileNameNoExtension ?? string.Empty)
            .ToList();

        foundDefaultDirectories = foundDefaultDirectories
            .OrderBy(x => x.Item?.AbsoluteFilePath.FileNameNoExtension ?? string.Empty)
            .ToList();
        
        var childFileTreeViewModels = Directory
            .GetFiles(parentAbsoluteFilePathString)
            .OrderBy(filePathString => filePathString)
            .Where(x => !x.EndsWith(ExtensionNoPeriodFacts.C_SHARP_PROJECT))
            .Select(x =>
            {
                var absoluteFilePath = new AbsoluteFilePath(x, false);

                var namespaceString = cSharpProjectTreeView.Item.Namespace;
                
                return (TreeViewNoType)new TreeViewNamespacePath(
                    new NamespacePath(
                        namespaceString,
                        absoluteFilePath),
                    cSharpProjectTreeView.CommonComponentRenderers,
                    cSharpProjectTreeView.SolutionExplorerStateWrap,
                    false,
                    false)
                {
                    TreeViewChangedKey = TreeViewChangedKey.NewTreeViewChangedKey()
                };
            });
        
        return 
            Task.FromResult(foundUniqueDirectories
                .Union(foundDefaultDirectories)
                .Union(childFileTreeViewModels)
                .ToList());
    }
}