using System.Diagnostics;
using System.IO.Compression;
using Typstio.Core.Models;

namespace Typstio.Core.Services;

public class TypstCompiler(string? typstPath = null)
{
    const string Version = "v0.13.1";
    const string Name = "typst-x86_64-pc-windows-msvc";
    const string DownloadUri = $"/{Version}/{Name}.zip";

    static string LocalTypstPath => ApplicationTypstDir + "/" + Version + "/" + Name + "/typst.exe";
    static string ApplicationTypstDir => AppDomain.CurrentDomain.BaseDirectory + "/typst";
    
    public async Task PdfAsync(ContentWriter content, string codePath, string outputPath, CancellationToken ctk = default)
    {
        if (string.IsNullOrWhiteSpace(typstPath) || !File.Exists(typstPath))
        {
            await DownloadTypstAsync(ctk).ConfigureAwait(false);
        }

        var code = CodeGenerator.ToCode(content);
        await File.WriteAllTextAsync(codePath, code, ctk).ConfigureAwait(false);

        await Process.Start(typstPath!, $"compile {codePath} {outputPath}")
            .WaitForExitAsync(ctk)
            .ConfigureAwait(false);
    }

    async Task DownloadTypstAsync(CancellationToken ctk = default)
    {
        if (File.Exists(LocalTypstPath))
        {
            typstPath = LocalTypstPath;
            return;
        }
        
        const string baseUrl = "https://github.com/typst/typst/releases/download";
        
        using var client = new HttpClient();
        await using var stream = await client.GetStreamAsync(baseUrl + DownloadUri, ctk).ConfigureAwait(false);

        using var archive = new ZipArchive(stream);
        archive.ExtractToDirectory(ApplicationTypstDir + "/" + Version);

        if (!File.Exists(LocalTypstPath))
        {
            throw new InvalidOperationException("Failed to download typst");
        }
        
        typstPath = LocalTypstPath;
    }
}