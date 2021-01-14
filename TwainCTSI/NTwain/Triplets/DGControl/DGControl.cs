﻿using NTwain.Data;
using NTwain.Internals;
using System;
namespace NTwain.Triplets
{
    /// <summary>
    /// Represents <see cref="DataGroups.Control"/>.
	/// </summary>
	public sealed class DGControl
	{
        ITwainSessionInternal _session;
        internal DGControl(ITwainSessionInternal session)
		{
			if (session == null) { throw new ArgumentNullException("session"); }
			_session = session;
		}

		Callback _callback;
		internal Callback Callback
		{
			get
			{
				if (_callback == null) { _callback = new Callback(_session); }
				return _callback;
			}
		}

		Callback2 _callback2;
		internal Callback2 Callback2
		{
			get
			{
				if (_callback2 == null) { _callback2 = new Callback2(_session); }
				return _callback2;
			}
		}
        Capability _capability;
        /// <summary>
        /// Gets the operations defined for DAT_CAPABILITY.
        /// </summary>
		public Capability Capability
		{
			get
			{
				if (_capability == null) { _capability = new Capability(_session); }
				return _capability;
			}
        }
        CapabilityCustom _capabilityCust;
        /// <summary>
        /// Gets the operations defined for a custom DAT_* value with capability data.
        /// </summary>
        public CapabilityCustom CapabilityCustom
        {
            get
            {
                if (_capabilityCust == null) { _capabilityCust = new CapabilityCustom(_session); }
                return _capabilityCust;
            }
        }
        CustomDSData _customDSData;
		internal CustomDSData CustomDSData
		{
			get
			{
				if (_customDSData == null) { _customDSData = new CustomDSData(_session); }
				return _customDSData;
			}
		}
		DeviceEvent _deviceEvent;
		internal DeviceEvent DeviceEvent
		{
			get
			{
				if (_deviceEvent == null) { _deviceEvent = new DeviceEvent(_session); }
				return _deviceEvent;
			}
		}
		EntryPoint _entryPoint;
		internal EntryPoint EntryPoint
		{
			get
			{
				if (_entryPoint == null) { _entryPoint = new EntryPoint(_session); }
				return _entryPoint;
			}
		}
		Event _event;
		internal Event Event
		{
			get
			{
				if (_event == null) { _event = new Event(_session); }
				return _event;
			}
		}
        FileSystem _fileSys;
        /// <summary>
        /// Gets the operations defined for DAT_FILESYSTEM.
        /// </summary>
		public FileSystem FileSystem
		{
			get
			{
				if (_fileSys == null) { _fileSys = new FileSystem(_session); }
				return _fileSys;
			}
		}
		Identity _identity;
		internal Identity Identity
		{
			get
			{
				if (_identity == null) { _identity = new Identity(_session); }
				return _identity;
			}
		}
		Parent _parent;
		internal Parent Parent
		{
			get
			{
				if (_parent == null) { _parent = new Parent(_session); }
				return _parent;
			}
		}
        PassThru _passThru;
        /// <summary>
        /// Gets the operations defined for DAT_PASSTHRU.
        /// </summary>
		public PassThru PassThru
		{
			get
			{
				if (_passThru == null) { _passThru = new PassThru(_session); }
				return _passThru;
			}
		}
		PendingXfers _pendingXfers;
		internal PendingXfers PendingXfers
		{
			get
			{
				if (_pendingXfers == null) { _pendingXfers = new PendingXfers(_session); }
				return _pendingXfers;
			}
		}
        SetupFileXfer _setupFileXfer;
        /// <summary>
        /// Gets the operations defined for DAT_SETUPFILEXFER.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Xfer")]
        public SetupFileXfer SetupFileXfer
		{
			get
			{
				if (_setupFileXfer == null) { _setupFileXfer = new SetupFileXfer(_session); }
				return _setupFileXfer;
			}
		}
        SetupMemXfer _setupMemXfer;
        /// <summary>
        /// Gets the operations defined for DAT_SETUPMEMXFER.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Xfer"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Mem")]
        public SetupMemXfer SetupMemXfer
		{
			get
			{
				if (_setupMemXfer == null) { _setupMemXfer = new SetupMemXfer(_session); }
				return _setupMemXfer;
			}
		}
		Status _status;
		internal Status Status
		{
			get
			{
				if (_status == null) { _status = new Status(_session); }
				return _status;
			}
		}
		StatusUtf8 _statusUtf8;
		internal StatusUtf8 StatusUtf8
		{
			get
			{
				if (_statusUtf8 == null) { _statusUtf8 = new StatusUtf8(_session); }
				return _statusUtf8;
			}
		}
		UserInterface _userInterface;
		internal UserInterface UserInterface
		{
			get
			{
				if (_userInterface == null) { _userInterface = new UserInterface(_session); }
				return _userInterface;
			}
		}
        XferGroup _xferGroup;
        /// <summary>
        /// Gets the operations defined for DAT_XFERGROUP.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Xfer")]
        public XferGroup XferGroup
		{
			get
			{
				if (_xferGroup == null) { _xferGroup = new XferGroup(_session); }
				return _xferGroup;
			}
		}
	}
}
