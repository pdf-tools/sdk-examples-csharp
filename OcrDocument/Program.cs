/****************************************************************************
 *
 * File:            Program.cs
 *
 * Usage:           PdfToolsOcrDocument <ocrEngineName> <language> <inputPath> <outputPath>
 *                  
 * Title:           OCR a PDF document
 *                  
 * Description:     Apply OCR to a PDF document to make scanned content
 *                  searchable. Text is recognized from images, existing text
 *                  is updated with correct Unicode, and tagging is added for
 *                  accessibility.
 *                  
 * Author:          PDF Tools AG
 *
 * Copyright:       Copyright (C) 2026 PDF Tools AG, Switzerland
 *                  Permission to use, copy, modify, and distribute this
 *                  software and its documentation for any purpose and without
 *                  fee is hereby granted, provided that the above copyright
 *                  notice appear in all copies and that both that copyright
 *                  notice and this permission notice appear in supporting
 *                  documentation. This software is provided "as is" without
 *                  express or implied warranty.
 *
 ***************************************************************************/

using System;
using System.IO;
using PdfTools;
using PdfTools.Pdf;
using PdfTools.Ocr;

namespace PdfToolsOcrDocument
{
    class Program
    {
        static void Usage()
        {
            Console.WriteLine("Usage: PdfToolsOcrDocument <ocrEngineName> <language> <inputPath> <outputPath>");
        }

        static void Main(string[] args)
        {
            // Check command line parameters
            if (args.Length < 4 || args.Length > 4)
            {
                Usage();
                return;
            }

            try
            {
                // By default, a test license key is active. In this case, a watermark is added to the output. 
                // If you have a license key, please uncomment the following call and set the license key.
                // Sdk.Initialize("insert-license-key-here");

                OcrDocument(args[0], args[1], args[2], args[3]);
                Console.WriteLine("Execution successful.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void OcrDocument(string ocrEngineName, string language, string inPath, string outPath)
        {
            // Create the OCR engine
            using var engine = Engine.Create(ocrEngineName);

            // Set the language(s) for OCR recognition (e.g. "German,English")
            engine.Languages = language;

            // Open input document
            using var inStr = File.OpenRead(inPath);
            using var inDoc = Document.Open(inStr);

            // Configure OCR options
            var options = new OcrOptions();

            // Configure image OCR: recognize text from scanned images
            options.ImageOptions.Mode = ImageProcessingMode.UpdateText;
            options.ImageOptions.RemoveOnlyInvisibleOcrText = true;
            options.ImageOptions.DeskewScan = true;
            options.ImageOptions.RotateScan = true;

            // Configure text OCR: update non-extractable text with correct Unicode
            options.TextOptions.Mode = TextProcessingMode.Update;
            options.TextOptions.SkipMode = TextSkipMode.KnownSymbolic;
            options.TextOptions.UnicodeSource = UnicodeSource.InstalledFont;

            // Configure page OCR: process all pages and add tagging for accessibility
            options.PageOptions.Mode = PageProcessingMode.All;
            options.PageOptions.Tagging = TaggingMode.Auto;

            // Create the OCR processor and add a warning handler
            var processor = new Processor();
            processor.Warning += (s, e) =>
            {
                Console.WriteLine("- {0}: {1} ({2}{3})",
                    e.Category, e.Message, e.Context, e.PageNo > 0 ? " page " + e.PageNo : "");
            };

            // Create stream for output file
            using var outStr = File.Create(outPath);

            // Process the document with OCR
            using var outDoc = processor.Process(inDoc, engine, outStr, options);
        }
    }
}
