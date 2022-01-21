using Discord;
using Discord.Rest;
using Discord.WebSocket;
using GLaDOSV3.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace GLaDOSV3.Module.Logger
{
    internal class LoggerService
    {

        // DiscordShardedClient, CommandService, and IConfigurationRoot are injected automatically from the IServiceProvider
        public LoggerService(DiscordShardedClient discord)
        {
            discord.UserJoined += this.DiscordOnUserJoined;
            discord.UserLeft += this.DiscordOnUserLeft;
            discord.ChannelDestroyed += this.DiscordChannelDestroyed;
            discord.RoleCreated += this.DiscordRoleCreated;
            discord.RoleDeleted += this.DiscordRoleDeleted;
            discord.RoleUpdated += this.DiscordRoleUpdated;
            discord.GuildUpdated += this.DiscordGuildUpdated;
            discord.UserBanned += this.DiscordUserBanned;
            discord.UserUnbanned += this.DiscordUserUnbanned;
            discord.GuildMemberUpdated += this.DiscordGuildMemberUpdated;
            discord.ChannelCreated += this.DiscordChannelCreated;
            discord.ChannelUpdated += this.DiscordChannelUpdated;

            discord.InviteDeleted += this.DiscordInviteDeleted;
            discord.InviteCreated += this.DiscordInviteCreated;
            //discord.MessagesBulkDeleted += this.DiscordMessagesBulkDeleted;
            discord.MessageDeleted += this.DiscordMessageDeleted;
            discord.MessageUpdated += this.DiscordMessageUpdated;
        }

        private async Task DiscordChannelUpdated(SocketChannel beforeChannel, SocketChannel afterChannel)
        {
            if (afterChannel is not SocketGuildChannel channel) return;

            var builder = new EmbedBuilder();
            builder.AddField("Name", channel.Name);
            builder.AddField("Id", $"```ini\nUser = Unknown\nChannel = {channel.Id}```");
            builder.AddField("Creation date", channel.CreatedAt);
            builder.WithAuthor("Unknown", "https://discordapp.com/assets/1cbd08c76f8af6dddce02c5138971129.png");
            var logs = await channel.Guild.GetAuditLogsAsync(15, null, null, null, ActionType.ChannelCreated).FlattenAsync();
            var log = logs.FirstOrDefault(a => ((ChannelUpdateAuditLogData)a.Data).ChannelId == channel.Id);
            if (log == null) return;
            if (DateTime.UtcNow.Ticks - log.CreatedAt.UtcTicks > 3000) return;
            builder.WithAuthor(log.User);
            builder.Fields[1].Value = $"```ini\nUser = {log.User.Id}\nChannel = {channel.Id}```";
            try { }
            finally { builder.Build(); }
        }

        //TODO: FINISH THIS YOU LAZY FUCK
        // https://github.com/curtisf/logger/blob/master/src/bot/events/messageDeleteBulk.js
#if false
        private async Task DiscordMessagesBulkDeleted(IReadOnlyCollection<Cacheable<IMessage, ulong>> cachedMessages, Cacheable<IMessageChannel, ulong> channel)
        {
            string fuck = "";
            foreach (Cacheable<IMessage, ulong> cachedMessage in cachedMessages)
            {
                if (!cachedMessage.HasValue) return;
                var    value = cachedMessage.Value;
                fuck  = $"<{value.Author.Username}#{value.Author.Discriminator} ({value.Author.Id})> {value.Content}\n";
            }
         

        }
#endif
        private async Task DiscordInviteCreated(SocketInvite invite)
        {
            var inviteChannel = invite.Channel;
            var name = inviteChannel.GetType().Name;
            name = name.Split("Socket")[1];
            name = name.Insert(name.IndexOf("Channel", StringComparison.Ordinal), " ");
            var builder = new EmbedBuilder
            {
                Description = $"Invite to {name} <#{inviteChannel.Id} created: {invite.Code}>"
            };
            builder.AddField("Name", inviteChannel.Name);
            builder.AddField("Id", $"```ini\nUser = Unknown\nChannel = {inviteChannel.Id}```");
            builder.WithAuthor("Unknown", "https://discordapp.com/assets/1cbd08c76f8af6dddce02c5138971129.png");
            var logs = await inviteChannel.Guild.GetAuditLogsAsync(15, null, null, null, ActionType.ChannelCreated).FlattenAsync();
            var log = logs.FirstOrDefault(a => ((InviteDeleteAuditLogData)a.Data).ChannelId == inviteChannel.Id);
            if (log                                            == null) return;
            if (DateTime.UtcNow.Ticks - log.CreatedAt.UtcTicks > 3000) return;
            builder.WithAuthor(log.User);
            builder.Fields[1].Value = $"```ini\nUser = {log.User.Id}\nChannel = {inviteChannel.Id}```";
            try { }
            finally { builder.Build(); }
        }

        private async Task DiscordInviteDeleted(SocketGuildChannel inviteChannel, string deletedCode)
        {
            var name = inviteChannel.GetType().Name;
            name = name.Split("Socket")[1];
            name = name.Insert(name.IndexOf("Channel", StringComparison.Ordinal), " ");
            var builder = new EmbedBuilder
            {
                Description = $"Invite to {name} <#{inviteChannel.Id} deleted: {deletedCode}>"
            };
            builder.AddField("Name", inviteChannel.Name);
            builder.AddField("Id", $"```ini\nUser = Unknown\nChannel = {inviteChannel.Id}```");
            builder.WithAuthor("Unknown", "https://discordapp.com/assets/1cbd08c76f8af6dddce02c5138971129.png");
            var logs = await inviteChannel.Guild.GetAuditLogsAsync(15, null, null, null, ActionType.ChannelCreated).FlattenAsync();
            var log = logs.FirstOrDefault(a => ((InviteDeleteAuditLogData)a.Data).ChannelId == inviteChannel.Id);
            if (log == null) return;
            if (DateTime.UtcNow.Ticks - log.CreatedAt.UtcTicks > 3000) return;
            builder.WithAuthor(log.User);
            builder.Fields[1].Value = $"```ini\nUser = {log.User.Id}\nChannel = {inviteChannel.Id}```";
            try { }
            finally { builder.Build(); }
        }

        private async Task DiscordChannelCreated(SocketChannel createdChannel)
        {
            if (createdChannel is not SocketGuildChannel channel) return;

            var name = channel.GetType().Name;
            name = name.Split("Socket")[1];
            name = name.Insert(name.IndexOf("Channel", StringComparison.Ordinal), " ");
            var builder = new EmbedBuilder
            {
                Description = $"{name} <#{channel.Id}> created"
            };
            builder.AddField("Name", channel.Name);
            builder.AddField("Id", $"```ini\nUser = Unknown\nChannel = {createdChannel.Id}```");
            builder.WithAuthor("Unknown", "https://discordapp.com/assets/1cbd08c76f8af6dddce02c5138971129.png");
            var logs = await channel.Guild.GetAuditLogsAsync(15, null, null, null, ActionType.ChannelCreated).FlattenAsync();
            var log = logs.FirstOrDefault(a => ((ChannelCreateAuditLogData)a.Data).ChannelId == channel.Id);
            if (log == null) return;
            if (DateTime.UtcNow.Ticks - log.CreatedAt.UtcTicks > 3000) return;
            builder.WithAuthor(log.User);
            builder.Fields[1].Value = $"```ini\nUser = {log.User.Id}\nChannel = {createdChannel.Id}```";
            try { }
            finally { builder.Build(); }
        }

        private async Task DiscordMessageUpdated(Cacheable<IMessage, ulong> originalMessage, SocketMessage updatedMessage, ISocketMessageChannel source)
        {
            var name = updatedMessage.Channel.GetType().Name;
            name = name.Split("Socket")[1];
            name = name.Insert(name.IndexOf("Channel", StringComparison.Ordinal), " ");
            var oldMessage = await Tools.EscapeMentionsAsync(updatedMessage.Channel,
                                                       originalMessage.HasValue ? originalMessage.Value.Content : "Not cached");
            var oldPinned  = originalMessage.HasValue && originalMessage.Value.IsPinned;
            var newMessage = await Tools.EscapeMentionsAsync(updatedMessage.Channel, updatedMessage.Content);
            var builder = new EmbedBuilder
            {
                Description = $"Message {(oldPinned != updatedMessage.IsPinned ? "pinned" :"edited")} {name} <#{updatedMessage.Channel.Id}>"
            };
            if (oldPinned != updatedMessage.IsPinned)
            {
                builder.AddField("Old message", oldMessage);
                builder.AddField("New message", newMessage);
            }

            builder.WithAuthor(updatedMessage.Author);
            builder.Fields[^1].Value = $"```ini\nUser = {updatedMessage.Author.Id}\nChannel = {updatedMessage.Channel.Id}\nMessage = {updatedMessage.Id}```";
            try { }
            finally { builder.Build(); }
        }

        private async Task DiscordMessageDeleted(Cacheable<IMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> cachedChannel)
        {
            SocketTextChannel channel = cachedChannel.HasValue ? (SocketTextChannel)cachedChannel.Value : null;
            if (channel == null) return;
            SocketMessage oldMessage = cachedMessage.HasValue ? (SocketMessage) cachedMessage.Value : null;
            if (oldMessage == null) return;
            var name = oldMessage.Channel.GetType().Name;
            name = name.Split("Socket")[1];
            name = name.Insert(name.IndexOf("Channel", StringComparison.Ordinal), " ");
            var builder = new EmbedBuilder
            {
                Description = $"Message deleted in {name} <#{channel.Id}>"
            };
            builder.WithAuthor(oldMessage.Author);
            builder.Fields[1].Value = $"```ini\nUser = {oldMessage.Author.Id}\nChannel = {channel.Id}\nMessage = {cachedMessage.Id}```";
            try { }
            finally { builder.Build(); }
        }

        //private Task DiscordUserVoiceStateUpdated(SocketUser user, SocketVoiceState before, SocketVoiceState after)
        //{
        //    throw new NotImplementedException();
        //}

        private async Task DiscordGuildMemberUpdated(Cacheable<SocketGuildUser, ulong> before, SocketGuildUser after)
        {
            if (!before.HasValue) return;
            var builder = new EmbedBuilder
            {
                Description = $"{after} {after.Mention} {(string.IsNullOrWhiteSpace(after.Nickname) ? "" : $"({after.Nickname})")} was updated"
            };
            builder.WithAuthor(after);
            var fuck = StaticTools.Compare(before.Value, after);
            var output = fuck.Select(field => field.Split(" ")).Aggregate("", (current, split) => current + $"Variable: {split[0]}\nOld: {split[1]}\nNew: {split[2]}");
            if (string.IsNullOrWhiteSpace(output)) return; // nothing changed?
            builder.WithAuthor("Unknown", "https://discordapp.com/assets/1cbd08c76f8af6dddce02c5138971129.png");
            var logs = await after.Guild.GetAuditLogsAsync(15, null, null, null, ActionType.MemberUpdated).FlattenAsync();
            var log = logs.FirstOrDefault(a => ((MemberUpdateAuditLogData)a.Data).Target.Id == after.Id);
            if (log == null) return;
            if (DateTime.UtcNow.Ticks - log.CreatedAt.UtcTicks > 3000) return;
            builder.WithAuthor(log.User);
            try { }
            finally { builder.Build(); }
        }

        private async Task DiscordUserUnbanned(SocketUser user, SocketGuild server)
        {
            var builder = new EmbedBuilder
            {
                Description = $"{user} was unbanned"
            };
            builder.AddField("User Information", $"{user.Username}#{user.Discriminator} ({user.Id}) {user.Mention} {(user.IsBot ? "(Bot)" : "")}");
            builder.AddField("Reason", "None provided");
            builder.AddField("Id", $"```ini\nUser = {user.Id}\nStaff member = Unknown```");
            builder.WithAuthor(user);
            await Task.Delay(1000);
            var logs = await server.GetAuditLogsAsync(15, null, null, null, ActionType.Unban).FlattenAsync();
            var log = logs.FirstOrDefault(a => ((UnbanAuditLogData)a.Data).Target.Id == user.Id);
            if (log == null) return;
            if (DateTime.UtcNow.Ticks - log.CreatedAt.UtcTicks > 3000) return;

            if (!string.IsNullOrWhiteSpace(log.Reason)) builder.Fields[1].Value = log.Reason;
            builder.Fields[2].Value = $"```ini\nUser = {user.Id}\nStaff member = {log.User.Id}```";
            builder.WithFooter(log.User.ToString(),
                               string.IsNullOrWhiteSpace(log.User.AvatarId)
                                   ? log.User.GetDefaultAvatarUrl()
                                   : log.User.GetAvatarUrl());
            try { }
            finally { builder.Build(); }
        }

        private async Task DiscordUserBanned(SocketUser user, SocketGuild server)
        {
            var builder = new EmbedBuilder
            {
                Description = $"{user} was banned"
            };
            builder.AddField("User Information", $"{user.Username}#{user.Discriminator} ({user.Id}) {user.Mention} {(user.IsBot ? "(Bot)" : "")}");
            builder.AddField("Reason", "None provided");
            builder.AddField("Id", $"```ini\nUser = {user.Id}\nStaff member = Unknown```");
            builder.WithAuthor(user);
            await Task.Delay(1000);
            var logs = await server.GetAuditLogsAsync(15, null, null, null, ActionType.Ban).FlattenAsync();
            var log = logs.FirstOrDefault(a => ((BanAuditLogData)a.Data).Target.Id == user.Id);
            if (log == null) return;
            if (DateTime.UtcNow.Ticks - log.CreatedAt.UtcTicks > 3000) return;

            if (!string.IsNullOrWhiteSpace(log.Reason)) builder.Fields[1].Value = log.Reason;
            builder.Fields[2].Value = $"```ini\nUser = {user.Id}\nStaff member = {log.User.Id}```";
            builder.WithFooter(log.User.ToString(),
                               string.IsNullOrWhiteSpace(log.User.AvatarId)
                                   ? log.User.GetDefaultAvatarUrl()
                                   : log.User.GetAvatarUrl());
            try { }
            finally { builder.Build(); }
        }

        private async Task DiscordGuildUpdated(SocketGuild before, SocketGuild after)
        {
            var builder = new EmbedBuilder
            {
                Description = $"Server {after} was modifed"
            };

            var fuck = StaticTools.Compare(before, after);
            var output = fuck.Select(field => field.Split(" ")).Aggregate("", (current, split) => current + $"Variable: {split[0]}\nOld: {split[1]}\nNew: {split[2]}");
            if (string.IsNullOrWhiteSpace(output)) return; // nothing changed?

            var logs = await before.GetAuditLogsAsync(15, null, null, null, ActionType.GuildUpdated).FlattenAsync();
            var log = logs.First();
            if (log == null) return;
            if (DateTime.UtcNow.Ticks - log.CreatedAt.UtcTicks > 3000) return;
            builder.WithAuthor(log.User);
            builder.AddField("Id", $"```ini\nUser = {log.User.Id}\nRole = {after.Id}```");
            try { }
            finally { if (builder.Fields.Count != 0) builder.Build(); }
        }

        private async Task DiscordRoleUpdated(SocketRole before, SocketRole after)
        {
            var builder = new EmbedBuilder
            {
                Description = $"Role {after} was modified"
            };

            var fuck = StaticTools.Compare(before, after);
            var output = fuck.Select(field => field.Split(" ")).Aggregate("", (current, split) => current + $"Variable: {split[0]}\nOld: {split[1]}\nNew: {split[2]}");
            if (string.IsNullOrWhiteSpace(output)) return; // nothing changed?

            var logs = await before.Guild.GetAuditLogsAsync(15, null, null, null, ActionType.RoleUpdated).FlattenAsync();
            var log = logs.FirstOrDefault(a => ((RoleUpdateAuditLogData)a.Data).RoleId == after.Id);
            if (log == null) return;
            if (DateTime.UtcNow.Ticks - log.CreatedAt.UtcTicks > 3000) return;
            builder.WithAuthor(log.User);
            builder.AddField("Id", $"```ini\nUser = {log.User.Id}\nRole = {after.Id}```");
            try { }
            finally { if (builder.Fields.Count != 0) builder.Build(); }
        }

        private async Task DiscordRoleDeleted(SocketRole role)
        {
            var builder = new EmbedBuilder
            {
                Description = $"Role {role} was deleted"
            };
            builder.AddField("Name", role.Name);
            builder.AddField("Id", $"```ini\nUser = Unknown\nRole = {role.Id}```");
            builder.AddField("Reason", "None.");
            builder.WithAuthor("Unknown", "https://discordapp.com/assets/1cbd08c76f8af6dddce02c5138971129.png");

            var logs = await role.Guild.GetAuditLogsAsync(15, null, null, null, ActionType.RoleDeleted).FlattenAsync();
            var log = logs.FirstOrDefault(a => ((RoleDeleteAuditLogData)a.Data).RoleId == role.Id);
            if (log == null) return;
            if (DateTime.UtcNow.Ticks - log.CreatedAt.UtcTicks > 3000) return;
            builder.WithAuthor(log.User);
            builder.Fields[1].Value = $"```ini\nUser = {log.User.Id}\nRole = {role.Id}```";
            builder.Fields[2].Value = log.Reason;
            try { }
            finally { builder.Build(); }
        }

        private async Task DiscordRoleCreated(SocketRole role)
        {
            var builder = new EmbedBuilder
            {
                Description = $"Role {role} was created"
            };
            builder.AddField("Name", role.Name);
            builder.AddField("Id", $"```ini\nUser = Unknown\nRole = {role.Id}```");
            builder.AddField("Type", "User", true);
            builder.WithAuthor("Unknown", "https://discordapp.com/assets/1cbd08c76f8af6dddce02c5138971129.png");
            var a = await role.Guild.GetUsersAsync().FlattenAsync();
            if (!a.Any(b => b.Username == role.Name))
            {
                if (role.IsManaged && a.Any(b => b.Username == role.Name)) builder.Fields[2].Value = "Bot";

                var logs = await role.Guild.GetAuditLogsAsync(15, null, null, null, ActionType.RoleCreated).FlattenAsync();
                var log = logs.FirstOrDefault(a => ((RoleCreateAuditLogData)a.Data).RoleId == role.Id);
                if (log == null) return;
                if (DateTime.UtcNow.Ticks - log.CreatedAt.UtcTicks > 3000) return;
                builder.WithAuthor(log.User);
                builder.Fields[1].Value = $"```ini\nUser = {log.User.Id}\nRole = {role.Id}```";
            }
            else
                builder.Fields[1].Value = $"```ini\nUser = *created by invite*\nRole = {role.Id}```";

            try { }
            finally { builder.Build(); }
        }

        private async Task DiscordChannelDestroyed(SocketChannel chan)
        {
            if (chan is not SocketGuildChannel channel) return;

            var name = channel.GetType().Name;
            name = name.Split("Socket")[1];
            name = name.Insert(name.IndexOf("Channel", StringComparison.Ordinal), " ");
            var builder = new EmbedBuilder
            {
                Description = $"{name} deleted <#{channel.Id}>"
            };
            builder.AddField("Name", channel.Name);
            builder.AddField("Id", $"```ini\nUser = Unknown\nChannel = {channel.Id}```");
            builder.AddField("Creation date", channel.CreatedAt, true);
            builder.AddField("Position", channel.Position, true);
            builder.WithAuthor("Unknown", "https://discordapp.com/assets/1cbd08c76f8af6dddce02c5138971129.png");

            if (channel.PermissionOverwrites.Count != 0)
            {
                foreach (var permission in channel.PermissionOverwrites)
                {
                    if (permission.TargetType != PermissionTarget.Role) continue;
                    var role = channel.Guild.GetRole(permission.TargetId);
                    if (role.Name == "@everyone") return;
                    string output =
                        $"Allow: {string.Join(", ", permission.Permissions.ToAllowList())}\nDeny: {string.Join(", ", permission.Permissions.ToDenyList())}";

                    builder.AddField("Permissions of " + role.Name, output);
                }
            }

            var logs = await channel.Guild.GetAuditLogsAsync(15, null, null, null, ActionType.ChannelDeleted).FlattenAsync();
            var log = logs.FirstOrDefault(a => ((ChannelDeleteAuditLogData)a.Data).ChannelId == channel.Id);
            if (log == null) return;
            if (DateTime.UtcNow.Ticks - log.CreatedAt.UtcTicks > 3000) return;
            builder.WithAuthor(log.User);
            builder.Fields[1].Value = $"```ini\nUser = {log.User.Id}\nChannel = {channel.Id}```";
            try { }
            finally { builder.Build(); }
        }

        private async Task DiscordOnUserJoined(SocketGuildUser socketGuildUser)
        {
            var guild = socketGuildUser.Guild;
            using var db = await SqLite.Connection.GetValuesAsync("servers", $"WHERE guildid='{guild.Id.ToString(CultureInfo.InvariantCulture)}'").ConfigureAwait(true);
            if (Convert.ToInt32(db.Rows[0]["join_toggle"], CultureInfo.InvariantCulture) == 1)
            {
                var text = await socketGuildUser.FormatText(db.Rows[0]["join_msg"].ToString()).ConfigureAwait(true);
                if (guild.GetChannel(Convert.ToUInt64(db.Rows[0]["joinleave_cid"], CultureInfo.InvariantCulture)) != null)
                    await ((ISocketMessageChannel)guild.GetChannel(Convert.ToUInt64(db.Rows[0]["joinleave_cid"], CultureInfo.InvariantCulture))).SendMessageAsync(text).ConfigureAwait(false);
                else
                {
                    await guild.Owner.SendMessageAsync($"I tried to send a welcome message to a channel, but it now longer exists. Please set this up again in server {guild.Name}.").ConfigureAwait(false); await this.Disable(guild).ConfigureAwait(false);
                }
            }
        }

        private async Task DiscordOnUserLeft(SocketGuildUser socketGuildUser)
        {
            var guild = socketGuildUser.Guild;
            using var db = await SqLite.Connection.GetValuesAsync("servers", $"WHERE guildid='{guild.Id.ToString(CultureInfo.InvariantCulture)}'").ConfigureAwait(true);
            if (Convert.ToInt32(db.Rows[0]["leave_toggle"], CultureInfo.InvariantCulture) == 1)
            {
                var text = await socketGuildUser.FormatText(db.Rows[0]["leave_msg"].ToString()).ConfigureAwait(true);
                if (guild.GetChannel(Convert.ToUInt64(db.Rows[0]["joinleave_cid"], CultureInfo.InvariantCulture)) != null)
                    await ((ISocketMessageChannel)guild.GetChannel(Convert.ToUInt64(db.Rows[0]["joinleave_cid"], CultureInfo.InvariantCulture)))
                        .SendMessageAsync(text).ConfigureAwait(false);
                else
                {
                    await guild.Owner
                        .SendMessageAsync($"I tried to send a farewell message to a channel, but it now longer exists. Please set this up again in server {guild.Name}.").ConfigureAwait(false);
                    await this.Disable(guild).ConfigureAwait(false);
                }
            }
        }

        private Task Disable(SocketGuild guild)
        {
            SqLite.Connection.SetValueAsync("servers", "join_toggle", 0, $"WHERE guildid={guild.Id.ToString(CultureInfo.InvariantCulture)}");
            SqLite.Connection.SetValueAsync("servers", "leave_toggle", 0, $"WHERE guildid={guild.Id.ToString(CultureInfo.InvariantCulture)}");
            return Task.CompletedTask;
        }
    }
}
