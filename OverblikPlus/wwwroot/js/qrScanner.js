// QR-kode scanningslogik
const codeReader = new ZXing.BrowserQRCodeReader();
let isScanning = false;

document.getElementById("startScanButton").addEventListener("click", () => {
    codeReader.decodeFromVideoDevice(null, 'video', (result, err) => {
        if (result && !isScanning) {
            isScanning = true;
            const fullUrl = result.text;

            // Ekstraktion af TaskId og StepNumber fra URL'en
            const urlPattern = /task-sequence\/(\d+)\/(\d+)/;
            const match = fullUrl.match(urlPattern);

            if (match) {
                const taskId = match[1];
                const stepNumber = match[2];

                // Naviger til task-sequence siden med de korrekte parametre
                window.location.href = `/task-sequence/${taskId}/${stepNumber}`;
            } else {
                console.error("Ugyldig QR-kode URL:", fullUrl);
            }

            // Stop kameraet
            setTimeout(() => {
                codeReader.reset();
                const video = document.getElementById('video');
                if (video) {
                    video.pause();
                    video.srcObject = null;
                }
            }, 500);
        }
        if (err && !(err instanceof ZXing.NotFoundException)) {
            console.error("Fejl:", err);
        }
    });
});
