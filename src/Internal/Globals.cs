using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Glory.Services.Core.Internal
{
    internal static class Globals
    {
        public static string HostMapPath
        {
            get
            {
                return AppDomain.CurrentDomain.BaseDirectory;
            }
        }
    }
}