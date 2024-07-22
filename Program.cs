﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace KeybindFinder
{
    internal enum ControllerKeybind
    {
        None,	
        DPadUp,
        DPadDown,		
        DPadLeft,		
        DPadRight,		
        Start,	
        Back,	
        LeftThumb,	
        RightThumb,	
        LeftShoulder,	
        RightShoulder,	
        A,		
        B,		
        X,		
        Y,	
    }
    
    internal class Program
    {
        public static void Main(string[] args)
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            string plugins_path = $"{currentDirectory}/Plugins";
            string lspdfr_plugins_path = $"{currentDirectory}/Plugins/LSPDFR";
            
            if (!Directory.Exists(plugins_path) | !Directory.Exists(lspdfr_plugins_path))
            {
                WriteLineInColor("This exe is not in the main game directory.", ConsoleColor.DarkRed);
                while (true)
                {
                    Thread.Sleep(0);
                }
            }
            SearchFiles(currentDirectory);
            while (true)
            {
                Console.WriteLine("Type in any keybind to see matching keybinds. It has to match what you would write in the ini. Type in combo(all lowercase) if you want to see which mod uses a combination of letters. quit to exit.");
                var input = Console.ReadLine();
                Console.WriteLine("----------");
                if (input.ToLower().Equals("quit"))
                {
                    return;
                }
                
                (bool, int) ck = CheckKeys(input);
                (bool, int) cck = CheckControllerKeys(input);
                if (!ck.Item1 && !cck.Item1)
                {
                    InvalidLog();
                    continue;
                }
                if (ck.Item2 == 0 && cck.Item2 == 0)
                {
                    WriteLineInColor($"No keys found for {input}", ConsoleColor.DarkGreen);
                }
                Console.WriteLine("----------");
            }
        }

        static void InvalidLog()
        {
            WriteLineInColor("Invalid keybind", ConsoleColor.DarkRed);
            Console.WriteLine("----------");
        }

        private static (bool, int) CheckControllerKeys(string input)
        {
            if (!Enum.TryParse(input, out ControllerKeybind c)) return (false,0);
            if (IniReader.keybinds.ContainsKey(c))
            {
                WriteLineInColor($"Found {IniReader.keybinds[c].Count} keys for {c}", ConsoleColor.DarkYellow);
                Console.WriteLine(String.Join("\n", IniReader.keybinds[c]));
                return (true,IniReader.keybinds[c].Count);
            }
            return (true, 0);

        }
         private static (bool, int) CheckKeys(string input)
        {
            if (!Enum.TryParse(input, out Keys c)) return (false,0);
            if (IniReader.keybinds.ContainsKey(c))
            {
                WriteLineInColor($"Found {IniReader.keybinds[c].Count} keys for {c}", ConsoleColor.DarkYellow);
                Console.WriteLine(String.Join("\n", IniReader.keybinds[c]));
                return (true,IniReader.keybinds[c].Count);
            }

            return (true,0);
        }
         
        static void WriteLineInColor(string value, ConsoleColor color)
        {
            // Write an entire line to the console with the string.
            Console.ForegroundColor = color;
            Console.WriteLine(value.PadRight(Console.WindowWidth - 1));
            // Reset the color.
            Console.ResetColor();
        }
        
        static void SearchFiles(string directory, string fileExtension = "*.ini")
        {
            try
            {
                // Get files in the current directory
                string[] files = Directory.GetFiles(directory, fileExtension);
                foreach (string file in files)
                {
                    Console.WriteLine($"Reading file {GetFileNameWithTwoFolders(file)}");
                    IniReader.ReadIni(file);
                }

                // Get subdirectories
                string[] subdirectories = Directory.GetDirectories(directory);
                foreach (string subdirectory in subdirectories)
                {
                    SearchFiles(subdirectory, fileExtension); // Recursively search in subdirectories
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Access denied to directory: {directory}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
        internal static string GetFileNameWithTwoFolders(string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            string directory = Path.GetDirectoryName(filePath);

            if (directory == null)
            {
                return fileName; // No directory in the path, return only the file name
            }

            string[] parts = directory.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            int partsCount = parts.Length;

            // Ensure there are at least two folders in the path
            if (partsCount >= 2)
            {
                string twoFolders = Path.Combine(parts[partsCount - 2], parts[partsCount - 1]);
                return Path.Combine(twoFolders, fileName);
            }

            return fileName; // Less than two folders in the path, return only the file name
        }
        
    }


    internal static class IniReader
    {
        internal static Dictionary<Enum, List<string>> keybinds = new Dictionary<Enum, List<string>>();
        
        internal static void ReadIni(string file)
        {
            string[] lines = File.ReadAllLines(file);
            foreach (string line in lines)
            {
                if (line.Contains("="))
                {
                    string[] split = line.Split('=');
                    if (split.Length == 2)
                    {
                        string key = split[1].Trim();
                        if (int.TryParse(key, out _)) continue;
                        string value = $"[{Program.GetFileNameWithTwoFolders(file)}] {split[0].Trim()}";
                        if (Enum.TryParse(key, out Keys k))
                        {
                            if (keybinds.ContainsKey(k))
                            {
                                keybinds[k].Add(value);
                                continue;
                            }
                            keybinds.Add(k, new List<string>() {value});
                            continue;
                        } 
                        if (Enum.TryParse(key, out ControllerKeybind c))
                        {
                            if (keybinds.ContainsKey(c))
                            {
                                keybinds[c].Add(value);
                                continue;
                            }
                            keybinds.Add(c, new List<string>() {value});
                        }
                    }
                }
            }
        }

        internal static void PrintDictionary()
        {
            foreach(KeyValuePair<Enum, List<string>> kvp in keybinds)
            {
                if (kvp.Key.ToString().ToLower().Equals("none")) continue;
                Console.WriteLine($"{kvp.Key} : {String.Join(",", kvp.Value.ToArray())}");
            }
        }
    }
}