window.getPDFsFromFilePicker = async () => {
    try {
        const input = document.getElementById('filePicker');
        if (!input || !input.files || input.files.length === 0) {
            console.error('No files selected');
            return [];
        }

        const files = Array.from(input.files);
        console.log('Selected files:', files);

        const fileInfos = files.map(file => {
            const info = {
                Name: file.name,
                Path: URL.createObjectURL(file)
            };
            console.log('Created FileInfo:', info);
            return info;
        });

        console.log('Final fileInfos array:', JSON.stringify(fileInfos));
        return fileInfos;
    } catch (error) {
        console.error('Error in getPDFsFromFilePicker:', error);
        throw error;
    }
};

window.getFileBytes = async (filePath) => {
    try {
        const response = await fetch(filePath);
        if (!response.ok) {
            throw new Error(`Failed to fetch file: ${response.statusText}`);
        }
        const blob = await response.blob();
        const arrayBuffer = await blob.arrayBuffer();
        return new Uint8Array(arrayBuffer);
    } catch (error) {
        console.error('Error reading file:', error);
        throw error;
    }
};
