﻿// This file contains all the structs defined in the twain.h file.

using System;
using System.Runtime.InteropServices;

// The following TWAIN basic types are mapped with "using"
// to aid in mapping against the twain.h file using copy-paste.
// Consumers will not see those names.

using TW_BOOL = System.UInt16; // unsigned short

// use HandleRef instead?
using TW_HANDLE = System.IntPtr; // HANDLE, todo: should really be uintptr?
using TW_MEMREF = System.IntPtr; // LPVOID
using TW_UINTPTR = System.UIntPtr; // UINT_PTR

using TW_INT16 = System.Int16; // short
using TW_INT32 = System.Int32; // long
using TW_INT8 = System.SByte;  // char

using TW_UINT16 = System.UInt16; // unsigned short
using TW_UINT32 = System.UInt32; // unsigned long
using TW_UINT8 = System.Byte;    // unsigned char


// This mono doc is awesome. An interop must-read
// http://www.mono-project.com/Interop_with_Native_Libraries (old)
// http://www.mono-project.com/docs/advanced/pinvoke/ (new url)

//////////////////////////////////
// Data structures that
// are passed to the TWAIN method
// are defined as classes to reduce
// ref/out in the low-level calls. 
// Others continue to be structs.
//////////////////////////////////


namespace NTwain.Data
{
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    partial struct TWFix32
    {
        TW_INT16 _whole;
        TW_UINT16 _frac;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    partial struct TWFrame
    {
        TWFix32 _left;
        TWFix32 _top;
        TWFix32 _right;
        TWFix32 _bottom;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    partial struct TWDecodeFunction
    {
        TWFix32 _startIn;
        TWFix32 _breakIn;
        TWFix32 _endIn;
        TWFix32 _startOut;
        TWFix32 _breakOut;
        TWFix32 _endOut;
        TWFix32 _gamma;
        TWFix32 _sampleCount;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    partial struct TWTransformStage
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        TWDecodeFunction[] _decode;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
        TWFix32[] _mix;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    partial class TWArray
    {
        TW_UINT16 _itemType;
        TW_UINT32 _numItems;
        object[] _itemList;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2),
    BestFitMapping(false, ThrowOnUnmappableChar = true)]
    partial class TWAudioInfo
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = TwainConst.String255)]
        string _name;

        TW_UINT32 _reserved;
    }

    
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    partial class TWCallback
    {
        [MarshalAs(UnmanagedType.FunctionPtr)]
        CallbackDelegate _callBackProc;
        TW_UINT32 _refCon;
        TW_INT16 _message;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    partial class TWCallback2
    {
        [MarshalAs(UnmanagedType.FunctionPtr)]
        CallbackDelegate _callBackProc;
        TW_UINTPTR _refCon;
        TW_INT16 _message;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    partial class TWCapability
    {
        TW_UINT16 _cap;
        TW_UINT16 _conType;
        TW_HANDLE _hContainer;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    partial struct TWCiePoint
    {
        TWFix32 _x;
        TWFix32 _y;
        TWFix32 _z;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    partial class TWCieColor
    {
        TW_UINT16 _colorSpace;
        TW_INT16 _lowEndian;
        TW_INT16 _deviceDependent;
        TW_INT32 _versionNumber;
        TWTransformStage _stageABC;
        TWTransformStage _stageLMN;
        TWCiePoint _whitePoint;
        TWCiePoint _blackPoint;
        TWCiePoint _whitePaper;
        TWCiePoint _blackInk;

        // TODO: may be totally wrong
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
        TWFix32[] _samples;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    partial class TWCustomDSData
    {
        TW_UINT32 _infoLength;
        TW_HANDLE _hData;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2),
    BestFitMapping(false, ThrowOnUnmappableChar = true)]
    partial class TWDeviceEvent
    {
        TW_UINT32 _event;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = TwainConst.String255)]
        string _deviceName;
        
        TW_UINT32 _batteryMinutes;
        TW_INT16 _batteryPercentage;
        TW_INT32 _powerSupply;
        TWFix32 _xResolution;
        TWFix32 _yResolution;
        TW_UINT32 _flashUsed2;
        TW_UINT32 _automaticCapture;
        TW_UINT32 _timeBeforeFirstCapture;
        TW_UINT32 _timeBetweenCaptures;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    partial struct TWElement8
    {
        TW_UINT8 _index;
        TW_UINT8 _channel1;
        TW_UINT8 _channel2;
        TW_UINT8 _channel3;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    partial class TWEnumeration
    {
        TW_UINT16 _itemType;
        TW_UINT32 _numItems;
        TW_UINT32 _currentIndex;
        TW_UINT32 _defaultIndex;
        object[] _itemList;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    partial class TWEvent
    {
        TW_MEMREF _pEvent;
        TW_UINT16 _tWMessage;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    partial struct TWInfo
    {
        TW_UINT16 _infoID;
        TW_UINT16 _itemType;
        TW_UINT16 _numItems;
        TW_UINT16 _returnCode;
        //TW_UINTPTR _item;
        TW_HANDLE _item; // easier to work with intptr
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    partial class TWExtImageInfo
    {
        TW_UINT32 _numInfos;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 200)]
        TWInfo[] _info;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2),
    BestFitMapping(false, ThrowOnUnmappableChar = true)]
    partial class TWFileSystem
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = TwainConst.String255)]
        string _inputName;
        
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = TwainConst.String255)]
        string _outputName;

        TW_MEMREF _context;
        TW_BOOL _subdirectories;
        TW_INT32 _fileType;
        TW_UINT32 _size;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = TwainConst.String32)]
        string _createTimeDate;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = TwainConst.String32)]
        string _modifiedTimeDate;

        TW_UINT32 _freeSpace;
        TW_INT32 _newImageSize;
        TW_UINT32 _numberOfFiles;
        TW_UINT32 _numberOfSnippets;
        TW_UINT32 _deviceGroupMask;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 508)]
        TW_INT8[] _reserved;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    partial class TWGrayResponse
    {
        // TODO: may be totally wrong
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
        TWElement8[] _response;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2),
    BestFitMapping(false, ThrowOnUnmappableChar = true)]
    partial struct TWVersion
    {
        TW_UINT16 _majorNum;
        TW_UINT16 _minorNum;
        TW_UINT16 _language;
        TW_UINT16 _country;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = TwainConst.String32)]
        string _info;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2),
    BestFitMapping(false, ThrowOnUnmappableChar = true)]
    partial class TWIdentity
    {
        TW_UINT32 _id;
        TWVersion _version;
        TW_UINT16 _protocolMajor;
        TW_UINT16 _protocolMinor;
        TW_UINT32 _supportedGroups;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = TwainConst.String32)]
        string _manufacturer;
        
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = TwainConst.String32)]
        string _productFamily;
        
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = TwainConst.String32)]
        string _productName;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    partial class TWImageInfo
    {
        TWFix32 _xResolution;
        TWFix32 _yResolution;
        TW_INT32 _imageWidth;
        TW_INT32 _imageLength;
        TW_INT16 _samplesPerPixel;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        TW_INT16[] _bitsPerSample;
        
        TW_INT16 _bitsPerPixel;
        TW_BOOL _planar;
        TW_INT16 _pixelType;
        TW_UINT16 _compression;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    partial class TWImageLayout
    {
        TWFrame _frame;
        TW_UINT32 _documentNumber;
        TW_UINT32 _pageNumber;
        TW_UINT32 _frameNumber;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    partial struct TWMemory
    {
        // this is not a class due to being embedded by other classes

        TW_UINT32 _flags;
        TW_UINT32 _length;
        TW_MEMREF _theMem;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    partial class TWImageMemXfer
    {
        TW_UINT16 _compression;
        TW_UINT32 _bytesPerRow;
        TW_UINT32 _columns;
        TW_UINT32 _rows;
        TW_UINT32 _xOffset;
        TW_UINT32 _yOffset;
        TW_UINT32 _bytesWritten;
        TWMemory _memory;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    partial class TWJpegCompression
    {
        TW_UINT16 _colorSpace;
        TW_UINT32 _subSampling;
        TW_UINT16 _numComponents;
        TW_UINT16 _restartFrequency;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        TW_UINT16[] _quantMap;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        TWMemory[] _quantTable;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        TW_UINT16[] _huffmanMap;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        TWMemory[] _huffmanDC;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        TWMemory[] _huffmanAC;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    partial class TWOneValue
    {
        TW_UINT16 _itemType;
        TW_UINT32 _item;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    partial class TWPalette8
    {
        TW_UINT16 _numColors;
        TW_UINT16 _paletteType;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        TWElement8[] _colors;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    partial class TWPassThru
    {
        TW_MEMREF _pCommand;
        TW_UINT32 _commandBytes;
        TW_INT32 _direction;
        TW_MEMREF _pData;
        TW_UINT32 _dataBytes;
        TW_UINT32 _dataBytesXfered;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    partial class TWPendingXfers
    {
        TW_UINT16 _count;
        TW_UINT32 _eOJ;

    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    partial class TWRange
    {
        TW_UINT16 _itemType;
        TW_UINT32 _minValue;
        TW_UINT32 _maxValue;
        TW_UINT32 _stepSize;
        TW_UINT32 _defaultValue;
        TW_UINT32 _currentValue;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    partial class TWRgbResponse
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
        TWElement8[] _response;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2),
    BestFitMapping(false, ThrowOnUnmappableChar = true)]
    partial class TWSetupFileXfer
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = TwainConst.String255)]
        string _fileName;

        TW_UINT16 _format;
        TW_INT16 _vRefNum = -1;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    partial class TWSetupMemXfer
    {
        TW_UINT32 _minBufSize;
        TW_UINT32 _maxBufSize;
        TW_UINT32 _preferred;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    partial class TWStatus
    {
        TW_UINT16 _conditionCode;
        TW_UINT16 _data;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    partial class TWStatusUtf8
    {
        // NOTE: rather than embedding the TWStatus directly I'm using its fields instead
        // so the TWStatus could become a class object. If TWStatus changes
        // definition remember to change it here
        TW_UINT16 _conditionCode;
        TW_UINT16 _data;
        TW_UINT32 _size;
        TW_HANDLE _uTF8string;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    partial class TWUserInterface
    {
        TW_BOOL _showUI;
        TW_BOOL _modalUI;
        TW_HANDLE _hParent;
    }

    delegate ReturnCode CallbackDelegate(TWIdentity origin, TWIdentity destination,
            DataGroups dg, DataArgumentType dat, Message msg, TW_MEMREF data);

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    partial class TWEntryPoint
    {
        TW_UINT32 _size;
        // this is not a delegate cuz it's not used by the app
        IntPtr _dSM_Entry;

        [MarshalAs(UnmanagedType.FunctionPtr)]
        MemAllocateDelegate _dSM_MemAllocate;
        [MarshalAs(UnmanagedType.FunctionPtr)]
        MemFreeDelegate _dSM_MemFree;
        [MarshalAs(UnmanagedType.FunctionPtr)]
        MemLockDelegate _dSM_MemLock;
        [MarshalAs(UnmanagedType.FunctionPtr)]
        MemUnlockDelegate _dSM_MemUnlock;

        public delegate TW_HANDLE MemAllocateDelegate(TW_UINT32 size);
        public delegate void MemFreeDelegate(TW_HANDLE handle);
        public delegate TW_MEMREF MemLockDelegate(TW_HANDLE handle);
        public delegate void MemUnlockDelegate(TW_HANDLE handle);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    partial class TWFilterDescriptor
    {
        TW_UINT32 _size;
        TW_UINT32 _hueStart;
        TW_UINT32 _hueEnd;
        TW_UINT32 _saturationStart;
        TW_UINT32 _saturationEnd;
        TW_UINT32 _valueStart;
        TW_UINT32 _valueEnd;
        TW_UINT32 _replacement;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    partial class TWFilter
    {
        TW_UINT32 _size;
        TW_UINT32 _descriptorCount;
        TW_UINT32 _maxDescriptorCount;
        TW_UINT32 _condition;
        TW_HANDLE _hDescriptors;
    }


}
