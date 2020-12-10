using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppSigmaAdmin.Exceptions
{
    /// <summary>
    /// 入力エラー
    /// </summary>
    public class InvalidInputException : Exception
    {
        public InvalidInputException(string msg) : base(msg) { }
    }
}