window.getPDFsFromFilePicker = () => {
    return new Promise((resolve, reject) => {
        const input = document.getElementById('filePicker');
        const files = Array.from(input.files);
        const fileInfos = files.map(file => {
            const info = {
                name: file.name,
                path: URL.createObjectURL(file)
            };
            console.log('FileInfo:', JSON.stringify(info)); // Log each file info
            return info;
        });
        console.log('All FileInfos:', JSON.stringify(fileInfos)); // Log all file infos
        resolve(fileInfos);
    });
};

window.getFileBytes = async (filePath) => {
    try {
        const response = await fetch(filePath);
        const blob = await response.blob();
        const arrayBuffer = await blob.arrayBuffer();
        return new Uint8Array(arrayBuffer);
    } catch (error) {
        console.error('Error reading file:', error);
        throw error;
    }
};
