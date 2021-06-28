using System;

namespace RoleX.Modules.Services
{
    /// <summary>
    /// The Commmand class attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class DiscordCommandClass : Attribute
    {
        public string ModuleName { get; set; }
        public string ModuleDescription { get; set; }
        /// <summary>
        /// The prefix for all commands in this class.
        /// </summary>
        public char prefix { get; set; }
        /// <summary>
        /// If <see langword="true"/> then only the property prefix will work on child commands, if <see langword="false"/> then the assigned prefix AND the prefix on the command are valid. Default is <see langword="true"/>
        /// </summary>
        public bool OverwritesPrefix { get; set; }
        /// <summary>
        /// Tells the command service that this class contains commands.
        /// </summary>
        public DiscordCommandClass(string ModuleName, string ModuleDescription)
        {
            this.ModuleName = ModuleName;
            this.ModuleDescription = ModuleDescription;
        }
    }
}
