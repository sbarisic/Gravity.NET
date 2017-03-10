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

			GravityDelegate D = new GravityDelegate() {
				ErrorCallback = OnError
			};
			GravityVM VM = new GravityVM(D);
			GravityCompiler Compiler = new GravityCompiler(D);

			string Src = File.ReadAllText("test.gravity");
			Console.WriteLine("{0}\n", Src);
			GravityClosurePtr ClosureThingie = Compiler.Run(Src, (IntPtr)Src.Length, 0, true);

			if (!ClosureThingie)
				throw new Exception("Wat, could not compile?");

			Compiler.Transfer(VM);

			if (VM.Run(ClosureThingie)) {
				Console.WriteLine("RESULT: {0} (in {1} ms)", VM.Result().Dump(), VM.Time());
			}

			Console.WriteLine("Done!");
			Console.ReadLine();
		}
	}
}