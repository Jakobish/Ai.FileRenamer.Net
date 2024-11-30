# File Renamer Project

## **Project Overview**
The File Renamer Project is a Blazor WebAssembly application designed to streamline the process of renaming PDF files. By leveraging AI, this app provides intelligent name suggestions based on file content, while using SQLite for efficient local storage. The ultimate goal is to create an intuitive, extensible, and highly functional tool for managing files.

---

## **Core Features**

### **Implemented**
- ✅ **File Selection**: Select individual PDF files or entire folders using a file picker.
- ✅ **File Grid Display**: Show selected files in a clean, sortable grid with:
  - File Name
  - Path
  - Suggested Name
  - Status
- ✅ **PDF Content Extraction**: Extract up to 300 words or the first 3 pages of a PDF.
- ✅ **AI Name Suggestions**: Use OpenAI GPT API to generate meaningful names based on file content.
- ✅ **SQLite Integration**: Store file details (name, path, suggested name, status) in a local database.
- ✅ **Responsive Design**: User interface optimized for various screen sizes.

### **Planned Enhancements**
- ⬜ **Progress Bar**: Visual indicator for file processing status.
- ⬜ **Undo/Redo**: Revert or redo suggested names for individual or all files.
- ⬜ **Error Handling**: Better feedback for unsupported files or processing errors.
- ⬜ **Manual Editing**: Allow direct editing of suggested names in the grid.
- ⬜ **Drag-and-Drop Support**: Enable users to drag files or folders directly into the application.
- ⬜ **Dark Mode**: Add a visually pleasing dark theme.
- ⬜ **Advanced PDF Parsing**: Improve handling of complex PDFs (e.g., tables, multi-column layouts).
- ⬜ **Cloud Integration**: Rename files stored in cloud services (e.g., Google Drive, OneDrive).
- ⬜ **Batch Processing**: Process multiple files simultaneously for better performance.
- ⬜ **Dry Run Mode**: Display proposed changes without applying them.
- ⬜ **Backup Changes**: Export a `.json` or `.csv` log of all changes for auditing or rollback.
- ⬜ **Search and Filter**: Add search functionality to find specific files in the grid.
- ⬜ **Pagination**: Break up large file lists into manageable pages.

---

## **Project Roadmap**

| Step                     | Description                                                                                     | Status    |
|--------------------------|-----------------------------------------------------------------------------------------|-----------|
| **File Selection**       | Enable users to select files or folders (PDFs only).                                             | ✅ Completed |
| **File Display**         | Show selected files in a grid with essential columns.                                            | ✅ Completed |
| **Content Extraction**   | Extract limited content (up to 300 words or 3 pages) from PDF files.                             | ✅ Completed |
| **AI Integration**       | Send extracted content to OpenAI GPT API for name suggestions.                                   | ✅ Completed |
| **Database Integration** | Store file data (name, path, suggested name, status) in SQLite using EF Core.                    | ✅ Completed |
| **Progress Bar**         | Add a visual indicator for file processing progress.                                             | ⬜ Not Started |
| **Undo/Redo**            | Implement functionality to revert or redo name changes.                                         | ⬜ Not Started |
| **Error Handling**       | Handle errors for unsupported files or issues during processing.                                 | ⬜ Not Started |
| **Manual Editing**       | Allow users to manually edit suggested names in the grid.                                        | ⬜ Not Started |
| **Drag and Drop**        | Add drag-and-drop support for selecting files.                                                   | ⬜ Not Started |
| **Dark Mode**            | Add Dark Mode support to improve UI appearance.                                                 | ⬜ Not Started |
| **Enhanced PDF Parsing** | Improve content extraction for complex PDFs (e.g., tables, multi-column layouts).                | ⬜ Not Started |
| **Cloud Integration**    | Enable renaming files stored in cloud services (e.g., Google Drive, OneDrive).                   | ⬜ Not Started |
| **Backup Changes**       | Create a backup file (e.g., `.json`, `.csv`) documenting all changes made.                       | ⬜ Not Started |
| **Dry Run Mode**         | Display changes without actually renaming files.                                                | ⬜ Not Started |
| **Search & Filter**      | Add a search bar and filters to help users find specific files in the grid.                      | ⬜ Not Started |
| **Pagination**           | Display files in pages when there are many files selected.                                       | ⬜ Not Started |
| **Parallel Processing**  | Process multiple files simultaneously to improve performance.                                    | ⬜ Not Started |

---

## **How to Run**

### **Prerequisites**
- [.NET SDK](https://dotnet.microsoft.com/download) version 6.0 or higher.
- PowerShell for running the setup script.
- OpenAI API Key for AI integration.

### **Setup Instructions**
1. Clone this repository:
   ```bash
   git clone https://github.com/yourusername/FileRenamerProject.git
   cd FileRenamerProject
   ```
2. Run the PowerShell setup script to initialize the project:
   ```powershell
   .\SetupFileRenamer.ps1
   ```
3. Open the project in Visual Studio or Visual Studio Code.
4. Restore dependencies:
   ```bash
   dotnet restore
   ```
5. Run the project:
   ```bash
   dotnet run
   ```

---

## **Usage**

1. **Select Files or Folders**:
   - Use the file picker to select `.pdf` files or entire folders.
   - Only PDF files will be processed.
2. **Process Files**:
   - Click "Process Files" to extract content and generate suggested names.
3. **View Results**:
   - Suggested names will appear in the grid, along with the processing status.

---

## **Future Enhancements**

The following features are planned for future updates:
1. **Visual Feedback**:
   - Add a progress bar to show file processing progress.
2. **Enhanced Usability**:
   - Allow manual editing of names and enable undo/redo actions.
3. **Cloud Integration**:
   - Support renaming files stored in Google Drive, OneDrive, and other cloud services.
4. **Drag-and-Drop**:
   - Simplify file selection with drag-and-drop support.
5. **Performance Optimization**:
   - Implement parallel processing for handling large file batches efficiently.

---

## **Contributing**

We welcome contributions! If you'd like to improve the project, feel free to fork the repository, create issues, or submit pull requests.

---

## **License**

This project is licensed under the [MIT License](LICENSE).

---

## **Contact**

For questions or suggestions, contact `your.email@example.com`.
