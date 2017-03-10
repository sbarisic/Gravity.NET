using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GravityNET {
	[UnmanagedFunctionPointer(Native.CConv, CharSet = Native.CSet)]
	public delegate void GravityErrorCallback(ErrorType EType, string Description, ErrorDesc EDesc, IntPtr Userdata);

	[UnmanagedFunctionPointer(Native.CConv, CharSet = Native.CSet)]
	public delegate void GravityLogCallback(string Msg, IntPtr Userdata);

	[UnmanagedFunctionPointer(Native.CConv, CharSet = Native.CSet)]
	public delegate string GravityLoadFileCallback(string File, ref IntPtr Size, ref uint FileID, IntPtr Userdata);

	public enum ErrorType {
		ERROR_NONE = 0,
		ERROR_SYNTAX,
		ERROR_SEMANTIC,
		ERROR_RUNTIME,
		ERROR_IO,
		WARNING
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct ErrorDesc {
		public int Code;
		public int LineNo;
		public int ColNo;
		public int FileID;
		public int Offset;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct GravityDelegate {
		public IntPtr Userdata;

		public GravityLogCallback LogCallback;
		public GravityErrorCallback ErrorCallback;
		public IntPtr UnitTestCallback;
		public IntPtr ParserCallback;
		public IntPtr PreCodeCallback;
		public GravityLoadFileCallback LoadFileCallback;
		public IntPtr FilenameCallback;

		public IntPtr BridgeInitInstance;
		public IntPtr BridgeSetValue;
		public IntPtr BridgeGetValue;
		public IntPtr BridgeSetUndef;
		public IntPtr BridgeGetUndef;
		public IntPtr BridgeExecute;
		public IntPtr BridgeSize;
		public IntPtr BridgeFree;
	}

	// GravityVM and GravityCompiler are opaque, don't need Ptr suffix
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct GravityVM {
		public IntPtr Pointer;

		public GravityVM(GravityDelegate D) {
			Pointer = Native.GravityVM_New(D).Pointer;
		}

		public static implicit operator bool(GravityVM VM) {
			return VM.Pointer != IntPtr.Zero;
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct GravityCompiler {
		public IntPtr Pointer;

		public GravityCompiler(GravityDelegate D) {
			Pointer = Native.GravityCompiler_Create(D).Pointer;
		}

		public static implicit operator bool(GravityCompiler C) {
			return C.Pointer != IntPtr.Zero;
		}
	}

	// Normal 
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct GravityClosurePtr {
		public IntPtr Pointer;

		public static implicit operator bool(GravityClosurePtr C) {
			return C.Pointer != IntPtr.Zero;
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct GravityClass {
		public IntPtr IsA;
	}

	[StructLayout(LayoutKind.Explicit)]
	public unsafe struct GravityValue {
		const int Offset = 8;

		[FieldOffset(0)]
		public GravityClass* IsA;

		[FieldOffset(Offset)]
		public IntPtr Ptr;
		[FieldOffset(Offset)]
		public int N;
		[FieldOffset(Offset)]
		public double F;
	}

	public static class Native {
		internal const string DllName = "GravityLang";
		internal const CallingConvention CConv = CallingConvention.Cdecl;
		internal const CharSet CSet = CharSet.Ansi;

		// VM

		[DllImport(DllName, EntryPoint = "gravity_vm_new", CallingConvention = CConv, CharSet = CSet)]
		public static extern GravityVM GravityVM_New(GravityDelegate D);

		[DllImport(DllName, EntryPoint = "gravity_vm_run", CallingConvention = CConv, CharSet = CSet)]
		public static extern bool Run(this GravityVM VM, GravityClosurePtr Closure);

		[DllImport(DllName, EntryPoint = "gravity_vm_result", CallingConvention = CConv, CharSet = CSet)]
		public static extern GravityValue Result(this GravityVM VM);

		[DllImport(DllName, EntryPoint = "gravity_vm_time", CallingConvention = CConv, CharSet = CSet)]
		public static extern double Time(this GravityVM VM);

		// Compiler

		[DllImport(DllName, EntryPoint = "gravity_compiler_create", CallingConvention = CConv, CharSet = CSet)]
		public static extern GravityCompiler GravityCompiler_Create(GravityDelegate D);

		[DllImport(DllName, EntryPoint = "gravity_compiler_run", CallingConvention = CConv, CharSet = CSet)]
		public static extern GravityClosurePtr Run(this GravityCompiler Compiler, string Src, IntPtr Len, uint FileID, bool IsStatic);

		[DllImport(DllName, EntryPoint = "gravity_compiler_transfer", CallingConvention = CConv, CharSet = CSet)]
		public static extern void Transfer(this GravityCompiler Compiler, GravityVM VM);

		// Shared

		[DllImport(DllName, EntryPoint = "gravity_value_dump", CallingConvention = CConv, CharSet = CSet)]
		public static extern void Dump(this GravityValue V, StringBuilder Buffer, ushort Len);

		public static string Dump(this GravityValue V, int MaxLen = 1024) {
			StringBuilder SB = new StringBuilder(1024);
			V.Dump(SB, (ushort)SB.MaxCapacity);
			return SB.ToString();
		}
	}
}