using SketchPen.Plot.Abstraction;
using System;
using System.Collections.Generic;
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

        #region IPlotCommand

        abstract public void Execute(IPlotContext context);

        public void SetStatement(string method, IEnumerable<object> parameters)
        {
            _method = method;
            _parameters = parameters;
        }

        #endregion
    }
}
