﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeFixes.GenerateMember;
using Microsoft.CodeAnalysis.CSharp.Extensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.GenerateType;
using Microsoft.CodeAnalysis.Shared.Extensions;

namespace Microsoft.CodeAnalysis.CSharp.CodeFixes.GenerateType;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = PredefinedCodeFixProviderNames.GenerateType), Shared]
[ExtensionOrder(After = PredefinedCodeFixProviderNames.GenerateVariable)]
[method: ImportingConstructor]
[method: SuppressMessage("RoslynDiagnosticsReliability", "RS0033:Importing constructor should be [Obsolete]", Justification = "Used in test code: https://github.com/dotnet/roslyn/issues/42814")]
internal sealed class GenerateTypeCodeFixProvider() : AbstractGenerateMemberCodeFixProvider
{
    private const string CS0103 = nameof(CS0103); // error CS0103: The name 'Goo' does not exist in the current context
    private const string CS0117 = nameof(CS0117); // error CS0117: 'x' does not contain a definition for 'y'
    private const string CS0234 = nameof(CS0234); // error CS0234: The type or namespace name 'C' does not exist in the namespace 'N' (are you missing an assembly reference?)
    private const string CS0246 = nameof(CS0246); // error CS0246: The type or namespace name 'T' could not be found (are you missing a using directive or an assembly reference?)
    private const string CS0305 = nameof(CS0305); // error CS0305: Using the generic type 'C<T1>' requires 1 type arguments
    private const string CS0308 = nameof(CS0308); // error CS0308: The non-generic type 'A' cannot be used with type arguments
    private const string CS0426 = nameof(CS0426); // error CS0426: The type name 'S' does not exist in the type 'Program'
    private const string CS0616 = nameof(CS0616); // error CS0616: 'x' is not an attribute class

    public override ImmutableArray<string> FixableDiagnosticIds
        => [CS0103, CS0117, CS0234, CS0246, CS0305, CS0308, CS0426, CS0616, IDEDiagnosticIds.UnboundIdentifierId];

    protected override bool IsCandidate(SyntaxNode node, SyntaxToken token, Diagnostic diagnostic)
        => node switch
        {
            QualifiedNameSyntax or MemberAccessExpressionSyntax => true,
            SimpleNameSyntax simple => !simple.IsParentKind(SyntaxKind.QualifiedName),
            _ => false,
        };

    protected override SyntaxNode? GetTargetNode(SyntaxNode node)
        => ((ExpressionSyntax)node).GetRightmostName();

    protected override Task<ImmutableArray<CodeAction>> GetCodeActionsAsync(
        Document document, SyntaxNode node, CancellationToken cancellationToken)
    {
        var service = document.GetRequiredLanguageService<IGenerateTypeService>();
        return service.GenerateTypeAsync(document, node, cancellationToken);
    }
}
