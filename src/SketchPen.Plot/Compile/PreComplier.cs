using SketchPen.Plot.Extensions;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace SketchPen.Plot.Compile;

class PreComplier
{
    private readonly string _fileName;
    private readonly string _code;

    // A `repeat(n) { ... }` header must be alone on its own (trimmed) line -- mirrors the
    // "#include " line-start check below. Braces never reach the lexer: this whole feature is a
    // pure text-preprocessing macro, exactly like #include already is.
    private static readonly Regex RepeatHeaderRegex = new Regex(@"^repeat\(\s*(\d+)\s*\)\s*\{\s*$", RegexOptions.Compiled);

    public PreComplier(string fileName, string customGlobalsName = "", bool appendGlobals = false)
    {
        _fileName = fileName;

        StringBuilder code = new StringBuilder();

        FileInfo fi = new FileInfo(fileName);
        DirectoryInfo di = fi.Directory;

        string globalsFile = new FileInfo($"{di.FullName}/_.globals").FullName;
        appendGlobals = appendGlobals && globalsFile != fi.FullName;

        if (!String.IsNullOrEmpty(customGlobalsName))
        {
            var customGlobalsFi = new FileInfo($"{di.FullName}/_{customGlobalsName}.globals");
            if (customGlobalsFi.Exists)
            {
                code.AppendCodefileComment(customGlobalsFi.FullName);
                code.Append(File.ReadAllText(customGlobalsFi.FullName));
                code.Append(Environment.NewLine);
            }
        }

        if (appendGlobals)
        {
            var globalsFi = new FileInfo($"{di.FullName}/_.globals");
            if (globalsFi.Exists)
            {
                code.AppendCodefileComment(globalsFi.FullName);
                code.Append(File.ReadAllText(globalsFi.FullName));
                code.Append(Environment.NewLine);
            }
        }

        code.AppendCodefileComment(_fileName);
        code.Append(File.ReadAllText(_fileName));

        _code = code.ToString();
    }

    public string Compile(string codeFile)
    {
        StringBuilder preComipiedCode = new StringBuilder();

        var stringReader = new StringReader(_code);
        ProcessLines(stringReader, preComipiedCode, codeFile);

        return preComipiedCode.ToString().Trim();
    }

    /// <summary>
    /// Processes lines from <paramref name="reader"/> into <paramref name="output"/>. Shared by
    /// the top-level file and by <see cref="ExpandRepeat"/>'s unrolled bodies, so a `repeat`
    /// body goes through the exact same rules (space-stripping, auto-semicolon, nested
    /// `#include`, nested `repeat`) as the top-level file -- not a brand-new file.
    /// </summary>
    private void ProcessLines(TextReader reader, StringBuilder output, string codeFile)
    {
        string codeLine;

        while ((codeLine = reader.ReadLine()) != null)
        {
            #region Include

            if (codeLine.Trim().StartsWith("#include "))
            {
                IncludeFile(codeLine.Substring("#include ".Length), output, codeFile);
                continue;
            }

            #endregion

            #region Repeat

            if (RepeatHeaderRegex.IsMatch(codeLine.Trim()))
            {
                ExpandRepeat(codeLine.Trim(), reader, output, codeFile);
                continue;
            }

            #endregion

            #region Spaces (no need for spaces in this language => dirty remove all)

            if (!codeLine.Trim().StartsWith("//"))
            {
                codeLine = codeLine.Replace(" ", "").Replace("\t", "").Trim();
            }
            else if (!codeLine.Trim().StartsWith("// "))
            {
                codeLine = $"// {codeLine.Substring(2)}";
            }

            #endregion

            #region Auto Append Semicolon

            if (!codeLine.EndsWith(";"))
            {
                codeLine = $"{codeLine};";
            }

            #endregion

            output.Append(codeLine);
            output.Append(Environment.NewLine);
        }
    }

    /// <summary>
    /// Expands a `repeat(n) { ... }` block: collects the raw body text (tracking brace depth so
    /// nested `repeat`/`#include` inside the body work), then re-processes that body text `n`
    /// times, appending a CodeFile marker after each iteration so line-number tracking resets to
    /// a sane (if approximate -- see docs/SYNTAX.md) state between copies.
    /// </summary>
    private void ExpandRepeat(string headerLine, TextReader reader, StringBuilder output, string codeFile)
    {
        var match = RepeatHeaderRegex.Match(headerLine);
        int count = int.Parse(match.Groups[1].Value);

        if (count <= 0)
        {
            throw new Exception($"Malformed repeat(...) block in {codeFile}: repeat count must be a positive integer, got '{headerLine}'.");
        }

        StringBuilder body = new StringBuilder();
        int depth = 1;
        string line;

        while ((line = reader.ReadLine()) != null)
        {
            string trimmed = line.Trim();

            if (RepeatHeaderRegex.IsMatch(trimmed))
            {
                depth++;
                body.Append(line);
                body.Append(Environment.NewLine);
                continue;
            }

            if (trimmed == "}")
            {
                depth--;
                if (depth == 0)
                {
                    break;
                }

                body.Append(line);
                body.Append(Environment.NewLine);
                continue;
            }

            body.Append(line);
            body.Append(Environment.NewLine);
        }

        if (depth != 0)
        {
            throw new Exception($"Unterminated repeat(...) block in {codeFile}: missing closing '}}'.");
        }

        string bodyText = body.ToString();
        for (int i = 0; i < count; i++)
        {
            ProcessLines(new StringReader(bodyText), output, codeFile);
            output.AppendCodefileComment(codeFile);
        }
    }

    private void IncludeFile(string includeFile, StringBuilder preComipiledCode, string sourceCodeFile)
    {
        includeFile = includeFile.Trim();
        if (includeFile.StartsWith("\"") && includeFile.EndsWith("\""))
        {
            includeFile = includeFile.Substring(1, includeFile.Length - 2);
        }

        FileInfo includeFileInfo;
        if (FileExists($"{new FileInfo(_fileName).Directory.FullName}/{includeFile}"))
        {
            includeFileInfo = new FileInfo($"{new FileInfo(_fileName).Directory.FullName}/{includeFile}");
        }
        else if (FileExists(includeFile))
        {
            includeFileInfo = new FileInfo(includeFile);
        }
        else
        {
            throw new Exception($"Can't find include file {includeFile} from {_fileName}");
        }

        var preCompiler = new PreComplier(includeFileInfo.FullName);

        preComipiledCode.Append(preCompiler.Compile(includeFileInfo.FullName));
        preComipiledCode.AppendCodefileComment(sourceCodeFile);
    }

    private bool FileExists(string path)
    {
        try
        {
            return new FileInfo(path).Exists;
        }
        catch
        {
            return false;
        }
    }
}
