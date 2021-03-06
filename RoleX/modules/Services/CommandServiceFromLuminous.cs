using Discord;
using Discord.Commands;
using Discord.WebSocket;
using RoleX.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static MoreLinq.Extensions.BatchExtension;
using static RoleX.Modules.Services.CustomCommandService;

namespace RoleX.Modules.Services
{
    public enum Punishment
    {
        Ban,
        Mute,
        HardMute,
        Softban,
        Kick,
        Unban
    }
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
    public class RequiredUserPermissions : Attribute
    {
        public GuildPermission[] Permissions { get; set; }

        public RequiredUserPermissions(params GuildPermission[] perms)
            => this.Permissions = perms;
    }
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class RequiredBotPermission : Attribute
    {
        public Discord.GuildPermission[] Permissions { get; set; }
        public RequiredBotPermission(params Discord.GuildPermission[] perms)
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
        public string example { get; set; }
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
            prefixes = Array.Empty<char>();
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
                prefixes = Array.Empty<char>();
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
        Unknown,
        BotMissingPermissions
    }
    /// <summary>
    /// The base class of <see cref="CustomCommandService"/>
    /// </summary>
    public class CustomCommandService
    {
        public static Dictionary<string, string> Modules { get; set; } = new Dictionary<string, string>();

        public static List<char> UsedPrefixes { get; set; }

        public static bool ContainsUsedPrefix(string msg)
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

            public RequiredUserPermissions perms { get; set; }
            public RequiredBotPermission bperms { get; set; }
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
            UsedPrefixes = new List<char>
            {
                s.DefaultPrefix
            };
            if (currentSettings.HasPermissionMethod == null)
                currentSettings.HasPermissionMethod = (SocketCommandContext s) => { return true; };
            if (s.CustomGuildPermissionMethod == null)
                currentSettings.CustomGuildPermissionMethod = new Dictionary<ulong, Func<SocketCommandContext, bool>>();
            CommandModuleBase.CommandDescriptions = new Dictionary<string, string>();
            CommandModuleBase.CommandHelps = new Dictionary<string, string>();
            CommandModuleBase.Commands = new List<Commands>();
            Dictionary<MethodInfo, Type> CommandMethods = new Dictionary<MethodInfo, Type>();
            var types = Assembly.GetEntryAssembly().GetTypes();
            var CommandClasses = types.Where(x => x.CustomAttributes.Any(x => x.AttributeType == typeof(DiscordCommandClass)));
            foreach (var item in CommandClasses)
            {
                var att = item.GetCustomAttribute<DiscordCommandClass>();

                if (!Modules.ContainsKey(att.ModuleName))
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
                if (alts.Any())
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
                    perms = item.Key.CustomAttributes.Any(x => x.AttributeType == typeof(RequiredUserPermissions))
                          ? item.Key.GetCustomAttribute<RequiredUserPermissions>()
                          : null,
                    bperms = item.Key.CustomAttributes.Any(x => x.AttributeType == typeof(RequiredBotPermission))
                          ? item.Key.GetCustomAttribute<RequiredBotPermission>()
                          : null,
                };
                CommandList.Add(cmdobj);

                var c = new Commands
                {
                    CommandName = cmdat.commandName,
                    CommandDescription = cmdat.description,
                    CommandHelpMessage = cmdat.commandHelp,
                    RequireUsrPerm = item.Key.CustomAttributes.Any(x => x.AttributeType == typeof(RequiredUserPermissions))
                          ? item.Key.GetCustomAttribute<RequiredUserPermissions>().Permissions
                          : null,
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
            bool IsMentionCommand = false;
            var msgcontent = context.Message.Content;
            if (!msgcontent.Split(' ')[0].Contains("alias"))
            {
                var alss = await SqliteClass.GuildAliasGetter(context.Guild.Id);
                foreach (var (aliasName, aliasContent) in alss)
                {
                    msgcontent = msgcontent.Replace(aliasName, aliasContent);
                }
            }
            string[] param = msgcontent.Split(' ');
            param = param.TakeLast(param.Length - 1).ToArray();
            string command = msgcontent.Remove(0, pref.Length).Split(' ')[0];
            var commandobj = CommandList.Where(x => x.CommandName.ToLower() == command);
            var altob = CommandList.Where(x => x.alts.Any(x => x.ToLower() == command));
            if (!commandobj.Any())
                commandobj = altob;
            if (!commandobj.Any())
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
            if (!commandobj.Any())
                return new CommandResult() { IsSuccess = false, Result = CommandStatus.InvalidParams };
            var idegk = param.ToList();
            try
            {
                // Add more flags here
                if (idegk.IndexOf("-q") != -1)
                {
                    idegk.RemoveAt(idegk.LastIndexOf("-q"));
                    await context.Message.DeleteAsync();
                }
            }
            catch
            { // ignore
            }
            param = idegk.ToArray();
            foreach (var cmd in commandobj)
            {
                results.Add(await ExecuteCommand(cmd, context, param));
            }
            if (results.Any(x => x.IsSuccess))
                return results.First(x => x.IsSuccess);
            if (results.Count == 1)
                return results[0];
            return new CommandResult() { Results = results.ToArray(), MultipleResults = true, IsSuccess = false };
        }
        private async Task<CommandResult> ExecuteCommand(Command cmd, SocketCommandContext context, string[] param)
        {
            if (!(context.User as SocketGuildUser).GuildPermissions.Administrator && context.User.Id != 701029647760097361 && context.User.Id != 615873008959225856)
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
            if (!(context.Guild.CurrentUser.GuildPermissions.Administrator))
            {
                if (cmd.bperms != null)
                {
                    foreach (var p in cmd.bperms.Permissions)
                    {
                        if (!(context.Guild.CurrentUser.GuildPermissions.Has(p)))
                        {
                            return new CommandResult()
                            {
                                Result = CommandStatus.BotMissingPermissions,
                                ResultMessage = Enum.GetName(typeof(GuildPermission), p)
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

            if (cmd.Paramaters.Length == 0 && param.Length > 0)
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
                        return new CommandResult() { Exception = s.Exception.InnerException, Result = CommandStatus.Error, IsSuccess = false };

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        return new CommandResult() { Exception = ex, Result = CommandStatus.Error, IsSuccess = false };
                    }

                }

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
                        return new CommandResult() { Exception = s.Exception.InnerException, Result = CommandStatus.Error, IsSuccess = false };

                    }
                    catch (TargetInvocationException ex)
                    {
                        Console.WriteLine(ex);
                        return new CommandResult() { Exception = ex, Result = CommandStatus.Error, IsSuccess = false };
                    }

                }

                return new CommandResult() { Result = CommandStatus.InvalidParams, IsSuccess = false };

            }

            return new CommandResult() { Result = CommandStatus.NotEnoughParams, IsSuccess = false };
        }
        public class Commands : ICommands
        {
            public string CommandName { get; set; }
            public string CommandDescription { get; set; }
            public string CommandHelpMessage { get; set; }
            public bool RequiresPermission { get; set; }
            public GuildPermission[] RequireUsrPerm { get; set; }
            public string example { get; set; }
            public char[] Prefixes { get; set; }
            public string ModuleName { get; set; }
            public List<string> Alts { get; set; } = new List<string>();
            public bool HasName(string name)
            {
                if (CommandName == name)
                    return true;
                if (Alts.Contains(name))
                    return true;
                return false;
            }
        }
    }

    /// <summary>
    /// The Class to interface from.
    /// </summary>
    public class CommandModuleBase
    {
        public readonly ulong[] devids = {
            701029647760097361,
            615873008959225856
        };
        /// <summary>
        /// Number of aliases allowed for non-premium users
        /// </summary>
        public static readonly ushort AllowedAliasesNonPremium = 20;
        /// <summary>
        /// Number of premium aliases allowed
        /// </summary>
        public static readonly ushort AllowedAliasesPremium = 50;
        public static readonly Color Blurple = new Color(114, 137, 218);
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
        public static List<Commands> Commands { get; internal set; }
        public static List<ICommands> ReadCurrentCommands(string prefix)
        {
            List<ICommands> cmds = new List<ICommands>();
            foreach (var cmd in Commands)
            {
                var c = new Commands
                {
                    CommandName = cmd.CommandName,
                    CommandDescription = cmd.CommandDescription?.Replace("(PREFIX)", prefix),
                    CommandHelpMessage = cmd.CommandHelpMessage?.Replace("(PREFIX)", prefix),
                    Prefixes = cmd.Prefixes,
                    RequiresPermission = cmd.RequiresPermission,
                    RequireUsrPerm = cmd.RequireUsrPerm,
                    ModuleName = cmd.ModuleName,
                    Alts = cmd.Alts
                };
                cmds.Add(c);
            }
            return cmds;
        }
        public async Task<GuildEmote> GetEmote(string str, SocketGuild Guild = null)
        {
            Guild ??= Context.Guild;
            var replstr = str.Replace("a:", "").Replace("<", "").Replace(">", "").Replace(":", "");
            Console.WriteLine(replstr);
            if (Guild.Emotes.Any(x => replstr.ToLower().StartsWith(x.Name.ToLower()))) return Guild.Emotes.First(x => replstr.ToLower().StartsWith(x.Name.ToLower()));
            Console.WriteLine(replstr);
            try
            {
                var resultString = ulong.Parse(Regex.Match(replstr, @"\d+").Value);

                if (resultString == 0 || await Context.Guild.GetEmoteAsync(resultString) == null)
                {
                    return null;
                }
                return await Guild.GetEmoteAsync(resultString);
            }
            catch { return null; }

        }
        public SocketGuildChannel GetChannel(string name)
        {
            var regex = new Regex(@"(\d{18}|\d{17})");
            if (regex.IsMatch(name))
            {
                var u = Context.Guild.GetChannel(ulong.Parse(regex.Match(name).Groups[1].Value));
                return u is SocketCategoryChannel ? null : u;
            }
            if (ulong.TryParse(name, out var res))
            {
                var x = Context.Guild.Channels.FirstOrDefault(x => x.Id == res);
                return x is SocketCategoryChannel ? null : x;
            }
            else
            {
                var x = Context.Guild.Channels.FirstOrDefault(x => x.Name.ToLower().StartsWith(name.ToLower()));
                return x is SocketCategoryChannel ? null : x;
            }


        }
        public SocketCategoryChannel GetCategory(string name)
        {
            var regex = new Regex(@"(\d{18}|\d{17})");
            if (regex.IsMatch(name))
            {
                var u = Context.Guild.GetCategoryChannel(ulong.Parse(regex.Match(name).Groups[1].Value));
                return u;
            }
            if (ulong.TryParse(name, out var res))
                return Context.Guild.CategoryChannels.FirstOrDefault(x => x.Id == res);
            return Context.Guild.CategoryChannels.FirstOrDefault(x => x.Name.ToLower().StartsWith(name.ToLower()));


        }
        public async Task<SocketGuildUser> GetUser(string user)
        {
            await Context.Guild.DownloadUsersAsync();
            var regex = new Regex(@"(\d{18}|\d{17})");
            if (regex.IsMatch(user))
            {
                var u = Context.Guild.GetUser(ulong.Parse(regex.Match(user).Groups[1].Value));
                return u;
            }

            user = user.ToLower();
            if (Context.Message.MentionedUsers.Any())
            {
                return Context.Message.MentionedUsers.First() as SocketGuildUser;
            }

            if (Context.Guild.Users.Any(x => x.Username.ToLower().StartsWith(user)))
            {
                return Context.Guild.Users.First(x => x.Username.ToLower().StartsWith(user));
            }
            if (Context.Guild.Users.Any(x => x.ToString().ToLower().StartsWith(user)))
            {
                return Context.Guild.Users.First(x => x.ToString().ToLower().StartsWith(user));
            }
            if (Context.Guild.Users.Any(x => x.Nickname != null && x.Nickname.ToLower().StartsWith(user)))
            {
                return Context.Guild.Users.First(x => x.Nickname != null && x.Nickname.ToLower().StartsWith(user));
            }
            return null;
        }
        public async Task<IUser> GetBannedUser(string uname)
        {
            var alr = await Context.Guild.GetBansAsync();
            var regex = new Regex(@"(\d{18}|\d{17})");
            if (regex.IsMatch(uname))
            {
                return alr.FirstOrDefault(aa => aa.User.Id == ulong.Parse(uname))?.User;
            }
            return alr.FirstOrDefault(x => x.User.Username.ToLower().Contains(uname.ToLower()))?.User;
        }
        public static OverwritePermissions GetOP(ChannelPermission cp, PermValue pv, OverwritePermissions eop)
        {
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
                ChannelPermission.UseVAD => eop.Modify(useVoiceActivation: pv),
                ChannelPermission.PrioritySpeaker => eop,
            };
            return x;
        }

        public static Discord.GuildPermissions EditPerm(SocketRole roleA, GuildPermission perm, bool add = true)
        {
            Console.WriteLine(roleA.Name);
            Console.WriteLine(perm);
            Console.WriteLine(add);
            var gp = perm switch
            {
                GuildPermission.AddReactions => roleA.Permissions.Modify(addReactions: add),
                GuildPermission.Administrator => roleA.Permissions.Modify(administrator: add),
                GuildPermission.AttachFiles =>
                   roleA.Permissions.Modify(attachFiles: add)
                    ,
                GuildPermission.BanMembers =>
                   roleA.Permissions.Modify(banMembers: add)
                    ,
                GuildPermission.ChangeNickname =>
                   roleA.Permissions.Modify(changeNickname: add)
                    ,
                GuildPermission.Connect =>
                   roleA.Permissions.Modify(connect: add)
                    ,
                GuildPermission.CreateInstantInvite =>
                   roleA.Permissions.Modify(createInstantInvite: add)
                    ,
                GuildPermission.DeafenMembers =>
                   roleA.Permissions.Modify(deafenMembers: add)
                    ,
                GuildPermission.EmbedLinks =>
                   roleA.Permissions.Modify(embedLinks: add)
                    ,
                GuildPermission.KickMembers =>
                   roleA.Permissions.Modify(kickMembers: add)
                    ,
                GuildPermission.ManageChannels =>
                   roleA.Permissions.Modify(manageChannels: add)
                    ,
                GuildPermission.ManageEmojis =>
                   roleA.Permissions.Modify(manageEmojis: add)
                    ,
                GuildPermission.ManageGuild =>
                   roleA.Permissions.Modify(manageGuild: add)
                    ,
                GuildPermission.ManageMessages =>
                   roleA.Permissions.Modify(manageMessages: add)
                    ,
                GuildPermission.ManageNicknames =>
                   roleA.Permissions.Modify(manageNicknames: add)
                    ,
                GuildPermission.ManageRoles =>
                   roleA.Permissions.Modify(manageRoles: add)
                    ,
                GuildPermission.ManageWebhooks =>
                   roleA.Permissions.Modify(manageWebhooks: add)
                    ,
                GuildPermission.MentionEveryone =>
                   roleA.Permissions.Modify(mentionEveryone: add)
                    ,
                GuildPermission.MoveMembers =>
                   roleA.Permissions.Modify(moveMembers: add)
                    ,
                GuildPermission.MuteMembers =>
                   roleA.Permissions.Modify(muteMembers: add)
                    ,
                GuildPermission.PrioritySpeaker =>
                   roleA.Permissions.Modify(prioritySpeaker: add)
                    ,
                GuildPermission.ReadMessageHistory =>
                   roleA.Permissions.Modify(readMessageHistory: add)
                    ,
                GuildPermission.ReadMessages or GuildPermission.ViewChannel =>
                   roleA.Permissions.Modify(viewChannel: add)
                    ,
                GuildPermission.SendMessages =>
                   roleA.Permissions.Modify(sendMessages: add)
                    ,
                GuildPermission.SendTTSMessages =>
                   roleA.Permissions.Modify(sendTTSMessages: add)
                    ,
                GuildPermission.Speak =>
                   roleA.Permissions.Modify(speak: add)
                    ,
                GuildPermission.Stream =>
                   roleA.Permissions.Modify(stream: add)
                    ,
                GuildPermission.UseExternalEmojis =>
                   roleA.Permissions.Modify(useExternalEmojis: add)
                    ,
                GuildPermission.UseVAD =>
                   roleA.Permissions.Modify(useVoiceActivation: add)
                    ,
                GuildPermission.ViewAuditLog =>
                   roleA.Permissions.Modify(viewAuditLog: add),

            };
            return gp;
        }
        public static Tuple<GuildPermission, bool> GetPermission(string perm)
        {
            perm = perm.Replace("admin", "administrator");
            if (Enum.TryParse(perm, true, out GuildPermission Gp))
            {
                return new Tuple<GuildPermission, bool>(Gp, true);
            }

            return new Tuple<GuildPermission, bool>(GuildPermission.AddReactions, false);
        }
        public static Tuple<ChannelPermission, bool> GetChannelPermission(string perm)
        {
            if (Enum.TryParse(perm, true, out ChannelPermission Gp))
            {
                return new Tuple<ChannelPermission, bool>(Gp, true);
            }

            return new Tuple<ChannelPermission, bool>(ChannelPermission.AddReactions, false);
        }
        public SocketRole GetRole(string role)
        {
            var regex = new Regex(@"(\d{18}|\d{17})");
            if (regex.IsMatch(role))
            {
                var u = Context.Guild.GetRole(ulong.Parse(regex.Match(role).Groups[1].Value));
                if (u != null && !u.IsEveryone) return u;
                return null;
            }

            if (Context.Guild.Roles.Any(x => !x.IsEveryone && x.Name.ToLower().StartsWith(role.ToLower())))
                return Context.Guild.Roles.First(x => x.Name.ToLower().StartsWith(role.ToLower()));
            if (Context.Guild.Roles.Any(x => !x.IsEveryone && x.Name.ToLower().StartsWith(role.ToLower())))
                return Context.Guild.Roles.First(x => x.Name.ToLower().StartsWith(role.ToLower()));
            return null;
        }
        /// <summary>
        /// Takes an Context, and sends a message to the channel it was sent in, while customizing the embed to fit parameters.
        /// </summary>
        /// <param name="message">The actual message</param>
        /// <param name="isTTS">Whether to speak the <paramref name="message"/> or not</param>
        /// <param name="embed">A <c>Discord.EmbedBuilder</c> for editing and making it work</param>
        /// <param name="options">Just a useless param to me ig</param>
        /// <returns></returns>
        protected virtual async Task<IUserMessage> ReplyAsync(string message = null, bool isTTS = false, EmbedBuilder embed = null, RequestOptions options = null)
        {
            var msgcontent = Context.Message.Content;
            if (!msgcontent.Split(' ')[0].Contains("alias"))
            {
                var alss = await SqliteClass.GuildAliasGetter(Context.Guild.Id);
                foreach (var (aliasName, aliasContent) in alss)
                {
                    msgcontent = msgcontent.Replace(aliasName, aliasContent);
                }
            }
            if (msgcontent.EndsWith("-q"))
            {
                return null;
            }
            if (message?.Length >= 2000)
            {
                const string filePath = "message.txt";
                await using (var sw = File.CreateText(filePath))
                {
                    await sw.WriteLineAsync(message);
                }
                await Context.Channel.SendFileAsync(filePath);
                message = null;
            }
            if (message == null && embed == null)
            {
                return null;
            }
            // Embed editing time!
            if (embed?.Description?.Length >= EmbedBuilder.MaxDescriptionLength ||
                embed?.Title?.Length >= EmbedBuilder.MaxTitleLength ||
                embed?.Fields?.Count >= EmbedBuilder.MaxFieldCount ||
                embed?.Length >= EmbedBuilder.MaxEmbedLength
                )
            {
                //Require some editing eh...
                if (embed?.Description?.Length >= EmbedBuilder.MaxDescriptionLength)
                {
                    //devids.Select(async x => await (await GetUser(x.ToString())).SendMessageAsync($"yet another too long description. ```{Context.Message.Content}```"));
                    const int chunkSize = 2000;
                    var chunks = embed.Description.Batch(chunkSize).Select(r => new string(r.ToArray()));
                    IUserMessage xyz = null;
                    try
                    {
                        xyz = await Context.Channel.SendMessageAsync(message, isTTS);
                    }
                    catch
                    {
                        // ignore
                    }
                    foreach (var chunk in chunks)
                    {
                        xyz = await Context.Channel.SendMessageAsync("", false, embed.WithDescription(chunk).Build());
                        await Task.Delay(500);
                    }
                    return xyz;
                }

                if (embed?.Title?.Length >= EmbedBuilder.MaxTitleLength)
                {
                    return await Context.Channel.SendMessageAsync(embed: embed.WithTitle(embed.Title.Substring(0, EmbedBuilder.MaxTitleLength - 5) + "...").Build());
                }
                if (embed?.Fields?.Count >= 6)
                {
                    var pM = new PaginatedMessage(PaginatedAppearanceOptions.Default, Context.Channel);
                    var lofb = embed.Fields;
                    pM.SetPages(embed.Description, lofb, 5);
                    await pM.Resend();
                }
                else if (embed?.Length >= EmbedBuilder.MaxEmbedLength)
                {
                    IUserMessage xyz = null;
                    try
                    {
                        xyz = await Context.Channel.SendMessageAsync(message, isTTS);
                    }
                    catch { }
                    var batches = embed.Fields.Batch(10);
                    foreach (var batch in batches)
                    {
                        embed.Fields = batch.ToList();
                        xyz = await Context.Channel.SendMessageAsync(message, isTTS, embed.Build());
                        await Task.Delay(500);

                    }
                    return xyz;
                }
            }
            var here = await Context.Channel.SendMessageAsync(message, isTTS, embed?.Build(), options).ConfigureAwait(false);
            var ranjom = new Random();
            var irdk = ranjom.Next(8);
            if (irdk == 1 && !await TopGG.HasVoted(Context.User.Id))
            {
                var idk = ranjom.Next(2);
                if (idk == 1 || (await RoleX.Program.CL2.GetGuildsAsync()).Any(x => x.Id == 591660163229024287 && x.GetUserAsync(Context.User.Id) != null)) await Context.Channel.SendMessageAsync("", false, new EmbedBuilder { Title = "Vote for RoleX!", Url = "https://tiny.cc/rolexdsl", Description = "Support RoleX by [voting](http:/tiny.cc/rolexdsl) for it in top.gg!", ImageUrl = "https://media.discordapp.net/attachments/745266816179241050/808311320373624832/B22rOemKFGmIAAAAAElFTkSuQmCC.png", Color = Blurple }.WithCurrentTimestamp().Build());
                else await Context.Channel.SendMessageAsync("", false, new EmbedBuilder { Title = "Join our support server!", Url = "https://tiny.cc/rolexdsl", Description = "Support RoleX by [voting](http:/tiny.cc/rolexdsl) for it on top.gg!", Color = Blurple }.WithCurrentTimestamp().Build());
            }
            return here;
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