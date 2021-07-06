using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hermes.Modules.Services;

namespace Hermes.Modules.Developer
{
    [DiscordCommandClass("Developer", "Dev commands that you can't use 🤣!")]
    public class Gencommandtotalus : CommandModuleBase
    {
        [DiscordCommand("gencommandtotalus", commandHelp = "", description = "")]
        public async Task getInves(params string[] args)
        {
            if (devids.Any(x => x == Context.User.Id))
                foreach (var x in CustomCommandService.Modules)
                {
                    var xyz = "# Hermes\n";
                    xyz += $"## {x.Key}\n*{x.Value}*\n";
                    foreach (var y in Commands.Where(y => y.ModuleName == x.Key))
                        xyz +=
                            $"### r{y.CommandName}         \nDescription: {(string.IsNullOrEmpty(y.CommandDescription) ? "None" : y.CommandDescription)}          \n{(y.RequireUsrPerm != null ? $"Permissions required: ```\n{string.Join(",\n", y.RequireUsrPerm.Select(k => k.ToString()))}```                \n" : "")}Alts: `{(string.IsNullOrEmpty(string.Join(", ", y.Alts)) ? "None" : string.Join(", ", y.Alts))}`          \nExamples: `{(string.IsNullOrEmpty(y.example) ? "None" : y.example)}`          \n";
                    var fst = new FileStream($"../Data/{x.Key}.md", FileMode.Create);
                    var swr = new StreamWriter(fst);
                    await swr.WriteLineAsync(xyz);
                    await swr.FlushAsync();
                    swr.Close();
                }
        }
    }
}