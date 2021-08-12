using SketchPen.Parse.Lexer;
using SketchPen.Parse.Lexer.Abstrations;
using SketchPen.Parse.Lexer.Exceptions;
using SketchPen.Plot.Abstraction;
using SketchPen.Plot.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SketchPen.Plot.Extensions
{
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
                            tokenString = $"\"{ tokenString }\"";
                            break;
                        case TokenType.CharacterConstant:
                            tokenString = $"'{ tokenString }'";
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

            List<Token> statement = new List<Token>();
            bool inComment = false;
            foreach (var token in tokens)
            {
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
                    continue;
                }

                if (token.TokenType == TokenType.Separator && syntax.Separator.Contains(token.TokenValue))
                {
                    if (statement.Count > 0)
                    {
                        statements.Add(statement);
                        statement = new List<Token>();
                    }
                }
                else
                {
                    if (statement.Count > 0 || token.TokenType == TokenType.Keyword)
                    {
                        statement.Add(token);
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
                throw new LexerException($"{ operatorToken.TokenValue } is no operator token. Syntax error?");
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
                throw new LexerException($"Can't determine closing token for '{ operatorToken.TokenValue }'");
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
                throw new LexerException($"CollectParameters: { tokens.ToCommandLine() }");
            }

            return parametes;
        }

        static public object ParameterValue(this Token token)
        {
            switch(token.TokenType)
            {
                case TokenType.NumericalConstant:
                    return float.Parse(token.TokenValue);
                default:
                    return token.TokenValue;
            }
        }

        static public IEnumerable<IPlotCommand> GetPlotCommands(this IEnumerable<IEnumerable<Token>> statements)
        {
            List<IPlotCommand> commands = new List<IPlotCommand>();

            foreach (var statement in statements)
            {

                var keyword = statement.First().TokenValue; // must be keyword

                var commandType = Plotter.PlotCommandTypes.Where(t => t.GetCustomAttribute<PlotCommandKeywordAttribute>().Keyword == keyword).FirstOrDefault();
                if (commandType == null)
                {
                    throw new Exception($"Unkown plotcommand { keyword }");
                }

                var command = (IPlotCommand)Activator.CreateInstance(commandType);

                var tokens = statement.ToArray();
                if (!tokens[1].IsPointOperator())
                    throw new Exception("Synatx error");
                if (tokens[2].TokenType != TokenType.Identifier)
                    throw new Exception("Synatx error");

                string method = tokens[2].TokenValue;
                List<Token> parameters = null;
                for(int i=2;i<tokens.Length;i++)
                {
                    if(tokens[i].TokenType == TokenType.Operator)
                    {
                        parameters = tokens.CollectParameters(ref i);
                    }
                }

                command.Init();
                command.SetStatement(method, parameters.Select(t => t.ParameterValue()));
                commands.Add(command);
            }

            return commands;
        }
    }
}
