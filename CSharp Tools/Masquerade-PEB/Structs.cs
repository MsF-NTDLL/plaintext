using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;

/*
This code is based on @FuzzSec https://twitter.com/FuzzySec work Masquerade-PEB.ps1
https://github.com/FuzzySecurity/PowerShell-Suite/blob/master/Masquerade-PEB.ps1
*/ 

namespace modifypeb
{
    public class structs
    {

	[StructLayout(LayoutKind.Sequential)]
    public struct UNICODE_STRING
    {
        public UInt16 Length;
        public UInt16 MaximumLength;
        public IntPtr Buffer;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct _LIST_ENTRY
    {
        public IntPtr Flink;
        public IntPtr Blink;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct _PROCESS_BASIC_INFORMATION
    {
        public IntPtr ExitStatus;
        public IntPtr PebBaseAddress;
        public IntPtr AffinityMask;
        public IntPtr BasePriority;
        public UIntPtr UniqueProcessId;
        public IntPtr InheritedFromUniqueProcessId;
    }
    /// Partial _PEB
    [StructLayout(LayoutKind.Explicit, Size = 64)]
    public struct _PEB
    {
        [FieldOffset(12)]
        public IntPtr Ldr32;
        [FieldOffset(16)]
        public IntPtr ProcessParameters32;
        [FieldOffset(24)]
        public IntPtr Ldr64;
        [FieldOffset(28)]
        public IntPtr FastPebLock32;
        [FieldOffset(32)]
        public IntPtr ProcessParameters64;
        [FieldOffset(56)]
        public IntPtr FastPebLock64;
    }
    /// Partial _PEB_LDR_DATA
    [StructLayout(LayoutKind.Sequential)]
    public struct _PEB_LDR_DATA
    {
        public UInt32 Length;
        public Byte Initialized;
        public IntPtr SsHandle;
        public _LIST_ENTRY InLoadOrderModuleList;
        public _LIST_ENTRY InMemoryOrderModuleList;
        public _LIST_ENTRY InInitializationOrderModuleList;
        public IntPtr EntryInProgress;
    }
    /// Partial _LDR_DATA_TABLE_ENTRY
    [StructLayout(LayoutKind.Sequential)]
    public struct _LDR_DATA_TABLE_ENTRY
    {
        public _LIST_ENTRY InLoadOrderLinks;
        public _LIST_ENTRY InMemoryOrderLinks;
        public _LIST_ENTRY InInitializationOrderLinks;
        public IntPtr DllBase;
        public IntPtr EntryPoint;
        public UInt32 SizeOfImage;
        public UNICODE_STRING FullDllName;
        public UNICODE_STRING BaseDllName;
    }
    public static class Kernel32
    {
        [DllImport("kernel32.dll")]
        public static extern UInt32 GetLastError();
        [DllImport("kernel32.dll")]
        public static extern Boolean VirtualProtectEx(
            IntPtr hProcess,
            IntPtr lpAddress,
            UInt32 dwSize,
            UInt32 flNewProtect,
            ref UInt32 lpflOldProtect);
        [DllImport("kernel32.dll")]
        public static extern Boolean WriteProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            IntPtr lpBuffer,
            UInt32 nSize,
            ref UInt32 lpNumberOfBytesWritten);
    }
    public static class Ntdll
    {
        [DllImport("ntdll.dll")]
        public static extern int NtQueryInformationProcess(
            IntPtr processHandle,
            int processInformationClass,
            ref _PROCESS_BASIC_INFORMATION processInformation,
            int processInformationLength,
            ref int returnLength);
        [DllImport("ntdll.dll")]
        public static extern void RtlEnterCriticalSection(
            IntPtr lpCriticalSection);
        [DllImport("ntdll.dll")]
        public static extern void RtlLeaveCriticalSection(
            IntPtr lpCriticalSection);
    }
}
}
