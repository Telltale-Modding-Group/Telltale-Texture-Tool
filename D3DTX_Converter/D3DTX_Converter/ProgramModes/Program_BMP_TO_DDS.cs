﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using D3DTX_Converter.Utilities;
using D3DTX_Converter.DirectX;
using D3DTX_Converter.Main;
using Newtonsoft.Json;

namespace D3DTX_Converter.ProgramModes
{
    public static class Program_BMP_TO_DDS
    {
        public static void Execute()
        {
            //intro message
            ConsoleFunctions.SetConsoleColor(ConsoleColor.Blue, ConsoleColor.White);
            Console.WriteLine("BMP to DDS Texture Converter");

            //-----------------GET TEXTURE FOLDER PATH-----------------
            ConsoleFunctions.SetConsoleColor(ConsoleColor.DarkGray, ConsoleColor.White);
            Console.WriteLine("Enter the folder path with the BMP textures.");

            //texture folder path (containing the path to the textures to be converted)
            string textureFolderPath = Program_Shared.GetFolderPathFromUser();

            //-----------------GET RESULT FOLDER PATH-----------------
            ConsoleFunctions.SetConsoleColor(ConsoleColor.DarkGray, ConsoleColor.White);
            Console.WriteLine("Enter the resulting path where converted DDS textures will be stored.");

            //result folder path (will contain the converted textures)
            string resultFolderPath = Program_Shared.GetFolderPathFromUser();

            //-----------------START CONVERSION-----------------
            //notify the user we are starting
            ConsoleFunctions.SetConsoleColor(ConsoleColor.Black, ConsoleColor.Green);
            Console.WriteLine("Conversion Starting...");

            //we got our paths, so lets begin
            ConvertBulk(textureFolderPath, resultFolderPath);

            //once the process is finished, it will come back here and we will notify the user that we are done
            ConsoleFunctions.SetConsoleColor(ConsoleColor.Black, ConsoleColor.Green);
            Console.WriteLine("Conversion Finished.");
            Console.ResetColor();
        }

        /// <summary>
        /// Begins the conversion process. Gathers the files found in the texture folder path, filters them, and converts each one.
        /// </summary>
        /// <param name="texPath"></param>
        /// <param name="resultPath"></param>
        public static void ConvertBulk(string texPath, string resultPath)
        {
            ConsoleFunctions.SetConsoleColor(ConsoleColor.Black, ConsoleColor.Yellow);
            Console.WriteLine("Collecting Files..."); //notify the user we are collecting files

            //gather the files from the texture folder path into an array
            List<string> textures = new List<string>(Directory.GetFiles(texPath));

            ConsoleFunctions.SetConsoleColor(ConsoleColor.Black, ConsoleColor.Yellow);
            Console.WriteLine("Filtering Textures..."); //notify the user we are filtering the array

            //filter the array so we only get .bmp files
            textures = IOManagement.FilterFiles(textures, ".bmp");

            //if no bmp files were found, abort the program from going on any further (we don't have any files to convert!)
            if (textures.Count < 1)
            {
                ConsoleFunctions.SetConsoleColor(ConsoleColor.Black, ConsoleColor.Red);
                Console.WriteLine("No .bmp files were found, aborting."); //notify the user we found x amount of bmp files in the array
                Console.ResetColor();
                return;
            }

            ConsoleFunctions.SetConsoleColor(ConsoleColor.Black, ConsoleColor.Green);
            Console.WriteLine("Found {0} Textures.", textures.Count.ToString()); //notify the user we found x amount of bmp files in the array
            Console.WriteLine("Starting...");//notify the user we are starting

            //Thread[] threads = new Thread[textures.Count];

            //run a loop through each of the found textures and convert each one
            for (int i = 0; i < textures.Count; i++)
            {
                //build the path for the resulting file
                string textureFileName = Path.GetFileName(textures[i]); //get the file name of the file + extension
                string textureFileNameOnly = Path.GetFileNameWithoutExtension(textures[i]);
                string textureResultPath = resultPath + "/" + textureFileNameOnly + Main_Shared.ddsExtension; //add the file name to the resulting folder path, this is where our converted file will be placed

                ConsoleFunctions.SetConsoleColor(ConsoleColor.Black, ConsoleColor.White);
                Console.WriteLine("||||||||||||||||||||||||||||||||");
                ConsoleFunctions.SetConsoleColor(ConsoleColor.Black, ConsoleColor.Blue);
                Console.WriteLine("Converting '{0}'...", textureFileName); //notify the user are converting 'x' file.
                Console.ResetColor();

                //runs the main method for converting the texture
                ConvertTextureFile(textures[i], textureResultPath);

                ConsoleFunctions.SetConsoleColor(ConsoleColor.Black, ConsoleColor.Green);
                Console.WriteLine("Finished converting '{0}'...", textureFileName); //notify the user we finished converting 'x' file.
                ConsoleFunctions.SetConsoleColor(ConsoleColor.Black, ConsoleColor.White);
            }
        }

        /// <summary>
        /// The main function for reading and converting said .bmp into a .dds file
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="destinationFile"></param>
        public static void ConvertTextureFile(string sourceFile, string destinationFile)
        {
            //GenericImageFormats.ConvertDDS_To_PSD(destinationFile);
        }
    }
}
