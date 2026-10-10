using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Prism.Core.Diagnostics;
using Prism.Core.Syntax;
using Prism.Core.Syntax.Green;

namespace Prism.Core.Parser;

internal sealed class LanguageParser(string text) : SyntaxParser(text)
{
    private readonly struct NamespaceBodyBuilder()
    {
        public readonly GreenSyntaxList<GreenUsingDirective>.Builder Usings = new();
        public readonly GreenSyntaxList<GreenDeclaration>.Builder Members = new();
    }

    [Flags]
    private enum ContextualModifiers : uint
    {
        None = 0,
        File = 1 << 0,
    }

    private enum PostSkipAction
    {
        Continue,
        Abort,
    }

    private delegate PostSkipAction SkipBadTokens<TNode>(
        LanguageParser parser,
        ref GreenToken openToken,
        GreenSeparatedList<TNode>.Builder builder,
        SyntaxKind expectedKind,
        SyntaxKind closeTokenKind
    )
        where TNode : GreenNode;

    [Flags]
    private enum TerminatorState
    {
        EndOfFile = 0,
        IsNamespaceMemberStartOrStop = 1 << 1,
        IsAttributeTerminator = 1 << 2,
        IsPossibleMemberStartOrStop = 1 << 3,
        IsEndOfParameterList = 1 << 4,
    }

    private const int LastTerminatorState = (int)TerminatorState.IsEndOfParameterList;

    private readonly ref struct TerminatorStateScope(
        LanguageParser parser,
        TerminatorState oldState
    ) : IDisposable
    {
        public void Dispose()
        {
            parser._terminatorState = oldState;
        }
    }

    private TerminatorState _terminatorState = TerminatorState.EndOfFile;

    private PostSkipAction SkipBadSeparatedListTokensWithExpectedKind<T, TNode>(
        ref T startToken,
        GreenSeparatedList<TNode>.Builder list,
        Func<LanguageParser, bool> isNotExpectedFunction,
        Func<LanguageParser, SyntaxKind, bool> abortFunction,
        SyntaxKind expected,
        SyntaxKind closeKind = SyntaxKind.None
    )
        where T : GreenNode
        where TNode : GreenNode
    {
        var (action, trailingTrivia) = SkipBadListTokensWithExpectedKindHelper(
            list.UnderlyingBuilder,
            isNotExpectedFunction,
            abortFunction,
            expected,
            closeKind
        );
        if (trailingTrivia is not null)
        {
            startToken = AddTrailingSkippedSyntax(startToken, trailingTrivia);
        }

        return action;
    }

    private (PostSkipAction, GreenNode?) SkipBadTokensWithExpectedKind(
        Func<LanguageParser, bool> isNotExpected,
        Func<LanguageParser, SyntaxKind, bool> abort,
        SyntaxKind expected,
        SyntaxKind closeKind
    )
    {
        var nodes = GreenSyntaxList.CreateBuilder<GreenNode>();
        var first = true;
        var action = PostSkipAction.Continue;
        while (isNotExpected(this))
        {
            if (abort(this, closeKind) || IsTerminator())
            {
                action = PostSkipAction.Abort;
                break;
            }

            var token =
                (first && !PeekToken().ContainsDiagnostics)
                    ? ExpectToken(expected)
                    : ConsumeToken();
            first = false;
            nodes.Add(token);
        }

        var trailingTrivia = nodes.BuildAndClear();
        return (action, trailingTrivia.Node);
    }

    private (PostSkipAction, GreenNode?) SkipBadListTokensWithExpectedKindHelper(
        GreenListNode.Builder list,
        Func<LanguageParser, bool> isNotExpectedFunction,
        Func<LanguageParser, SyntaxKind, bool> abortFunction,
        SyntaxKind expected,
        SyntaxKind closeKind
    )
    {
        if (list.Count == 0)
        {
            return SkipBadTokensWithExpectedKind(
                isNotExpectedFunction,
                abortFunction,
                expected,
                closeKind
            );
        }

        var (action, lastItemTrailingTrivia) = SkipBadTokensWithExpectedKind(
            isNotExpectedFunction,
            abortFunction,
            expected,
            closeKind
        );
        if (lastItemTrailingTrivia is not null)
        {
            AddTrailingSkippedSyntax(list, lastItemTrailingTrivia);
        }

        return (action, null);
    }

    public T ConsumeUnexpectedTokens<T>(T node)
        where T : GreenNode
    {
        var token = PeekToken();
        if (token.Kind == SyntaxKind.EofToken)
            return node;

        var builder = GreenSyntaxList.CreateBuilder<GreenToken>();
        while (token.Kind != SyntaxKind.EofToken)
        {
            builder.Add(ConsumeToken());
        }

        var list = builder.BuildAndClear();
        var copy = Unsafe.As<T>(
            node.WithDiagnostics(
                node.Diagnostics.Add(
                    new SyntaxDiagnosticInfo(DiagnosticInfo.UnexpectedToken(list[0].ToString()))
                )
            )
        );
        Debug.Assert(list.Node is not null);
        return AddTrailingSkippedSyntax(copy, list.Node);
    }

    private TerminatorStateScope EnterTerminatorStateScope(TerminatorState state)
    {
        var oldState = _terminatorState;
        _terminatorState |= state;
        return new TerminatorStateScope(this, oldState);
    }

    private bool IsTerminator()
    {
        if (AtEnd)
        {
            return true;
        }

        for (var i = 0; i < LastTerminatorState; i++)
        {
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (_terminatorState & (TerminatorState)i)
            {
                case TerminatorState.IsNamespaceMemberStartOrStop
                    when IsNamespaceMemberStartOrStop():
                case TerminatorState.IsAttributeTerminator when IsAttributeTerminator():
                case TerminatorState.IsPossibleMemberStartOrStop when IsPossibleMemberStartOrStop():
                case TerminatorState.IsEndOfParameterList when IsEndOfParameterList():
                    return true;
            }
        }

        return false;
    }

    private bool IsTrueIdentifier()
    {
        return PeekToken().Kind == SyntaxKind.IdentifierToken;
    }

    private GreenSeparatedList<TNode> ParseCommaSeparatedSyntaxList<TNode>(
        ref GreenToken openToken,
        SyntaxKind closeTokenKind,
        Func<LanguageParser, bool> isPossibleElement,
        Func<LanguageParser, TNode> parseElement,
        SkipBadTokens<TNode> skipBadTokens,
        bool allowTrailingSeparator,
        bool requireOneElement
    )
        where TNode : GreenNode
    {
        return ParseCommaSeparatedSyntaxList(
            ref openToken,
            closeTokenKind,
            isPossibleElement,
            parseElement,
            null,
            skipBadTokens,
            allowTrailingSeparator,
            requireOneElement
        );
    }

    private GreenSeparatedList<TNode> ParseCommaSeparatedSyntaxList<TNode>(
        ref GreenToken openToken,
        SyntaxKind closeTokenKind,
        Func<LanguageParser, bool> isPossibleElement,
        Func<LanguageParser, TNode> parseElement,
        Func<TNode, bool>? immediatelyAbort,
        SkipBadTokens<TNode> skipBadTokens,
        bool allowTrailingSeparator,
        bool requireOneElement
    )
        where TNode : GreenNode
    {
        var nodes = GreenSeparatedList.CreateBuilder<TNode>();

        tryAgain:
        while (true)
        {
            if (!requireOneElement && PeekToken().Kind == closeTokenKind)
            {
                break;
            }

            if (requireOneElement || ShouldParseSeparatorOrElement())
            {
                var node = parseElement(this);
                nodes.AddItem(node);

                requireOneElement = false;

                var lastTokenPosition = -1;

                while (
                    immediatelyAbort?.Invoke(node) is not true
                    || IsMakingProgress(ref lastTokenPosition)
                )
                {
                    if (PeekToken().Kind == closeTokenKind)
                        break tryAgain;

                    if (ShouldParseSeparatorOrElement())
                    {
                        nodes.AddSeparator(ExpectToken(SyntaxKind.CommaToken));

                        if (allowTrailingSeparator)
                        {
                            if (PeekToken().Kind == closeTokenKind)
                                break tryAgain;

                            if (!isPossibleElement(this))
                            {
                                continue tryAgain;
                            }
                        }

                        node = parseElement(this);
                        nodes.AddItem(node);
                        continue;
                    }

                    if (
                        skipBadTokens(
                            this,
                            ref openToken,
                            nodes,
                            SyntaxKind.CommaToken,
                            closeTokenKind
                        ) == PostSkipAction.Abort
                    )
                    {
                        break;
                    }
                }
            }
            else if (
                skipBadTokens(
                    this,
                    ref openToken,
                    nodes,
                    SyntaxKind.IdentifierToken,
                    closeTokenKind
                ) == PostSkipAction.Continue
            )
            {
                continue;
            }

            break;
        }

        return nodes.Build();

        bool ShouldParseSeparatorOrElement()
        {
            return PeekToken().Kind == SyntaxKind.CommaToken || isPossibleElement(this);
        }
    }

    public GreenCompilationUnit ParseCompilationUnit()
    {
        GreenToken? tmp = null;
        var body = new NamespaceBodyBuilder();
        var initialBadNodes = new GreenListNode.Builder();
        ParseNamespaceBody(ref tmp, ref body, ref initialBadNodes);

        var eof = ExpectToken(SyntaxKind.EofToken);
        var result = new GreenCompilationUnit(
            body.Usings.BuildAndClear(),
            body.Members.BuildAndClear(),
            eof
        );

        var skippedSyntax = initialBadNodes.BuildAndClear();
        if (skippedSyntax is not null)
        {
            result = AddLeadingSkippedSyntax(result, skippedSyntax);
        }

        return result;
    }

    public GreenDeclaration? ParseTopLevelDeclaration()
    {
        var attributes = ParseAttributes();
        var modifiers = ParseModifiers(ContextualModifiers.File);
        return PeekToken().Kind switch
        {
            SyntaxKind.NamespaceKeyword => ParseNamespaceDeclaration(attributes, modifiers),
            SyntaxKind.VarKeyword or SyntaxKind.ConstKeyword => ParseGlobalVariableDeclaration(
                attributes,
                modifiers,
                ConsumeToken()
            ),
            SyntaxKind.FuncKeyword => ParseFunctionDeclaration(attributes, modifiers),
            SyntaxKind.AttributeKeyword => ParseAttributeDeclaration(attributes, modifiers),
            SyntaxKind.ClassKeyword => ParseClassDeclaration(attributes, modifiers),
            _ => ParseIncompleteDeclaration(attributes, modifiers),
        };
    }

    private static GreenIncompleteDeclaration? ParseIncompleteDeclaration(
        GreenSyntaxList<GreenAttributeList> attributes,
        GreenSyntaxList<GreenToken> modifiers
    )
    {
        if (attributes.Count == 0 && modifiers.Count == 0)
            return null;

        return new GreenIncompleteDeclaration(attributes, modifiers);
    }

    private GreenDeclaration? ParseClassLevelDeclaration()
    {
        var attributes = ParseAttributes();
        var modifiers = ParseModifiers();
        return PeekToken().Kind switch
        {
            SyntaxKind.ConstKeyword or SyntaxKind.IdentifierToken =>
                ParseFieldOrSpecialMemberDeclaration(attributes, modifiers),
            SyntaxKind.FuncKeyword => ParseFunctionDeclaration(attributes, modifiers),
            SyntaxKind.AttributeKeyword => ParseAttributeDeclaration(attributes, modifiers),
            SyntaxKind.ClassKeyword => ParseClassDeclaration(attributes, modifiers),
            _ => ParseIncompleteDeclaration(attributes, modifiers),
        };
    }

    private bool IsPossibleMemberStartOrStop()
    {
        return IsPossibleMemberStart() || PeekToken().Kind == SyntaxKind.CloseBraceToken;
    }

    private bool IsPossibleMemberStart()
    {
        return CanStartMember(PeekToken().Kind);
    }

    private static bool CanStartMember(SyntaxKind kind)
    {
        return kind
            is SyntaxKind.PublicKeyword
                or SyntaxKind.InternalKeyword
                or SyntaxKind.ProtectedKeyword
                or SyntaxKind.PrivateKeyword
                or SyntaxKind.ExternKeyword
                or SyntaxKind.StaticKeyword
                or SyntaxKind.FuncKeyword
                or SyntaxKind.IdentifierToken
                or SyntaxKind.ClassKeyword
                or SyntaxKind.AttributeKeyword
                or SyntaxKind.ConstKeyword
                or SyntaxKind.OpenBracketToken;
    }

    private GreenNamespaceDeclaration ParseNamespaceDeclaration(
        GreenSyntaxList<GreenAttributeList> attributes,
        GreenSyntaxList<GreenToken> modifiers
    )
    {
        var namespaceKeyword = ExpectToken(SyntaxKind.NamespaceKeyword);
        var identifier = ParseName();
        var semicolon = MatchToken(SyntaxKind.SemicolonToken);
        if (semicolon is not null)
        {
            var body = new NamespaceBodyBuilder();
            var initialBadNodes = new GreenListNode.Builder();
            ParseNamespaceBody(ref semicolon, ref body, ref initialBadNodes);
            return new GreenFileScopedNamespaceDeclaration(
                attributes,
                modifiers,
                namespaceKeyword,
                identifier,
                semicolon,
                body.Usings.BuildAndClear(),
                body.Members.BuildAndClear()
            );
        }
        else
        {
            var openBrace = ExpectToken(SyntaxKind.OpenBraceToken);
            var body = new NamespaceBodyBuilder();
            var initialBadNodes = new GreenListNode.Builder();
            ParseNamespaceBody(ref openBrace, ref body, ref initialBadNodes);
            Debug.Assert(initialBadNodes.Count == 0);
            return new GreenBlockNamespaceDeclaration(
                attributes,
                modifiers,
                namespaceKeyword,
                identifier,
                openBrace,
                body.Usings.BuildAndClear(),
                body.Members.BuildAndClear(),
                ExpectToken(SyntaxKind.CloseBraceToken)
            );
        }
    }

    private GreenGlobalVariableDeclaration ParseGlobalVariableDeclaration(
        GreenSyntaxList<GreenAttributeList> attributes,
        GreenSyntaxList<GreenToken> modifiers,
        GreenToken keyword
    )
    {
        return new GreenGlobalVariableDeclaration(
            attributes,
            modifiers,
            keyword,
            ExpectToken(SyntaxKind.IdentifierToken),
            ParseRequiredTypeSpecifier(),
            ParseInitializer(),
            ExpectToken(SyntaxKind.SemicolonToken)
        );
    }

    private GreenLocalVariableDeclaration? ParseLocalVariableDeclaration()
    {
        GreenSyntaxList<GreenToken> modifiers;
        GreenToken keyword;
        switch (PeekToken().Kind)
        {
            case SyntaxKind.MutableKeyword:
                modifiers = ParseModifiers();
                keyword = ExpectToken(SyntaxKind.VarKeyword);
                break;
            case SyntaxKind.VarKeyword:
            case SyntaxKind.ConstKeyword:
                modifiers = new GreenSyntaxList<GreenToken>();
                keyword = ConsumeToken();
                break;
            default:
                return null;
        }

        return new GreenLocalVariableDeclaration(
            [],
            modifiers,
            keyword,
            ExpectToken(SyntaxKind.IdentifierToken),
            ParseTypeSpecifier(),
            ParseInitializer()
        );
    }

    private GreenDeclaration ParseFieldOrSpecialMemberDeclaration(
        GreenSyntaxList<GreenAttributeList> attributes,
        GreenSyntaxList<GreenToken> modifiers
    )
    {
        var constKeyword = MatchToken(SyntaxKind.ConstKeyword);
        if (constKeyword is not null)
        {
            return new GreenFieldDeclaration(
                attributes,
                modifiers,
                constKeyword,
                ExpectToken(SyntaxKind.IdentifierToken),
                ParseRequiredTypeSpecifier(),
                ParseInitializer(),
                ExpectToken(SyntaxKind.SemicolonToken)
            );
        }

        if (MatchToken(SyntaxKind.ConstructorKeyword) is { } constructor)
        {
            return ParseConstructorDeclaration(attributes, modifiers, constructor);
        }

        return new GreenFieldDeclaration(
            attributes,
            modifiers,
            null,
            ExpectToken(SyntaxKind.IdentifierToken),
            ParseRequiredTypeSpecifier(),
            ParseInitializer(),
            ExpectToken(SyntaxKind.SemicolonToken)
        );
    }

    private GreenFunctionDeclaration ParseFunctionDeclaration(
        GreenSyntaxList<GreenAttributeList> attributes,
        GreenSyntaxList<GreenToken> modifiers
    )
    {
        var funcKeyword = ExpectToken(SyntaxKind.FuncKeyword);
        var name = ExpectToken(SyntaxKind.IdentifierToken);
        var parameters = ParseParameterList();
        var refQualifier = ParseRefQualifier();
        var returnType = ParseTypeSpecifier();

        var (block, expressionBody, semicolon) = PeekToken().Kind switch
        {
            SyntaxKind.OpenBraceToken => (
                ParseBlock(),
                (GreenExpressionBody?)null,
                (GreenToken?)null
            ),
            SyntaxKind.ArrowToken => (
                null,
                ParseExpressionBody(),
                ExpectToken(SyntaxKind.SemicolonToken)
            ),
            _ => (null, null, ExpectToken(SyntaxKind.SemicolonToken)),
        };

        return new GreenFunctionDeclaration(
            attributes,
            modifiers,
            funcKeyword,
            name,
            parameters,
            refQualifier,
            returnType,
            block,
            expressionBody,
            semicolon
        );
    }

    private GreenConstructorDeclaration ParseConstructorDeclaration(
        GreenSyntaxList<GreenAttributeList> attributes,
        GreenSyntaxList<GreenToken> modifiers,
        GreenToken constructorKeyword
    )
    {
        var parameters = ParseParameterList();
        var (block, expressionBody, equalSign, defaultKeyword, semicolon) = PeekToken().Kind switch
        {
            SyntaxKind.OpenBraceToken => (
                ParseBlock(),
                (GreenExpressionBody?)null,
                (GreenToken?)null,
                (GreenToken?)null,
                (GreenToken?)null
            ),
            SyntaxKind.ArrowToken => (
                null,
                ParseExpressionBody(),
                null,
                null,
                ExpectToken(SyntaxKind.SemicolonToken)
            ),
            _ => (
                null,
                null,
                ExpectToken(SyntaxKind.EqualToken),
                ExpectToken(SyntaxKind.DefaultKeyword),
                ExpectToken(SyntaxKind.SemicolonToken)
            ),
        };

        return new GreenConstructorDeclaration(
            attributes,
            modifiers,
            constructorKeyword,
            parameters,
            block,
            expressionBody,
            equalSign,
            defaultKeyword,
            semicolon
        );
    }

    private GreenAttributeDeclaration ParseAttributeDeclaration(
        GreenSyntaxList<GreenAttributeList> attributes,
        GreenSyntaxList<GreenToken> modifiers
    )
    {
        var keyword = ExpectToken(SyntaxKind.AttributeKeyword);
        var identifier = ExpectToken(SyntaxKind.IdentifierToken);
        var parameters =
            PeekToken().Kind == SyntaxKind.OpenParenToken ? ParseParameterList() : null;
        var semicolon = ExpectToken(SyntaxKind.SemicolonToken);

        return new GreenAttributeDeclaration(
            attributes,
            modifiers,
            keyword,
            identifier,
            parameters,
            semicolon
        );
    }

    private GreenClassDeclaration ParseClassDeclaration(
        GreenSyntaxList<GreenAttributeList> attributes,
        GreenSyntaxList<GreenToken> modifiers
    )
    {
        var keyword = ExpectToken(SyntaxKind.ClassKeyword);
        var identifier = ExpectToken(SyntaxKind.IdentifierToken);

        if (MatchToken(SyntaxKind.SemicolonToken) is { } semicolon)
        {
            return new GreenClassDeclaration(
                attributes,
                modifiers,
                keyword,
                identifier,
                null,
                null,
                null,
                semicolon
            );
        }

        var openBrace = ExpectToken(SyntaxKind.OpenBraceToken);

        var parseMembers = !openBrace.IsMissing;

        var members = GreenSyntaxList.CreateBuilder<GreenDeclaration>();
        if (parseMembers)
        {
            while (true)
            {
                var kind = PeekToken().Kind;
                if (CanStartMember(kind))
                {
                    using var scope = EnterTerminatorStateScope(
                        TerminatorState.IsPossibleMemberStartOrStop
                    );

                    var member = ParseClassLevelDeclaration();
                    if (member is not null)
                    {
                        members.Add(member);
                    }
                    else
                    {
                        SkipBadMemberListTokens(ref openBrace, ref members);
                    }
                }
                else if (
                    kind is SyntaxKind.CloseBraceToken or SyntaxKind.EofToken
                    || IsTerminator()
                )
                {
                    break;
                }
                else
                {
                    SkipBadMemberListTokens(ref openBrace, ref members);
                }
            }
        }

        var closeBrace = openBrace.IsMissing
            ? GreenToken.GetMissing(SyntaxKind.CloseBraceToken)
            : ExpectToken(SyntaxKind.CloseBraceToken);

        return new GreenClassDeclaration(
            attributes,
            modifiers,
            keyword,
            identifier,
            openBrace,
            members.BuildAndClear(),
            closeBrace,
            null
        );
    }

    private void SkipBadMemberListTokens(
        [NotNullIfNotNull(nameof(openBrace))] ref GreenToken? openBrace,
        ref GreenSyntaxList<GreenDeclaration>.Builder members
    )
    {
        if (members.Count > 0)
        {
            var tmp = members[^1];
            SkipBadMemberListTokens(ref tmp);
            members[^1] = tmp;
        }
        else
        {
            Debug.Assert(openBrace is not null);
            var tmp = openBrace;
            SkipBadMemberListTokens(ref tmp);
            openBrace = tmp;
        }
    }

    private void SkipBadMemberListTokens<TNode>(ref TNode previousNode)
        where TNode : GreenNode
    {
        var curlyCount = 0;
        var tokens = new GreenListNode.Builder();

        var done = false;

        var token = ConsumeToken();
        token = token.WithDiagnostics(
            token.Diagnostics.Add(
                new SyntaxDiagnosticInfo(DiagnosticInfo.InvalidMemberDeclaration(token.Text))
            )
        );
        tokens.Add(token);

        while (!done)
        {
            var kind = PeekToken().Kind;

            if (CanStartMember(kind))
            {
                done = true;
                continue;
            }

            switch (kind)
            {
                case SyntaxKind.OpenBraceToken:
                    curlyCount++;
                    break;
                case SyntaxKind.CloseBraceToken:
                    if (curlyCount-- == 0)
                    {
                        done = true;
                        continue;
                    }

                    break;
                case SyntaxKind.EofToken:
                    done = true;
                    continue;
            }

            tokens.Add(ConsumeToken());
        }

        var skippedSyntax = tokens.BuildAndClear();
        if (skippedSyntax is not null)
        {
            previousNode = AddTrailingSkippedSyntax(previousNode, skippedSyntax);
        }
    }

    public GreenStatement ParseStatement()
    {
        var variableDeclaration = ParseVariableDeclarationStatement();
        if (variableDeclaration is not null)
            return variableDeclaration;

        return PeekToken().Kind switch
        {
            SyntaxKind.ReturnKeyword => ParseReturnStatement(),
            SyntaxKind.OpenBraceToken => ParseBlock(),
            SyntaxKind.SemicolonToken => new GreenEmptyStatement(ConsumeToken()),
            SyntaxKind.IfKeyword => ParseIfStatement(),
            SyntaxKind.BreakKeyword => ParseBreakStatement(),
            SyntaxKind.ContinueKeyword => ParseContinueStatement(),
            SyntaxKind.WhileKeyword => ParseWhileStatement(),
            SyntaxKind.LoopKeyword => ParseLoopStatement(),
            SyntaxKind.ForKeyword => ParseForStatement(),
            SyntaxKind.IdentifierToken when PeekToken(2).Kind == SyntaxKind.ColonToken =>
                ParseLabeledStatement(),
            _ => ParseExpressionStatement(),
        };
    }

    private GreenReturnStatement ParseReturnStatement()
    {
        var returnKeyword = ExpectToken(SyntaxKind.ReturnKeyword);

        var semicolon = MatchToken(SyntaxKind.SemicolonToken);
        if (semicolon is not null)
        {
            return new GreenReturnStatement(returnKeyword, null, semicolon);
        }

        return new GreenReturnStatement(
            returnKeyword,
            ParseExpression(),
            ExpectToken(SyntaxKind.SemicolonToken)
        );
    }

    private GreenVariableDeclarationStatement? ParseVariableDeclarationStatement()
    {
        var local = ParseLocalVariableDeclaration();
        if (local is null)
        {
            return null;
        }

        return new GreenVariableDeclarationStatement(local, ExpectToken(SyntaxKind.SemicolonToken));
    }

    private GreenExpressionStatement ParseExpressionStatement()
    {
        return new GreenExpressionStatement(
            ParseExpression(),
            ExpectToken(SyntaxKind.SemicolonToken)
        );
    }

    private GreenBlock ParseBlock()
    {
        var builder = GreenSyntaxList.CreateBuilder<GreenStatement>();
        var start = ExpectToken(SyntaxKind.OpenBraceToken);

        while (!AtEnd && PeekToken().Kind != SyntaxKind.CloseBraceToken)
        {
            builder.Add(ParseStatement());
        }

        return new GreenBlock(
            start,
            builder.BuildAndClear(),
            ExpectToken(SyntaxKind.CloseBraceToken)
        );
    }

    private GreenIfStatement ParseIfStatement()
    {
        return new GreenIfStatement(
            ExpectToken(SyntaxKind.IfKeyword),
            ExpectToken(SyntaxKind.OpenParenToken),
            ParseExpression(),
            ExpectToken(SyntaxKind.CloseParenToken),
            ParseBlock(),
            ParseElseClause()
        );
    }

    private GreenWhileStatement ParseWhileStatement()
    {
        return new GreenWhileStatement(
            ExpectToken(SyntaxKind.WhileKeyword),
            ExpectToken(SyntaxKind.OpenParenToken),
            ParseExpression(),
            ExpectToken(SyntaxKind.CloseParenToken),
            ParseBlock()
        );
    }

    private GreenLoopStatement ParseLoopStatement()
    {
        return new GreenLoopStatement(ExpectToken(SyntaxKind.LoopKeyword), ParseBlock());
    }

    private GreenForStatement ParseForStatement()
    {
        var forKeyword = ExpectToken(SyntaxKind.ForKeyword);
        var openKeyword = ExpectToken(SyntaxKind.OpenParenToken);
        var declaration = ParseLocalVariableDeclaration();
        var initializers = GreenSeparatedList.CreateBuilder<GreenExpression>();
        if (declaration is null)
        {
            while (PeekToken().Kind != SyntaxKind.SemicolonToken)
            {
                initializers.AddItem(ParseExpression());

                var comma = MatchToken(SyntaxKind.CommaToken);
                if (comma is null)
                    break;

                initializers.AddSeparator(comma);
            }
        }

        var firstSemicolon = ExpectToken(SyntaxKind.SemicolonToken);
        var condition = PeekToken().Kind != SyntaxKind.SemicolonToken ? ParseExpression() : null;
        var secondSemicolon = ExpectToken(SyntaxKind.SemicolonToken);
        var incrementors = GreenSeparatedList.CreateBuilder<GreenExpression>();
        while (PeekToken().Kind != SyntaxKind.CloseParenToken)
        {
            incrementors.AddItem(ParseExpression());

            var comma = MatchToken(SyntaxKind.CommaToken);
            if (comma is null)
                break;

            incrementors.AddSeparator(comma);
        }

        return new GreenForStatement(
            forKeyword,
            openKeyword,
            declaration,
            initializers.BuildAndClear(),
            firstSemicolon,
            condition,
            secondSemicolon,
            incrementors.BuildAndClear(),
            ExpectToken(SyntaxKind.CloseParenToken),
            ParseBlock()
        );
    }

    private GreenBreakStatement ParseBreakStatement()
    {
        return new GreenBreakStatement(
            ExpectToken(SyntaxKind.BreakKeyword),
            MatchToken(SyntaxKind.IdentifierToken),
            ExpectToken(SyntaxKind.SemicolonToken)
        );
    }

    private GreenContinueStatement ParseContinueStatement()
    {
        return new GreenContinueStatement(
            ExpectToken(SyntaxKind.ContinueKeyword),
            MatchToken(SyntaxKind.IdentifierToken),
            ExpectToken(SyntaxKind.SemicolonToken)
        );
    }

    private GreenLabeledStatement ParseLabeledStatement()
    {
        return new GreenLabeledStatement(
            ExpectToken(SyntaxKind.IdentifierToken),
            ExpectToken(SyntaxKind.ColonToken),
            ParseStatement()
        );
    }

    private bool IsPossibleExpression()
    {
        var kind = PeekToken().Kind;
        return kind switch
        {
            SyntaxKind.TrueKeyword
            or SyntaxKind.FalseKeyword
            or SyntaxKind.NullKeyword
            or SyntaxKind.SizeOfKeyword
            or SyntaxKind.OpenBracketToken
            or SyntaxKind.OpenParenToken
            or SyntaxKind.IntegerLiteralToken
            or SyntaxKind.FloatingPointLiteralToken
            or SyntaxKind.CharacterLiteralToken
            or SyntaxKind.StringLiteralToken
            or { IsPrefixOperator: true } => true,
            SyntaxKind.IdentifierToken => IsTrueIdentifier(),
            _ => false,
        };
    }

    public GreenExpression ParseExpression()
    {
        return ParseExpression(ParseCastExpression(), 0);
    }

    private GreenExpression ParseExpression(GreenExpression left, int minPrecedence)
    {
        var next = PeekToken();
        var precedence = next.Kind.OperatorPrecedence;
        while (precedence >= minPrecedence)
        {
            if (next.Kind == SyntaxKind.QuestionToken)
            {
                left = ParseTernaryExpression(left);
            }
            else
            {
                var op = ConsumeToken();
                var right = ParseCastExpression();
                next = PeekToken();
                var innerPrecedence = next.Kind.OperatorPrecedence;
                while (innerPrecedence >= precedence)
                {
                    right = ParseExpression(
                        right,
                        innerPrecedence > precedence ? precedence + 1 : precedence
                    );
                    innerPrecedence = right.Kind.OperatorPrecedence;
                }

                if (op.Kind.IsAssignmentOperator)
                {
                    left = new GreenAssignmentExpression(left, op, right);
                }
                else
                {
                    left = new GreenBinaryExpression(left, op, right);
                }
            }

            next = PeekToken();
            precedence = next.Kind.OperatorPrecedence;
        }

        return left;
    }

    private GreenTernaryExpression ParseTernaryExpression(GreenExpression condition)
    {
        return new GreenTernaryExpression(
            condition,
            ExpectToken(SyntaxKind.QuestionToken),
            ParseExpression(),
            ExpectToken(SyntaxKind.ColonToken),
            ParseExpression()
        );
    }

    private GreenExpression ParsePrimaryExpression()
    {
        return PeekToken().Kind switch
        {
            SyntaxKind.FalseKeyword
            or SyntaxKind.TrueKeyword
            or SyntaxKind.IntegerLiteralToken
            or SyntaxKind.FloatingPointLiteralToken
            or SyntaxKind.CharacterLiteralToken
            or SyntaxKind.StringLiteralToken => new GreenLiteralExpression(ConsumeToken()),
            SyntaxKind.NullKeyword => new GreenNullLiteralExpression(ConsumeToken()),
            SyntaxKind.ThisKeyword => new GreenThisExpression(ConsumeToken()),
            SyntaxKind.SizeOfKeyword => ParseSizeOfExpression(),
            SyntaxKind.OpenParenToken => ParseParenthesizedExpression(),
            SyntaxKind.OpenBracketToken => ParseCollectionExpression(),
            _ => new GreenIdentifierExpression(ParseName()),
        };
    }

    private GreenExpression ParseCastExpression()
    {
        var expr = ParsePrefixExpression();

        while (PeekToken().Kind == SyntaxKind.AsKeyword)
        {
            expr = new GreenCastExpression(expr, ConsumeToken(), ParseType());
        }

        return expr;
    }

    private GreenExpression ParsePrefixExpression()
    {
        var nextTokenKind = PeekToken().Kind;
        if (!nextTokenKind.IsPrefixOperator)
            return ParsePostfixExpression();

        if (nextTokenKind == SyntaxKind.AmpToken)
        {
            return new GreenAddressOfExpression(
                ConsumeToken(),
                MatchToken(SyntaxKind.MutableKeyword),
                ParsePrefixExpression()
            );
        }

        return new GreenPrefixExpression(ConsumeToken(), ParsePrefixExpression());
    }

    private GreenExpression ParsePostfixExpression()
    {
        var expression = ParsePrimaryExpression();

        while (true)
        {
            var kind = PeekToken().Kind;
            switch (kind)
            {
                case SyntaxKind.OpenParenToken:
                    expression = new GreenInvocationExpression(expression, ParseArgumentList());
                    break;
                case { IsPostfixOperator: true }:
                    expression = new GreenPostfixExpression(expression, ConsumeToken());
                    break;
                case SyntaxKind.PeriodToken:
                    expression = new GreenMemberAccessExpression(
                        expression,
                        ConsumeToken(),
                        ParseSimpleName()
                    );
                    break;
                case SyntaxKind.OpenBracketToken:
                    expression = new GreenIndexExpression(
                        expression,
                        ConsumeToken(),
                        ParseExpression(),
                        ExpectToken(SyntaxKind.CloseBracketToken)
                    );
                    break;
                default:
                    return expression;
            }
        }
    }

    private GreenSizeOfExpression ParseSizeOfExpression()
    {
        return new GreenSizeOfExpression(
            ExpectToken(SyntaxKind.SizeOfKeyword),
            ExpectToken(SyntaxKind.OpenParenToken),
            ParseType(),
            ExpectToken(SyntaxKind.CloseParenToken)
        );
    }

    private GreenParenthesizedExpression ParseParenthesizedExpression()
    {
        return new GreenParenthesizedExpression(
            ExpectToken(SyntaxKind.OpenParenToken),
            ParseExpression(),
            ExpectToken(SyntaxKind.CloseParenToken)
        );
    }

    private GreenCollectionExpression ParseCollectionExpression()
    {
        var openBracket = ExpectToken(SyntaxKind.OpenBracketToken);
        var elements = ParseCommaSeparatedSyntaxList(
            ref openBracket,
            SyntaxKind.CloseBracketToken,
            static @this => @this.IsPossibleExpression(),
            static @this => @this.ParseExpression(),
            SkipBadCollectionElementTokens,
            allowTrailingSeparator: false,
            requireOneElement: true
        );

        return new GreenCollectionExpression(
            openBracket,
            elements,
            ExpectToken(SyntaxKind.CloseBracketToken)
        );

        static PostSkipAction SkipBadCollectionElementTokens(
            LanguageParser @this,
            ref GreenToken openBracket,
            GreenSeparatedList<GreenExpression>.Builder list,
            SyntaxKind expectedKind,
            SyntaxKind closeKind
        )
        {
            return @this.SkipBadSeparatedListTokensWithExpectedKind(
                ref openBracket,
                list,
                static p =>
                    p.PeekToken().Kind != SyntaxKind.CommaToken && !p.IsPossibleExpression(),
                static (p, closeKind) => p.PeekToken().Kind == closeKind,
                expectedKind,
                closeKind
            );
        }
    }

    private void ParseNamespaceBody(
        [NotNullIfNotNull(nameof(openBraceOrSemicolon))] ref GreenToken? openBraceOrSemicolon,
        ref NamespaceBodyBuilder body,
        ref GreenListNode.Builder initialBadNodes
    )
    {
        var isGlobal = openBraceOrSemicolon is null;

        var pendingIncompleteMembers = GreenSyntaxList.CreateBuilder<GreenDeclaration>();
        var seen = NamespaceParts.None;
        var reportUnexpectedToken = true;

        try
        {
            using var scope = EnterTerminatorStateScope(
                TerminatorState.IsNamespaceMemberStartOrStop
            );

            while (true)
            {
                switch (PeekToken().Kind)
                {
                    case SyntaxKind.NamespaceKeyword:
                        AddIncompleteMembers(ref pendingIncompleteMembers, ref body);

                        body.Members.Add(AdjustState(ref seen, ParseNamespaceDeclaration([], [])));
                        reportUnexpectedToken = true;
                        break;
                    case SyntaxKind.EofToken:
                        return;

                    case SyntaxKind.CloseBraceToken:
                        if (!isGlobal)
                            return;

                        ReduceIncompleteMembers(
                            ref pendingIncompleteMembers,
                            ref openBraceOrSemicolon,
                            ref body,
                            ref initialBadNodes
                        );

                        var token = ConsumeToken();
                        token = token.WithDiagnostics(
                            token.Diagnostics.Add(
                                new SyntaxDiagnosticInfo(DiagnosticInfo.ExpectedEOF())
                            )
                        );

                        AddSkippedNamespaceText(
                            ref openBraceOrSemicolon,
                            ref body,
                            ref initialBadNodes,
                            token
                        );
                        reportUnexpectedToken = true;
                        break;
                    case SyntaxKind.UsingKeyword:
                    {
                        ReduceIncompleteMembers(
                            ref pendingIncompleteMembers,
                            ref openBraceOrSemicolon,
                            ref body,
                            ref initialBadNodes
                        );

                        var @using = ParseUsingDirective();
                        if (seen > NamespaceParts.Usings)
                        {
                            @using = @using.WithDiagnostics(
                                @using.Diagnostics.Add(
                                    new SyntaxDiagnosticInfo(DiagnosticInfo.UsingAfterElements())
                                )
                            );
                            AddSkippedNamespaceText(
                                ref openBraceOrSemicolon,
                                ref body,
                                ref initialBadNodes,
                                @using
                            );
                        }
                        else
                        {
                            body.Usings.Add(@using);
                            seen = NamespaceParts.Usings;
                        }

                        break;
                    }

                    default:
                        var member = ParseTopLevelDeclaration();
                        if (member is null)
                        {
                            ReduceIncompleteMembers(
                                ref pendingIncompleteMembers,
                                ref openBraceOrSemicolon,
                                ref body,
                                ref initialBadNodes
                            );

                            var skippedToken = ConsumeToken();
                            if (reportUnexpectedToken && !skippedToken.ContainsDiagnostics)
                            {
                                skippedToken = skippedToken.WithDiagnostics(
                                    skippedToken.Diagnostics.Add(
                                        new SyntaxDiagnosticInfo(DiagnosticInfo.ExpectedEOF())
                                    )
                                );
                                reportUnexpectedToken = false;
                            }

                            AddSkippedNamespaceText(
                                ref openBraceOrSemicolon,
                                ref body,
                                ref initialBadNodes,
                                skippedToken
                            );
                        }
                        else if (
                            member.Kind == SyntaxKind.IncompleteDeclaration
                            && seen < NamespaceParts.Members
                        )
                        {
                            pendingIncompleteMembers.Add(member);
                            reportUnexpectedToken = true;
                        }
                        else
                        {
                            AddIncompleteMembers(ref pendingIncompleteMembers, ref body);
                            body.Members.Add(AdjustState(ref seen, member));
                            reportUnexpectedToken = true;
                        }

                        break;
                }
            }
        }
        finally
        {
            AddIncompleteMembers(ref pendingIncompleteMembers, ref body);
        }

        static GreenDeclaration AdjustState(ref NamespaceParts seen, GreenDeclaration declaration)
        {
            switch (declaration.Kind)
            {
                case SyntaxKind.BlockNamespaceDeclaration:
                case SyntaxKind.FileScopedNamespaceDeclaration:
                case SyntaxKind.ClassDeclaration:
                case SyntaxKind.GlobalVariableDeclaration:
                case SyntaxKind.FunctionDeclaration:
                    if (seen < NamespaceParts.Members)
                    {
                        seen = NamespaceParts.Members;
                    }
                    break;
            }

            return declaration;
        }
    }

    private static void AddIncompleteMembers(
        ref GreenSyntaxList<GreenDeclaration>.Builder incompleteMembers,
        ref NamespaceBodyBuilder body
    )
    {
        if (incompleteMembers.Count > 0)
        {
            body.Members.AddRange(incompleteMembers.BuildAndClear());
        }
    }

    private void ReduceIncompleteMembers(
        ref GreenSyntaxList<GreenDeclaration>.Builder incompleteMembers,
        ref GreenToken? openBraceOrSemicolon,
        ref NamespaceBodyBuilder body,
        ref GreenListNode.Builder initialBadNodes
    )
    {
        for (var i = 0; i < incompleteMembers.Count; i++)
        {
            AddSkippedNamespaceText(
                ref openBraceOrSemicolon,
                ref body,
                ref initialBadNodes,
                incompleteMembers[i]
            );
        }

        incompleteMembers.Clear();
    }

    private enum NamespaceParts
    {
        None = 0,
        Usings = 1,
        Members = 2,
    }

    private void AddSkippedNamespaceText(
        [NotNullIfNotNull(nameof(openBraceOrSemicolon))] ref GreenToken? openBraceOrSemicolon,
        ref NamespaceBodyBuilder body,
        ref GreenListNode.Builder initialBadNodes,
        GreenNode skippedSyntax
    )
    {
        if (body.Members.Count > 0)
        {
            AddTrailingSkippedSyntax(body.Members, skippedSyntax);
        }
        else if (body.Usings.Count > 0)
        {
            AddTrailingSkippedSyntax(body.Usings, skippedSyntax);
        }
        else if (openBraceOrSemicolon is not null)
        {
            openBraceOrSemicolon = AddTrailingSkippedSyntax(openBraceOrSemicolon, skippedSyntax);
        }
        else
        {
            initialBadNodes.AddRange(skippedSyntax);
        }
    }

    private bool IsNamespaceMemberStartOrStop()
    {
        return IsEndOfNamespace() || IsPossibleNamespaceMemberDeclaration();
    }

    private bool IsPossibleNamespaceMemberDeclaration()
    {
        switch (PeekToken().Kind)
        {
            case SyntaxKind.NamespaceKeyword:
            case SyntaxKind.UsingKeyword:
                return true;
            default:
                return IsPossibleStartOfTopLevelDeclaration(PeekToken().Kind);
        }
    }

    private static bool IsPossibleStartOfTopLevelDeclaration(SyntaxKind kind)
    {
        return IsTopLevelModifierOrKeyword(kind) || kind == SyntaxKind.OpenBracketToken;
    }

    private static bool IsTopLevelModifierOrKeyword(SyntaxKind kind)
    {
        return kind
            is SyntaxKind.ClassKeyword
                or SyntaxKind.AttributeKeyword
                or SyntaxKind.VarKeyword
                or SyntaxKind.FuncKeyword
                or SyntaxKind.ConstKeyword
                or SyntaxKind.PublicKeyword
                or SyntaxKind.InternalKeyword
                or SyntaxKind.ProtectedKeyword
                or SyntaxKind.PrivateKeyword
                or SyntaxKind.ExternKeyword;
    }

    private bool IsEndOfNamespace()
    {
        return PeekToken().Kind == SyntaxKind.CloseBraceToken;
    }

    private GreenUsingDirective ParseUsingDirective()
    {
        return new GreenUsingDirective(
            ExpectToken(SyntaxKind.UsingKeyword),
            ParseName(),
            ExpectToken(SyntaxKind.SemicolonToken)
        );
    }

    private GreenSyntaxList<GreenAttributeList> ParseAttributes()
    {
        var builder = GreenSyntaxList.CreateBuilder<GreenAttributeList>();
        using (EnterTerminatorStateScope(TerminatorState.IsAttributeTerminator))
        {
            while (IsPossibleAttributeList())
            {
                var next = PeekToken();
                if (next.Kind != SyntaxKind.OpenBracketToken)
                    break;

                builder.Add(ParseAttributeList());
            }
        }

        return builder.BuildAndClear();
    }

    private GreenAttributeList ParseAttributeList()
    {
        var openBracket = ExpectToken(SyntaxKind.OpenBracketToken);
        var attributes = ParseCommaSeparatedSyntaxList(
            ref openBracket,
            SyntaxKind.CloseBracketToken,
            static @this => @this.IsPossibleAttribute(),
            static @this => @this.ParseAttribute(),
            SkipBadAttributeListTokens,
            allowTrailingSeparator: false,
            requireOneElement: true
        );

        return new GreenAttributeList(
            openBracket,
            attributes,
            ExpectToken(SyntaxKind.CloseBracketToken)
        );

        static PostSkipAction SkipBadAttributeListTokens(
            LanguageParser @this,
            ref GreenToken openBracket,
            GreenSeparatedList<GreenAttribute>.Builder list,
            SyntaxKind expectedKind,
            SyntaxKind closeKind
        )
        {
            return @this.SkipBadSeparatedListTokensWithExpectedKind(
                ref openBracket,
                list,
                static p => p.PeekToken().Kind != SyntaxKind.CommaToken && !p.IsPossibleAttribute(),
                static (p, closeKind) => p.PeekToken().Kind == closeKind,
                expectedKind,
                closeKind
            );
        }
    }

    private bool IsAttributeTerminator()
    {
        return PeekToken().Kind == SyntaxKind.CloseBracketToken || IsPossibleAttributeList();
    }

    private bool IsPossibleAttributeList()
    {
        return PeekToken().Kind == SyntaxKind.OpenBracketToken;
    }

    private bool IsPossibleAttribute()
    {
        return IsTrueIdentifier();
    }

    private GreenAttribute ParseAttribute()
    {
        var type = ParseNamedType();
        var arguments = PeekToken().Kind == SyntaxKind.OpenParenToken ? ParseArgumentList() : null;
        return new GreenAttribute(type, arguments);
    }

    private GreenSyntaxList<GreenToken> ParseModifiers(
        ContextualModifiers contextualModifiers = ContextualModifiers.None
    )
    {
        var builder = GreenSyntaxList.CreateBuilder<GreenToken>();
        while (!AtEnd)
        {
            var next = PeekToken();
            if (next.Kind.IsModifier)
            {
                builder.Add(ConsumeToken());
            }
            else if (TryParseContextualModifier(next, contextualModifiers) is { } contextual)
            {
                builder.Add(contextual);
            }
            else
            {
                break;
            }
        }

        return builder.BuildAndClear();
    }

    private GreenToken? TryParseContextualModifier(GreenToken next, ContextualModifiers modifiers)
    {
        if (
            modifiers == ContextualModifiers.None
            || next.TryGetValue<IdentifierData>() is not { IsEscaped: false, Value: var value }
        )
            return null;

        if (modifiers.HasFlag(ContextualModifiers.File) && value == "file")
            return ReplaceWithContextualKeyword(SyntaxKind.FileKeyword, next);

        return null;
    }

    private GreenTypeSpecifier? ParseTypeSpecifier()
    {
        var colon = MatchToken(SyntaxKind.ColonToken);
        if (colon is null)
            return null;

        return new GreenTypeSpecifier(colon, ParseType());
    }

    private GreenTypeSpecifier ParseRequiredTypeSpecifier()
    {
        return new GreenTypeSpecifier(ExpectToken(SyntaxKind.ColonToken), ParseType());
    }

    private GreenType ParseType()
    {
        var type = ParseBaseType();

        while (!AtEnd)
        {
            switch (PeekToken().Kind)
            {
                case SyntaxKind.AmpToken:
                    type = new GreenReferenceType(type, null, ConsumeToken());
                    break;
                case SyntaxKind.MutableKeyword:
                    type = new GreenReferenceType(
                        type,
                        ConsumeToken(),
                        ExpectToken(SyntaxKind.AmpToken)
                    );
                    break;
                case SyntaxKind.OpenBracketToken:
                {
                    var openBracket = ConsumeToken();
                    var expression =
                        PeekToken().Kind != SyntaxKind.CloseBracketToken ? ParseExpression() : null;
                    type = new GreenArrayType(
                        type,
                        openBracket,
                        expression,
                        ExpectToken(SyntaxKind.CloseBracketToken)
                    );
                    break;
                }
                case SyntaxKind.QuestionToken:
                    type = new GreenNullableType(type, ConsumeToken());
                    break;
                default:
                    return type;
            }
        }

        return type;
    }

    private GreenType ParseBaseType()
    {
        return PeekToken().Kind.IsBuiltInType
            ? new GreenPredefinedType(ConsumeToken())
            : ParseNamedType();
    }

    private GreenNamedType ParseNamedType()
    {
        return new GreenNamedType(ParseName());
    }

    private GreenName ParseName()
    {
        GreenName name = ParseSimpleName();

        while (!AtEnd)
        {
            var separator = MatchToken(SyntaxKind.DoubleColonToken);
            if (separator is null)
                break;

            name = new GreenQualifiedName(name, separator, ParseSimpleName());
        }

        return name;
    }

    private GreenSimpleName ParseSimpleName()
    {
        return new GreenSimpleName(ExpectToken(SyntaxKind.IdentifierToken));
    }

    private GreenInitializer? ParseInitializer()
    {
        var equal = MatchToken(SyntaxKind.EqualToken);
        return equal is not null ? new GreenInitializer(equal, ParseExpression()) : null;
    }

    private GreenParameterList ParseParameterList()
    {
        var open = ExpectToken(SyntaxKind.OpenParenToken);

        GreenSeparatedList<GreenParameter> parameters;

        using (EnterTerminatorStateScope(TerminatorState.IsEndOfParameterList))
        {
            parameters = ParseCommaSeparatedSyntaxList(
                ref open,
                SyntaxKind.CloseParenToken,
                static @this => @this.IsPossibleParameter(),
                static @this => @this.ParseParameter(),
                SkipBadParameterListTokens,
                allowTrailingSeparator: false,
                requireOneElement: false
            );
        }

        return new GreenParameterList(open, parameters, ExpectToken(SyntaxKind.CloseParenToken));

        static PostSkipAction SkipBadParameterListTokens(
            LanguageParser @this,
            ref GreenToken open,
            GreenSeparatedList<GreenParameter>.Builder list,
            SyntaxKind expectedKind,
            SyntaxKind closeKind
        )
        {
            return @this.SkipBadSeparatedListTokensWithExpectedKind(
                ref open,
                list,
                static p => p.PeekToken().Kind != SyntaxKind.CommaToken && !p.IsPossibleParameter(),
                static (p, closeKind) => p.PeekToken().Kind == closeKind,
                expectedKind,
                closeKind
            );
        }
    }

    private bool IsEndOfParameterList()
    {
        return PeekToken().Kind
            is SyntaxKind.CloseParenToken
                or SyntaxKind.CloseBracketToken
                or SyntaxKind.SemicolonToken;
    }

    private bool IsPossibleParameter()
    {
        return PeekToken().Kind switch
        {
            SyntaxKind.OpenBracketToken or SyntaxKind.MutableKeyword => true,
            SyntaxKind.IdentifierToken => IsTrueIdentifier(),
            _ => false,
        };
    }

    private GreenParameter ParseParameter()
    {
        return new GreenParameter(
            ParseAttributes(),
            MatchToken(SyntaxKind.MutableKeyword),
            ExpectToken(SyntaxKind.IdentifierToken),
            ParseRequiredTypeSpecifier(),
            ParseInitializer()
        );
    }

    private GreenToken? ParseRefQualifier()
    {
        return MatchToken(SyntaxKind.MutableKeyword) ?? MatchToken(SyntaxKind.ValueKeyword);
    }

    private GreenExpressionBody ParseExpressionBody()
    {
        return new GreenExpressionBody(ExpectToken(SyntaxKind.ArrowToken), ParseExpression());
    }

    private GreenArgumentList ParseArgumentList()
    {
        var openParen = ExpectToken(SyntaxKind.OpenParenToken);
        var args = ParseCommaSeparatedSyntaxList(
            ref openParen,
            SyntaxKind.CloseParenToken,
            static @this => @this.IsPossibleArgument(),
            static @this => @this.ParseArgument(),
            SkipBadArgumentTokens,
            allowTrailingSeparator: false,
            requireOneElement: true
        );

        return new GreenArgumentList(openParen, args, ExpectToken(SyntaxKind.CloseParenToken));

        static PostSkipAction SkipBadArgumentTokens(
            LanguageParser @this,
            ref GreenToken openBracket,
            GreenSeparatedList<GreenArgument>.Builder list,
            SyntaxKind expectedKind,
            SyntaxKind closeKind
        )
        {
            return @this.SkipBadSeparatedListTokensWithExpectedKind(
                ref openBracket,
                list,
                static p => p.PeekToken().Kind != SyntaxKind.CommaToken && !p.IsPossibleArgument(),
                static (p, closeKind) => p.PeekToken().Kind == closeKind,
                expectedKind,
                closeKind
            );
        }
    }

    private bool IsPossibleArgument()
    {
        return PeekToken().Kind == SyntaxKind.IdentifierToken || IsPossibleExpression();
    }

    private GreenArgument ParseArgument()
    {
        return new GreenArgument(ParseNamedParameter(), ParseExpression());
    }

    private GreenNamedParameter? ParseNamedParameter()
    {
        if (
            PeekToken().Kind != SyntaxKind.IdentifierToken
            || PeekToken(2).Kind != SyntaxKind.ColonToken
        )
            return null;

        return new GreenNamedParameter(
            ExpectToken(SyntaxKind.IdentifierToken),
            ExpectToken(SyntaxKind.ColonToken)
        );
    }

    private GreenElseClause? ParseElseClause()
    {
        var elseToken = MatchToken(SyntaxKind.ElseKeyword);
        if (elseToken is null)
            return null;

        return PeekToken().Kind == SyntaxKind.IfKeyword
            ? new GreenElseClause(elseToken, ParseIfStatement())
            : new GreenElseClause(elseToken, ParseBlock());
    }
}
