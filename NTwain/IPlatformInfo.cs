﻿using System;
using System.IO;
namespace NTwain
{
    /// <summary>
    /// Contains various platform requirements and conditions for TWAIN.
    /// </summary>
    public interface IPlatformInfo
    {
        /// <summary>
        /// Gets a value indicating whether the applicable TWAIN DSM library exists in the operating system.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the TWAIN DSM; otherwise, <c>false</c>.
        /// </value>
        bool DsmExists { get; }

        /// <summary>
        /// Gets the expected TWAIN DSM dll path.
        /// </summary>
        /// <value>
        /// The expected DSM path.
        /// </value>
        string ExpectedDsmPath { get; }

        /// <summary>
        /// Gets a value indicating whether the application is running in 64-bit.
        /// </summary>
        /// <value>
        /// <c>true</c> if the application is 64-bit; otherwise, <c>false</c>.
        /// </value>
        bool IsApp64Bit { get; }

        /// <summary>
        /// Gets a value indicating whether this library is supported on current OS.
        /// Check the other platform properties to determine the reason if this is false.
        /// </summary>
        /// <value>
        /// <c>true</c> if this library is supported; otherwise, <c>false</c>.
        /// </value>
        bool IsSupported { get; }
        /// <summary>
        /// Gets a value indicating whether the lib is expecting to use new DSM.
        /// </summary>
        /// <value>
        ///   <c>true</c> if using the new DSM; otherwise, <c>false</c>.
        /// </value>
        bool UseNewWinDSM { get; }

        /// <summary>
        /// Gets or sets a value indicating whether to prefer using the new DSM on Windows over old twain_32 dsm if applicable.
        /// </summary>
        /// <value>
        ///   <c>true</c> to prefer new DSM; otherwise, <c>false</c>.
        /// </value>
        bool PreferNewDSM { get; set; }

        /// <summary>
        /// Gets a value indicating whether the current runtime is mono.
        /// </summary>
        /// <value>
        /// <c>true</c> if the current runtime is on mono; otherwise, <c>false</c>.
        /// </value>
        bool IsOnMono { get; }

        /// <summary>
        /// Gets a value indicating whether the current OS is windows.
        /// </summary>
        /// <value>
        /// <c>true</c> if the current OS is windows; otherwise, <c>false</c>.
        /// </value>
        bool IsWindows { get; }
        /// <summary>
        /// Gets a value indicating whether the current OS is linux.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the current OS is linux; otherwise, <c>false</c>.
        /// </value>
        bool IsLinux { get; }

        /// <summary>
        /// Gets the <see cref="IMemoryManager"/> for communicating with data sources.
        /// This should only be used when a <see cref="TwainSession"/> is open.
        /// </summary>
        /// <value>
        /// The memory manager.
        /// </value>
        IMemoryManager MemoryManager { get; }

        /// <summary>
        /// Gets or sets the log used by NTwain.
        /// </summary>
        /// <value>
        /// The log.
        /// </value>
        ILog Log { get; set; }
    }
}
