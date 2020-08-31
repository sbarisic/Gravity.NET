using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using GravityNET;
using System.IO;

namespace TestThingie {
	unsafe static class Program {
		static void OnError(IntPtr vm, error_type_t error_type, string message, error_desc_t error_desc, IntPtr xdata) {
			if (error_type == error_type_t.GRAVITY_ERROR_RUNTIME)
				Console.Write("RUNTIME ERROR: ");
			else
				Console.Write("{0} ERROR on {1} ({2}, {3}): ", error_type.ToString(), error_desc.FileID, error_desc.LineNo, error_desc.ColNo);

			Console.WriteLine(message);
		}

		static GravityErrorCallback OnErrorFunc = OnError;

		static void Main(string[] args) {
			gravity_delegate_t del = new gravity_delegate_t();
			del.error_callback = Marshal.GetFunctionPointerForDelegate(OnErrorFunc);

			IntPtr delPtr = (IntPtr)(&del);

			string Source = File.ReadAllText("test.gravity");

			gravity_compiler_t* compiler = Native.gravity_compiler_create(delPtr);
			gravity_closure_t* closure = Native.gravity_compiler_run(compiler, Source, (uint)Source.Length, 0, true, true);

			if (closure == null)
				throw new Exception();

			// setup a new VM and a new fiber
			gravity_vm* vm = Native.gravity_vm_new(delPtr);

			// transfer memory from compiler to VM and then free compiler
			Native.gravity_compiler_transfer(compiler, vm);
			Native.gravity_compiler_free(compiler);

			// load closure into VM context
			Native.gravity_vm_loadclosure(vm, closure);

			// create parameters (that must be boxed) for the closure
			gravity_value_t n1 = Native.gravity_value_from_int(30);
			gravity_value_t n2 = Native.gravity_value_from_int(50);
			gravity_value_t[] parms = new gravity_value_t[] { n1, n2 };

			// lookup add closure
			gravity_value_t add =Native. gravity_vm_getvalue(vm, "add", 3);

			// execute add closure and print result
			if (Native.gravity_vm_runclosure(vm, Native.VALUE_AS_CLOSURE(add), add, parms, 2)) {
				gravity_value_t result = Native.gravity_vm_result(vm);
				Console.Write("add result");
				Native.gravity_value_dump(vm, result, IntPtr.Zero, 0);
			}

			// lookup mul closure
			gravity_value_t mul = Native.gravity_vm_getvalue(vm, "mul", 3);

			// execute mul closure and print result
			if (Native.gravity_vm_runclosure(vm, Native.VALUE_AS_CLOSURE(mul), mul, parms, 2)) {
				gravity_value_t result = Native.gravity_vm_result(vm);
				Console.WriteLine("mul result ");
				Native.gravity_value_dump(vm, result, IntPtr.Zero, 0);
			}

			// free vm and core classes
			Native.gravity_vm_free(vm);
			Native.gravity_core_free();

			Console.WriteLine("Done!");
			Console.ReadLine();
		}
	}
}