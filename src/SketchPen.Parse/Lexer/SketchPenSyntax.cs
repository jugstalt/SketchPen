using SketchPen.Parse.Lexer.Abstrations;

namespace SketchPen.Parse.Lexer
{
    public class SketchPenSyntax : ISyntax
    {
        public const string StatementSeparator = ";";

        #region Const

        private string[] _keywords = new[] { "pen", "brush", "line", "rect", "circle", "text", "transform", "path", "line", "gradientbrush" };

        private string[] _separator = new[] { StatementSeparator, "\r", "\n", "\r\n" };

        private string[] _comments = new[] { "//" };

        private string[] _operators = new[] { "(", ")", ",", "()", "." };

        #endregion

        public string[] Keywords => _keywords;

        public string[] Separator => _separator;

        public string[] Comments => _comments;

        public string[] Operators => _operators;
    }
}
