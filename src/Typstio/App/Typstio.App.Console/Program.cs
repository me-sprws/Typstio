using System.Diagnostics;
using System.Reactive.Linq;
using Serilog;
using Typstio.Core;
using Typstio.Core.Extensions;
using Typstio.Core.Functions;
using Typstio.Core.Functions.Colors;
using Typstio.Core.Functions.Containers;
using Typstio.Core.Functions.Text;
using Typstio.Core.Models;
using Typstio.Core.Services;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .MinimumLevel.Debug()
    .CreateLogger();

var cts = new CancellationTokenSource();

var shutdown = Observable.FromEventPattern<ConsoleCancelEventHandler, ConsoleCancelEventArgs>(
    h => Console.CancelKeyPress += h,
    h => Console.CancelKeyPress -= h
).Take(1);

shutdown.Subscribe(_ =>
{
    cts.Cancel();
    cts.Dispose();
});

Log.Information("Started. Writing typist content by C#.");

var document = new ContentWriter();

document.SetRuleLine(new TextRule(size: "18pt", font: "Atkinson Hyperlegible"));
document.SetRuleLine(new BoxRule(inset: "15pt"));
document.SetRuleLine(SetRule.FromElementFunction(new Table(ArraySegment<string>.Empty, ArraySegment<Content>.Empty, align: "horizon", inset: "10pt")));
document.NextLine();

document.Write(new Box(c => c.WriteString("Hello, Typst!"), new Rgb("#ff4136")));
document.WriteBlock();

document.Write(CreateUserTable());

Log.Information("Starting to compile the final pdf file. Typst will load automatically.");

const string output = "./code.pdf";
await new TypstCompiler().PdfAsync(document, "./code.txt", output).ConfigureAwait(false);

Process.Start("explorer", Path.GetFullPath(output));

Log.Information("Done. Shutdown.");

Console.WriteLine(CodeGenerator.ToCode(document));

return;

ITypstFunction CreateUserTable()
{
    var items = new Content[]
    {
        _ => { },
        c => c.Write(new Strong("Name")),
        c => c.Write(new Strong("Phone")),
        
        c => c.WriteString("1"),
        c => c.WriteString("Sparrow"),
        c => c.WriteString("+79531345309").Linebreak()
              .WriteString("+89231365311")
    };
    
    return new Table(("auto", "1fr", "1fr"), items, inset: "10pt", align: "horizon");
}