using System.Collections.Generic;

namespace Hermes.Modules.Services
{
    public interface ICommands
    {
        string CommandName { get; }
        string CommandDescription { get; }
        string CommandHelpMessage { get; }
        string example { get; }
        char[] Prefixes { get; }
        bool RequiresPermission { get; }
        string ModuleName { get; }
        List<string> Alts { get; }
        bool HasName(string name);
    }
}