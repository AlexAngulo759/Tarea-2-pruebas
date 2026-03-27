using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Proyecto_Grafos.Core.Interfaces;
using Proyecto_Grafos.Core.Models;
using Proyecto_Grafos.Services;
using Proyecto_Grafos.Services.Validation;
using Proyecto_Grafos.UI.Forms;

namespace Proyecto_Grafos
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // --- MODO BENCHMARK ---
            // Uso: Proyecto Grafos.exe -bench -size 1000 -output resultado.txt
            if (args.Length > 0 && args[0] == "-bench")
            {
                int size = 100;
                string outputFile = "bench_result.txt";

                for (int i = 1; i < args.Length; i++)
                {
                    if (args[i] == "-size" && i + 1 < args.Length)
                        int.TryParse(args[i + 1], out size);
                    else if (args[i] == "-output" && i + 1 < args.Length)
                        outputFile = args[i + 1];
                }

                RunBenchmark(size, outputFile);
                return;
            }

            // --- MODO NORMAL (UI) ---
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        private static void RunBenchmark(int size, string outputFile)
        {
            var results = new List<string>();
            var currentProcess = Process.GetCurrentProcess();

            try
            {
                // ── 1. Inicializar servicios ──────────────────────────────────────
                IFamilyGraph familyGraph = new FamilyGraph();
                IValidationService validator = new GraphValidator(familyGraph);
                var graphService = new GraphService(familyGraph, validator);
                var layoutService = new LayoutService();
                var drawingService = new DrawingService();

                // ── 2. Snapshot inicial de CPU y memoria ──────────────────────────
                GC.Collect();
                long memBefore = GC.GetTotalMemory(true);
                TimeSpan cpuBefore = currentProcess.TotalProcessorTime;

                // ── 3. Benchmark: Agregar N personas ─────────────────────────────
                var swAdd = Stopwatch.StartNew();

                for (int i = 0; i < size; i++)
                {
                    string name = $"Persona_{i}";
                    graphService.AddPerson(
                        name,
                        latitude: 9.9 + i * 0.001,
                        longitude: -84.1 + i * 0.001,
                        cedula: $"{100000000 + i}",
                        fechaNacimiento: new DateTime(1980, 1, 1).AddDays(i),
                        estaVivo: true,
                        fechaFallecimiento: null,
                        photoPath: ""
                    );

                    if (i > 0)
                        graphService.AddRelationship($"Persona_{i - 1}", name);
                }

                swAdd.Stop();
                results.Add($"TimeMs:{swAdd.ElapsedMilliseconds}");

                // ── 4. Benchmark: Buscar relaciones ───────────────────────────────
                var swSearch = Stopwatch.StartNew();

                var allPeople = graphService.GetPeople();
                for (int i = 0; i < allPeople.Count; i++)
                {
                    string name = allPeople.Get(i);
                    _ = graphService.GetParents(name);
                    _ = graphService.GetChildren(name);
                }

                swSearch.Stop();
                results.Add($"SearchTimeMs:{swSearch.ElapsedMilliseconds}");

                // ── 5. Benchmark: Calcular layout ─────────────────────────────────
                var peopleList = new List<string>();
                for (int i = 0; i < allPeople.Count; i++)
                    peopleList.Add(allPeople.Get(i));

                var swLayout = Stopwatch.StartNew();
                var visualNodes = layoutService.CalculateLayout(peopleList, graphService);
                swLayout.Stop();
                results.Add($"LayoutTimeMs:{swLayout.ElapsedMilliseconds}");

                // ── 6. Benchmark: DrawNode ─────────────────────────────────────────
                // Se usa un bitmap en memoria como superficie de dibujo (no necesita UI)
                // Se mide cuanto tarda en dibujar todos los nodos una vez
                long drawNodeTimeMs = 0;
                if (visualNodes != null && visualNodes.Count > 0)
                {
                    using (var bitmap = new Bitmap(1920, 1080))
                    using (var g = Graphics.FromImage(bitmap))
                    {
                        // Warm-up: dibujar el primer nodo fuera de la medicion
                        // para evitar que la inicializacion de GDI+ afecte los resultados
                        drawingService.DrawTree(g, visualNodes, graphService);

                        var swDraw = Stopwatch.StartNew();
                        drawingService.DrawTree(g, visualNodes, graphService);
                        swDraw.Stop();
                        drawNodeTimeMs = swDraw.ElapsedMilliseconds;
                    }
                }
                results.Add($"DrawTreeTimeMs:{drawNodeTimeMs}");

                // ── 7. CPU time total ─────────────────────────────────────────────
                currentProcess.Refresh();
                long cpuTimeMs = (long)(currentProcess.TotalProcessorTime - cpuBefore).TotalMilliseconds;
                results.Add($"CpuTimeMs:{cpuTimeMs}");

                // ── 8. Peak working set ───────────────────────────────────────────
                long peakWorkingSetMB = currentProcess.PeakWorkingSet64 / (1024 * 1024);
                results.Add($"PeakWorkingSetMB:{peakWorkingSetMB}");

                // ── 9. Memoria heap .NET ──────────────────────────────────────────
                long gcMemoryMB = (GC.GetTotalMemory(false) - memBefore) / (1024 * 1024);
                if (gcMemoryMB < 0) gcMemoryMB = 0;
                results.Add($"PeakMemory:{gcMemoryMB}");

                results.Add($"Size:{size}");
                results.Add("Status:OK");

                (drawingService as IDisposable)?.Dispose();
            }
            catch (Exception ex)
            {
                results.Add("TimeMs:0");
                results.Add("SearchTimeMs:0");
                results.Add("LayoutTimeMs:0");
                results.Add("DrawTreeTimeMs:0");
                results.Add("CpuTimeMs:0");
                results.Add("PeakWorkingSetMB:0");
                results.Add("PeakMemory:0");
                results.Add($"Size:{size}");
                results.Add("Status:ERROR");
                results.Add($"Error:{ex.Message}");
            }

            // ── 10. Escribir resultados ───────────────────────────────────────────
            try
            {
                string dir = Path.GetDirectoryName(outputFile);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                File.WriteAllLines(outputFile, results);
            }
            catch
            {
                foreach (var line in results)
                    Console.WriteLine(line);
            }
        }
    }
}