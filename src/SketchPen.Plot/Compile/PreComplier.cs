using System;
using System.IO;
using System.Text;

namespace SketchPen.Plot.Compile
{
    class PreComplier
    {
        private readonly string _fileName;
        private readonly string _code;

        public PreComplier(string fileName)
        {
            _fileName = fileName;
            _code = File.ReadAllText(_fileName);
        }

        public string Compile()
        {
            StringBuilder preComipiedCode = new StringBuilder();

            var stringReader = new StringReader(_code);
            string codeLine;

            while ((codeLine = stringReader.ReadLine()) != null)
            {
                #region Empty Lines

                if (String.IsNullOrWhiteSpace(codeLine))
                {
                    continue;
                }

                #endregion

                #region Remove Comment Lines

                if (codeLine.Trim().StartsWith("//"))
                {
                    continue;
                }

                #endregion

                #region Include

                if (codeLine.Trim().StartsWith("#include "))
                {
                    IncludeFile(codeLine.Substring("#include ".Length), preComipiedCode);
                    preComipiedCode.Append(Environment.NewLine);
                    continue;
                }

                #endregion

                #region Spaces (no need for spaces in this language => dirty remove all)

                codeLine = codeLine.Replace(" ", "").Replace("\t", "").Trim();

                #endregion

                #region Auto Append Semicolon

                if(!codeLine.EndsWith(";"))
                {
                    codeLine = $"{ codeLine };";
                }

                #endregion

                preComipiedCode.Append(codeLine);
                preComipiedCode.Append(Environment.NewLine);
            }

            return preComipiedCode.ToString().Trim();
        }

        private void IncludeFile(string includeFile, StringBuilder preComipiledCode)
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
            preComipiledCode.Append(preCompiler.Compile());
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
