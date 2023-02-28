using System;
using System.Text.Json.Serialization;

namespace SketchPen.Plot.Models;

public class EditorCompletionModel
{
    public EditorCompletionModel(string method, string suggestion, string snippet)
    {
        this.Method = method;
        this.Suggestion = !String.IsNullOrEmpty(suggestion) ? suggestion : $"{method}()";
        this.Snippet = !String.IsNullOrEmpty(snippet) ? snippet : $"{method}();";
    }

    [JsonPropertyName("method")]
    public string Method { get; set; }

    [JsonPropertyName("suggestion")]
    public string Suggestion { get; set; }

    [JsonPropertyName("snippet")]
    public string Snippet { get; set; }
}
