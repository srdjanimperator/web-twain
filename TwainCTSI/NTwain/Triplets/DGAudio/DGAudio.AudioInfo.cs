﻿using NTwain.Data;
using NTwain.Internals;

namespace NTwain.Triplets
{
    /// <summary>
    /// Represents <see cref="DataArgumentType.AudioInfo"/>.
    /// </summary>
	sealed class AudioInfo : TripletBase
	{
		internal AudioInfo(ITwainSessionInternal session) : base(session) { }
	
        /// <summary>
		/// Used to get the information of the current audio data ready to transfer.
		/// </summary>
		/// <param name="info">The info.</param>
		/// <returns></returns>
        public ReturnCode Get(out TWAudioInfo info)
		{
			Session.VerifyState(6, 7, DataGroups.Audio, DataArgumentType.AudioInfo, Message.Get);
			info = new TWAudioInfo();
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, Message.Get, info);
		}
	}
}