using EasyMLEduApp.Examples.MLP;
using EasyMLEduApp.Examples.ReservoirComputing;
using System;

namespace EasyMLEduApp
{
    class Program
    {
        /// <summary>
        /// Application entry point.
        /// </summary>
        static void Main()
        {
            //Run the root menu
            RootMenu();
        }//Main

        private static void RootMenu()
        {
            bool exit = false;
            while (!exit)
            {
                bool wait = true;
                //Root menu
                Console.Clear();
                Console.WriteLine("Main menu:");
                Console.WriteLine("  1. (EasyMLCore.MLP) MLP models code examples...");
                Console.WriteLine("  2. (EasyMLCore.TimeSeries) Reservoir Computer code examples...");
                Console.WriteLine("  P. Playground");
                Console.WriteLine("  X. Exit the application");
                Console.WriteLine();
                Console.WriteLine("  Press the digit or letter of your choice...");
                ConsoleKeyInfo consoleKeyInfo = Console.ReadKey(true);
                Console.Clear();
                switch (consoleKeyInfo.KeyChar.ToString().ToUpperInvariant())
                {
                    case "1":
                        //MLP models code examples sub menu
                        MLPCodeExamplesMenu();
                        wait = false;
                        break;

                    case "2":
                        //Reservoir Computing code examples sub menu
                        ReservoirComputingCodeExamplesMenu();
                        wait = false;
                        break;

                    case "P":
                        try
                        {
                            Playground.Run();
                        }
                        catch (Exception e)
                        {
                            ReportException(e);
                        }
                        break;

                    case "X":
                        exit = true;
                        wait = false;
                        break;

                    default:
                        Console.WriteLine();
                        Console.WriteLine("Undefined choice.");
                        break;

                }//Switch choice

                //Wait for Enter to loop the menu
                if (wait)
                {
                    Console.WriteLine();
                    Console.WriteLine("Press Enter to return to menu...");
                    Console.ReadLine();
                }

            }//Menu loop
            return;
        }

        private static void MLPCodeExamplesMenu()
        {
            bool exit = false;
            while (!exit)
            {
                bool wait = true;
                //Menu
                Console.Clear();
                Console.WriteLine("EasyMLCore.MLP components code examples:");
                Console.WriteLine("  1. Network model solves Boolean algebra.");
                Console.WriteLine("  2. Network, CrossVal, BHS, Stacking and Composite models competition in the Categorical tasks (plus DiagnosticTest).");
                Console.WriteLine("  3. Network, CrossVal, BHS, Stacking and Composite models competition in the Binary tasks (plus serialization/deserialization).");
                Console.WriteLine("  4. Network, CrossVal, BHS, Stacking and Composite models competition in the Regression tasks.");
                Console.WriteLine("  5. Models deep tests.");
                Console.WriteLine("  X. Back to Root menu");
                Console.WriteLine();
                Console.WriteLine("  Press the digit or letter of your choice...");
                ConsoleKeyInfo consoleKeyInfo = Console.ReadKey(true);
                Console.Clear();
                switch (consoleKeyInfo.KeyChar.ToString().ToUpperInvariant())
                {
                    case "1":
                        try
                        {
                            MLPNetworkModelBooleanAlgebra.Run();
                        }
                        catch (Exception e)
                        {
                            ReportException(e);
                        }
                        break;

                    case "2":
                        try
                        {
                            MLPModelsCategoricalCompetition.Run();
                        }
                        catch (Exception e)
                        {
                            ReportException(e);
                        }
                        break;

                    case "3":
                        try
                        {
                            MLPModelsBinaryCompetition.Run();
                        }
                        catch (Exception e)
                        {
                            ReportException(e);
                        }
                        break;

                    case "4":
                        try
                        {
                            MLPModelsRegressionCompetition.Run();
                        }
                        catch (Exception e)
                        {
                            ReportException(e);
                        }
                        break;

                    case "5":
                        try
                        {
                            MLPModelsDeepTests.Run();
                        }
                        catch (Exception e)
                        {
                            ReportException(e);
                        }
                        break;

                    case "X":
                        exit = true;
                        wait = false;
                        break;

                    default:
                        Console.WriteLine();
                        Console.WriteLine("Undefined choice.");
                        break;

                }//Switch choice

                //Wait for Enter to loop the menu
                if (wait)
                {
                    Console.WriteLine();
                    Console.WriteLine("Press Enter to return to menu...");
                    Console.ReadLine();
                }

            }//Menu loop
            return;
        }



        private static void ReservoirComputingCodeExamplesMenu()
        {
            bool exit = false;
            while (!exit)
            {
                bool wait = true;
                //Menu
                Console.Clear();
                Console.WriteLine("Reservoir Computing code examples menu:");
                Console.WriteLine("  1. Reservoir Computer (pattern feeding) Categorical tasks");
                Console.WriteLine("  2. Reservoir Computer (pattern feeding) Binary tasks");
                Console.WriteLine("  3. Reservoir Computer (pattern feeding) Regression tasks");
                Console.WriteLine("  4. Reservoir Computer (pattern feeding) Simultaneous categorical, binary and regression tasks");
                Console.WriteLine("  5. Reservoir Computer (pattern feeding) Deep tests");
                Console.WriteLine("  6. Reservoir Computer (time point feeding) Regression tasks");
                Console.WriteLine("  7. ICANN/PASCAL2 Challenge: MEG MindReading");
                Console.WriteLine("  X. Back to Root menu");
                Console.WriteLine();
                Console.WriteLine("  Press the digit or letter of your choice...");
                ConsoleKeyInfo consoleKeyInfo = Console.ReadKey(true);
                Console.Clear();
                switch (consoleKeyInfo.KeyChar.ToString().ToUpperInvariant())
                {
                    case "1":
                        try
                        {
                            ResCompPFCategoricalTasks.Run();
                        }
                        catch (Exception e)
                        {
                            ReportException(e);
                        }
                        break;

                    case "2":
                        try
                        {
                            ResCompPFBinaryTasks.Run();
                        }
                        catch (Exception e)
                        {
                            ReportException(e);
                        }
                        break;

                    case "3":
                        try
                        {
                            ResCompPFRegressionTasks.Run();
                        }
                        catch (Exception e)
                        {
                            ReportException(e);
                        }
                        break;

                    case "4":
                        try
                        {
                            ResCompPFSimultaneousTasks.Run();
                        }
                        catch (Exception e)
                        {
                            ReportException(e);
                        }
                        break;

                    case "5":
                        try
                        {
                            ResCompPFDeepTests.Run();
                        }
                        catch (Exception e)
                        {
                            ReportException(e);
                        }
                        break;

                    case "6":
                        try
                        {
                            ResCompTPRegressionTasks.Run();
                        }
                        catch (Exception e)
                        {
                            ReportException(e);
                        }
                        break;

                    case "7":
                        try
                        {
                            ResCompMindReadingChallenge.Run();
                        }
                        catch (Exception e)
                        {
                            ReportException(e);
                        }
                        break;

                    case "X":
                        exit = true;
                        wait = false;
                        break;

                    default:
                        Console.WriteLine();
                        Console.WriteLine("Undefined choice.");
                        break;

                }//Switch choice

                //Wait for Enter to loop the menu
                if (wait)
                {
                    Console.WriteLine();
                    Console.WriteLine("Press Enter to return to menu...");
                    Console.ReadLine();
                }

            }//Menu loop
            return;
        }

        /// <summary>
        /// Displays the exception content.
        /// </summary>
        /// <param name="e">An exception to be displayed.</param>
        private static void ReportException(Exception e)
        {
            Console.WriteLine();
            Console.WriteLine("--------------------------------------------------------------");
            while (e != null)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                e = e.InnerException;
                Console.WriteLine("--------------------------------------------------------------");
            }
            Console.ReadLine();
            return;
        }

    }//Program

}//Namespace
