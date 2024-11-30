
async function getPDFsFromFilePicker() {
    const inputElement = document.getElementById("filePicker");
    const files = Array.from(inputElement.files)
        .filter(file => file.type === "application/pdf")
        .map(file => file.path || file.webkitRelativePath);
    return files;
}

