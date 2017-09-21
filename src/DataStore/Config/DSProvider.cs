
#region Usings

using Microsoft.Extensions.Configuration;
using System.Collections.Specialized;
using System;
using System.Linq;
#endregion

namespace Glory.Services.Core.Config
{
    public class DSProvider : Provider
    {
        public DSProvider(IConfigurationSection section) : base(section)
        { }

        public int StageSpanMinutes
        {
            get
            {
                return int.Parse(this.Attributes["StageSpanMinutes"]);
            }
        }
    }
}