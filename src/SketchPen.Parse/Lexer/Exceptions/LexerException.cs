using System;

namespace SketchPen.Parse.Lexer.Exceptions
{
    public class LexerException : Exception
    {
        public LexerException() { }
        public LexerException(string message, Exception innerException = null)
            : base(message, innerException)
        {

        }
    }
}
