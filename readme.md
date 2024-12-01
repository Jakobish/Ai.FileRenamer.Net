# File Renamer Project

## **Project Overview**
The File Renamer Project is a Blazor WebAssembly application designed to streamline the process of renaming PDF files. By leveraging AI, this app provides intelligent name suggestions based on file content, while using SQLite for efficient local storage. The ultimate goal is to create an intuitive, extensible, and highly functional tool for managing files.

---

## **Core Features**

### **Implemented**
- ✓ **File Selection**: Select individual PDF files or entire folders using a file picker.
- ✓ **File Grid Display**: Show selected files in a clean, sortable grid with:
  - File Name
  - Path
  - Suggested Name
  - Status
- ✓ **PDF Content Extraction**: Extract up to 300 words or the first 3 pages of a PDF.
- ✓ **AI Name Suggestions**: Use OpenAI GPT API to generate meaningful names based on file content.
- ✓ **SQLite Integration**: Store file details (name, path, suggested name, status) in a local database.
- ✓ **Responsive Design**: User interface optimized for various screen sizes.
- ✓ **Batch Processing**: Process files in small batches for optimal performance.
- ✓ **Error Handling**: Comprehensive error handling with user-friendly messages.

### **Planned Enhancements**
- ✓ **Progress Bar**: Visual indicator for file processing status.
- ⏳ **Undo/Redo**: Revert or redo suggested names for individual or all files.
- ⏳ **Manual Editing**: Allow direct editing of suggested names in the grid.
- ⏳ **Drag-and-Drop Support**: Enable users to drag files or folders directly into the application.
- ⏳ **Dark Mode**: Add a visually pleasing dark theme.
- ⏳ **Advanced PDF Parsing**: Improve handling of complex PDFs (e.g., tables, multi-column layouts).
- ⏳ **Cloud Integration**: Rename files stored in cloud services (e.g., Google Drive, OneDrive).
- ⏳ **Dry Run Mode**: Display proposed changes without applying them.
- ⏳ **Backup Changes**: Export a `.json` or `.csv` log of all changes for auditing or rollback.
- ⏳ **Search and Filter**: Add search functionality to find specific files in the grid.
- ⏳ **Pagination**: Break up large file lists into manageable pages.

---

## **Project Roadmap**

| Step                     | Description                                                                                     | Status    |
|--------------------------|-----------------------------------------------------------------------------------------|-----------|
| **File Selection**       | Enable users to select files or folders (PDFs only).                                             | ✓ Completed |
| **File Display**         | Show selected files in a grid with essential columns.                                            | ✓ Completed |
| **Content Extraction**   | Extract limited content (up to 300 words or 3 pages) from PDF files.                             | ✓ Completed |
| **AI Integration**       | Send extracted content to OpenAI GPT API for name suggestions.                                   | ✓ Completed |
| **Database Integration** | Store file data (name, path, suggested name, status) in SQLite using EF Core.                    | ✓ Completed |
| **Batch Processing**     | Process files in small batches for optimal performance.                                          | ✓ Completed |
| **Error Handling**       | Handle errors for unsupported files or issues during processing.                                 | ✓ Completed |
| **Progress Bar**         | Add a visual indicator for file processing progress.                                             | ✓ Completed |
| **Undo/Redo**           | Implement functionality to revert or redo name changes.                                          | ⏳ Planned |
| **Manual Editing**       | Allow users to manually edit suggested names in the grid.                                        | ⏳ Planned |
| **Drag and Drop**        | Add drag-and-drop support for selecting files.                                                   | ⏳ Planned |
| **Dark Mode**            | Add Dark Mode support to improve UI appearance.                                                 | ⏳ Planned |
| **Enhanced PDF Parsing** | Improve content extraction for complex PDFs (e.g., tables, multi-column layouts).                | ⏳ Planned |
| **Cloud Integration**    | Enable renaming files stored in cloud services (e.g., Google Drive, OneDrive).                   | ⏳ Planned |
| **Backup Changes**       | Create a backup file (e.g., `.json`, `.csv`) documenting all changes made.                       | ⏳ Planned |
| **Dry Run Mode**         | Display changes without actually renaming files.                                                | ⏳ Planned |
| **Search & Filter**      | Add a search bar and filters to help users find specific files in the grid.                      | ⏳ Planned |
| **Pagination**           | Display files in pages when there are many files selected.                                       | ⏳ Planned |

---

## **How to Run**

### **Prerequisites**
- [.NET SDK](https://dotnet.microsoft.com/download) version 7.0 or higher
- OpenAI API Key for AI integration

### **Setup Instructions**
1. Clone or download this repository
2. Create a new file called `appsettings.Development.json` in the project root with your OpenAI API key:
   ```json
   {
     "OpenAI": {
       "ApiKey": "your-api-key-here"
     }
   }
   ```
3. Open the project in Visual Studio or Visual Studio Code
4. Restore dependencies:
   ```bash
   dotnet restore
   ```
5. Run the project:
   ```bash
   dotnet run
   ```
6. Open your browser and navigate to `https://localhost:5001` or the URL shown in the console

---

## **Usage**

1. **Select Files**:
   - Use the file picker to select `.pdf` files
   - Only PDF files will be processed
   - Invalid or non-PDF files will be rejected with an error message
2. **Process Files**:
   - Click "Process Files" to extract content and generate suggested names
   - Files are processed in small batches for optimal performance
   - Progress is shown in the UI
3. **View and Apply Results**:
   - Suggested names will appear in the grid
   - Each file's status is clearly shown
   - Click "Apply" to rename a file using its suggested name
   - Duplicate names are automatically handled by adding a number suffix

---

## **Features in Development**

The following features are actively being developed:
1. **Visual Feedback**:
   - Progress bar for overall processing status
   - Better visual indicators for file status
2. **Enhanced Usability**:
   - Manual name editing
   - Undo/redo functionality
   - Drag-and-drop file selection
3. **Dark Mode**:
   - Modern dark theme support
4. **Advanced Features**:
   - Cloud storage integration
   - Backup and restore functionality
   - Search and filter capabilities

---

## **Contributing**

Contributions are welcome! Feel free to:
- Report bugs
- Suggest new features
- Submit pull requests

---

## **License**

This project is licensed under the MIT License.
