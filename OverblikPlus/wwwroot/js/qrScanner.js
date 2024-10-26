// QR-kode scanningslogik
const codeReader = new ZXing.BrowserQRCodeReader();
let isScanning = false;

// Start scanningen, når brugeren trykker på knappen
function startScanning() {
    if (isScanning) return;
    console.log("Start scanning initiatet"); // Tilføjet for at bekræfte funktionen kører
    isScanning = true;

    const videoElement = document.getElementById("video");
    if (!videoElement) {
        console.error("Videoelementet blev ikke fundet!");
        return;
    }
    videoElement.style.display = "block"; // Vis videoelementet, når scanningen starter

    codeReader.decodeFromVideoDevice(null, 'video', (result, err) => {
        if (result) {
            console.log("QR-kode fundet:", result.text); // Tilføjet til fejlfindingsformål
            const fullUrl = result.text;
            document.getElementById('result').textContent = fullUrl;

            // Ekstraktion af TaskId og StepNumber fra URL'en
            const urlPattern = /task-sequence\/(\d+)\/(\d+)/;
            const match = fullUrl.match(urlPattern);

            if (match) {
                const taskId = match[1];
                const stepNumber = match[2];
                console.log("Navigerer til:", `/task-sequence/${taskId}/${stepNumber}`);
                window.location.href = `/task-sequence/${taskId}/${stepNumber}`;
            } else {
                console.error("Ugyldig QR-kode URL:", fullUrl);
            }

            stopScanning(); // Stop kameraet efter scanningen
        }
        if (err && !(err instanceof ZXing.NotFoundException)) {
            console.error("Fejl:", err);
        }
    });
}

// Funktion til at stoppe scanningen og frigøre kameraet
function stopScanning() {
    console.log("Stopper scanningen"); // Tilføjet til fejlfindingsformål
    codeReader.reset();
    isScanning = false;
    const videoElement = document.getElementById("video");
    if (videoElement) {
        videoElement.pause();
        videoElement.style.display = "none";
        videoElement.srcObject = null;
    }
}
