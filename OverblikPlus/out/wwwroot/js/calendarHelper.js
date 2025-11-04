function downloadFile(filename, contentType, content) {
    const blob = new Blob([Uint8Array.from(atob(content), c => c.charCodeAt(0))], { type: contentType });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    link.click();
    URL.revokeObjectURL(url);
}