﻿using BlazorStudio.ClassLib.Parsing.C.SyntaxNodes;
using BlazorStudio.ClassLib.Parsing.C.SyntaxTokens;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorStudio.ClassLib.Parsing.C.BoundNodes;

public class BoundVariableDeclarationStatementNode : ISyntaxNode
{
    public BoundVariableDeclarationStatementNode(
        BoundTypeNode boundTypeNode,
        ISyntaxToken identifierToken)
    {
        BoundTypeNode = boundTypeNode;
        IdentifierToken = identifierToken;

        Children = new ISyntax[]
        {
            BoundTypeNode,
            IdentifierToken
        }.ToImmutableArray();
    }
    
    public BoundVariableDeclarationStatementNode(
        BoundTypeNode boundTypeNode,
        ISyntaxToken identifierToken,
        bool isInitialized)
    {
        BoundTypeNode = boundTypeNode;
        IdentifierToken = identifierToken;
        IsInitialized = isInitialized;

        Children = new ISyntax[]
        {
            BoundTypeNode,
            IdentifierToken
        }.ToImmutableArray();
    }

    public ImmutableArray<ISyntax> Children { get; }
    public SyntaxKind SyntaxKind => SyntaxKind.BoundVariableDeclarationStatementNode;

    public BoundTypeNode BoundTypeNode { get; }
    public ISyntaxToken IdentifierToken { get; }

    public bool IsInitialized { get; }

    public BoundVariableDeclarationStatementNode WithIsInitialized(
        bool isInitialized)
    {
        return new BoundVariableDeclarationStatementNode(
            BoundTypeNode,
            IdentifierToken,
            isInitialized);
    }
}