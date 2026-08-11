using SketchPen.Parse.Lexer;
using SketchPen.Plot.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SketchPen.Plot.Extensions;

/// <summary>
/// A tiny arithmetic expression tree for command parameters (+, -, *, /, parens, @@variable
/// references, and numeric literals). Only ever constructed for parameters with more than one
/// token -- a bare literal or bare "@@name" parameter never goes through this at all (see
/// <see cref="TokenListExtensions.ToParameterValue"/>), which is what keeps every existing
/// .sp/.spt/.globals file's behavior byte-for-byte unchanged. Deliberately scoped to numeric
/// arithmetic only -- no string concatenation, no comparison/boolean operators, no functions.
/// </summary>
abstract class ParamExpr
{
    public abstract object Evaluate(IDictionary<string, object> globals, IEnumerable<Token> statement);

    protected static float AsNumber(object value, IEnumerable<Token> statement)
    {
        if (value is float f)
        {
            return f;
        }

        throw new SyntaxErrorException($"Expected a number in arithmetic expression, got '{value}'", statement);
    }
}

class LiteralExpr : ParamExpr
{
    private readonly object _value;

    public LiteralExpr(object value)
    {
        _value = value;
    }

    public override object Evaluate(IDictionary<string, object> globals, IEnumerable<Token> statement) => _value;
}

class VariableExpr : ParamExpr
{
    private readonly string _name;

    public VariableExpr(string name)
    {
        _name = name;
    }

    public override object Evaluate(IDictionary<string, object> globals, IEnumerable<Token> statement)
    {
        // Same lookup/exception as GeneralPlotCommand.Execute's existing bare-"@@name" path --
        // same message, same exception type, just reachable from inside an expression too now.
        if (!globals.ContainsKey(_name))
        {
            throw new SyntaxErrorException($"Unknown variable: {_name}", statement);
        }

        return globals[_name];
    }
}

class UnaryMinusExpr : ParamExpr
{
    private readonly ParamExpr _operand;

    public UnaryMinusExpr(ParamExpr operand)
    {
        _operand = operand;
    }

    public override object Evaluate(IDictionary<string, object> globals, IEnumerable<Token> statement)
    {
        return -AsNumber(_operand.Evaluate(globals, statement), statement);
    }
}

class BinaryExpr : ParamExpr
{
    private readonly char _op;
    private readonly ParamExpr _left;
    private readonly ParamExpr _right;

    public BinaryExpr(char op, ParamExpr left, ParamExpr right)
    {
        _op = op;
        _left = left;
        _right = right;
    }

    public override object Evaluate(IDictionary<string, object> globals, IEnumerable<Token> statement)
    {
        float left = AsNumber(_left.Evaluate(globals, statement), statement);
        float right = AsNumber(_right.Evaluate(globals, statement), statement);

        return _op switch
        {
            '+' => left + right,
            '-' => left - right,
            '*' => left * right,
            '/' => left / right,
            _ => throw new SyntaxErrorException($"Unknown operator '{_op}' in arithmetic expression", statement)
        };
    }
}

static class TokenListExtensions
{
    /// <summary>
    /// Converts one parameter's token list into the runtime value <c>GeneralPlotCommand.Execute</c>
    /// consumes: for a single token, the exact same <c>object</c> the pre-Tier-3 code produced
    /// (numeric literal -> float, everything else -> raw string, including an unresolved
    /// "@@name") -- this single-token fast path is what guarantees zero behavior change for
    /// every pre-existing parameter. For more than one token, parses and returns a <see cref="ParamExpr"/>
    /// tree, evaluated later at command-execution time (once <c>context.Globals</c> is available).
    /// </summary>
    public static object ToParameterValue(this List<Token> tokens, IEnumerable<Token> statement)
    {
        var normalized = NormalizeImplicitSubtraction(tokens);

        if (normalized.Count == 1)
        {
            return normalized[0].ParameterValue();
        }

        var parser = new ExpressionParser(normalized, statement);
        var expr = parser.ParseExpression();
        parser.ExpectEnd();
        return expr;
    }

    /// <summary>
    /// The lexer unconditionally absorbs a '-' immediately followed by a digit into one negative
    /// numeric token (LexicalAnalyser.GetNextLexicalAtom), regardless of what precedes it --
    /// which is exactly right for a leading/unary negative literal ("-45", or "-5" right after
    /// "(" or another operator), but wrong when it's really subtraction ("@@x-5", indistinguishable
    /// after PreComplier strips all whitespace from "@@x - 5"). Fix: split the absorbed literal
    /// back into an explicit '-' operator + a positive literal whenever it's immediately preceded
    /// by an operand (a value, or a ")" closing a sub-expression) -- the grammar has no other way
    /// for two operand tokens to be adjacent, so this is unambiguous.
    /// </summary>
    private static List<Token> NormalizeImplicitSubtraction(List<Token> tokens)
    {
        var result = new List<Token>(tokens.Count);

        foreach (var token in tokens)
        {
            bool isAbsorbedNegative = token.TokenType == TokenType.NumericalConstant
                && token.TokenValue.StartsWith("-")
                && result.Count > 0
                && IsOperandToken(result[result.Count - 1]);

            if (isAbsorbedNegative)
            {
                result.Add(new Token(TokenType.Operator, "-"));
                result.Add(new Token(TokenType.NumericalConstant, token.TokenValue.Substring(1)));
            }
            else
            {
                result.Add(token);
            }
        }

        return result;
    }

    private static bool IsOperandToken(Token token)
    {
        if (token.TokenType != TokenType.Operator)
        {
            return true; // numeric/string/char literal, @@name identifier -- all values.
        }

        return token.TokenValue == ")"; // a closing paren ends a sub-expression, itself a value.
    }
}

/// <summary>
/// Recursive-descent parser over one (already space-normalized) parameter's token list.
/// Precedence: unary '-' tightest, then '*'/'/', then '+'/'-'; parens group. Scope is
/// deliberately minimal -- see ParamExpr's own doc comment.
/// </summary>
class ExpressionParser
{
    private readonly List<Token> _tokens;
    private readonly IEnumerable<Token> _statement;
    private int _position;

    public ExpressionParser(List<Token> tokens, IEnumerable<Token> statement)
    {
        _tokens = tokens;
        _statement = statement;
    }

    public ParamExpr ParseExpression() => ParseAddSub();

    public void ExpectEnd()
    {
        if (_position < _tokens.Count)
        {
            throw new SyntaxErrorException($"Unexpected token '{_tokens[_position].TokenValue}' in arithmetic expression", _statement);
        }
    }

    private ParamExpr ParseAddSub()
    {
        var left = ParseMulDiv();

        while (_position < _tokens.Count && IsOperator("+", "-"))
        {
            char op = _tokens[_position].TokenValue[0];
            _position++;
            var right = ParseMulDiv();
            left = new BinaryExpr(op, left, right);
        }

        return left;
    }

    private ParamExpr ParseMulDiv()
    {
        var left = ParseUnary();

        while (_position < _tokens.Count && IsOperator("*", "/"))
        {
            char op = _tokens[_position].TokenValue[0];
            _position++;
            var right = ParseUnary();
            left = new BinaryExpr(op, left, right);
        }

        return left;
    }

    private ParamExpr ParseUnary()
    {
        if (_position < _tokens.Count && IsOperator("-"))
        {
            _position++;
            return new UnaryMinusExpr(ParseUnary());
        }

        return ParsePrimary();
    }

    private ParamExpr ParsePrimary()
    {
        if (_position >= _tokens.Count)
        {
            throw new SyntaxErrorException("Unexpected end of arithmetic expression", _statement);
        }

        var token = _tokens[_position];

        if (token.TokenType == TokenType.Operator && token.TokenValue == "(")
        {
            _position++;
            var expr = ParseAddSub();

            if (_position >= _tokens.Count || _tokens[_position].TokenValue != ")")
            {
                throw new SyntaxErrorException("Missing closing ')' in arithmetic expression", _statement);
            }
            _position++;

            return expr;
        }

        if (token.TokenType == TokenType.NumericalConstant)
        {
            _position++;
            return new LiteralExpr(token.ParameterValue());
        }

        if (token.TokenType == TokenType.Identifier && token.TokenValue.StartsWith("@@"))
        {
            _position++;
            return new VariableExpr(token.TokenValue.Substring(2));
        }

        throw new SyntaxErrorException($"Unexpected token '{token.TokenValue}' in arithmetic expression", _statement);
    }

    private bool IsOperator(params string[] values)
    {
        var token = _tokens[_position];
        return token.TokenType == TokenType.Operator && values.Contains(token.TokenValue);
    }
}
