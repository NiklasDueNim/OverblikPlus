const codeReader = new ZXing.BrowserQRCodeReader();
let isScanning = false;

window.startScanning = function () {
    if (isScanning) return;
    console.log("Starter scanning...");
    isScanning = true;

    const videoElement = document.getElementById("video");
    if (!videoElement) {
        console.error("Videoelementet blev ikke fundet!");
        return;
    }
    videoElement.style.display = "block";

    codeReader.decodeFromVideoDevice(null, 'video', async (result, err) => {
        if (result) {
            console.log("QR-kode fundet:", result.text);
            await DotNet.invokeMethodAsync("TaskMicroService", "OnQrCodeScanned", result.text);
            stopScanning();
        }
        if (err && !(err instanceof ZXing.NotFoundException)) {
            console.error("Fejl:", err);
        }
    });
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
