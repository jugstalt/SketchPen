using System;

namespace SketchPen.Parse.Lexer.Exceptions
{
    class LexerSyntaxException : LexerException
    {
        public LexerSyntaxException(string message, Exception innerException = null)
            : base($"Lexer Syntax Error: { message }", innerException)
        { }
    }
}
