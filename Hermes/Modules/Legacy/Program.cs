using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading;

namespace LuminousCodeSplitter
{
    class Program
    {
        static void Main(string[] args)
        {
            string UsingRegex = "using (.*?);";
            string Namespace = "namespace RoleX.Modules";
            string ModuleHeader = "    [DiscordCommandClass(\"General\", \"General commands for all!\")]";
            string AltRegex = "\\[Alt\\(\"(.*?)\"\\)\\]";
            string CommandRegex = "\\[DiscordCommand\\(([.\\s\\S]*?)\n        }";
            string CommandNameRegex = "\\[DiscordCommand\\(\"(.*?)\"";

            Start:
            Console.Write("Enter Module File Path: ");
            string pth = Console.ReadLine();
            if (!File.Exists(pth))
            {
                Console.WriteLine("File doesn't exists!");
                goto Start;
            }

            string FinalContent = "";

            // Read our file here
            string Content = File.ReadAllText(pth);

            // Start with Usings
            var usingMatch = Regex.Matches(Content, UsingRegex);

            string UsingContent = "";

            foreach (Match match in usingMatch)
                UsingContent += match.Groups[0].Value + "\n";

            FinalContent += UsingContent;


            
            var CommandMatches = Regex.Matches(Content, CommandRegex);

            foreach (Match m in CommandMatches)
            {
                string command = m.Value;
                string commandName = Regex.Match(command, CommandNameRegex).Groups[1].Value;

                string final = "";
                final += UsingContent + Namespace + "\n{\n" + ModuleHeader + $"\n    public class {UppercaseFirst(commandName)} : CommandModuleBase\n    {{\n        {command}\n    }}\n}}";
                File.WriteAllText(Environment.CurrentDirectory + $"\\{UppercaseFirst(commandName)}.cs", final);
                Console.WriteLine("Made " + commandName);
            }
            Console.WriteLine("Done!");
            //Console.WriteLine(FinalContent);
            Console.ReadLine();
        }

        static string UppercaseFirst(string s)
        {
            // Check for empty string.
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            // Return char and concat substring.
            return char.ToUpper(s[0]) + s.Substring(1);
        }
    }
}
