using Discord;
using Discord.Rest;
using Discord.WebSocket;
using GLaDOSV3.Helpers;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace GLaDOSV3.Module.Logger
{
    internal class LoggerService
    {

        // DiscordShardedClient, CommandService, and IConfigurationRoot are injected automatically from the IServiceProvider
        public LoggerService(DiscordShardedClient discord)
        {
            discord.UserJoined += this.DiscordOnUserJoined;
            discord.UserLeft += this.DiscordOnUserLeft;
            //discord.ChannelDestroyed += this.DiscordChannelDestroyed;
            //discord.RoleCreated += this.DiscordRoleCreated;
            //discord.RoleDeleted += this.DiscordRoleDeleted;
            //discord.RoleUpdated += this.DiscordRoleUpdated;
            //discord.GuildUpdated += this.DiscordGuildUpdated;
            //discord.UserBanned += this.DiscordUserBanned;
            //discord.UserUnbanned += this.DiscordUserUnbanned;
            //discord.GuildMemberUpdated += this.DiscordGuildMemberUpdated;
            //discord.UserVoiceStateUpdated += this.DiscordUserVoiceStateUpdated;
            //discord.ReactionsCleared += this.DiscordReactionsCleared;
            //discord.MessageDeleted += this.DiscordMessageDeleted;
            //discord.MessageUpdated += this.DiscordMessageUpdated;
            //discord.ChannelCreated += this.DiscordChannelCreated;
            //discord.InviteDeleted += this.DiscordInviteDeleted;
            //discord.InviteCreated += this.DiscordInviteCreated;
            //discord.MessagesBulkDeleted += this.DiscordMessagesBulkDeleted;
            //discord.ChannelUpdated += this.DiscordChannelUpdated;
        }

        //private Task DiscordChannelUpdated(SocketChannel beforeChannel, SocketChannel afterChannel)
        //{
        //    throw new NotImplementedException();
        //}

        //private Task DiscordMessagesBulkDeleted(IReadOnlyCollection<Cacheable<IMessage, ulong>> cachedMessages, Cacheable<IMessageChannel, ulong> channel)
        //{
        //    throw new NotImplementedException();
        //}

        //private Task DiscordInviteCreated(SocketInvite invite)
        //{
        //    throw new NotImplementedException();
        //}

        //private Task DiscordInviteDeleted(SocketGuildChannel inviteChannel, string deletedCode)
        //{
        //    throw new NotImplementedException();
        //}

        private async Task DiscordChannelCreated(SocketChannel createdChannel)
        {
            if (createdChannel is not SocketGuildChannel channel) return;

            var builder = new EmbedBuilder();
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

        //private Task DiscordMessageUpdated(Cacheable<IMessage, ulong> originalMessage, SocketMessage updatedMessage, ISocketMessageChannel source)
        //{
        //    throw new NotImplementedException();
        //}

        //private Task DiscordMessageDeleted(Cacheable<IMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> channel)
        //{
        //    throw new NotImplementedException();
        //}

        //private Task DiscordReactionsCleared(Cacheable<IUserMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> channel)
        //{
        //    throw new NotImplementedException();
        //}

        //private Task DiscordUserVoiceStateUpdated(SocketUser user, SocketVoiceState before, SocketVoiceState after)
        //{
        //    throw new NotImplementedException();
        //}

        //private Task DiscordGuildMemberUpdated(Cacheable<SocketGuildUser, ulong> before, SocketGuildUser after)
        //{
        //    throw new NotImplementedException();
        //}

        //private Task DiscordUserUnbanned(SocketUser user, SocketGuild server)
        //{
        //    throw new NotImplementedException();
        //}

        //private Task DiscordUserBanned(SocketUser user, SocketGuild server)
        //{
        //    throw new NotImplementedException();
        //}

        //private Task DiscordGuildUpdated(SocketGuild before, SocketGuild after)
        //{
        //    throw new NotImplementedException();
        //}

        //private Task DiscordRoleUpdated(SocketRole before, SocketRole after)
        //{
        //    throw new NotImplementedException();
        //}

        //private Task DiscordRoleDeleted(SocketRole role)
        //{
        //    throw new NotImplementedException();
        //}

        //private Task DiscordRoleCreated(SocketRole role)
        //{
        //    throw new NotImplementedException();
        //}

        //private Task DiscordChannelDestroyed(SocketChannel channel)
        //{
        //    throw new NotImplementedException();
        //}

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
