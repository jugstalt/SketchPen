using SketchPen.Parse.Lexer;
using SketchPen.Parse.Lexer.Abstrations;
using SketchPen.Parse.Lexer.Exceptions;
using SketchPen.Plot.Abstraction;
using SketchPen.Plot.Compile;
using SketchPen.Plot.Exceptions;
using SketchPen.Plot.Platform;
using SketchPen.Plot.Reflection;
using SketchPen.Plot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SketchPen.Plot.Extensions;

static public class TokenExtensions
{
    static public string ToCommandLine(this IEnumerable<Token> tokens)
    {
        var sb = new StringBuilder();

        if (tokens != null)
        {
            foreach (var token in tokens)
            {
                var tokenString = token.TokenValue;

                switch (token.TokenType)
                {
                    case TokenType.LiteralConstant:
                        tokenString = $"\"{tokenString}\"";
                        break;
                    case TokenType.CharacterConstant:
                        tokenString = $"'{tokenString}'";
                        break;
                }

                sb.Append(tokenString);
            }
        }

        return sb.ToString();
    }

    static public IEnumerable<IEnumerable<Token>> GetStatements(this IEnumerable<Token> tokens, ISyntax syntax)
    {
        List<IEnumerable<Token>> statements = new List<IEnumerable<Token>>();
        Dictionary<string, int> lineNumber = new Dictionary<string, int>();

        List<Token> statement = new List<Token>();
        bool inComment = false;
        var tokensArray = tokens.ToArray();
        string codeFilename = String.Empty;

        lineNumber[codeFilename] = 1;

        for (int t = 0, to = tokensArray.Length; t < to; t++)
        {
            var token = tokensArray[t];

            if (inComment)
            {
                if (token.TokenType == TokenType.NewLine)
                {
                    inComment = false;
                }
                else
                {
                    continue;
                }
            }

            if (token.IsComment(syntax))
            {
                if (statement.Count > 0)
                {
                    statements.Add(statement);
                    statement = new List<Token>();
                }
                inComment = true;

                string commentLine = tokensArray
                                        .Skip(t)
                                        .NextStatement(lineNumber[codeFilename], codeFilename)
                                        .ToCommandLine();

                lineNumber[codeFilename] = lineNumber[codeFilename] + 1;

                if (commentLine.IsCodefileComment())
                {
                    codeFilename = commentLine.GetCodefile();

                    if (!lineNumber.ContainsKey(codeFilename))
                    {
                        lineNumber[codeFilename] = 1;
                    }
                }

                continue;
            }

            if (token.TokenType == TokenType.Separator && syntax.Separator.Contains(token.TokenValue))
            {
                if (statement.Count > 0)
                {
                    statement.First().LineNumber = lineNumber[codeFilename];
                    statement.First().CodeFile = codeFilename;

                    statements.Add(statement);
                    statement = new List<Token>();
                }

                lineNumber[codeFilename] = lineNumber[codeFilename] + 1;
            }
            else
            {
                if (statement.Count > 0 || token.TokenType == TokenType.Keyword)
                {
                    statement.Add(token);
                }
                else if (statement.Count == 0 && token.TokenType != TokenType.NewLine)
                {
                    throw new SyntaxErrorException($"Unkown Keyword/{token.TokenType}: {token.TokenValue}",
                        tokensArray
                            .Skip(t)
                            .NextStatement(lineNumber[codeFilename], codeFilename));
                }
            }
        }

        return statements;
    }

    static public bool IsPointOperator(this Token token)
    {
        return token.TokenType == TokenType.Operator && token.TokenValue == ".";
    }

    static public bool IsCommaOperator(this Token token)
    {
        return token.TokenType == TokenType.Operator && token.TokenValue == ",";
    }

    static public bool IsComment(this Token token, ISyntax syntax)
    {
        return token.TokenType == TokenType.Identifier && syntax.Comments.Contains(token.TokenValue);
    }

    static public List<Token> CollectParameters(this Token[] tokens, ref int index)
    {
        List<Token> parametes = new List<Token>();
        var operatorToken = tokens[index];

        if (operatorToken.TokenType != TokenType.Operator)
        {
            throw new LexerException($"{operatorToken.TokenValue} is no operator token. Syntax error?");
        }

        if (operatorToken.TokenValue == "()")
        {
            return parametes;
        }

        string closingTokken = String.Empty;
        switch (operatorToken.TokenValue)
        {
            case "(":
                closingTokken = ")";
                break;
            case "{":
                closingTokken = "}";
                break;
            case "[":
                closingTokken = "]";
                break;
        }

        if (String.IsNullOrEmpty(closingTokken))
        {
            throw new LexerException($"Can't determine closing token for '{operatorToken.TokenValue}'");
        }

        int level = 0;
        for (int i = index + 1; i < tokens.Length; i++)
        {
            if (tokens[i].TokenType == TokenType.Operator && tokens[i].TokenValue == operatorToken.TokenValue)
            {
                level++;
            }

            if (tokens[i].TokenType == TokenType.Operator && tokens[i].TokenValue == closingTokken)
            {
                if (level == 0)
                {
                    index = i;
                    break;
                }
                else
                {
                    level--;
                }
            }

            if (!tokens[i].IsCommaOperator())
            {
                parametes.Add(tokens[i]);
            }
        }

        if (level != 0)
        {
            throw new LexerException($"CollectParameters: {tokens.ToCommandLine()}");
        }

        return parametes;
    }

    static public object ParameterValue(this Token token)
    {
        switch (token.TokenType)
        {
            case TokenType.NumericalConstant:
                //return float.Parse(token.TokenValue);
                return token.TokenValue.ToFloat();
            default:
                return token.TokenValue;
        }
    }

    static public IEnumerable<IPlotCommand> GetPlotCommands(this IEnumerable<IEnumerable<Token>> statements,
                                                            CommandTypesService commandTypes)
    {
        List<IPlotCommand> commands = new List<IPlotCommand>();

        foreach (var statement in statements)
        {
            var keyword = statement.First().TokenValue; // must be keyword

            var commandType = commandTypes.PlotCommandTypes.Where(t => t.GetCustomAttribute<PlotCommandKeywordAttribute>().Keyword == keyword).FirstOrDefault();
            if (commandType == null)
            {
                throw new Exception($"Unkown plotcommand {keyword}");
            }

            var command = (IPlotCommand)Activator.CreateInstance(commandType);

            var tokens = statement.ToArray();
            if (!tokens[1].IsPointOperator())
            {
                throw new Exception("Synatx error");
            }

            if (tokens[2].TokenType != TokenType.Identifier)
            {
                throw new Exception("Synatx error");
            }

            string method = tokens[2].TokenValue;
            IEnumerable<Token> parameters = Array.Empty<Token>();

            for (int i = 2; i < tokens.Length; i++)
            {
                if (tokens[i].TokenType == TokenType.Operator)
                {
                    parameters = tokens.CollectParameters(ref i);
                }
            }

            command.Init(statement);
            command.SetStatement(method, parameters.Select(t => t.ParameterValue()));
            commands.Add(command);
        }

        return commands;
    }

    static public IEnumerable<Token> NextStatement(this IEnumerable<Token> tokens, int lineNumber, string codeFile)
    {
        List<Token> statement = new List<Token>();

        foreach (var token in tokens)
        {
            if (token.TokenType == TokenType.Separator ||
               token.TokenType == TokenType.NewLine)
            {
                break;
            }

            statement.Add(token);
        }

        if (statement.Count > 0)
        {
            statement.First().LineNumber = lineNumber;
            statement.First().CodeFile = codeFile;
        }

        return statement;
    }
}
