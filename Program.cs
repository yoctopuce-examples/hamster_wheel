using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Yoctopuce_Hamster_Wheel
{
    class Program
    {
        // -url localhost -pwmInput YPWMRX01-408ED.pwmInput1 -display YD128X64-FF240.display -nextButton YD128X64-FF240.anButton3 -prevButton YD128X64-FF240.anButton6 -diameter 190mm -inactivity 10s -export_csv test.csv


        static int Main(string[] args)
        {
            string url = "usb";
            string pwmHwId = "";
            string displayHwId = "";
            string nextButtonHwId = "next";
            string prevButtonHwId = "prev";
            string csvfile = "";
            uint diameterMm = 0;
            uint initactivityDelay = 10;

            for (int i = 0; i < args.Length; i++) {
                if (i + 1 >= args.Length) {
                    Console.Error.WriteLine("missing argument for " + args[i]);
                    printUsage();
                    return 1;
                }

                switch (args[i]) {
                    case "--diameter":
                        diameterMm = UInt32.Parse(args[i + 1]);
                        break;
                    case "--pwmInput":
                        pwmHwId = args[i + 1];
                        break;
                    case "--inactivity":
                        initactivityDelay = UInt32.Parse(args[i + 1]);
                        break;
                    case "--display":
                        displayHwId = args[i + 1];
                        break;
                    case "--nextButton":
                        nextButtonHwId = args[i + 1];
                        break;
                    case "--prevButton":
                        prevButtonHwId = args[i + 1];
                        break;
                    case "--export_csv":
                        csvfile = args[i + 1];
                        break;
                    case "--url":
                        url = args[i + 1];
                        break;
                    default:
                        Console.Error.WriteLine("Unknown option " + args[i]);
                        printUsage();
                        return 1;
                }

                i++;
            }

            if (diameterMm == 0) {
                Console.Error.WriteLine("Missing --diameter option");
                printUsage();
                return 1;
            }

            if (pwmHwId == "") {
                Console.Error.WriteLine("Missing --pwmInput option");
                printUsage();
                return 1;
            }

            var program = new HamsterController(url, pwmHwId, displayHwId, nextButtonHwId, prevButtonHwId, diameterMm, initactivityDelay,csvfile);
            return program.RunForever();
        }

        private static void printUsage()
        {
            Console.Out.WriteLine("Mandatory: ");
            Console.Out.WriteLine("--diameter <diameter> ");
            Console.Out.WriteLine("	The hamster wheel diameter in mm ");
            Console.Out.WriteLine("--pwmInput <hardwareID or logical name> ");
            Console.Out.WriteLine("	The hardwareId or logical name of the pwmInput used to count");
            Console.Out.WriteLine("	the wheel rotation. ");
            Console.Out.WriteLine("");
            Console.Out.WriteLine("");
            Console.Out.WriteLine("Optional:");
            Console.Out.WriteLine("--inactivity <delay>");
            Console.Out.WriteLine("	The number of second of inactivity that trigger the end of a");
            Console.Out.WriteLine("	run. By default this value is 10 seconds");
            Console.Out.WriteLine("--display <hardwareID or logical name> ");
            Console.Out.WriteLine("	The hardwareId or logical name of the Yocto-Display or Yocto-MaxiDisplay used ");
            Console.Out.WriteLine("	to display the statistics. If not set the application will use the first one");
            Console.Out.WriteLine("	available.");
            Console.Out.WriteLine("--nextButton <hardwareID or logical name> ");
            Console.Out.WriteLine("	The hardwareId or logical name of the anButton used for \"next\". By default the ");
            Console.Out.WriteLine("	application will search for the anButton function named \"next\".");
            Console.Out.WriteLine("--prevButton <hardwareID or logical name> ");
            Console.Out.WriteLine("	The hardwareId or logical name of the anButton used for \"prev\". By default the ");
            Console.Out.WriteLine("	application will search for the anButton function named \"prev\".");
            Console.Out.WriteLine("--export_csv <filename>");
            Console.Out.WriteLine("	If set append all hamster run to a CSV file.");
            Console.Out.WriteLine("--url <url> ");
            Console.Out.WriteLine("	The URL of YoctoHub if an YoctoHub is used. By default the application");
            Console.Out.WriteLine("	will use Yoctopuce devices that are connected by USB.");
        }
    }
}