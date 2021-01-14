﻿using NTwain.Data;
using NTwain.Internals;

namespace NTwain.Triplets
{
    /// <summary>
    /// Represents <see cref="DataArgumentType.EntryPoint"/>.
    /// </summary>
	sealed class EntryPoint : TripletBase
	{
		internal EntryPoint(ITwainSessionInternal session) : base(session) { }
		/// <summary>
		/// Gets the function entry points for twain 2.0 or higher.
		/// </summary>
		/// <param name="entryPoint">The entry point.</param>
		/// <returns></returns>
		public ReturnCode Get(out TWEntryPoint entryPoint)
		{
			Session.VerifyState(3, 3, DataGroups.Control, DataArgumentType.EntryPoint, Message.Get);
			entryPoint = new TWEntryPoint();
			return Dsm.DsmEntry(Session.AppId, Message.Get, entryPoint);
		}
	}
}