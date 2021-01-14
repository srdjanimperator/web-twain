﻿using NTwain.Data;
using NTwain.Internals;
using System;

namespace NTwain.Triplets
{
	sealed class ImageNativeXfer : TripletBase
	{
		internal ImageNativeXfer(ITwainSessionInternal session) : base(session) { }

		/// <summary>
		/// Causes the transfer of an image’s data from the Source to the application, via the Native transfer
		/// mechanism, to begin. The resulting data is stored in main memory in a single block. The data is
		/// stored in Picture (PICT) format on the Macintosh and as a device-independent bitmap (DIB)
		/// under Microsoft Windows. The size of the image that can be transferred is limited to the size of
		/// the memory block that can be allocated by the Source.
		/// </summary>
		/// <param name="handle">The handle.</param>
		/// <returns></returns>
		public ReturnCode Get(ref IntPtr handle)
		{
			Session.VerifyState(6, 6, DataGroups.Image, DataArgumentType.ImageNativeXfer, Message.Get);
			return Dsm.DsmEntry(Session.AppId, Session.CurrentSource.Identity, DataGroups.Image, DataArgumentType.ImageNativeXfer, Message.Get, ref handle);
		}
	}
}