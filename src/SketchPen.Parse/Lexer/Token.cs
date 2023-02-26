namespace SketchPen.Parse.Lexer
{
    public class Token
    {
        public Token(TokenType tokenType, string tokenValue = "")
        {
            this.TokenType = tokenType;
            this.TokenValue = tokenValue;
        }

        public TokenType TokenType { get; set; }
        public string TokenValue { get; set; }

        public int LineNumber { get; set; }
        public string CodeFile { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"{ this.TokenType }: { this.TokenValue }";
        }
    }
}
