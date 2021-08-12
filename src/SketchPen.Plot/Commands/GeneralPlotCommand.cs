using SketchPen.Plot.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SketchPen.Plot.Commands
{
    abstract class GeneralPlotCommand : IPlotCommand
    {
        private string _method = null;
        private IEnumerable<object> _parameters = null;

        protected string Method => _method;
        protected IEnumerable<object> Parameters => _parameters;

        #region IDisposable

        virtual public void Dispose()
        {
            
        }

        #endregion

        abstract protected void ExecuteCommand(IPlotContext context, IEnumerable<object> parameters);

        #region IPlotCommand

        virtual public void Init() { }

        public void Execute(IPlotContext context)
        {
            ExecuteCommand(context, Parameters.Select(p =>
            {
                if (p != null && p.ToString().StartsWith("@@"))
                {
                    return context.Globals[p?.ToString().Substring(2)];
                } 
                else
                {
                    return p;
                }
            }));
        }

        public void SetStatement(string method, IEnumerable<object> parameters)
        {
            _method = method;
            _parameters = parameters;
        }

        #endregion
    }
}
