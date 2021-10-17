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
            : base(errorMessage)
        {
            _statement = statement;
        }

        public string Statement
        {
            get
            {
                int? lineNumber = _statement?
                            .Where(s => s.LineNumber > 0)
                            .FirstOrDefault()?
                            .LineNumber;

                return $"{ (lineNumber.HasValue ? lineNumber.Value.ToString() : "") }: { String.Concat(_statement?.Select(s => s.TokenValue).ToArray()) }";
            }
        }
    }
}
