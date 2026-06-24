# BarcodeScanner
Program is an API that can be called from my PC. To call the raspberry I use the call `curl -X POST http://RASPBERRY_PI_IP:5000/scan` which then triggers the program to take a picture, use the installed packages to find the barcode and translate said barcode to digits, then pass it on via the API.

Raspberry Pi
├─ ASP.NET API
├─ Webcam attached via USB
├─ fswebcam installed
└─ ZXing barcode decoder

My PC
└─ Calls the API over HTTP

# packages on the pc:
dotnet add package ZXing.Net
dotnet add package ZXing.Net.Bindings.SkiaSharp

# dependancy on raspberry pi
sudo apt install -y fswebcam v4l-utils
