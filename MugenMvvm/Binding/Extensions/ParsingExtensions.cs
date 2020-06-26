﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MugenMvvm.Binding.Attributes;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Metadata;
using MugenMvvm.Binding.Parsing;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Extensions
{
    public static partial class MugenBindingExtensions
    {
        #region Fields

        private static readonly HashSet<char> BindingTargetDelimiters = new HashSet<char> { ',', ';', ' ' };
        private static readonly HashSet<char> BindingDelimiters = new HashSet<char> { ',', ';' };

        #endregion

        #region Methods

        public static bool TryConvertExtension(this IExpressionConverterContext<Expression> context, MemberInfo member, Expression? expression, out IExpressionNode? result)
        {
            var attribute = BindingSyntaxExtensionAttributeBase.TryGet(member);
            if (attribute != null)
                return attribute.TryConvert(context, expression, out result);
            result = null;
            return false;
        }

        public static IExpressionNode? ConvertTarget(this IExpressionConverterContext<Expression> context, Expression? expression, MemberInfo member)
        {
            if (!context.TryConvertExtension(member.DeclaringType, expression, out var result))
                result = context.ConvertOptional(expression) ?? ConstantExpressionNode.Get(member.DeclaringType);
            if (ReferenceEquals(result, ConstantExpressionNode.Null) || ReferenceEquals(result, MemberExpressionNode.Empty))
                result = null;
            return result;
        }

        [return: NotNullIfNotNull("expression")]
        public static IExpressionNode? ConvertOptional(this IExpressionConverterContext<Expression> context, Expression? expression)
        {
            return expression == null ? null : context.Convert(expression);
        }

        [return: NotNullIfNotNull("expression")]
        public static List<IExpressionNode> Convert(this IExpressionConverterContext<Expression> context, IReadOnlyList<Expression> expressions)
        {
            var nodes = new List<IExpressionNode>(expressions.Count);
            for (int i = 0; i < expressions.Count; i++)
                nodes.Add(context.Convert(expressions[i]));
            return nodes;
        }

        public static IMethodCallExpressionNode ConvertMethodCall(this IExpressionConverterContext<Expression> context, MethodCallExpression methodCallExpression, string? methodName = null)
        {
            var method = methodCallExpression.Method;
            ParameterInfo[]? parameters = null;
            IExpressionNode? target;
            var args = context.Convert(methodCallExpression.Arguments);
            if (method.GetAccessModifiers(true, ref parameters).HasFlagEx(MemberFlags.Extension))
            {
                target = args[0];
                args.RemoveAt(0);
            }
            else
                target = context.ConvertTarget(methodCallExpression.Object, method);

            string[]? typeArgs = null;
            if (method.IsGenericMethod)
            {
                var genericArguments = method.GetGenericArguments();
                typeArgs = new string[genericArguments.Length];
                for (var i = 0; i < typeArgs.Length; i++)
                    typeArgs[i] = genericArguments[i].AssemblyQualifiedName;
            }

            return new MethodCallExpressionNode(target, methodName ?? method.Name, args, typeArgs);
        }

        public static IExpressionNode Convert<T>(this IExpressionConverterContext<T> context, T expression) where T : class
        {
            Should.NotBeNull(context, nameof(context));
            Should.NotBeNull(expression, nameof(expression));
            var exp = context.TryConvert(expression);
            if (exp != null)
                return exp;

            context.ThrowCannotParse(expression);
            return null;
        }

        [DoesNotReturn]
        public static void ThrowCannotParse<T>(this IParserContext context, T expression)
        {
            var errors = context.TryGetErrors();
            if (errors != null && errors.Count != 0)
            {
                errors.Reverse();
                BindingExceptionManager.ThrowCannotParseExpression(expression, BindingMessageConstant.PossibleReasons + string.Join(Environment.NewLine, errors));
            }
            else
                BindingExceptionManager.ThrowCannotParseExpression(expression);
        }

        public static List<string>? TryGetErrors(this IParserContext context)
        {
            Should.NotBeNull(context, nameof(context));
            if (context.GetMetadataOrDefault().TryGet(ParsingMetadata.ParsingErrors, out var errors))
                return errors;
            return null;
        }

        public static int GetPosition(this ITokenParserContext context, int? position = null)
        {
            Should.NotBeNull(context, nameof(context));
            return position.GetValueOrDefault(context.Position);
        }

        public static char TokenAt(this ITokenParserContext context, int? position = null)
        {
            return context.TokenAt(context.GetPosition(position));
        }

        public static bool IsEof(this ITokenParserContext context, int? position = null)
        {
            return context.GetPosition(position) >= context.Length;
        }

        public static bool IsToken(this ITokenParserContext context, char token, int? position = null)
        {
            if (context.IsEof(position))
                return false;
            return context.TokenAt(position) == token;
        }

        public static bool IsToken(this ITokenParserContext context, string token, int? position = null, bool isPartOfIdentifier = true)
        {
            if (token.Length == 1)
                return context.IsToken(token[0], position);

            var p = context.GetPosition(position);
            var i = 0;
            while (i != token.Length)
            {
                if (context.IsEof(p) || TokenAt(context, p) != token[i])
                    return false;
                ++i;
                ++p;
            }

            return isPartOfIdentifier || context.IsEof(p) || !context.TokenAt(p).IsValidIdentifierSymbol(false);
        }

        public static bool IsAnyOf(this ITokenParserContext context, HashSet<char> tokens, int? position = null)
        {
            if (context.IsEof(position))
                return false;
            return tokens.Contains(context.TokenAt(position));
        }

        public static bool IsAnyOf(this ITokenParserContext context, IReadOnlyList<string> tokens, int? position = null)
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

        public static bool IsEofOrAnyOf(this ITokenParserContext context, HashSet<char>? tokens, int? position = null)
        {
            return context.IsEof(position) || tokens != null && context.IsAnyOf(tokens, position);
        }

        public static bool IsEofOrAnyOf(this ITokenParserContext context, IReadOnlyList<string> tokens, int? position = null)
        {
            return context.IsEof(position) || context.IsAnyOf(tokens, position);
        }

        public static bool IsIdentifier(this ITokenParserContext context, out int endPosition, int? position = null)
        {
            endPosition = context.GetPosition(position);
            if (context.IsEof(endPosition) || !TokenAt(context, endPosition).IsValidIdentifierSymbol(true))
                return false;

            do
            {
                ++endPosition;
            } while (!context.IsEof(endPosition) && TokenAt(context, endPosition).IsValidIdentifierSymbol(false));

            return true;
        }

        public static bool IsDigit(this ITokenParserContext context, int? position = null)
        {
            return !context.IsEof(position) && char.IsDigit(context.TokenAt(position));
        }

        public static int FindAnyOf(this ITokenParserContext context, HashSet<char> tokens, int? position = null)
        {
            var start = context.GetPosition(position);
            for (var i = start; i < context.Length; i++)
            {
                if (tokens.Contains(context.TokenAt(i)))
                    return i;
            }

            return -1;
        }

        public static int SkipWhitespacesPosition(this ITokenParserContext context, int? position = null)
        {
            var p = context.GetPosition(position);
            while (!context.IsEof(p) && char.IsWhiteSpace(TokenAt(context, p)))
                ++p;
            return p;
        }

        public static ITokenParserContext SkipWhitespaces(this ITokenParserContext context,
            int? position = null)
        {
            context.Position = context.SkipWhitespacesPosition(position);
            return context;
        }

        public static ITokenParserContext MoveNext(this ITokenParserContext context,
            int value = 1)
        {
            if (!context.IsEof(context.Position))
                context.Position += value;
            return context;
        }

        public static IExpressionNode Parse(this ITokenParserContext context, IExpressionNode? expression = null)
        {
            Should.NotBeNull(context, nameof(context));
            var node = context.TryParse(expression);
            if (node == null)
                context.ThrowCannotParse(context);
            return node;
        }

        public static IExpressionNode? TryParseWhileNotNull(this ITokenParserContext context, IExpressionNode? expression = null)
        {
            Should.NotBeNull(context, nameof(context));
            while (true)
            {
                var node = context.TryParse(expression);
                if (node == null)
                    return expression;
                expression = node;
            }
        }

        public static IExpressionNode ParseWhileAnyOf(this ITokenParserContext context, HashSet<char>? tokens, int? position = null,
            IExpressionNode? expression = null)
        {
            var expressionNode = context.Parse(expression);
            while (!context.SkipWhitespaces().IsEofOrAnyOf(tokens, position))
                expressionNode = context.Parse(expressionNode);
            return expressionNode;
        }

        public static List<IExpressionNode>? ParseArguments(this ITokenParserContext context, string endSymbol)
        {
            LazyList<IExpressionNode> args = default;
            while (true)
            {
                var node = context.TryParseWhileNotNull();
                if (node != null)
                {
                    args.Add(node);
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
                }

                context.TryGetErrors()?.Add(node == null
                    ? BindingMessageConstant.CannotParseArgumentExpressionsExpectedExpressionFormat1.Format(args.List == null ? null : string.Join(",", args))
                    : BindingMessageConstant.CannotParseArgumentExpressionsExpectedFormat2.Format(string.Join(",", args), endSymbol));

                return null;
            }

            return args;
        }

        public static string[]? ParseStringArguments(this ITokenParserContext context, string endSymbol, bool isPointSupported)
        {
            LazyList<(int start, int end)> args = default;
            var start = context.Position;
            int? end = null;
            while (true)
            {
                var isEnd = context.SkipWhitespaces().IsToken(endSymbol);
                if (isEnd || context.IsToken(','))
                {
                    if (end == null)
                    {
                        context.TryGetErrors()?.Add(BindingMessageConstant.CannotParseArgumentExpressionsExpectedExpressionFormat1.Format(context.Format(args)));
                        return null;
                    }

                    args.Add((start, end.Value));
                    context.MoveNext();
                    if (isEnd)
                        break;

                    start = context.SkipWhitespaces().Position;
                    end = null;
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
                    context.Position = position;
                    continue;
                }

                context.TryGetErrors()?.Add(BindingMessageConstant.CannotParseArgumentExpressionsExpectedFormat2.Format(context.Format(args), endSymbol));
                return null;
            }

            var list = args.List;
            if (list == null)
                return null;
            var result = new string[list.Count];
            for (int i = 0; i < result.Length; i++)
            {
                var t = list[i];
                result[i] = context.GetValue(t.start, t.end);
            }

            return result;
        }

        public static ItemOrList<ExpressionParserResult, IReadOnlyList<ExpressionParserResult>> ParseExpression(this ITokenParserContext context)
        {
            ExpressionParserResult itemResult = default;
            List<ExpressionParserResult>? result = null;
            while (!context.IsEof())
            {
                var r = TryParseNext(context);
                if (r.IsEmpty)
                    break;
                if (itemResult.IsEmpty)
                    itemResult = r;
                else
                {
                    if (result == null)
                        result = new List<ExpressionParserResult> { itemResult };
                    result.Add(r);
                }

                context.SkipWhitespaces();
            }

            if (result == null)
                return itemResult;
            return result;
        }

        private static ExpressionParserResult TryParseNext(ITokenParserContext context)
        {
            var isActionToken = context.SkipWhitespaces().IsToken('@');
            int delimiterPos;
            if (isActionToken)
            {
                context.MoveNext();
                delimiterPos = -1;
            }
            else
                delimiterPos = context.FindAnyOf(BindingTargetDelimiters);
            var oldLimit = context.Limit;
            if (delimiterPos > 0)
                context.Limit = delimiterPos;

            var errors = context.TryGetErrors();
            var target = context.ParseWhileAnyOf(BindingDelimiters);
            context.Limit = oldLimit;
            errors?.Clear();

            IExpressionNode? source = null;
            if (context.IsToken(' '))
            {
                source = context.ParseWhileAnyOf(BindingDelimiters);
                errors?.Clear();
            }

            List<IExpressionNode>? parameters = null;
            IExpressionNode? parameter = null;
            while (context.IsToken(','))
            {
                var param = context.MoveNext().ParseWhileAnyOf(BindingDelimiters);
                if (parameter == null)
                    parameter = param;
                else
                {
                    if (parameters == null)
                        parameters = new List<IExpressionNode> { parameter };
                    parameters.Add(param);
                }
                errors?.Clear();
            }

            if (context.SkipWhitespaces().IsEof() || context.IsToken(';'))
            {
                if (context.IsToken(';'))
                    context.MoveNext();
                if (isActionToken)
                    return new ExpressionParserResult(UnaryExpressionNode.ActionMacros, target, parameters ?? new ItemOrList<IExpressionNode, IReadOnlyList<IExpressionNode>>(parameter));
                return new ExpressionParserResult(target, source ?? MemberExpressionNode.Empty, parameters ?? new ItemOrList<IExpressionNode, IReadOnlyList<IExpressionNode>>(parameter));
            }

            context.ThrowCannotParse(context);
            return default;
        }

        private static bool IsValidIdentifierSymbol(this char symbol, bool isFirstSymbol)
        {
            if (isFirstSymbol)
                return char.IsLetter(symbol) || symbol == '@' || symbol == '_';
            return char.IsLetterOrDigit(symbol) || symbol == '_';
        }

        private static string? Format(this ITokenParserContext context, LazyList<(int start, int end)> args)
        {
            return args.List == null ? null : string.Join(",", args.List.Select(tuple => context.GetValue(tuple.start, tuple.end)));
        }

        #endregion
    }
}