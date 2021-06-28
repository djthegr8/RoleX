using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using ParameterInfo = System.Reflection.ParameterInfo;

namespace Hermes.Modules.Services
{
    /// <summary>
    /// The base class of <see cref="CustomCommandService"/>
    /// </summary>
    public class CustomCommandService
    {
        public static Dictionary<string, string> Modules { get; set; } = new Dictionary<string, string>();

        public static List<char> UsedPrefixes { get; set; }

        public static bool ContainsUsedPrefix(string msg)
        {
            return UsedPrefixes.Any(msg.StartsWith);
        }

        private readonly List<Command> CommandList = new ();
        private class Command
        {
            public bool RequirePermission { get; set; }
            public string CommandName { get; set; }
            public List<string> alts { get; set; } = new List<string>();
            public char[] Prefixes { get; set; }
            public ParameterInfo[] Paramaters { get; set; }
            public MethodInfo Method { get; set; }
            public DiscordCommand attribute { get; set; }
            public CommandClassobj parent { get; set; }

            public RequiredUserPermissions perms { get; set; }
            public RequiredBotPermission bperms { get; set; }
            public bool IsPremium { get; set; } = false;
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
        private RedisClass redis;

        /// <summary>
        /// Creates a new command service instance
        /// </summary>
        /// <param name="s">the <see cref="Settings"/> for the command service</param>
        public CustomCommandService(Settings s)
        {
            redis = new RedisClass();
            currentSettings = s;
            UsedPrefixes = new List<char>
            {
                s.DefaultPrefix
            };
            if (currentSettings.HasPermissionMethod == null)
                currentSettings.HasPermissionMethod = s => { return true; };
            if (s.CustomGuildPermissionMethod == null)
                currentSettings.CustomGuildPermissionMethod = new Dictionary<ulong, Func<SocketCommandContext, bool>>();
            CommandModuleBase.CommandDescriptions = new Dictionary<string, string>();
            CommandModuleBase.CommandHelps = new Dictionary<string, string>();
            CommandModuleBase.Commands = new List<Commands>();
            Dictionary<MethodInfo, Type> CommandMethods = new Dictionary<MethodInfo, Type>();
            var types = Assembly.GetEntryAssembly() == null ? new List<Type>().ToArray() : Assembly.GetEntryAssembly().GetTypes();
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
                Command cmdobj = new Command
                {
                    CommandName = cmdat.commandName,
                    Method = item.Key,
                    Prefixes = cmdat.prefixes.Length == 0 ? new[] { currentSettings.DefaultPrefix } : cmdat.prefixes,
                    attribute = cmdat,
                    alts = altsL,
                    parent = new CommandClassobj
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
                    Prefixes = parat.prefix == '\0' ? cmdobj.Prefixes : Enumerable.Append(cmdobj.Prefixes, parat.prefix).ToArray(),
                    RequiresPermission = cmdat.RequiredPermission,
                    ModuleName = cmdobj.parent.attribute.ModuleName,
                    isPremium = cmdat.IsPremium
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
            var IsMentionCommand = false;
            var msgcontent = context.Message.Content;
            var hasN = msgcontent.Split(' ').Any(k => k == "-n");
            var ignoreAlias = msgcontent.Split(' ')[0].Contains("alias") || hasN;
            if (hasN)
            {
                msgcontent = msgcontent.Replace("-n", "");
            }
            if (!ignoreAlias)
            {
                var alss = await SqliteClass.GuildAliasGetter(context.Guild.Id);
                foreach (var (aliasName, aliasContent) in alss)
                {
                    msgcontent = msgcontent.Split(" ")[0].Replace(aliasName, aliasContent) + " " + string.Join(" ",msgcontent.Split(" ").Skip(1));
                }
            }

            var listOfWords = msgcontent.Split('\"').ToList();
            List<string> holdingList = new();
            for (var i = 0; i < listOfWords.Count; i++)
            {
                if (i % 2 == 0)
                {
                    holdingList.AddRange(listOfWords[i].Split(' '));
                }
                else
                {
                    holdingList.Add(listOfWords[i]);
                }

            }

            var param = holdingList.ToArray();
            /*if (fs == -1) param = msgcontent.Split(' ');
            else
            {
                var idx = msgcontent.IndexOf("\"", fs + 1);
                if (idx == -1) param = msgcontent.Split(' ');
                else
                {
                    var paramL = msgcontent.Substring(0, fs).Split(' ').ToList();
                    paramL.Add(msgcontent.Substring(fs + 1, idx - fs));
                    paramL.Add(msg);
                }
            }
         param = msgcontent.Split(' ');*/
            param = Enumerable.TakeLast(param, param.Length - 1).ToArray();
            var command = msgcontent.Remove(0, pref.Length).Split(' ')[0];
            IEnumerable<Command> commandobj = CommandList.Where(x => x.CommandName.ToLower() == command).ToList();
            var altob = CommandList.Where(x => x.alts.Any(x => x.ToLower() == command));
            if (!commandobj.Any())
                commandobj = altob;
            if (!commandobj.Any())
                return new CommandResult
                {
                    Result = CommandStatus.NotFound,
                    IsSuccess = false
                };
            if (context.Channel.GetType() == typeof(SocketDMChannel) && !currentSettings.DMCommands)
                return new CommandResult { IsSuccess = false, Result = CommandStatus.InvalidParams };
            var results = new List<CommandResult>();
            //1 find if param counts match
            if (commandobj.Any(x => x.Paramaters.Length > 0))
                if (commandobj.Where(x => x.Paramaters.Length > 0).Any(x => x.Paramaters.Last().GetCustomAttributes(typeof(ParamArrayAttribute), false).Length == 0))
                    commandobj = commandobj.Where(x => x.Paramaters.Length == param.Length).ToList();
            if (!commandobj.Any())
                return new CommandResult { IsSuccess = false, Result = CommandStatus.InvalidParams };
            var idegk = param.ToList();
            try
            {
                // Add more flags here
                // Flag name: -q
                // What it does: Quiet mode - Deletes user msg and doesnt print msg
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
            return new CommandResult { Results = results.ToArray(), MultipleResults = true, IsSuccess = false };
        }
        private async Task<CommandResult> ExecuteCommand(Command cmd, SocketCommandContext context, string[] param)
        {
            if (!(context.User as SocketGuildUser).GuildPermissions.Administrator && CommandModuleBase.devids.All(k => k != context.User.Id))
            {
                if (cmd.perms != null)
                {
                    if (cmd.perms.Permissions.Any(p => !(context.User as SocketGuildUser).GuildPermissions.Has(p)))
                    {
                        return new CommandResult
                        {
                            Result = CommandStatus.MissingGuildPermission,
                            ResultMessage = "" +
                                            "```\n" +
                                            $"{string.Join('\n', cmd.perms.Permissions.Where(x => !(context.User as SocketGuildUser).GuildPermissions.Has(x)).Select(x => x.ToString()))}" +
                                            "```"
                        };
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
                            return new CommandResult
                            {
                                Result = CommandStatus.BotMissingPermissions,
                                ResultMessage = Enum.GetName(typeof(GuildPermission), p)
                            };
                        }
                    }
                }
            }
            if (!cmd.attribute.BotCanExecute && context.Message.Author.IsBot)
                return new CommandResult { Result = CommandStatus.InvalidPermissions };
            if (cmd.attribute.IsPremium && !await SqliteClass.PremiumOrNot(context.Guild.Id))
            {
                return new CommandResult { Result = CommandStatus.ServerNotPremium };
            }
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
                    return new CommandResult { Result = CommandStatus.Success, IsSuccess = true };
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return new CommandResult { Exception = ex, Result = CommandStatus.Error, IsSuccess = false };
                }
            }

            if (cmd.Paramaters.Length == 0 && param.Length > 0)
                return new CommandResult { Result = CommandStatus.InvalidParams, IsSuccess = false };
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
                            return new CommandResult { Result = CommandStatus.InvalidParams, IsSuccess = false };
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
                            return new CommandResult { IsSuccess = false, Result = CommandStatus.InvalidPermissions };

                        cmd.parent.ClassInstance.GetType().GetProperty("Context").SetValue(cmd.parent.ClassInstance, context);
                        if (await RedisClass.ServerOnCd(context.Guild.Id))
                            return new CommandResult {Result = CommandStatus.OnCooldown, IsSuccess = false};
                        await RedisClass.SetServerCD(context.Guild.Id);
                        Task s = (Task)cmd.Method.Invoke(cmd.parent.ClassInstance, parsedparams.ToArray());
                        Task.Run(async () => await s).Wait();
                        if (s.Exception == null)
                            return new CommandResult { Result = CommandStatus.Success, IsSuccess = true };
                        return new CommandResult { Exception = s.Exception.InnerException, Result = CommandStatus.Error, IsSuccess = false };

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        return new CommandResult { Exception = ex, Result = CommandStatus.Error, IsSuccess = false };
                    }

                }

                return new CommandResult { Result = CommandStatus.InvalidParams, IsSuccess = false };
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
                            return new CommandResult { IsSuccess = false, Result = CommandStatus.InvalidPermissions };

                        cmd.parent.ClassInstance.GetType().GetProperty("Context").SetValue(cmd.parent.ClassInstance, context);
                        if (await RedisClass.ServerOnCd(context.Guild.Id))
                            return new CommandResult { Result = CommandStatus.OnCooldown, IsSuccess = false };
                        await RedisClass.SetServerCD(context.Guild.Id);
                        Task s = (Task)cmd.Method.Invoke(cmd.parent.ClassInstance, parsedparams.ToArray());
                        Task.Run(async () => await s).Wait();
                        if (s.Exception == null)
                            return new CommandResult { Result = CommandStatus.Success, IsSuccess = true };
                        return new CommandResult { Exception = s.Exception.InnerException, Result = CommandStatus.Error, IsSuccess = false };

                    }
                    catch (TargetInvocationException ex)
                    {
                        Console.WriteLine(ex);
                        return new CommandResult { Exception = ex, Result = CommandStatus.Error, IsSuccess = false };
                    }

                }

                return new CommandResult { Result = CommandStatus.InvalidParams, IsSuccess = false };

            }

            return new CommandResult { Result = CommandStatus.NotEnoughParams, IsSuccess = false };
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
            public bool isPremium { get; set; }
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
}