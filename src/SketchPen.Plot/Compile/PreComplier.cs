using SketchPen.Plot.Extensions;
using System;
using System.IO;
using System.Text;

namespace SketchPen.Plot.Compile
{
    class PreComplier
    {
        private readonly string _fileName;
        private readonly string _code;

        public PreComplier(string fileName, string customGlobalsName = "", bool appendGlobals = false)
        {
            _fileName = fileName;

            StringBuilder code = new StringBuilder();

            DirectoryInfo di = new FileInfo(fileName).Directory;

            if (!String.IsNullOrEmpty(customGlobalsName))
            {
                var customGlobalsFi = new FileInfo($"{ di.FullName }/_{ customGlobalsName }.globals");
                if (customGlobalsFi.Exists)
                {
                    code.AppendCodefileComment(customGlobalsFi.FullName);
                    code.Append(File.ReadAllText(customGlobalsFi.FullName));
                    code.Append(Environment.NewLine);
                }
            }

            if (appendGlobals)
            {
                var globalsFi = new FileInfo($"{ di.FullName }/_.globals");
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
            //preComipiedCode.AppendCodefileComment(codeFile);

            var stringReader = new StringReader(_code);
            string codeLine;

            while ((codeLine = stringReader.ReadLine()) != null)
            {
                #region Empty Lines

                //if (String.IsNullOrWhiteSpace(codeLine))
                //{
                //    continue;
                //}

                #endregion

                #region Remove Comment Lines

                //if (codeLine.Trim().StartsWith("//"))
                //{
                //    continue;
                //}

                #endregion

                #region Include

                if (codeLine.Trim().StartsWith("#include "))
                {
                    IncludeFile(codeLine.Substring("#include ".Length), preComipiedCode, codeFile);
                    //preComipiedCode.Append(Environment.NewLine);

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
                    codeLine = $"// { codeLine.Substring(2) }";
                }

                #endregion

                #region Auto Append Semicolon

                if (!codeLine.EndsWith(";"))
                {
                    codeLine = $"{ codeLine };";
                }

                #endregion

                preComipiedCode.Append(codeLine);
                preComipiedCode.Append(Environment.NewLine);
            }

            return preComipiedCode.ToString().Trim();
        }

        private void IncludeFile(string includeFile, StringBuilder preComipiledCode, string sourceCodeFile)
        {
            includeFile = includeFile.Trim();
            if (includeFile.StartsWith("\"") && includeFile.EndsWith("\""))
            {
                includeFile = includeFile.Substring(1, includeFile.Length - 2);
            }

            FileInfo includeFileInfo = null;
            if (FileExists($"{ new FileInfo(_fileName).Directory.FullName }/{ includeFile }"))
            {
                includeFileInfo = new FileInfo($"{ new FileInfo(_fileName).Directory.FullName }/{ includeFile }");
            }
            else if (FileExists(includeFile))
            {
                includeFileInfo = new FileInfo(includeFile);
            }
            else
            {
                throw new Exception($"Can't find include file { includeFile } from { _fileName }");
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
}
