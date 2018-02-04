using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Options;
using OpenLED_Windows_Host.LEDModeDrivers;

namespace OpenLED_Windows_Host
{
	class Program
	{
		static void Main(string[] args)
		{
			bool helpshown = false;
			LEDModes ledmode = LEDModes.NULL;
			OptionSet options = new OptionSet
			{
				{ "OpenLED:" + Environment.NewLine + "\tDesigned to control the LED lighting within a PC" + Environment.NewLine },

				//{ "o|output=", "Output folder for conversion", (string o) => vm.DestinationFolder = o },
				//{ "i|input=", "Input file(s) and/or folder(s) for conversion, can be set multiple times", (string i) => vm.AddFiles(new string[]{i}) },

				////header
				{ "m|mode=", "Mode of LED operation " + string.Join(", ", Enum.GetNames(typeof(LEDModes))).Replace("Null, ", ""), (LEDModes m) => ledmode = m },

				//{ "ot|output-type=", "Type of file to convert to\nPossible Values: PDF, PNG, XPS", (string d) =>
				//	{
				//		if(vm.DestinationFileTypes.Contains(d))
				//			vm.DestinationFileType = d;
				//		else
				//		{
				//			Console.WriteLine("Invalid destination type for the source type: " + vm.SourceFileType);
				//			Console.WriteLine("Valid options are: " + string.Join(", ", vm.DestinationFileTypes));
				//		}
				//	}
				//},

				{ "h|help", "show this message and exit", h => { helpshown = true; } },
			};

			try
			{
				//parsing the arguments will execute the methods associated with them, setting up the program for conversion tasks
				options.Parse(args);

				//if help was show, don't run
				if (!helpshown)
				{
					switch(ledmode)
					{
						case (LEDModes.VolumeAndPitchReactive):
							{
								//TODO: make this work
								break;
							}
						case (LEDModes.SingleColor):
							{
								break;
							}
					}

					Console.WriteLine();
				}
				else
					options.WriteOptionDescriptions(Console.Out);
			}
			catch (OptionException e)
			{
				//Will tell the user they are stupid and to get help
				Console.WriteLine(e.Message);
				Console.WriteLine("Try --help for more information.");
				return;
			}
		}
	}
}
