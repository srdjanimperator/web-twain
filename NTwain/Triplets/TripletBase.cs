﻿using NTwain.Internals;
using System;

namespace NTwain.Triplets
{
	/// <summary>
	/// Base class for grouping triplet operations messages.
	/// </summary>
	public abstract class TripletBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TripletBase" /> class.
		/// </summary>
		/// <param name="session">The session.</param>
		/// <exception cref="System.ArgumentNullException"></exception>
        internal TripletBase(ITwainSessionInternal session)
		{
			if (session == null) { throw new ArgumentNullException("session"); }
			Session = session;
		}

		/// <summary>
		/// Gets the twain session.
		/// </summary>
		/// <value>
		/// The session.
		/// </value>
        internal ITwainSessionInternal Session { get; private set; }
	}
}
