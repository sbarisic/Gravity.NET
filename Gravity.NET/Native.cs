using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using gravity_int_t = System.Int32;
using gravity_float_t = System.Single;
using System.Security.Cryptography;

namespace GravityNET {
	[UnmanagedFunctionPointer(Native.CConv, CharSet = Native.CSet)]
	public delegate void GravityErrorCallback(IntPtr vm, error_type_t error_type, string message, error_desc_t error_desc, IntPtr xdata);

	[UnmanagedFunctionPointer(Native.CConv, CharSet = Native.CSet)]
	public delegate void GravityLogCallback(string Msg, IntPtr Userdata);

	[UnmanagedFunctionPointer(Native.CConv, CharSet = Native.CSet)]
	public delegate string GravityLoadFileCallback(string File, ref IntPtr Size, ref uint FileID, IntPtr Userdata);

	[UnmanagedFunctionPointer(Native.CConv, CharSet = Native.CSet)]
	public unsafe delegate bool vm_filter_cb(IntPtr obj);

	[UnmanagedFunctionPointer(Native.CConv, CharSet = Native.CSet)]
	public unsafe delegate void vm_transfer_cb(gravity_vm* vm, IntPtr obj);

	[UnmanagedFunctionPointer(Native.CConv, CharSet = Native.CSet)]
	public unsafe delegate void vm_cleanup_cb(gravity_vm* vm);

	public enum error_type_t {
		GRAVITY_ERROR_NONE = 0,
		GRAVITY_ERROR_SYNTAX,
		GRAVITY_ERROR_SEMANTIC,
		GRAVITY_ERROR_RUNTIME,
		GRAVITY_ERROR_IO,
		GRAVITY_WARNING,
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct error_desc_t {
		public uint LineNo;
		public uint ColNo;
		public uint FileID;
		public uint Offset;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct gravity_delegate_t {
		public IntPtr xdata;
		public bool report_null_errors;     // by default messages sent to null objects are silently ignored (if this flag is false)
		public bool disable_gccheck_1;      // memory allocations are protected so it could be useful to automatically check gc when enabled is restored

		// callbacks
		public IntPtr log_callback;           // log reporting callback
		public IntPtr log_clear;              // log reset callback
		public IntPtr error_callback;         // error reporting callback
		public IntPtr unittest_callback;      // special unit test callback
		public IntPtr parser_callback;        // lexer callback used for syntax highlight
		public IntPtr type_callback;          // callback used to bind a token with a declared type
		public IntPtr precode_callback;       // called at parse time in order to give the opportunity to add custom source code
		public IntPtr loadfile_callback;      // callback to give the opportunity to load a file from an import statement
		public IntPtr filename_callback;      // called while reporting an error in order to be able to convert a fileid to a real filename
		public IntPtr optional_classes;       // optional classes to be exposed to the semantic checker as extern (to be later registered)

		// bridge
		public IntPtr bridge_initinstance;    // init class
		public IntPtr bridge_setvalue;        // setter
		public IntPtr bridge_getvalue;        // getter
		public IntPtr bridge_setundef;        // setter not found
		public IntPtr bridge_getundef;        // getter not found
		public IntPtr bridge_execute;         // execute a method/function
		public IntPtr bridge_blacken;         // blacken obj to be GC friend
		public IntPtr bridge_string;          // instance string conversion
		public IntPtr bridge_equals;          // check if two objects are equals
		public IntPtr bridge_clone;           // clone
		public IntPtr bridge_size;            // size of obj
		public IntPtr bridge_free;            // free obj
	}


	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct gravity_compiler_t {
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct gravity_closure_t {
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct gravity_vm {
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct gnode_t {
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct gravity_fiber_t {
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct gravity_function_t {
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct gravity_gc_t {
		public bool isdark;         // flag to check if object is reachable
		public bool visited;        // flag to check if object has already been counted in memory size
		public IntPtr next;          // to track next object in the linked list
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	public unsafe struct gravity_value_t {
		[FieldOffset(0)]
		public IntPtr isa;

		[FieldOffset(1)]
		public int n;

		[FieldOffset(1)]
		public float f;

		[FieldOffset(1)]
		public IntPtr p;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct gravity_class_s {
		public IntPtr isa;           // to be an object
		public gravity_gc_t gc;             // to be collectable by the garbage collector

		public IntPtr objclass;      // meta class
		public string identifier;    // class name
		public bool has_outer;      // flag used to automatically set ivar 0 to outer class (if any)
		public bool is_struct;      // flag to mark class as a struct
		public bool is_inited;      // flag used to mark already init meta-classes (to be improved)
		public bool unused;         // unused padding byte
		public IntPtr xdata;         // extra bridged data
		public IntPtr superclass;    // reference to the super class
		public string superlook;     // when a superclass is set to extern a runtime lookup must be performed
		public IntPtr htable;        // hash table
		public uint nivars;         // number of instance variables
									//gravity_value_r			inames;			    // ivar names
		public IntPtr ivars;         // static variables
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct gravity_object_t {
		public gravity_class_s gravity_class;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct gravity_class_t {
		public gravity_class_s gravity_class;
	}


	public static unsafe class Native {
		internal const string DllName = "gravity";
		internal const CallingConvention CConv = CallingConvention.Cdecl;
		internal const CharSet CSet = CharSet.Ansi;

		// Compiler

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern gravity_compiler_t* gravity_compiler_create(IntPtr del);

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern gravity_closure_t* gravity_compiler_run(gravity_compiler_t* compiler, string source, ulong len, uint fileid, bool is_static, bool add_debug);

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern IntPtr gravity_compiler_serialize(gravity_compiler_t* compiler, gravity_closure_t* closure);

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern bool gravity_compiler_serialize_infile(gravity_compiler_t* compiler, gravity_closure_t* closure, string path);

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern void gravity_compiler_transfer(gravity_compiler_t* compiler, gravity_vm* vm);

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern gnode_t* gravity_compiler_ast(gravity_compiler_t* compiler);

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern void gravity_compiler_free(gravity_compiler_t* compiler);

		// VM


		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern gravity_vm* gravity_vm_new(IntPtr del);

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern gravity_vm* gravity_vm_newmini();

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern void gravity_vm_set_callbacks(gravity_vm* vm, vm_transfer_cb vm_transfer, vm_cleanup_cb vm_cleanup);

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern void gravity_vm_free(gravity_vm* vm);

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern void gravity_vm_reset(gravity_vm* vm);

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern bool gravity_vm_runclosure(gravity_vm* vm, gravity_closure_t* closure, gravity_value_t sender, gravity_value_t[] parms, ushort nparams);

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern bool gravity_vm_runmain(gravity_vm* vm, gravity_closure_t* closure);

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern void gravity_vm_loadclosure(gravity_vm* vm, gravity_closure_t* closure);

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern void gravity_vm_setvalue(gravity_vm* vm, string key, gravity_value_t value);

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern gravity_value_t gravity_vm_lookup(gravity_vm* vm, gravity_value_t key);

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern gravity_value_t gravity_vm_getvalue(gravity_vm* vm, string key, uint keylen);

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern double gravity_vm_time(gravity_vm* vm);

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern gravity_value_t gravity_vm_result(gravity_vm* vm);

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern gravity_delegate_t* gravity_vm_delegate(gravity_vm* vm);

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern gravity_fiber_t* gravity_vm_fiber(gravity_vm* vm);

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern void gravity_vm_setfiber(gravity_vm* vm, gravity_fiber_t* fiber);

		//[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		//public static extern void gravity_vm_seterror(gravity_vm* vm, const char* format, ...);

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern void gravity_vm_seterror_string(gravity_vm* vm, string str);

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern bool gravity_vm_ismini(gravity_vm* vm);

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern gravity_value_t gravity_vm_keyindex(gravity_vm* vm, uint index);

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern bool gravity_vm_isaborted(gravity_vm* vm);

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern void gravity_vm_setaborted(gravity_vm* vm);

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern gravity_closure_t* gravity_vm_getclosure(gravity_vm* vm);


		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern void gravity_gray_value(gravity_vm* vm, gravity_value_t v);

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern void gravity_gray_object(gravity_vm* vm, IntPtr obj);

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern void gravity_gc_start(gravity_vm* vm);

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern void gravity_gc_setenabled(gravity_vm* vm, bool enabled);

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern void gravity_gc_temppush(gravity_vm* vm, IntPtr obj);

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern void gravity_gc_temppop(gravity_vm* vm);

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern void gravity_gc_tempnull(gravity_vm* vm, IntPtr obj);

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern void gravity_gc_setvalues(gravity_vm* vm, gravity_int_t threshold, gravity_int_t minthreshold, gravity_float_t ratio);


		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern void gravity_vm_transfer(gravity_vm* vm, IntPtr obj);

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern void gravity_vm_cleanup(gravity_vm* vm);

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern void gravity_vm_filter(gravity_vm* vm, vm_filter_cb cleanup_filter);


		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern gravity_closure_t* gravity_vm_loadfile(gravity_vm* vm, string path);

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern gravity_closure_t* gravity_vm_loadbuffer(gravity_vm* vm, string buffer, ulong len);

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern void gravity_vm_initmodule(gravity_vm* vm, gravity_function_t* f);


		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern gravity_closure_t* gravity_vm_fastlookup(gravity_vm* vm, IntPtr c, int index);

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern void gravity_vm_setslot(gravity_vm* vm, gravity_value_t value, uint index);

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern gravity_value_t gravity_vm_getslot(gravity_vm* vm, uint index);

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern void gravity_vm_setdata(gravity_vm* vm, void* data);

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern void* gravity_vm_getdata(gravity_vm* vm);

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern void gravity_vm_memupdate(gravity_vm* vm, gravity_int_t value);

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern gravity_int_t gravity_vm_maxmemblock(gravity_vm* vm);


		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern gravity_value_t gravity_vm_get(gravity_vm* vm, string key);

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern bool gravity_vm_set(gravity_vm* vm, string key, gravity_value_t value);

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern char* gravity_vm_anonymous(gravity_vm* vm);


		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern bool gravity_isopt_class(IntPtr c);

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern void gravity_opt_register(gravity_vm* vm);

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern void gravity_opt_free();

		// Value

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern gravity_value_t gravity_value_from_error(string msg);

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern gravity_value_t gravity_value_from_object(void* obj);

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern gravity_value_t gravity_value_from_int(gravity_int_t n);

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern gravity_value_t gravity_value_from_float(gravity_float_t f);

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern gravity_value_t gravity_value_from_null();

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern gravity_value_t gravity_value_from_undefined();

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern gravity_value_t gravity_value_from_bool(bool b);

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern void gravity_value_dump(gravity_vm* vm, gravity_value_t v, StringBuilder buffer, ushort len);


		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern void gravity_value_dump(gravity_vm* vm, gravity_value_t v, IntPtr buffer, ushort len);

		public static string gravity_value_dump(gravity_vm* vm, gravity_value_t v) {
			StringBuilder sb = new StringBuilder(1024);
			gravity_value_dump(vm, v, sb, (ushort)sb.Capacity);
			return sb.ToString();
		}

		// core functions

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern void gravity_core_init();

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern void gravity_core_register(gravity_vm* vm);

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern bool gravity_iscore_class(IntPtr c);

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern void gravity_core_free();

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern string[] gravity_core_identifiers();

		[DllImport(DllName, CallingConvention = CConv, CharSet = CSet)]
		public static extern IntPtr gravity_core_class_from_name(string name);

		public static bool VALUE_ISA_CLOSURE(gravity_value_t v) {
			throw new NotImplementedException();
		}


		public static IntPtr VALUE_AS_OBJECT(gravity_value_t v) {
			return v.p;
		}

		public static gravity_closure_t* VALUE_AS_CLOSURE(gravity_value_t x) {
			return ((gravity_closure_t*)VALUE_AS_OBJECT(x));
		}
	}
}