using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CronoCord.Classes
{
    public class Event
    {
        /// <summary>
        /// ID of the user who created the event
        /// </summary>
        public ulong CreatorID { get; private set; }
        /// <summary>
        /// Name of the event
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Description of the event
        /// </summary>
        public string Description { get; private set; }
        /// <summary>
        /// Unix timestamp of the event start date and time
        /// </summary>
        public long StartTimeUnix { get; private set; }
        /// <summary>
        /// Unix timestamp of the event end date and time
        /// </summary>
        public long EndTimeUnix { get; private set; }
        /// <summary>
        /// Event status
        /// </summary>
        public EventsStatuses Status { get; private set; }
        /// <summary>
        /// ID of the channel the event was created in
        /// </summary>
        public ulong ChannelID { get; private set; }
        /// <summary>
        /// ID of the original announcement message
        /// </summary>
        public ulong MessageID { get; private set; }
        /// <summary>
        /// Was the owner already reminded to confirm the event
        /// </summary>
        public bool AlreadyRemindedOwner { get; private set; }
        /// <summary>
        /// Were participants already reminded before the event started
        /// </summary>
        public bool AlreadyRemindedParticipants { get; private set; }
        /// <summary>
        /// Was the event already announced
        /// </summary>
        public bool AlreadyAnnounced { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="Event"/> class.
        /// </summary>
        /// <param name="creatorId">The ID of the creator of the event (a unique identifier for the user who created the event).</param>
        /// <param name="name">The name of the event, which should be a brief and descriptive title.</param>
        /// <param name="description">A detailed description of the event, providing more information about what the event is about.</param>
        /// <param name="startTimeUnix">The start time of the event in Unix timestamp format (number of seconds since January 1, 1970, UTC).</param>
        /// <param name="endTimeUnix">The end time of the event in Unix timestamp format (number of seconds since January 1, 1970, UTC).</param>
        /// <param name="status">The current status of the event (e.g., "Scheduled", "Completed", "Canceled").</param>
        /// <param name="channelId">The ID of the Discord channel where the event is associated with.</param>
        /// <param name="messageId">The ID of the message related to the event, used to identify and potentially edit the message later.</param>
        /// <param name="alreadyRemindedOwner">A boolean flag indicating whether the event creator has already been reminded (true if reminded, false if not).</param>
        /// <param name="alreadyRemindedParticipants">A boolean flag indicating whether the event participants have been reminded (true if reminded, false if not).</param>
        /// <param name="alreadyAnnounced">A boolean flag indicating whether the event has already been announced (true if announced, false if not).</param>
        public Event(ulong creatorId, string name, string description, long startTimeUnix, long endTimeUnix, EventsStatuses status, ulong channelId, ulong messageId, bool alreadyRemindedOwner, bool alreadyRemindedParticipants, bool alreadyAnnounced)
        {
            CreatorID = creatorId;
            Name = name;
            Description = description;
            StartTimeUnix = startTimeUnix;
            EndTimeUnix = endTimeUnix;
            Status = status;
            ChannelID = channelId;
            MessageID = messageId;
            AlreadyRemindedOwner = alreadyRemindedOwner;
            AlreadyRemindedParticipants = alreadyRemindedParticipants;
            AlreadyAnnounced = alreadyAnnounced;
        }



        /// <summary>
        /// Initializes a new instance of the <see cref="Event"/> class.
        /// </summary>
        /// <param name="creatorId">The ID of the creator of the event (a unique identifier for the user who created the event).</param>
        /// <param name="name">The name of the event, which should be a brief and descriptive title.</param>
        /// <param name="description">A detailed description of the event, providing more information about what the event is about.</param>
        /// <param name="startTimeUnix">The start time of the event in Unix timestamp format (number of seconds since January 1, 1970, UTC).</param>
        /// <param name="endTimeUnix">The end time of the event in Unix timestamp format (number of seconds since January 1, 1970, UTC).</param>
        /// <param name="channelId">The ID of the Discord channel where the event is associated with.</param>
        /// <param name="messageId">The ID of the message related to the event, used to identify and potentially edit the message later.</param>
        public Event(ulong creatorId, string name, string description, long startTimeUnix, long endTimeUnix, ulong channelId, ulong messageId)
            : this(creatorId, name, description, startTimeUnix, endTimeUnix, EventsStatuses.PendingConfirmation, channelId, messageId, false, false, false)
        {
        }

        /// <summary>
        /// Event statuses
        /// </summary>
        public enum EventsStatuses
        {
            PendingConfirmation,
            Confirmed,
            Cancelled,
            // Not including Finished as finished events will be deleted
        }
    }
}
