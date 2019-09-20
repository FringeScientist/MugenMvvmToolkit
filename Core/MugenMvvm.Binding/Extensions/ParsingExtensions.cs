﻿using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Nodes;
using MugenMvvm.Interfaces.Metadata;

// ReSharper disable once CheckNamespace
namespace MugenMvvm.Binding
{
    public static partial class BindingMugenExtensions
    {
        #region Methods

        public static int GetPosition(this IBindingParserContext context, int? position = null)
        {
            Should.NotBeNull(context, nameof(context));
            return position.GetValueOrDefault(context.Position);
        }

        public static char TokenAt(this IBindingParserContext context, int? position = null)
        {
            return context.TokenAt(context.GetPosition(position));
        }

        public static bool IsEof(this IBindingParserContext context, int? position = null)
        {
            return context.GetPosition(position) >= context.Length;
        }

        public static bool IsToken(this IBindingParserContext context, char token, int? position = null)
        {
            if (context.IsEof(position))
                return false;
            return context.TokenAt(position) == token;
        }

        public static bool IsToken(this IBindingParserContext context, string token, int? position = null)
        {
            if (token.Length == 1)
                return context.IsToken(token[0], position);

            var p = context.GetPosition(position);
            var i = 0;
            while (i != token.Length)
            {
                var pos = p + i;
                if (context.IsEof(pos) || TokenAt(context, pos) != token[i])
                    return false;
                ++i;
            }

            return true;
        }

        public static bool IsAnyOf(this IBindingParserContext context, HashSet<char> tokens, int? position = null)
        {
            if (context.IsEof(position))
                return false;
            return tokens.Contains(context.TokenAt(position));
        }

        public static bool IsAnyOf(this IBindingParserContext context, IReadOnlyList<string> tokens, int? position = null)
        {
            if (context.IsEof(position))
                return false;
            for (var i = 0; i < tokens.Count; i++)
            {
                if (context.IsToken(tokens[i], position))
                    return true;
            }

            return false;
        }

        public static bool IsEofOrAnyOf(this IBindingParserContext context, HashSet<char> tokens, int? position = null)
        {
            return context.IsEof(position) || context.IsAnyOf(tokens, position);
        }

        public static bool IsEofOrAnyOf(this IBindingParserContext context, IReadOnlyList<string> tokens, int? position = null)
        {
            return context.IsEof(position) || context.IsAnyOf(tokens, position);
        }

        public static bool IsIdentifier(this IBindingParserContext context, out int endPosition, int? position = null)
        {
            endPosition = context.GetPosition(position);
            if (context.IsEof(endPosition) || !IsValidIdentifierSymbol(true, TokenAt(context, endPosition)))
                return false;

            do
            {
                ++endPosition;
            } while (!context.IsEof(endPosition) && IsValidIdentifierSymbol(false, TokenAt(context, endPosition)));

            return true;
        }

        public static bool IsDigit(this IBindingParserContext context, int? position = null)
        {
            return !context.IsEof(position) && char.IsDigit(context.TokenAt(position));
        }

        public static int FindToken(this IBindingParserContext context, char token, int? position = null)
        {
            var start = context.GetPosition(position);
            for (var i = start; i < context.Length; i++)
            {
                if (TokenAt(context, i) == token)
                    return i;
            }

            return -1;
        }

        public static int FindAnyOf(this IBindingParserContext context, HashSet<char> tokens, int? position = null)
        {
            var start = context.GetPosition(position);
            for (var i = start; i < context.Length; i++)
            {
                if (tokens.Contains(context.TokenAt(i)))
                    return i;
            }

            return -1;
        }

        public static int SkipWhitespacesPosition(this IBindingParserContext context, int? position = null)
        {
            var p = context.GetPosition(position);
            while (!context.IsEof(p) && char.IsWhiteSpace(TokenAt(context, p)))
                ++p;
            return p;
        }

        public static IBindingParserContext SkipWhitespaces(this IBindingParserContext context, int? position = null)
        {
            context.SetPosition(context.SkipWhitespacesPosition(position));
            return context;
        }

        public static IBindingParserContext MoveNext(this IBindingParserContext context, int value = 1)
        {
            if (!context.IsEof(context.Position))
                context.SetPosition(context.Position + value);
            return context;
        }

        public static PositionState SavePosition(this IBindingParserContext context)
        {
            return new PositionState(context);
        }

        public static IExpressionNode Parse(this IBindingParserContext context, IExpressionNode? expression = null, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(context, nameof(context));
            var node = context.TryParse(expression, metadata);
            if (node == null)
                throw new Exception(); //todo add
            return node;
        }

        public static IExpressionNode? TryParseWhileNotNull(this IBindingParserContext context, IExpressionNode? expression = null, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(context, nameof(context));
            while (true)
            {
                var node = context.TryParse(expression, metadata);
                if (node == null)
                    return expression;
                expression = node;
            }
        }

        public static IExpressionNode ParseWhileToken(this IBindingParserContext context, char token, int? position = null, IExpressionNode? expression = null,
            IReadOnlyMetadataContext? metadata = null)
        {
            var expressionNode = context.Parse(expression, metadata);
            while (!context.IsToken(token, position) && !context.IsEof(position))
                expressionNode = context.Parse(expressionNode, metadata);
            return expressionNode;
        }

        public static IExpressionNode ParseWhileAnyOf(this IBindingParserContext context, HashSet<char> tokens, int? position = null, IExpressionNode? expression = null,
            IReadOnlyMetadataContext? metadata = null)
        {
            var expressionNode = context.Parse(expression, metadata);
            while (!context.IsEofOrAnyOf(tokens, position))
                expressionNode = context.Parse(expressionNode, metadata);
            return expressionNode;
        }

        public static IExpressionNode ParseWhileAnyOf(this IBindingParserContext context, IReadOnlyList<string> tokens, int? position = null, IExpressionNode? expression = null,
            IReadOnlyMetadataContext? metadata = null)
        {
            var expressionNode = context.Parse(expression, metadata);
            while (!context.IsEofOrAnyOf(tokens, position))
                expressionNode = context.Parse(expressionNode, metadata);
            return expressionNode;
        }

        public static List<IExpressionNode>? ParseArguments(this IBindingParserContext context, string endSymbol, IReadOnlyMetadataContext? metadata = null)
        {
            List<IExpressionNode>? args = null;
            while (true)
            {
                var node = context.TryParseWhileNotNull(null, metadata);
                if (node != null)
                {
                    if (args == null)
                        args = new List<IExpressionNode>();
                    args.Add(node);
                }

                if (context.SkipWhitespaces().IsToken(','))
                {
                    context.MoveNext();
                    continue;
                }

                if (context.IsToken(endSymbol))
                {
                    context.MoveNext();
                    break;
                }

                return null;
            }

            return args;
        }

        public static List<string>? ParseStringArguments(this IBindingParserContext context, string endSymbol, bool isPointSupported)
        {
            List<string>? args = null;
            var start = context.Position;
            int? end = null;
            while (true)
            {
                var isEnd = context.SkipWhitespaces().IsToken(endSymbol);
                if (isEnd || context.IsToken(','))
                {
                    if (end == null)
                        return null;

                    if (args == null)
                        args = new List<string>();
                    args.Add(context.GetValue(start, end.Value));
                    context.MoveNext();
                    if (isEnd)
                        break;

                    start = context.SkipWhitespaces().Position;
                    continue;
                }

                if (isPointSupported && context.IsToken('.'))
                {
                    context.MoveNext();
                    continue;
                }

                if (context.IsIdentifier(out var position))
                {
                    end = position;
                    context.SetPosition(position);
                    continue;
                }

                return null;
            }

            return args;
        }

        private static bool IsValidIdentifierSymbol(bool firstSymbol, char symbol)
        {
            if (firstSymbol)
                return char.IsLetter(symbol) || symbol == '@' || symbol == '_';
            return char.IsLetterOrDigit(symbol) || symbol == '_';
        }

        #endregion

        #region Nested types

        public ref struct PositionState
        {
            #region Fields

            private readonly int _position;
            private IBindingParserContext? _context;

            #endregion

            #region Constructors

            public PositionState(IBindingParserContext context)
            {
                Should.NotBeNull(context, nameof(context));
                _context = context;
                _position = context.Position;
            }

            #endregion

            #region Methods

            public void Accept()
            {
                _context = null;
            }

            public readonly void Dispose()
            {
                _context?.SetPosition(_position);
            }

            #endregion
        }

        #endregion
    }
}