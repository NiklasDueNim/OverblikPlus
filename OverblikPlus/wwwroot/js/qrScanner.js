const codeReader = new ZXing.BrowserQRCodeReader();
let isScanning = false;

// Start scanningen, når brugeren trykker på knappen
function startScanning() {
    if (isScanning) return;

    isScanning = true;
    const videoElement = document.getElementById('video');
    videoElement.style.display = "block"; // Vis videoelementet, når scanningen starter

    codeReader.decodeFromVideoDevice(null, 'video', (result, err) => {
        if (result) {
            const taskId = result.text; // Hent taskId fra QR-koden
            document.getElementById('result').textContent = taskId;

            // Naviger til task-sequence-siden med taskId
            window.location.href = `/task-sequence/${taskId}/step/1`;

            // Stop kameraet efter scanningen
            stopScanning();
        }
        if (err && !(err instanceof ZXing.NotFoundException)) {
            console.error("Fejl:", err);
        }
    });
}

// Funktion til at stoppe scanningen og frigøre kameraet
function stopScanning() {
    codeReader.reset();
    isScanning = false;
    const video = document.getElementById('video');
    if (video) {
        video.pause();
        video.style.display = "none";
        video.srcObject = null;
    }
}
