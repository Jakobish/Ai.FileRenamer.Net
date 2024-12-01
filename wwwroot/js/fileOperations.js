window.getPDFsFromFilePicker = () => {
    return new Promise((resolve, reject) => {
        try {
            const input = document.getElementById('filePicker');
            if (!input || !input.files) {
                console.error('No file input or files found');
                reject(new Error('No files selected'));
                return;
            }

            const files = Array.from(input.files);
            console.log('Selected files:', files);

            const fileInfos = files.map(file => {
                const info = {
                    "name": file.name,
                    "path": URL.createObjectURL(file)
                };
                console.log('Created FileInfo:', info);
                return info;
            });

            console.log('Final fileInfos array:', fileInfos);
            resolve(fileInfos);
        } catch (error) {
            console.error('Error in getPDFsFromFilePicker:', error);
            reject(error);
        }
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
