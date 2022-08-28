﻿using BlazorStudio.ClassLib.FileSystem.Classes;
using BlazorStudio.ClassLib.Sequence;

namespace BlazorStudio.ClassLib.Store.SolutionExplorerCase;

public record SetSolutionExplorerAction(AbsoluteFilePathDotNet? SolutionAbsoluteFilePath, SequenceKey SequenceKey);