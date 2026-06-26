const codeReader = new ZXing.BrowserQRCodeReader();
let isScanning = false;

// startScanning accepts an optional .NET object reference. When provided, the scanned
// value is sent back to that component's [JSInvokable] OnQrCodeScanned method. Without
// it (e.g. the standalone Scan page) the value is written to the #result element.
window.startScanning = function (dotNetRef) {
    if (isScanning) return;
    console.log("Starter scanning...");
    isScanning = true;

    setTimeout(() => {
        const videoElement = document.getElementById("video");
        if (!videoElement) {
            console.error("Videoelementet blev ikke fundet!");
            isScanning = false;
            return;
        }
        videoElement.style.display = "block";

        codeReader.decodeFromVideoDevice(null, 'video', async (result, err) => {
            if (result) {
                console.log("QR-kode fundet.");
                if (dotNetRef) {
                    await dotNetRef.invokeMethodAsync("OnQrCodeScanned", result.text);
                } else {
                    const resultSpan = document.getElementById("result");
                    if (resultSpan) resultSpan.textContent = result.text;
                }
                stopScanning();
            }
            if (err && !(err instanceof ZXing.NotFoundException)) {
                console.error("Fejl ved scanning:", err);
            }
        });
    }, 500);
};

window.stopScanning = function () {
    console.log("Stopper scanning...");
    codeReader.reset();
    isScanning = false;
    const videoElement = document.getElementById("video");
    if (videoElement) {
        const stream = videoElement.srcObject;
        if (stream) {
            stream.getTracks().forEach(track => track.stop());
        }
        videoElement.srcObject = null;
        videoElement.style.display = "none";
    }
};
