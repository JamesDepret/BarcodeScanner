using System.Diagnostics;
using SkiaSharp;
using ZXing;
using ZXing.SkiaSharp;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var captureFolder = Path.Combine(AppContext.BaseDirectory, "captures");
Directory.CreateDirectory(captureFolder);

app.MapGet("/", () => "Barcode scanner service is running.");

app.MapPost("/scan", async () =>
{
    var fileName = $"barcode-capture-{DateTime.Now:yyyyMMdd-HHmmss}.jpg";
    var imagePath = Path.Combine(captureFolder, fileName);

    var captureResult = await CaptureImage(imagePath);

    if (!captureResult.Success)
    {
        return Results.Problem(
            title: "Camera capture failed",
            detail: captureResult.Error
        );
    }

    var barcode = DecodeBarcode(imagePath);

    if (barcode == null)
    {
        return Results.NotFound(new
        {
            success = false,
            message = "No barcode found in image.",
            imagePath,
            fileName
        });
    }

    return Results.Ok(new
    {
        success = true,
        barcode.Text,
        format = barcode.BarcodeFormat.ToString(),
        imagePath,
        fileName
    });
});

app.Run("http://0.0.0.0:5000");

static async Task<(bool Success, string? Error)> CaptureImage(string outputPath)
{
    var process = new Process
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = "fswebcam",
            Arguments = $"-r 1920x1080 --no-banner --skip 10 --delay 1 --set sharpness=4 --set power_line_frequency=1 \"{outputPath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        }
    };

    process.Start();

    var stdout = await process.StandardOutput.ReadToEndAsync();
    var stderr = await process.StandardError.ReadToEndAsync();

    await process.WaitForExitAsync();

    if (process.ExitCode != 0)
        return (false, stderr + stdout);

    if (!File.Exists(outputPath))
        return (false, $"fswebcam exited successfully, but no image was created at {outputPath}");

    return (true, null);
}

static Result? DecodeBarcode(string imagePath)
{
    using var bitmap = SKBitmap.Decode(imagePath);

    if (bitmap == null)
        return null;

    var reader = new BarcodeReader
    {
        Options =
        {
            TryHarder = true,
            TryInverted = true,
            PossibleFormats =
            [
                BarcodeFormat.EAN_13,
                BarcodeFormat.EAN_8,
                BarcodeFormat.UPC_A,
                BarcodeFormat.UPC_E,
                BarcodeFormat.CODE_128,
                BarcodeFormat.CODE_39,
                BarcodeFormat.QR_CODE,
                BarcodeFormat.DATA_MATRIX
            ]
        }
    };

    return reader.Decode(bitmap);
}
