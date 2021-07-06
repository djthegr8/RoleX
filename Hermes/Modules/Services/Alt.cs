using System;

namespace Hermes.Modules.Services
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class Alt : Attribute
    {
        public Alt(string Alt)
        {
            alt = Alt;
        }

        public string alt { get; set; }
    }
}