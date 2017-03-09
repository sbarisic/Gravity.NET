using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using GravityNET;
using System.IO;

namespace TestThingie {
	class Program {
		static void OnError(ErrorType EType, string Desc, ErrorDesc EDesc, IntPtr Userdata) {
			if (EType == ErrorType.ERROR_RUNTIME)
				Console.Write("RUNTIME ERROR: ");
			else
				Console.Write("{0} on {1} ({2}, {3}): ", EType, EDesc.FileID, EDesc.LineNo, EDesc.ColNo);

			Console.WriteLine(Desc);
		}

		static void Main(string[] args) {
			Console.Title = "Gravity.NET Test";

			GravityDelegate D = new GravityDelegate();
			D.ErrorCallback = OnError;
			GravityVM VM = Native.gravity_vm_new(D);

			GravityCompiler Compiler = Native.gravity_compiler_create(D);

			string Src = File.ReadAllText("test.gravity");
			Console.WriteLine("{0}\n", Src);
			GravityClosurePtr ClosureThingie = Native.gravity_compiler_run(Compiler, Src, (IntPtr)Src.Length, 0, true);
			if (!ClosureThingie)
				throw new Exception("Wat, could not compile?");

			Native.gravity_compiler_transfer(Compiler, VM);
			if (Native.gravity_vm_run(VM, ClosureThingie)) {
				GravityValue V = Native.gravity_vm_result(VM);

				StringBuilder Res = new StringBuilder(1024);
				Native.gravity_value_dump(V, Res, (ushort)Res.MaxCapacity);
				Console.WriteLine("RESULT: {0} (in {1} ms)", Res.ToString(), Native.gravity_vm_time(VM));
			}

			Console.WriteLine("Done!");
			Console.ReadLine();
		}
	}
}