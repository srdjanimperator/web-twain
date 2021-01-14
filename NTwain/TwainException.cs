﻿using NTwain.Data;
using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace NTwain
{
    /// <summary>
    /// Represents a general exception with TWAIN.
    /// </summary>
    [Serializable]
    public class TwainException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TwainException"/> class.
        /// </summary>
        public TwainException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TwainException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public TwainException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TwainException" /> class.
        /// </summary>
        /// <param name="returnCode">The return code.</param>
        /// <param name="message">The message.</param>
        public TwainException(ReturnCode returnCode, string message)
            : this(returnCode, message, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TwainException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public TwainException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TwainException" /> class.
        /// </summary>
        /// <param name="returnCode">The return code.</param>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public TwainException(ReturnCode returnCode, string message, Exception innerException)
            : base(message, innerException)
        {
            ReturnCode = returnCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TwainException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="info"/> parameter is null.
        /// </exception>
        /// <exception cref="T:System.Runtime.Serialization.SerializationException">
        /// The class name is null or <see cref="P:System.Exception.HResult"/> is zero (0).
        /// </exception>
        protected TwainException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            if (info != null)
            {
                ReturnCode = (ReturnCode)info.GetUInt16("RC");
            }
        }

        /// <summary>
        /// When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo"/> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// The <paramref name="info"/> parameter is a null reference (Nothing in Visual Basic).
        /// </exception>
        /// <PermissionSet>
        /// 	<IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*"/>
        /// 	<IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="SerializationFormatter"/>
        /// </PermissionSet>
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info != null)
            {
                info.AddValue("RC", ReturnCode);
            }
            base.GetObjectData(info, context);
        }

        /// <summary>
        /// Gets the return code from the TWAIN operation if applicable.
        /// </summary>
        /// <value>
        /// The return code.
        /// </value>
        public ReturnCode ReturnCode { get; private set; }
    }
}
