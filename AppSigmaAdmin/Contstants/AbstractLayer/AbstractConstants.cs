using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace AppSigmaAdmin.Contstants.AbstractLayer
{
    public abstract class AbstractConstants<T> : ConstantsScheme<T, string>
        where T : class
    {
        protected AbstractConstants(string name, string value)
            : base(name, value)
        {

        }
    }
}