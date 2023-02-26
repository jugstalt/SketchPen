using System;

namespace SketchPen.Parse.Lexer.Exceptions
{
    class LexerSyntaxException : LexerException
    {
        public LexerSyntaxException(string message)
            : base($"Lexer Syntax Error: {message}") {}
        public LexerSyntaxException(string message, Exception innerException)
            : base($"Lexer Syntax Error: { message }", innerException) {}
    }
}
