using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Options;

namespace OpenLED_Windows_Host
{
	class Program
	{
		static void Main(string[] args)
		{
			bool helpshown = false;
			OptionSet options = new OptionSet
			{
				{ "MultiConverter:" + Environment.NewLine + "\tDesigned to convert Office and DWG files to generic forms primarily for use with FactoryLogistics" + Environment.NewLine },

				//{ "o|output=", "Output folder for conversion", (string o) => vm.DestinationFolder = o },
				//{ "i|input=", "Input file(s) and/or folder(s) for conversion, can be set multiple times", (string i) => vm.AddFiles(new string[]{i}) },

				////header
				//{ "it|input-type=", "Type of file to convert from\nPossible Values: " + string.Join(", ", Enum.GetNames(typeof(MainViewModel.SourceFileTypes))).Replace("Null, ", ""), (MainViewModel.SourceFileTypes s) => vm.SourceFileType = s },

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
					//Start conversion

					//Sit here spinning our wheels until worker thread finishes
					//If this application didn't have a GUI, all tasks would have been done on a single thread

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
