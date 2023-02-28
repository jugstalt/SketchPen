using System;

namespace SketchPen.Parse.Lexer.Exceptions;

public class LexerException : Exception
{
    public LexerException() { }
    public LexerException(string message) : base(message) { }
    public LexerException(string message, Exception innerException)
        : base(message, innerException) { }
}
