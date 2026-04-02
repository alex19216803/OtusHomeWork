using System.Diagnostics;
using System.Text.Json;
using Newtonsoft.Json;
using OtusHomeWork7Serialize;

// ─────────────────────────────────────────────
// Configuration
// ─────────────────────────────────────────────
const int Iterations = 10_000;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
Console.WriteLine("║  OtusHomeWork 7 – Serialization & Reflection Benchmark       ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
Console.WriteLine();
Console.WriteLine($"Iterations : {Iterations:N0}");
Console.WriteLine();

var sample = F.Get();
Console.WriteLine($"Sample object : {sample}");
Console.WriteLine();

Console.WriteLine("─── Custom Reflection CSV Serializer ─────────────────────────");

// --- Serialization ---
string csvResult = string.Empty;

var sw = Stopwatch.StartNew();
for (int i = 0; i < Iterations; i++)
    csvResult = ReflectionCsvSerializer.Serialize(sample);
sw.Stop();

long reflSerializeMs = sw.ElapsedMilliseconds;
long reflSerializeTicks = sw.ElapsedTicks;

Console.WriteLine("CSV output:");
Console.WriteLine(csvResult);
Console.WriteLine();
Console.WriteLine($"[Reflection] Serialize   {Iterations:N0} iterations : {reflSerializeMs} ms  ({reflSerializeTicks} ticks)");

// --- Deserialization ---
F? csvDeserialized = null;

sw.Restart();
for (int i = 0; i < Iterations; i++)
    csvDeserialized = ReflectionCsvSerializer.Deserialize<F>(csvResult);
sw.Stop();

long reflDeserializeMs = sw.ElapsedMilliseconds;
long reflDeserializeTicks = sw.ElapsedTicks;

Console.WriteLine($"[Reflection] Deserialize {Iterations:N0} iterations : {reflDeserializeMs} ms  ({reflDeserializeTicks} ticks)");
Console.WriteLine($"Deserialized object: {csvDeserialized}");
Console.WriteLine();

Console.Write("[Reflection] Time to write CSV to console: ");
sw.Restart();
Console.WriteLine(csvResult);
sw.Stop();
Console.WriteLine($"  → Console.WriteLine took: {sw.ElapsedMilliseconds} ms  ({sw.ElapsedTicks} ticks)");
Console.WriteLine();


//  System.Text.Json
Console.WriteLine("─── System.Text.Json ─────────────────────────────────────────");

string systemJsonResult = string.Empty;

sw.Restart();
for (int i = 0; i < Iterations; i++)
    systemJsonResult = System.Text.Json.JsonSerializer.Serialize(sample);
sw.Stop();

long stjSerializeMs = sw.ElapsedMilliseconds;
long stjSerializeTicks = sw.ElapsedTicks;

Console.WriteLine($"JSON output (System.Text.Json): {systemJsonResult}");
Console.WriteLine($"[System.Text.Json] Serialize   {Iterations:N0} iterations : {stjSerializeMs} ms  ({stjSerializeTicks} ticks)");


F? stjDeserialized = null;

sw.Restart();
for (int i = 0; i < Iterations; i++)
    stjDeserialized = System.Text.Json.JsonSerializer.Deserialize<F>(systemJsonResult);
sw.Stop();

long stjDeserializeMs = sw.ElapsedMilliseconds;
long stjDeserializeTicks = sw.ElapsedTicks;

Console.WriteLine($"[System.Text.Json] Deserialize {Iterations:N0} iterations : {stjDeserializeMs} ms  ({stjDeserializeTicks} ticks)");
Console.WriteLine($"Deserialized object: {stjDeserialized}");
Console.WriteLine();

//  Newtonsoft.Json
Console.WriteLine("─── Newtonsoft.Json ──────────────────────────────────────────");

string newtonsoftResult = string.Empty;

sw.Restart();
for (int i = 0; i < Iterations; i++)
    newtonsoftResult = JsonConvert.SerializeObject(sample);
sw.Stop();

long nsjSerializeMs = sw.ElapsedMilliseconds;
long nsjSerializeTicks = sw.ElapsedTicks;

Console.WriteLine($"JSON output (Newtonsoft): {newtonsoftResult}");
Console.WriteLine($"[Newtonsoft.Json] Serialize   {Iterations:N0} iterations : {nsjSerializeMs} ms  ({nsjSerializeTicks} ticks)");

F? nsjDeserialized = null;

sw.Restart();
for (int i = 0; i < Iterations; i++)
    nsjDeserialized = JsonConvert.DeserializeObject<F>(newtonsoftResult);
sw.Stop();

long nsjDeserializeMs = sw.ElapsedMilliseconds;
long nsjDeserializeTicks = sw.ElapsedTicks;

Console.WriteLine($"[Newtonsoft.Json] Deserialize {Iterations:N0} iterations : {nsjDeserializeMs} ms  ({nsjDeserializeTicks} ticks)");
Console.WriteLine($"Deserialized object: {nsjDeserialized}");
Console.WriteLine();

//  Summary Table
Console.WriteLine("══════════════════════════════════════════════════════════════");
Console.WriteLine($"  SUMMARY   ({Iterations:N0} iterations each)");
Console.WriteLine("══════════════════════════════════════════════════════════════");
Console.WriteLine($"  {"Serializer",-22} {"Serialize (ms)",16}  {"Deserialize (ms)",18}");
Console.WriteLine($"  {"─────────────────────",-22} {"──────────────",16}  {"────────────────",18}");
Console.WriteLine($"  {"My Reflection CSV",-22} {reflSerializeMs,16}  {reflDeserializeMs,18}");
Console.WriteLine($"  {"System.Text.Json",-22} {stjSerializeMs,16}  {stjDeserializeMs,18}");
Console.WriteLine($"  {"Newtonsoft.Json",-22} {nsjSerializeMs,16}  {nsjDeserializeMs,18}");
Console.WriteLine("══════════════════════════════════════════════════════════════");
Console.WriteLine();
Console.WriteLine("Press any key to exit...");
Console.ReadKey();
