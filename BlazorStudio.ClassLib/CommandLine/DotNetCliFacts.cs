﻿using BlazorStudio.ClassLib.FileSystem.Interfaces;

namespace BlazorStudio.ClassLib.CommandLine;

/// <summary>
/// Any values given will be wrapped in quotes internally
/// </summary>
public static class DotNetCliFacts
{
    public const string DotnetNewSlnCommand = "dotnet new sln";
    
    public static string FormatStartProjectWithoutDebugging(IAbsoluteFilePath projectAbsoluteFilePath)
    {
        return FormatStartProjectWithoutDebugging(
            projectAbsoluteFilePath.GetAbsoluteFilePathString());
    }
    
    public static string FormatStartProjectWithoutDebugging(string projectPath)
    {
        projectPath = QuoteValue(projectPath);
        
        return $"dotnet run --project {projectPath}";
    }
    
    public static string FormatDotnetNewSln(string solutionName)
    {
        solutionName = QuoteValue(solutionName);
        
        return $"{DotnetNewSlnCommand} -o {solutionName}";
    }
    
    public static string FormatDotnetNewCSharpProject(
        string projectTemplateName, 
        string cSharpProjectName, 
        string optionalParameters)
    {
        projectTemplateName = QuoteValue(projectTemplateName);
        cSharpProjectName = QuoteValue(cSharpProjectName);
        
        return $"dotnet new {projectTemplateName} -o {cSharpProjectName} {optionalParameters}";
    }
    
    public static string FormatAddExistingProjectToSolution(
        string solutionAbsoluteFilePathString, 
        string cSharpProjectPath)
    {
        solutionAbsoluteFilePathString = QuoteValue(solutionAbsoluteFilePathString);
        cSharpProjectPath = QuoteValue(cSharpProjectPath);
        
        return $"dotnet sln {solutionAbsoluteFilePathString} add {cSharpProjectPath}";
    }
    
    public static string FormatRemoveCSharpProjectReferenceFromSolutionAction(
        string solutionAbsoluteFilePathString, 
        string cSharpProjectAbsoluteFilePathString)
    {
        solutionAbsoluteFilePathString = QuoteValue(solutionAbsoluteFilePathString);
        cSharpProjectAbsoluteFilePathString = QuoteValue(cSharpProjectAbsoluteFilePathString);
        
        return $"dotnet sln {solutionAbsoluteFilePathString} remove {cSharpProjectAbsoluteFilePathString}";
    }
    
    public static string FormatAddNugetPackageReferenceToProject(
        string cSharpProjectAbsoluteFilePathString, 
        string nugetPackageId,
        string nugetPackageVersion)
    {
        cSharpProjectAbsoluteFilePathString = QuoteValue(cSharpProjectAbsoluteFilePathString);
        nugetPackageId = QuoteValue(nugetPackageId);
        nugetPackageVersion = QuoteValue(nugetPackageVersion);
        
        return $"dotnet add {cSharpProjectAbsoluteFilePathString} package {nugetPackageId} --version {nugetPackageVersion}";
    }
    
    public static string FormatAddProjectToProjectReference(
        string receivingProjectAbsoluteFilePathString, 
        string referencedProjectAbsoluteFilePathString)
    {
        receivingProjectAbsoluteFilePathString = QuoteValue(receivingProjectAbsoluteFilePathString);
        referencedProjectAbsoluteFilePathString = QuoteValue(referencedProjectAbsoluteFilePathString);
        
        return $"dotnet add {receivingProjectAbsoluteFilePathString} reference {referencedProjectAbsoluteFilePathString}";
    }

    private static string QuoteValue(string parameter)
    {
        return $"\"{parameter}\"";
    }
}