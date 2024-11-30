using System;

namespace FileRenamerProject.Data
{
    public class ProgressState
    {
        public int TotalFiles { get; private set; }
        public int ProcessedFiles { get; private set; }
        public int CurrentBatchSize { get; private set; }
        public int CurrentBatchProcessed { get; private set; }
        public string CurrentFileName { get; private set; } = string.Empty;
        public string CurrentStatus { get; private set; } = string.Empty;
        public bool IsProcessing { get; private set; }
        public double OverallProgress => TotalFiles == 0 ? 0 : (ProcessedFiles * 100.0) / TotalFiles;
        public double BatchProgress => CurrentBatchSize == 0 ? 0 : (CurrentBatchProcessed * 100.0) / CurrentBatchSize;

        public void Initialize(int totalFiles, int batchSize)
        {
            TotalFiles = totalFiles;
            ProcessedFiles = 0;
            CurrentBatchSize = Math.Min(batchSize, totalFiles);
            CurrentBatchProcessed = 0;
            CurrentFileName = string.Empty;
            CurrentStatus = "Initializing...";
            IsProcessing = true;
        }

        public void UpdateBatch(int processedInBatch, int batchSize)
        {
            CurrentBatchProcessed = processedInBatch;
            CurrentBatchSize = batchSize;
        }

        public void UpdateProgress(string fileName, string status)
        {
            CurrentFileName = fileName;
            CurrentStatus = status;
            if (status == "Completed" || status == "Error")
            {
                ProcessedFiles++;
                CurrentBatchProcessed++;
            }
        }

        public void Complete()
        {
            IsProcessing = false;
            CurrentStatus = "Processing completed";
        }

        public void Reset()
        {
            TotalFiles = 0;
            ProcessedFiles = 0;
            CurrentBatchSize = 0;
            CurrentBatchProcessed = 0;
            CurrentFileName = string.Empty;
            CurrentStatus = string.Empty;
            IsProcessing = false;
        }
    }
}
