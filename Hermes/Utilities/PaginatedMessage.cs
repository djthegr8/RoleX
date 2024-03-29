﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Hermes.Modules.Services;
using Hermes.Utilities.Collector;

namespace Hermes.Utilities
{
    public class PaginatedMessage : DisposableBase
    {
        private readonly EmbedBuilder _embedBuilder = new();
        private readonly SingleTask<IUserMessage?> _resendTask;

        private readonly Timer _timeoutTimer;
        private readonly SingleTask _updateTask;
        public readonly PaginatedAppearanceOptions Options;
        private CollectorController? _collectorController;
        private bool _isCollectionUpdating;
        private bool _jumpEnabled;

        public IUserMessage? Message;

        public PaginatedMessage(PaginatedAppearanceOptions options, IUserMessage message, MessagePage? errorPage = null)
            : this(options, message.Channel, errorPage)
        {
            Channel = message.Channel;
            Message = message;
            SetupReactions();
            if (Message.Author.Id != Program.Client.CurrentUser.Id)
                throw new ArgumentException($"{nameof(message)} must be from the current user");
        }

        public PaginatedMessage(PaginatedAppearanceOptions options, IMessageChannel channel,
            MessagePage? errorPage = null)
        {
            Channel = channel;
            Options = options;

            ErrorPage = errorPage ?? new MessagePage("Loading...");

            Pages.CollectionChanged += (sender, args) => Update();

            _timeoutTimer = new Timer(state => { Dispose(); });

            _updateTask = new SingleTask(async () =>
            {
                if (IsDisposed) return;
                UpdateTimeout(true);
                try
                {
                    if (Message == null)
                        await Resend();
                    else
                        try
                        {
                            await Message!.ModifyAsync(properties =>
                            {
#pragma warning disable 618
                                UpdateEmbed();
#pragma warning restore 618

                                properties.Embed = _embedBuilder.Build();
                                properties.Content = null;
                            });
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                }
                catch
                {
                    // ignored
                }
                finally
                {
                    UpdateTimeout();
                }
            }) {CanBeDirty = true, BetweenExecutionsDelay = TimeSpan.FromSeconds(4)};

            _resendTask = new SingleTask<IUserMessage?>(async () =>
            {
                UpdateTimeout(true);
                try
                {
                    Message?.SafeDelete();
                    Message = null;
#pragma warning disable 618
                    UpdateEmbed();
#pragma warning restore 618
                    Message = await Channel.SendMessageAsync(null, false,
                        _embedBuilder.WithColor(CommandModuleBase.Blurple).Build());
                    SetupReactions();
                    return Message;
                }
                catch
                {
                    // ignored
                }
                finally
                {
                    UpdateTimeout();
                }

                return null;
            }) {BetweenExecutionsDelay = TimeSpan.FromSeconds(10)};

            _isCollectionUpdating = false;
        }

        public IMessageChannel Channel { get; set; }

        public ObservableCollection<MessagePage> Pages { get; } = new();

        public MessagePage ErrorPage { get; set; }

        public int PageNumber { get; set; }

        public string Title
        {
            get => _embedBuilder.Title;
            set => _embedBuilder.Title = value;
        }

        public string Url
        {
            get => _embedBuilder.Url;
            set => _embedBuilder.Url = value;
        }

        public string ThumbnailUrl
        {
            get => _embedBuilder.ThumbnailUrl;
            set => _embedBuilder.ThumbnailUrl = value;
        }

        public string ImageUrl
        {
            get => _embedBuilder.ImageUrl;
            set => _embedBuilder.ImageUrl = value;
        }

        public DateTimeOffset? Timestamp
        {
            get => _embedBuilder.Timestamp;
            set => _embedBuilder.Timestamp = value;
        }

        public Color? Color
        {
            get => _embedBuilder.Color;
            set => _embedBuilder.Color = value;
        }

        public EmbedAuthorBuilder Author
        {
            get => _embedBuilder.Author;
            set => _embedBuilder.Author = value;
        }

        public EmbedFooterBuilder? Footer { get; set; }

        [Obsolete("Internal method, do not use")]
        private void UpdateEmbed()
        {
            try
            {
                UpdateInternal(Pages[PageNumber], true);
            }
            catch (Exception)
            {
                UpdateInternal(ErrorPage, false);
            }

            void UpdateInternal(MessagePage messagePage, bool withFooter)
            {
                //_embedBuilder.Fields.Clear();
                _embedBuilder.Fields = messagePage.Fields;
                _embedBuilder.Description = messagePage.Description;
                _embedBuilder.Footer = Footer ?? new EmbedFooterBuilder();
                if (withFooter)
                    _embedBuilder.Footer =
                        _embedBuilder.Footer.WithText(string.Format(Options.FooterFormat, PageNumber + 1, Pages.Count) +
                                                      (string.IsNullOrWhiteSpace(Footer?.Text)
                                                          ? ""
                                                          : $" | {Footer.Text}"));
            }
        }

        private void SetupReactions()
        {
            try
            {
                Message?.RemoveAllReactionsAsync();
            }
            catch (Exception)
            {
                // Ignored
            }

            _collectorController = CollectorsUtils.CollectReaction(Message!, _ => true, args =>
            {
                if (args.Reaction.Emote.Equals(Options.Back))
                {
                    PageNumber--;
                }
                else if (args.Reaction.Emote.Equals(Options.Next))
                {
                    PageNumber++;
                }
                else if (args.Reaction.Emote.Equals(Options.First))
                {
                    PageNumber = 0;
                }
                else if (args.Reaction.Emote.Equals(Options.Last))
                {
                    PageNumber = Pages.Count - 1;
                }
                else if (args.Reaction.Emote.Equals(Options.Stop))
                {
                    Dispose();
                    return;
                }
                else if (Options.DisplayInformationIcon && args.Reaction.Emote.Equals(Options.Info))
                {
                    try
                    {
                        Channel.SendMessageAsync(Options.InformationText).DelayedDelete(Options.InfoTimeout);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
                else if (_jumpEnabled && args.Reaction.Emote.Equals(Options.Jump))
                {
                    CollectorsUtils.CollectMessage(args.Reaction.Channel,
                        message => message.Author.Id == args.Reaction.UserId, async eventArgs =>
                        {
                            eventArgs.StopCollect();
                            await eventArgs.RemoveReason();
                            if (int.TryParse(eventArgs.Message.Content, out var result))
                            {
                                PageNumber = result - 1;
                                CoercePageNumber();
                                Update(false);
                            }
                        });
                }
                else
                {
                    return;
                }

                args.RemoveReason();
                CoercePageNumber();
                _updateTask.Execute(true, TimeSpan.FromSeconds(1));
            }, CollectorFilter.IgnoreBots);
            _ = Task.Run(async () =>
            {
                await Message!.AddReactionAsync(Options.First);
                await Task.Delay(1000);
                await Message!.AddReactionAsync(Options.Back);
                await Task.Delay(1000);
                await Message!.AddReactionAsync(Options.Next);
                await Task.Delay(1000);
                await Message!.AddReactionAsync(Options.Last);
                var manageMessages = Channel is IGuildChannel guildChannel &&
                                     (await guildChannel.GetUserAsync(Program.Client.CurrentUser.Id))
                                     .GetPermissions(guildChannel).ManageMessages;

                _jumpEnabled = Options.JumpDisplayOptions == JumpDisplayOptions.Always
                               || Options.JumpDisplayOptions == JumpDisplayOptions.WithManageMessages && manageMessages;
                if (_jumpEnabled) await Message!.AddReactionAsync(Options.Jump);

                if (Options.StopEnabled)
                    await Message!.AddReactionAsync(Options.Stop);

                if (Options.DisplayInformationIcon)
                    await Message!.AddReactionAsync(Options.Info);
            });
        }

        public void UpdateTimeout(bool pauseTimer = false)
        {
            _timeoutTimer.Change(
                pauseTimer ? TimeSpan.FromMilliseconds(-1) : Options.Timeout ?? TimeSpan.FromMilliseconds(-1),
                TimeSpan.FromMilliseconds(-1));
        }

        public void CoercePageNumber()
        {
            PageNumber = PageNumber.Normalize(0, Pages.Count - 1);
        }

        public void SetDefaultPage()
        {
            PageNumber = -1;
        }

        /// <summary>
        ///     Accepts a string that is broken for description in embed'e and formats
        /// </summary>
        /// <param name="content">Source string</param>
        /// <param name="format">
        ///     Format string, every page will be formatted with this string. {0} - For content {1} - For page
        ///     number
        /// </param>
        /// <param name="lineLimit">Max lines at one page</param>
        /// <param name="fields">Persistent fields rendered for any page</param>
        public void SetPages(string content, string format = "{0}", int lineLimit = int.MaxValue,
            IEnumerable<EmbedFieldBuilder>? fields = null)
        {
            PagesRecreating(() =>
            {
                var pageContentLength = 2048 - format.Length;
                var fieldsList = fields?.ToList();
                if (fieldsList != null)
                    pageContentLength -=
                        fieldsList.Sum(builder => builder.Name.Length + builder.Value.ToString()!.Length);

                var lines = content.Split("\n");
                var page = new MessagePage {Fields = fieldsList?.ToList() ?? new List<EmbedFieldBuilder>()};
                var currentLinesCount = 0;
                foreach (var line in lines)
                {
                    if (page.Description.Length + line.Length > pageContentLength || currentLinesCount > lineLimit)
                    {
                        FinishCurrentPage();
                        currentLinesCount = 0;
                    }

                    page.Description += "\n" + line;
                    currentLinesCount++;
                }

                FinishCurrentPage();

                void FinishCurrentPage()
                {
                    page.Description = string.Format(format, page.Description, Pages.Count + 1);
                    Pages.Add(page!);
                    page = new MessagePage();
                }
            });
        }

        public void SetPages(IEnumerable<MessagePage> pages)
        {
            PagesRecreating(() => Pages.AddRange(pages));
        }

        public void SetPages(string description, IEnumerable<EmbedFieldBuilder> fields, int? fieldsLimit)
        {
            PagesRecreating(() =>
            {
                fieldsLimit = (fieldsLimit ?? EmbedBuilder.MaxFieldCount).Normalize(0, EmbedBuilder.MaxFieldCount);
                _embedBuilder.Fields.Clear();
                _embedBuilder.Description = null;
                _embedBuilder.Footer = Footer;
                var lengthLimit = EmbedBuilder.MaxEmbedLength - _embedBuilder.Length - 10;


                var pagesData = fields.GroupContiguous((list, builder) =>
                    list.Count <= fieldsLimit &&
                    list.Sum(fieldBuilder => fieldBuilder.Name.Length + fieldBuilder.Value.ToString()!.Length) +
                    builder.Name.Length + builder.Value.ToString()!.Length < lengthLimit - description.Length);

                Pages.AddRange(pagesData.Select(list => new MessagePage(description, list)));
            });
        }

        private void PagesRecreating(Action action)
        {
            _isCollectionUpdating = true;
            Pages.Clear();
            try
            {
                action();
            }
            finally
            {
                CoercePageNumber();
                _isCollectionUpdating = false;
                Update();
            }
        }


        public Task Update(bool resendIfNeeded = true)
        {
            if (_isCollectionUpdating) return Task.CompletedTask;

            if (!resendIfNeeded && Message == null && !_updateTask.IsExecuting) return Task.CompletedTask;

            return _updateTask.Execute();
        }

        public Task Resend()
        {
            return _resendTask.Execute();
        }

        protected override void DisposeInternal()
        {
            base.DisposeInternal();
            _timeoutTimer.Dispose();
            _collectorController?.Dispose();
            _resendTask.Dispose();
            _updateTask.Dispose();
            Message?.SafeDelete();
        }


        public class MessagePage
        {
            public MessagePage()
            {
            }

            public MessagePage(string description)
            {
                Description = description;
            }

            public MessagePage(string description, IEnumerable<EmbedFieldBuilder> fieldBuilders)
            {
                Description = description;
                Fields = fieldBuilders.ToList();
            }

            public string Description { get; set; } = "";
            public List<EmbedFieldBuilder> Fields { get; set; } = new();
        }
    }

    public class PaginatedAppearanceOptions
    {
        public static PaginatedAppearanceOptions Default = new();
        public IEmote Back = new Emoji("◀️");
        public bool DisplayInformationIcon;
        public IEmote First = new Emoji("⏮️");
        public string FooterFormat = "{0}/{1}";
        public IEmote Info = new Emoji("ℹ️");
        public string InformationText = "This is a paginator. React with the respective icons to change page.";
        public TimeSpan InfoTimeout = TimeSpan.FromSeconds(30);
        public IEmote Jump = new Emoji("🔢");

        public JumpDisplayOptions JumpDisplayOptions = JumpDisplayOptions.Never;
        public IEmote Last = new Emoji("⏭️");
        public IEmote Next = new Emoji("▶️");
        public IEmote Stop = new Emoji("⏹️");

        public bool StopEnabled = false;

        public TimeSpan? Timeout = TimeSpan.FromMilliseconds(-1);

        public PaginatedAppearanceOptions()
        {
        }

        public PaginatedAppearanceOptions(string informationText)
        {
            InformationText = informationText;
            DisplayInformationIcon = true;
        }
    }


    public enum JumpDisplayOptions
    {
        Never,
        WithManageMessages,
        Always
    }
}