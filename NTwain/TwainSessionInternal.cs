﻿using NTwain.Data;
using NTwain.Internals;
using NTwain.Interop;
using NTwain.Triplets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace NTwain
{
    // for internal pieces since the main twain session file is getting too long

    partial class TwainSession : ITwainSessionInternal, IWinMessageFilter
    {
        #region ITwainSessionInternal Members

        MessageLoopHook _msgLoopHook;
        MessageLoopHook ITwainSessionInternal.MessageLoopHook { get { return _msgLoopHook; } }

        /// <summary>
        /// Gets the app id used for the session.
        /// </summary>
        /// <value>The app id.</value>
        TWIdentity ITwainSessionInternal.AppId { get { return _appId; } }

        bool _closeRequested;
        bool ITwainSessionInternal.CloseDSRequested { get { return _closeRequested; } }

        void ITwainSessionInternal.UpdateCallback()
        {
            if (State < 4)
            {
                _callbackObj = null;
            }
            else
            {
                ReturnCode rc = ReturnCode.Failure;
                // per the spec (8-10) apps for 2.2 or higher uses callback2 so try this first
                if (_appId.ProtocolMajor > 2 || (_appId.ProtocolMajor >= 2 && _appId.ProtocolMinor >= 2))
                {
                    var cb = new TWCallback2(HandleCallback);
                    rc = ((ITwainSessionInternal)this).DGControl.Callback2.RegisterCallback(cb);

                    if (rc == ReturnCode.Success)
                    {
                        PlatformInfo.Current.Log.Debug("Registered callback2 OK.");
                        _callbackObj = cb;
                    }
                }

                if (rc != ReturnCode.Success)
                {
                    // always try old callback
                    var cb = new TWCallback(HandleCallback);

                    rc = ((ITwainSessionInternal)this).DGControl.Callback.RegisterCallback(cb);

                    if (rc == ReturnCode.Success)
                    {
                        PlatformInfo.Current.Log.Debug("Registered callback OK.");
                        _callbackObj = cb;
                    }
                }
            }
        }

        void ITwainSessionInternal.ChangeState(int newState, bool notifyChange)
        {
            _state = newState;
            if (notifyChange)
            {
                OnPropertyChanged("State");
                SafeAsyncSyncableRaiseOnEvent(OnStateChanged, StateChanged);
            }
        }

        ICommittable ITwainSessionInternal.GetPendingStateChanger(int newState)
        {
            return new TentativeStateCommitable(this, newState);
        }

        void ITwainSessionInternal.ChangeCurrentSource(DataSource source)
        {
            CurrentSource = source;
            DisableReason = Message.Null;
            OnPropertyChanged("CurrentSource");
            SafeAsyncSyncableRaiseOnEvent(OnSourceChanged, SourceChanged);
        }

        void ITwainSessionInternal.SafeSyncableRaiseEvent(DataTransferredEventArgs e)
        {
            SafeSyncableRaiseOnEvent(OnDataTransferred, DataTransferred, e);
        }
        void ITwainSessionInternal.SafeSyncableRaiseEvent(TransferErrorEventArgs e)
        {
            SafeSyncableRaiseOnEvent(OnTransferError, TransferError, e);
        }
        void ITwainSessionInternal.SafeSyncableRaiseEvent(TransferReadyEventArgs e)
        {
            SafeSyncableRaiseOnEvent(OnTransferReady, TransferReady, e);
        }

        DGAudio _dgAudio;
        DGAudio ITwainSessionInternal.DGAudio
        {
            get
            {
                if (_dgAudio == null) { _dgAudio = new DGAudio(this); }
                return _dgAudio;
            }
        }

        DGControl _dgControl;
        DGControl ITripletControl.DGControl { get { return DGControl; } }
        protected DGControl DGControl
        {
            get
            {
                if (_dgControl == null) { _dgControl = new DGControl(this); }
                return _dgControl;
            }
        }


        DGImage _dgImage;
        DGImage ITripletControl.DGImage { get { return DGImage; } }
        protected DGImage DGImage
        {
            get
            {
                if (_dgImage == null) { _dgImage = new DGImage(this); }
                return _dgImage;
            }
        }

        DGCustom _dgCustom;
        DGCustom ITripletControl.DGCustom { get { return DGCustom; } }
        protected DGCustom DGCustom
        {
            get
            {
                if (_dgCustom == null) { _dgCustom = new DGCustom(this); }
                return _dgCustom;
            }
        }


        /// <summary>
        /// Enables the source to start transferring.
        /// </summary>
        /// <param name="mode">The mode.</param>
        /// <param name="modal">if set to <c>true</c> any driver UI will display as modal.</param>
        /// <param name="windowHandle">The window handle if modal.</param>
        /// <returns></returns>
        ReturnCode ITwainSessionInternal.EnableSource(SourceEnableMode mode, bool modal, IntPtr windowHandle)
        {
            _closeRequested = false;
            DisableReason = Message.Null;
            var rc = ReturnCode.Failure;

            _msgLoopHook?.Invoke(() =>
            {
                PlatformInfo.Current.Log.Debug("Thread {0}: EnableSource with {1}.", Thread.CurrentThread.ManagedThreadId, mode);


                _twui = new TWUserInterface();
                _twui.ShowUI = mode == SourceEnableMode.ShowUI;
                _twui.ModalUI = modal;
                _twui.hParent = windowHandle;

                if (mode == SourceEnableMode.ShowUIOnly)
                {
                    rc = ((ITwainSessionInternal)this).DGControl.UserInterface.EnableDSUIOnly(_twui);
                }
                else
                {
                    rc = ((ITwainSessionInternal)this).DGControl.UserInterface.EnableDS(_twui);
                }
            });
            return rc;
        }

        bool _disabling;
        ReturnCode ITwainSessionInternal.DisableSource()
        {
            var rc = ReturnCode.Failure;
            if (!_disabling) // temp hack as a workaround to this being called from multiple threads (xfer logic & closedsreq msg)
            {
                _disabling = true;
                try
                {
                    _msgLoopHook?.Invoke(() =>
                    {
                        PlatformInfo.Current.Log.Debug("Thread {0}: DisableSource.", Thread.CurrentThread.ManagedThreadId);

                        rc = ((ITwainSessionInternal)this).DGControl.UserInterface.DisableDS(_twui);
                        if (rc == ReturnCode.Success)
                        {
                            SafeAsyncSyncableRaiseOnEvent(OnSourceDisabled, SourceDisabled);
                        }
                    });
                }
                finally
                {
                    _disabling = false;
                }
            }
            return rc;
        }


        #endregion

        #region IWinMessageFilter Members

        /// <summary>
        /// Checks and handles the message if it's a TWAIN message.
        /// </summary>
        /// <param name="hwnd">The window handle.</param>
        /// <param name="msg">The message.</param>
        /// <param name="wParam">The w parameter.</param>
        /// <param name="lParam">The l parameter.</param>
        /// <returns>
        /// true if handled internally.
        /// </returns>
        public bool IsTwainMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam)
        {
            bool handled = false;
            // this handles the message from a typical WndProc message loop and check if it's from the TWAIN source.
            if (_state >= 5)
            {
                // transform it into a pointer for twain
                IntPtr msgPtr = IntPtr.Zero;
                try
                {
                    var winMsg = new MESSAGE(hwnd, msg, wParam, lParam);

                    // no need to do another lock call when using marshal alloc
                    msgPtr = Marshal.AllocHGlobal(Marshal.SizeOf(winMsg));
                    Marshal.StructureToPtr(winMsg, msgPtr, false);

                    var evt = new TWEvent();
                    evt.pEvent = msgPtr;
                    if (handled = (((ITwainSessionInternal)this).DGControl.Event.ProcessEvent(evt) == ReturnCode.DSEvent))
                    {
                        PlatformInfo.Current.Log.Debug("Thread {0}: HandleWndProcMessage at state {1} with MSG={2}.", Thread.CurrentThread.ManagedThreadId, State, evt.TWMessage);

                        HandleSourceMsg(evt.TWMessage);
                    }
                }
                finally
                {
                    if (msgPtr != IntPtr.Zero) { Marshal.FreeHGlobal(msgPtr); }
                }
            }
            return handled;
        }

        #endregion

        #region handle twain ds message


        ReturnCode HandleCallback(TWIdentity origin, TWIdentity destination, DataGroups dg, DataArgumentType dat, Message msg, IntPtr data)
        {
            if (origin != null && CurrentSource != null && origin.Id == CurrentSource.Identity.Id && _state >= 5)
            {
                PlatformInfo.Current.Log.Debug("Thread {0}: CallbackHandler at state {1} with MSG={2}.", Thread.CurrentThread.ManagedThreadId, State, msg);
                // spec says we must handle this on the thread that enabled the DS.
                // by using the internal dispatcher this will be the case.

                // In any event the trick to get this thing working is to return from the callback first
                // before trying to process the msg or there will be unpredictable errors.

                // changed to sync invoke instead of begininvoke for hp scanjet.
                if (origin.ProductName.IndexOf("scanjet", StringComparison.OrdinalIgnoreCase) > -1)
                {
                    _msgLoopHook?.Invoke(() =>
                    {
                        HandleSourceMsg(msg);
                    });
                }
                else
                {
                    _msgLoopHook?.BeginInvoke(() =>
                    {
                        HandleSourceMsg(msg);
                    });
                }
                return ReturnCode.Success;
            }
            return ReturnCode.Failure;
        }

        // final method that handles msg from the source, whether it's from wndproc or callbacks
        void HandleSourceMsg(Message msg)
        {
            PlatformInfo.Current.Log.Debug("Got TWAIN msg " + msg);
            switch (msg)
            {
                case Message.XferReady:
                    if (State < 6)
                    {
                        State = 6;
                    }
                    TransferLogic.DoTransferRoutine(this);
                    break;
                case Message.DeviceEvent:
                    TWDeviceEvent de;
                    var rc = ((ITwainSessionInternal)this).DGControl.DeviceEvent.Get(out de);
                    if (rc == ReturnCode.Success)
                    {
                        SafeSyncableRaiseOnEvent(OnDeviceEvent, DeviceEvent, new DeviceEventArgs(de));
                    }
                    break;
                case Message.CloseDSReq:
                case Message.CloseDSOK:
                    DisableReason = msg;
                    // even though it says closeDS it's really disable.
                    // dsok is sent if source is enabled with uionly

                    // some sources send this at other states so do a step down
                    if (State > 5)
                    {
                        // rather than do a close here let the transfer logic handle the close down now
                        //ForceStepDown(4);
                        _closeRequested = true;
                    }
                    else if (State == 5)
                    {
                        // needs this state check since some source sends this more than once
                        ((ITwainSessionInternal)this).DisableSource();
                    }
                    break;
            }
        }

        #endregion
    }
}
