// Copyright 2006 Alp Toker <alp@atoker.com>
// This software is made available under the MIT License
// See COPYING for details

using System;
using System.Collections.Generic;
using NDesk.DBus;
using org.freedesktop.DBus;

public class Monitor
{
	public static void Main (string[] args)
	{
		Connection bus;

		if (args.Length >= 1) {
			string arg = args[0];

			switch (arg)
			{
				case "--system":
					bus = Bus.System;
					break;
				case "--session":
					bus = Bus.Session;
					break;
				default:
					Console.Error.WriteLine ("Usage: monitor.exe [--system | --session] [watch expressions]");
					Console.Error.WriteLine ("       If no watch expressions are provided, defaults will be used.");
					return;
			}
		} else {
			bus = Bus.Session;
		}

		if (args.Length > 1) {
			//install custom match rules only
			for (int i = 1 ; i != args.Length ; i++)
				bus.AddMatch (args[i]);
		} else {
			//no custom match rules, install the defaults
			bus.AddMatch (MessageFilter.CreateMatchRule (MessageType.Signal));
			bus.AddMatch (MessageFilter.CreateMatchRule (MessageType.MethodReturn));
			bus.AddMatch (MessageFilter.CreateMatchRule (MessageType.Error));
			bus.AddMatch (MessageFilter.CreateMatchRule (MessageType.MethodCall));
		}

		while (true) {
			Message msg = bus.ReadMessage ();
			if (msg == null)
				break;
			PrintMessage (msg);
			Console.WriteLine ();
		}
	}

	internal static void PrintMessage (Message msg)
	{
		Console.WriteLine ("Message:");
		//Console.WriteLine ("\t" + "Endianness: " + msg.Header.Endianness);
		Console.WriteLine ("\t" + "Type: " + msg.Header.MessageType);
		Console.WriteLine ("\t" + "Flags: " + msg.Header.Flags);
		//Console.WriteLine ("\t" + "MajorVersion: " + msg.Header.MajorVersion);
		Console.WriteLine ("\t" + "Serial: " + msg.Header.Serial);
		//foreach (HeaderField hf in msg.HeaderFields)
		//	Console.WriteLine ("\t" + hf.Code + ": " + hf.Value);
		Console.WriteLine ("\tHeader Fields:");
		foreach (KeyValuePair<FieldCode,object> field in msg.Header.Fields)
			Console.WriteLine ("\t\t" + field.Key + ": " + field.Value);

		if (msg.Body != null) {
			Console.WriteLine ("\tBody:");
			MessageReader reader = new MessageReader (msg);

			//TODO: this needs to be done more intelligently
			try {
				foreach (DType dtype in msg.Signature.GetBuffer ()) {
					if (dtype == DType.Invalid)
						continue;
					object arg;
					reader.GetValue (dtype, out arg);
					Console.WriteLine ("\t\t" + dtype + ": " + arg);
				}
			} catch {
				Console.WriteLine ("\t\tmonitor is too dumb to decode message body");
			}
		}
	}
}
