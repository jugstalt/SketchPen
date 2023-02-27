using SketchPen.Plot.Compile;
using SketchPen.Plot.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SketchPen.Plot.Services
{
    public class CompilerService
    {
        public CompilerService() 
        {
            var compiler = new Compiler(); // create instance => static constructor
        }

        public IEnumerable<Type> PlotCommandTypes => Compiler.PlotCommandTypes;
        public IDictionary<string, IEnumerable<EditorCompletionModel>> EditorCompletion => Compiler.EditorCompletion;
    }
}
