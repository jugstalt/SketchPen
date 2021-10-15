using SketchPen.Parse.Lexer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SketchPen.Plot.Exceptions
{
    public class SyntaxErrorException : Exception
    {
        private readonly IEnumerable<Token> _statement;

        public SyntaxErrorException(string errorMessage, IEnumerable<Token> statement) 
            :base(errorMessage)
        {
            _statement = statement;
        }

        public string Statement
        {
            get
            {
                return $"{ String.Concat(_statement?.Select(s => s.TokenValue).ToArray()) };";
            }
        }
    }
}
