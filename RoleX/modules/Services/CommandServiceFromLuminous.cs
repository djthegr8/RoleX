using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using static Public_Bot.CustomCommandService;

namespace Public_Bot
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
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class GuildPermissions : Attribute
    {
        public GuildPermission[] Permissions { get; set; }

        public GuildPermissions(params GuildPermission[] perms)
            => this.Permissions = perms;
    }
    /// <summary>
    /// Discord command class
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class DiscordCommand : Attribute
    {
        /// <summary>
        /// If <see langword="true"/> then <see cref="Settings.HasPermissionMethod"/> will be called and checked if the user has permission to execute the command, this result will be in the <see cref="CommandModuleBase"/>
        /// </summary>
        public bool RequiredPermission { get; set; }
        /// <summary>
        /// Name of the command
        /// </summary>
        internal string commandName;
        /// <summary>
        /// The prefix for the command. This is optional, the command will work with the default prefix you passed or an overwrited one from the <see cref="DiscordCommandClass"/>
        /// </summary>
        public char[] prefixes { get; set; }
        /// <summary>
        /// Description of this command
        /// </summary>
        public string description { get; set; }
        /// <summary>
        /// Command help message, use this to create a generic help message
        /// </summary>
        public string commandHelp { get; set; }
        /// <summary>
        /// An example of the commands' usage
        /// </summary>
        public string example {get; set;}
        /// <summary>
        /// If <see langword="true"/> then bots can execute the command, default is <see langword="false"/>
        /// </summary>
        public bool BotCanExecute { get; set; }
        /// <summary>
        /// Tells the service that this method is a command
        /// </summary>
        /// <param name="commandName">the name of the command</param>
        public DiscordCommand(string commandName)
        {
            this.commandName = commandName;
            prefixes = new char[] { };
            BotCanExecute = false;
        }
        /// <summary>
        /// Tells the service that this method is a command
        /// </summary>
        /// <param name="commandName">the name of the command</param>
        /// <param name="Prefix">A prefix to overwrite the default one</param>
        public DiscordCommand(string commandName, char Prefix)
        {
            this.commandName = commandName;
            prefixes = new char[] { Prefix };
            BotCanExecute = false;
        }
        /// <summary>
        /// Tells the service that this method is a command
        /// </summary>
        /// <param name="commandName">the name of the command</param>
        /// <param name="Prefixes">The Prefix(es) of the command, if this is empty it will use the default prefix</param>
        public DiscordCommand(string commandName, params char[] Prefixes)
        {
            this.commandName = commandName;
            if (Prefixes.Length > 0)
                prefixes = Prefixes;
            else
                prefixes = new char[] { };
            BotCanExecute = false;
        }
    }
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class Alt : Attribute
    {
        public string alt { get; set; }
        public Alt(string Alt)
        {
            this.alt = Alt;
        }
    }
    /// <summary>
    /// The settings for the Command Service
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// The Default prefix for the command service
        /// </summary>
        public char DefaultPrefix { get; set; }
        /// <summary>
        /// This method will be called and when a command is called and checkes if the user has permission to execute the command, this result will be in the <see cref="CommandModuleBase.HasExecutePermission"/> if you dont set this, the <see cref="CommandModuleBase.HasExecutePermission"/> will always be <see langword="true"/>
        /// </summary>
        public Func<SocketCommandContext, bool> HasPermissionMethod { get; set; }
        /// <summary>
        /// A Dictionary containing specific permission methods for guilds, The Key would be the Guilds ID, and the value would be a Method that takes <see cref="SocketCommandContext"/> and returns a <see cref="bool"/>. this will overwrite the <see cref="HasPermissionMethod"/> if the guilds permission method is added
        /// </summary>
        public Dictionary<ulong, Func<SocketCommandContext, bool>> CustomGuildPermissionMethod { get; set; }
        /// <summary>
        /// Boolean indicating if commands can be accessable in Direct messages, default value is <see cref="false"/>
        /// </summary>
        [DefaultValue(false)]
        public bool DMCommands { get; set; }
        /// <summary>
        /// If the user has invalid permissions to execute the command and this bool is <see langword="true"/> then the command will still execute with the <see cref="CommandModuleBase.HasExecutePermission"/> set to <see langword="false"/>. Default is <see langword="false"/>
        /// </summary>
        public bool AllowCommandExecutionOnInvalidPermissions { get; set; }
    }
    /// <summary>
    /// Status of the command
    /// </summary>
    public enum CommandStatus
    {
        /// <summary>
        /// The command executed successfully
        /// </summary>
        Success,
        /// <summary>
        /// There was an error with the execution, look at the exception in <see cref="CommandResult.Exception"/>
        /// </summary>
        Error,
        /// <summary>
        /// Could not find a command, if this is a mistake check if you have the <see cref="DiscordCommand"/> attribute attached to the command
        /// </summary>
        NotFound,
        /// <summary>
        /// The command was found but there was not enough parameters passed for the command to execute correctly
        /// </summary>
        NotEnoughParams,
        /// <summary>
        /// The parameters were there but they were unable to be parsed to the correct type
        /// </summary>
        InvalidParams,
        /// <summary>
        /// If the user has incorrect permissions to execute the command
        /// </summary>
        InvalidPermissions,
        Disabled,
        /// <summary>
        /// Somthing happend that shouldn't have, i dont know what to say here other than :/
        /// </summary>
        MissingGuildPermission,
        Unknown
    }
    /// <summary>
    /// The base class of <see cref="CustomCommandService"/>
    /// </summary>
    public class CustomCommandService
    {
        public static Dictionary<string, string> Modules { get; set; } = new Dictionary<string, string>();

        public static List<char> UsedPrefixes { get; set; }

        public bool ContainsUsedPrefix(string msg)
        {
            return UsedPrefixes.Any(x => msg.StartsWith(x));
        }

        private List<Command> CommandList = new List<Command>();
        private class Command
        {
            public bool RequirePermission { get; set; }
            public string CommandName { get; set; }
            public List<string> alts { get; set; } = new List<string>();
            public char[] Prefixes { get; set; }
            public System.Reflection.ParameterInfo[] Paramaters { get; set; }
            public MethodInfo Method { get; set; }
            public DiscordCommand attribute { get; set; }
            public CommandClassobj parent { get; set; }

            public GuildPermissions perms { get; set; }
        }
        private class CommandClassobj
        {
            public char Prefix { get; set; }
            public DiscordCommandClass attribute { get; set; }
            public Type type { get; set; }
            public List<Command> ChildCommands { get; set; }
            public object ClassInstance { get; set; }

        }

        private static Settings currentSettings;
        private List<Type> CommandClasses { get; set; }

        /// <summary>
        /// Creates a new command service instance
        /// </summary>
        /// <param name="s">the <see cref="Settings"/> for the command service</param>
        public CustomCommandService(Settings s)
        {
            currentSettings = s;
            UsedPrefixes = new List<char>();
            UsedPrefixes.Add(s.DefaultPrefix);
            if (currentSettings.HasPermissionMethod == null)
                currentSettings.HasPermissionMethod = (SocketCommandContext s) => { return true; };
            if (s.CustomGuildPermissionMethod == null)
                currentSettings.CustomGuildPermissionMethod = new Dictionary<ulong, Func<SocketCommandContext, bool>>();
            CommandModuleBase.CommandDescriptions = new Dictionary<string, string>();
            CommandModuleBase.CommandHelps = new Dictionary<string, string>();
            CommandModuleBase.Commands = new List<ICommands>();
            Dictionary<MethodInfo, Type> CommandMethods = new Dictionary<MethodInfo, Type>();
            var types = Assembly.GetEntryAssembly().GetTypes();
            var CommandClasses = types.Where(x => x.CustomAttributes.Any(x => x.AttributeType == typeof(DiscordCommandClass)));
            foreach (var item in CommandClasses)
            {
                var att = item.GetCustomAttribute<DiscordCommandClass>();
                Modules.Add(att.ModuleName, att.ModuleDescription);
            }
            var Commandmethods = types.SelectMany(x => x.GetMethods().Where(y => y.GetCustomAttributes(typeof(DiscordCommand), false).Length > 0).ToArray());
            foreach (var t in Commandmethods)
            {
                CommandMethods.Add(t, t.DeclaringType);
            }
            //add to base command list
            //UsedPrefixes = new List<char>();
            foreach (var item in CommandMethods)
            {
                var cmdat = item.Key.GetCustomAttribute<DiscordCommand>();
                var parat = item.Value.GetCustomAttribute<DiscordCommandClass>();
                if (cmdat.commandHelp != null)
                    if (!CommandModuleBase.CommandHelps.ContainsKey(cmdat.commandName))
                    {
                        CommandModuleBase.CommandHelps.Add(cmdat.commandName, cmdat.commandHelp);
                    }
                    else
                    {
                        CommandModuleBase.CommandHelps[cmdat.commandName] += "\n" + cmdat.commandHelp;
                    }
                if (cmdat.description != null)
                    if (!CommandModuleBase.CommandDescriptions.ContainsKey(cmdat.commandName))
                    {
                        CommandModuleBase.CommandDescriptions.Add(cmdat.commandName, cmdat.description);
                    }
                    else
                    {
                        CommandModuleBase.CommandDescriptions[cmdat.commandName] += "\n" + cmdat.description;
                    }

                var alts = item.Key.GetCustomAttributes<Alt>();
                List<string> altsL = new List<string>();
                if (alts.Count() != 0)
                {
                    foreach (var alt in alts)
                    {
                        altsL.Add(alt.alt);
                    }
                }
                Command cmdobj = new Command()
                {
                    CommandName = cmdat.commandName,
                    Method = item.Key,
                    Prefixes = cmdat.prefixes.Length == 0 ? new char[] { currentSettings.DefaultPrefix } : cmdat.prefixes,
                    attribute = cmdat,
                    alts = altsL,
                    parent = new CommandClassobj()
                    {
                        Prefix = parat == null ? currentSettings.DefaultPrefix : parat.prefix,
                        ClassInstance = Activator.CreateInstance(item.Value),
                        attribute = parat,
                    },
                    Paramaters = item.Key.GetParameters(),
                    RequirePermission = cmdat.RequiredPermission,
                    perms = item.Key.CustomAttributes.Any(x => x.AttributeType == typeof(GuildPermissions))
                          ? item.Key.GetCustomAttribute<GuildPermissions>()
                          : null,
                };
                CommandList.Add(cmdobj);

                var c = new Commands
                {
                    CommandName = cmdat.commandName,
                    CommandDescription = cmdat.description,
                    CommandHelpMessage = cmdat.commandHelp,
                    example = cmdat.example,
                    Prefixes = parat.prefix == '\0' ? cmdobj.Prefixes : cmdobj.Prefixes.Append(parat.prefix).ToArray(),
                    RequiresPermission = cmdat.RequiredPermission,
                    ModuleName = cmdobj.parent.attribute.ModuleName,
                };
                c.Alts = altsL;
                CommandModuleBase.Commands.Add(c);

                foreach (var pr in cmdat.prefixes)
                    if (!UsedPrefixes.Contains(pr))
                        UsedPrefixes.Add(pr);
                if (!UsedPrefixes.Contains(parat.prefix) && parat.prefix != '\0')
                    UsedPrefixes.Add(parat.prefix);
            }
        }

        /// <summary>
        /// The command result object
        /// </summary>
        public class CommandResult : ICommandResult
        {
            public bool IsSuccess { get; set; }

            [DefaultValue(false)]
            public bool MultipleResults { get; set; }
            public string commandUsed { get; set; }
            public CommandStatus Result { get; set; }
            public ICommandResult[] Results { get; set; }
            public string ResultMessage { get; set; }
            public Exception Exception { get; set; }
        }
        /// <summary>
        /// The Command Result, this contains the <see cref="CommandStatus"/> enum and a <see cref="Exception"/> object if there was an error.
        /// </summary>
        public interface ICommandResult
        {
            /// <summary>
            /// <see langword="true"/> the execution of the command is successful
            /// </summary>
            bool IsSuccess { get; }
            /// <summary>
            /// The status of the command
            /// </summary>
            CommandStatus Result { get; }
            /// <summary>
            /// a <see cref="bool"/> determining if there was multiple results, if true look in <see cref="Results"/>
            /// </summary>
            bool MultipleResults { get; }
            /// <summary>
            /// The multi-Result Array
            /// </summary>
            ICommandResult[] Results { get; }
            string ResultMessage { get; }
            /// <summary>
            /// Exception if there was an error
            /// </summary>
            Exception Exception { get; }
        }
        /// <summary>
        /// This method will execute a command if there is one
        /// </summary>
        /// <param name="context">The current discord <see cref="ICommandContext"/></param>
        /// <returns>The <see cref="ICommandResult"/> containing what the status of the execution is </returns>
        public async Task<ICommandResult> ExecuteAsync(SocketCommandContext context, string pref)
        {
            bool IsMentionCommand = context.Message.Content.StartsWith($"<@{context.Client.CurrentUser.Id}>") ? true : context.Message.Content.StartsWith($"<@!{context.Client.CurrentUser.Id}>") ? true : false;
            string[] param = IsMentionCommand
                ? context.Message.Content.Replace($"<@{context.Client.CurrentUser.Id}>", string.Empty).Replace($"<@!{context.Client.CurrentUser.Id}>", "").Trim().Split(' ')
                : context.Message.Content.Split(' ');

            param = param.TakeLast(param.Length - 1).ToArray();
            string command = IsMentionCommand
                ? context.Message.Content.Replace($"<@{context.Client.CurrentUser.Id}>", string.Empty).Replace($"<@!{context.Client.CurrentUser.Id}>", "").Trim().Split(' ')[0]
                : context.Message.Content.Remove(0, pref.Length).Split(' ')[0];
            var commandobj = CommandList.Where(x => x.CommandName.ToLower() == command);
            var altob = CommandList.Where(x => x.alts.Any(x => x.ToLower() == command));
            if (commandobj.Count() == 0)
                commandobj = altob;
            if (commandobj.Count() == 0)
                return new CommandResult()
                {
                    Result = CommandStatus.NotFound,
                    IsSuccess = false
                };
            if (context.Channel.GetType() == typeof(SocketDMChannel) && !currentSettings.DMCommands)
                return new CommandResult() { IsSuccess = false, Result = CommandStatus.InvalidParams };
            List<CommandResult> results = new List<CommandResult>();
            //1 find if param counts match
            if (commandobj.Any(x => x.Paramaters.Length > 0))
                if (commandobj.Where(x => x.Paramaters.Length > 0).Any(x => x.Paramaters.Last().GetCustomAttributes(typeof(ParamArrayAttribute), false).Length == 0))
                    commandobj = commandobj.Where(x => x.Paramaters.Length == param.Length);
            if (commandobj.Count() == 0)
                return new CommandResult() { IsSuccess = false, Result = CommandStatus.InvalidParams };
            foreach (var cmd in commandobj)
            {
                results.Add(await ExecuteCommand(cmd, context, param));
            }
            if (results.Any(x => x.IsSuccess))
                return results.First(x => x.IsSuccess);
            if (results.Count == 1)
                return results[0];
            else
                return new CommandResult() { Results = results.ToArray(), MultipleResults = true, IsSuccess = false };
        }
        private async Task<CommandResult> ExecuteCommand(Command cmd, SocketCommandContext context, string[] param)
        {
            if (!(context.User as SocketGuildUser).GuildPermissions.Administrator)
            {
                if (cmd.perms != null)
                {
                    foreach (var p in cmd.perms.Permissions)
                    {
                        if (!(context.User as SocketGuildUser).GuildPermissions.Has(p))
                        {
                            return new CommandResult()
                            {
                                Result = CommandStatus.MissingGuildPermission,
                                ResultMessage = $"" +
                                                $"```\n" +
                                                $"{string.Join('\n', cmd.perms.Permissions.Where(x => !(context.User as SocketGuildUser).GuildPermissions.Has(x)).Select(x => x.ToString()))}" +
                                                $"```"
                            };
                        }
                    }
                }
            }
            if (!cmd.attribute.BotCanExecute && context.Message.Author.IsBot)
                return new CommandResult() { Result = CommandStatus.InvalidPermissions };
            if (cmd.Paramaters.Length == 0 && param.Length == 0)
            {
                try
                {
                    var u = context.Guild.GetUser(context.Message.Author.Id);
                    CommandModuleBase.HasExecutePermission = true;

                    cmd.parent.ClassInstance.GetType().GetProperty("Context").SetValue(cmd.parent.ClassInstance, context);

                    var d = (Func<Task>)Delegate.CreateDelegate(typeof(Func<Task>), cmd.parent.ClassInstance, cmd.Method);
                    await d();
                    //cmd.Method.Invoke(cmd.parent.ClassInstance, null);
                    return new CommandResult() { Result = CommandStatus.Success, IsSuccess = true };
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return new CommandResult() { Exception = ex, Result = CommandStatus.Error, IsSuccess = false };
                }
            }
            else if (cmd.Paramaters.Length == 0 && param.Length > 0)
                return new CommandResult() { Result = CommandStatus.InvalidParams, IsSuccess = false };
            if (cmd.Paramaters.Last().GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 0)
            {
                List<object> parsedparams = new List<object>();
                bool check = true;
                for (int i = 0; i != cmd.Paramaters.Length; i++)
                {
                    var dp = cmd.Paramaters[i];
                    if (dp.GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 0)
                    {
                        string[] arr = param.Skip(i).Where(x => x != "").ToArray();
                        if (arr.Length == 0)
                        {
                            ArrayList al = new ArrayList();
                            Type destyp = dp.ParameterType.GetElementType();
                            parsedparams.Add(al.ToArray(destyp));
                        }
                        else
                        {
                            ArrayList al = new ArrayList();
                            Type destyp = dp.ParameterType.GetElementType();
                            foreach (var item in arr)
                            {
                                if (TypeDescriptor.GetConverter(destyp).IsValid(item))
                                {
                                    //we can
                                    var pparam = TypeDescriptor.GetConverter(destyp).ConvertFromString(item);
                                    al.Add(pparam);
                                }
                                else
                                    check = false;
                            }
                            if (check)
                                parsedparams.Add(al.ToArray(destyp));
                        }
                    }
                    else
                    {
                        if (param.Length < cmd.Paramaters.Length - 1)
                            return new CommandResult() { Result = CommandStatus.InvalidParams, IsSuccess = false };
                        var p = param[i];
                        if (TypeDescriptor.GetConverter(dp.ParameterType).IsValid(p))
                        {
                            //we can
                            var pparam = TypeDescriptor.GetConverter(dp.ParameterType).ConvertFromString(p);
                            parsedparams.Add(pparam);
                        }
                        else
                            check = false;
                    }
                }
                //if it worked
                if (check)
                {
                    //invoke the method
                    try
                    {
                        var u = context.Guild.GetUser(context.Message.Author.Id);
                        CommandModuleBase.HasExecutePermission = true;
                        if (!currentSettings.AllowCommandExecutionOnInvalidPermissions && !CommandModuleBase.HasExecutePermission)
                            return new CommandResult() { IsSuccess = false, Result = CommandStatus.InvalidPermissions };

                        cmd.parent.ClassInstance.GetType().GetProperty("Context").SetValue(cmd.parent.ClassInstance, context);

                        Task s = (Task)cmd.Method.Invoke(cmd.parent.ClassInstance, parsedparams.ToArray());
                        Task.Run(async () => await s).Wait();
                        if (s.Exception == null)
                            return new CommandResult() { Result = CommandStatus.Success, IsSuccess = true };
                        else
                            return new CommandResult() { Exception = s.Exception.InnerException, Result = CommandStatus.Error, IsSuccess = false };

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        return new CommandResult() { Exception = ex, Result = CommandStatus.Error, IsSuccess = false };
                    }

                }
                else
                    return new CommandResult() { Result = CommandStatus.InvalidParams, IsSuccess = false };
            }
            if (cmd.Paramaters.Length == param.Length)
            {
                List<object> parsedparams = new List<object>();
                bool check = true;

                //right command lengths met or params object in it now we gotta parse the params
                for (int i = 0; i != param.Length; i++)
                {
                    //PrimitiveParsers.Get(unparsedparam)
                    var p = param[i];
                    var dp = cmd.Paramaters[i];
                    //check if we can parse the value
                    if (TypeDescriptor.GetConverter(dp.ParameterType).IsValid(p))
                    {
                        //we can
                        var pparam = TypeDescriptor.GetConverter(dp.ParameterType).ConvertFromString(p);
                        parsedparams.Add(pparam);
                    }
                    else
                        check = false;
                }
                //if we parsed all the params correctly
                if (check)
                {
                    //invoke the method
                    try
                    {
                        var u = context.Guild.GetUser(context.Message.Author.Id);
                        CommandModuleBase.HasExecutePermission = true;
                        if (!currentSettings.AllowCommandExecutionOnInvalidPermissions && !CommandModuleBase.HasExecutePermission)
                            return new CommandResult() { IsSuccess = false, Result = CommandStatus.InvalidPermissions };

                        cmd.parent.ClassInstance.GetType().GetProperty("Context").SetValue(cmd.parent.ClassInstance, context);

                        Task s = (Task)cmd.Method.Invoke(cmd.parent.ClassInstance, parsedparams.ToArray());
                        Task.Run(async () => await s).Wait();
                        if (s.Exception == null)
                            return new CommandResult() { Result = CommandStatus.Success, IsSuccess = true };
                        else
                            return new CommandResult() { Exception = s.Exception.InnerException, Result = CommandStatus.Error, IsSuccess = false };

                    }
                    catch (TargetInvocationException ex)
                    {
                        Console.WriteLine(ex);
                        return new CommandResult() { Exception = ex, Result = CommandStatus.Error, IsSuccess = false };
                    }

                }
                else
                    return new CommandResult() { Result = CommandStatus.InvalidParams, IsSuccess = false };

            }
            else
                return new CommandResult() { Result = CommandStatus.NotEnoughParams, IsSuccess = false };
        }
        public class Commands : ICommands
        {
            public string CommandName { get; set; }
            public string CommandDescription { get; set; }
            public string CommandHelpMessage { get; set; }
            public bool RequiresPermission { get; set; }
            public string example { get; set; }
            public char[] Prefixes { get; set; }
            public string ModuleName { get; set; }
            public List<string> Alts { get; set; } = new List<string>();
            public bool HasName(string name)
            {
                if (CommandName == name)
                    return true;
                else if (Alts.Contains(name))
                    return true;
                else
                    return false;
            }
        }
    }

    /// <summary>
    /// The Class to interface from.
    /// </summary>
    public class CommandModuleBase
    {
        public static Color Blurple = new Color(114, 137, 218);
        /// <summary>
        /// If the user has execute permission based on the <see cref="CustomCommandService.Settings.HasPermissionMethod"/>
        /// </summary>
        public static bool HasExecutePermission { get; set; }
        /// <summary>
        /// The Context of the current command
        /// </summary>
        public SocketCommandContext Context { get; internal set; }

        /// <summary>
        /// Contains all the help messages. Key is the command name, Value is the help message
        /// </summary>
        public static Dictionary<string, string> CommandHelps { get; internal set; }

        /// <summary>
        /// Contains all the help messages. Key is the command name, Value is the Command Description
        /// </summary>
        public static Dictionary<string, string> CommandDescriptions { get; internal set; }
        /// <summary>
        /// The superlist with all the commands
        /// </summary>
        public static List<ICommands> Commands { get; internal set; }
        public static List<ICommands> ReadCurrentCommands(string prefix)
        {
            List<ICommands> cmds = new List<ICommands>();
            foreach (var cmd in Commands)
            {
                var c = new Commands
                {
                    CommandName = cmd.CommandName,
                    CommandDescription = cmd.CommandDescription == null ? null : cmd.CommandDescription.Replace("(PREFIX)", prefix),
                    CommandHelpMessage = cmd.CommandHelpMessage == null ? null : cmd.CommandHelpMessage.Replace("(PREFIX)", prefix),
                    Prefixes = cmd.Prefixes,
                    RequiresPermission = cmd.RequiresPermission,
                    ModuleName = cmd.ModuleName,
                    Alts = cmd.Alts
                };
                cmds.Add(c);
            }
            return cmds;
        }
        public SocketGuildChannel GetChannel(string name)
        {
            var regex = new Regex(@"(\d{18}|\d{17})");
            if (regex.IsMatch(name))
            {
                var u = Context.Guild.GetChannel(ulong.Parse(regex.Match(name).Groups[1].Value));
                return u;
            }
            if (ulong.TryParse(name, out var res))
                return Context.Guild.Channels.Any(x => x.Id == res) ? Context.Guild.Channels.First(x => x.Id == res) : null;
            else
                return Context.Guild.Channels.Any(x => x.Name.ToLower().StartsWith(name.ToLower())) ? Context.Guild.Channels.First(x => x.Name.ToLower().StartsWith(name.ToLower())) : null;


        }
        public SocketGuildUser GetUser(string user)
        {
                var regex = new Regex(@"(\d{18}|\d{17})");
                if (regex.IsMatch(user))
                {
                    var u = Context.Guild.GetUser(ulong.Parse(regex.Match(user).Groups[1].Value));
                    return u;
                }
                else
                {
                    if (Context.Guild.Users.Any(x => x.Username.StartsWith(user)))
                    {
                        return Context.Guild.Users.First(x => x.Username.StartsWith(user));
                    }
                    else if (Context.Guild.Users.Any(x => x.ToString().StartsWith(user)))
                    {
                        return Context.Guild.Users.First(x => x.ToString().StartsWith(user));
                    }
                    else if (Context.Guild.Users.Any(x => x.Nickname != null && x.Nickname.StartsWith(user)))
                    {
                        return Context.Guild.Users.First(x => x.Nickname != null && x.Nickname.StartsWith(user));
                    }
                    else
                        return null;
                }
        }
        public async Task<IUser> GetBannedUser(string uname)
        {
            var alr = await Context.Guild.GetBansAsync();
            var regex = new Regex(@"(\d{18}|\d{17})");
            if (regex.IsMatch(uname))
            {
                return alr.First(aa => aa.User.Id == ulong.Parse(uname)).User;
            }
            return alr.First(x => x.User.Username.ToLower().Contains(uname.ToLower())).User;
        }
        public OverwritePermissions GetOP(ChannelPermission cp, PermValue pv)
        {
            var eop = new OverwritePermissions();
            var x = cp switch
            {
                ChannelPermission.AddReactions => eop.Modify(addReactions: pv),
                ChannelPermission.AttachFiles => eop.Modify(attachFiles: pv),
                ChannelPermission.Connect => eop.Modify(connect: pv),
                ChannelPermission.CreateInstantInvite => eop.Modify(createInstantInvite: pv),
                ChannelPermission.DeafenMembers => eop.Modify(deafenMembers: pv),
                ChannelPermission.EmbedLinks => eop.Modify(embedLinks: pv),
                ChannelPermission.ManageChannels => eop.Modify(manageChannel: pv),
                ChannelPermission.ManageMessages => eop.Modify(manageMessages: pv),
                ChannelPermission.ManageRoles => eop.Modify(manageRoles: pv),
                ChannelPermission.ManageWebhooks => eop.Modify(manageWebhooks: pv),
                ChannelPermission.MentionEveryone => eop.Modify(mentionEveryone: pv),
                ChannelPermission.MoveMembers => eop.Modify(moveMembers: pv),
                ChannelPermission.MuteMembers => eop.Modify(muteMembers: pv),
                ChannelPermission.ReadMessageHistory => eop.Modify(readMessageHistory: pv),
                ChannelPermission.ReadMessages or ChannelPermission.ViewChannel => eop.Modify(viewChannel: pv),
                ChannelPermission.SendMessages => eop.Modify(sendMessages: pv),
                ChannelPermission.SendTTSMessages => eop.Modify(sendTTSMessages: pv),
                ChannelPermission.Speak => eop.Modify(speak: pv),
                ChannelPermission.UseExternalEmojis => eop.Modify(useExternalEmojis: pv),
                ChannelPermission.UseVAD => eop.Modify(useVoiceActivation: pv)
            };
            return x;
        }
        public Discord.GuildPermissions EditPerm(SocketRole roleA, GuildPermission perm, bool add = true)
        {
            Console.WriteLine(roleA.Name);
            Console.WriteLine(perm);
            Console.WriteLine(add);
            var gp = perm switch
            {
                GuildPermission.AddReactions => roleA.Permissions.Modify(addReactions: add),
                GuildPermission.Administrator => roleA.Permissions.Modify(administrator: add),
                 GuildPermission.AttachFiles=>
                    roleA.Permissions.Modify(attachFiles: add)
                    ,
                 GuildPermission.BanMembers=>
                    roleA.Permissions.Modify(banMembers: add)
                    ,
                 GuildPermission.ChangeNickname=>
                    roleA.Permissions.Modify(changeNickname: add)
                    ,
                 GuildPermission.Connect=>
                    roleA.Permissions.Modify(connect: add)
                    ,
                 GuildPermission.CreateInstantInvite=>
                    roleA.Permissions.Modify(createInstantInvite: add)
                    ,
                 GuildPermission.DeafenMembers=>
                    roleA.Permissions.Modify(deafenMembers: add)
                    ,
                 GuildPermission.EmbedLinks=>
                    roleA.Permissions.Modify(embedLinks: add)
                    ,
                 GuildPermission.KickMembers=>
                    roleA.Permissions.Modify(kickMembers: add)
                    ,
                 GuildPermission.ManageChannels=>
                    roleA.Permissions.Modify(manageChannels: add)
                    ,
                 GuildPermission.ManageEmojis=>
                    roleA.Permissions.Modify(manageEmojis: add)
                    ,
                 GuildPermission.ManageGuild=>
                    roleA.Permissions.Modify(manageGuild: add)
                    ,
                 GuildPermission.ManageMessages=>
                    roleA.Permissions.Modify(manageMessages: add)
                    ,
                 GuildPermission.ManageNicknames=>
                    roleA.Permissions.Modify(manageNicknames: add)
                    ,
                 GuildPermission.ManageRoles=>
                    roleA.Permissions.Modify(manageRoles: add)
                    ,
                 GuildPermission.ManageWebhooks=>
                    roleA.Permissions.Modify(manageWebhooks: add)
                    ,
                 GuildPermission.MentionEveryone=>
                    roleA.Permissions.Modify(mentionEveryone: add)
                    ,
                 GuildPermission.MoveMembers=>
                    roleA.Permissions.Modify(moveMembers: add)
                    ,
                 GuildPermission.MuteMembers=>
                    roleA.Permissions.Modify(muteMembers: add)
                    ,
                 GuildPermission.PrioritySpeaker=>
                    roleA.Permissions.Modify(prioritySpeaker: add)
                    ,
                 GuildPermission.ReadMessageHistory=>
                    roleA.Permissions.Modify(readMessageHistory: add)
                    ,
                 GuildPermission.ReadMessages or GuildPermission.ViewChannel=>
                    roleA.Permissions.Modify(viewChannel: add)
                    ,
                 GuildPermission.SendMessages=>
                    roleA.Permissions.Modify(sendMessages: add)
                    ,
                 GuildPermission.SendTTSMessages=>
                    roleA.Permissions.Modify(sendTTSMessages: add)
                    ,
                 GuildPermission.Speak=>
                    roleA.Permissions.Modify(speak: add)
                    ,
                 GuildPermission.Stream=>
                    roleA.Permissions.Modify(stream: add)
                    ,
                 GuildPermission.UseExternalEmojis=>
                    roleA.Permissions.Modify(useExternalEmojis: add)
                    ,
                 GuildPermission.UseVAD=>
                    roleA.Permissions.Modify(useVoiceActivation: add)
                    ,
                 GuildPermission.ViewAuditLog=>
                    roleA.Permissions.Modify(viewAuditLog: add),
                    
            };
            return gp;
        }
        public Tuple<GuildPermission,bool> GetPermission(string perm)
        {
            if (Enum.TryParse(perm, true, out GuildPermission Gp)){
                return new Tuple<GuildPermission, bool>(Gp, true);
            }
            else return new Tuple<GuildPermission, bool>(GuildPermission.AddReactions, false);
        }
        public Tuple<ChannelPermission, bool> GetChannelPermission (string perm)
        {
            if ( Enum.TryParse(perm, true, out ChannelPermission Gp))
            {
                return new Tuple<ChannelPermission, bool>(Gp, true);
            }
            else return new Tuple<ChannelPermission, bool>(ChannelPermission.AddReactions, false);
        }
        public SocketRole GetRole(string role)
        {
            var regex = new Regex(@"(\d{18}|\d{17})");
            if (regex.IsMatch(role))
            {
                var u = Context.Guild.GetRole(ulong.Parse(regex.Match(role).Groups[1].Value));
                return u;
            }
            else
                if (Context.Guild.Roles.Any(x => x.Name.StartsWith(role)))
                return Context.Guild.Roles.First(x => x.Name.StartsWith(role));
            else
                    if (Context.Guild.Roles.Any(x => x.Name.StartsWith(role)))
                return Context.Guild.Roles.First(x => x.Name.StartsWith(role));
            else
                return null;
        }
        protected virtual async Task<IUserMessage> ReplyAsync(string message = null, bool isTTS = false, Embed embed = null, RequestOptions options = null)
        {
            return await Context.Channel.SendMessageAsync(message, isTTS, embed, options).ConfigureAwait(false);
        }

    }
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