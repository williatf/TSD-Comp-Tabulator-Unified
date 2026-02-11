using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using Microsoft.Win32;
using Tsd.Tabulator.Core.Models;
using Tsd.Tabulator.Application.Interfaces;

namespace Tsd.Tabulator.Wpf.ViewModels.Reports;

/// <summary>
/// ViewModel for Duets Awards report tab.
/// </summary>
public sealed class DuetsAwardsReportTabViewModel : Screen, IReportTab
{
    private readonly ShellViewModel _shell;
    private readonly IDuetAwardReportService _reportService;
    
    private bool _isLoading;
    private string? _errorMessage;
    private string? _warningMessage;
    private DuetAwardReport? _report;

    public DuetsAwardsReportTabViewModel(
        ShellViewModel shell,
        IDuetAwardReportService reportService)
    {
        _shell = shell;
        _reportService = reportService;
        DisplayName = "Duet Awards";
    }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            NotifyOfPropertyChange();
            NotifyOfPropertyChange(nameof(CanExportCsv));
        }
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set
        {
            _errorMessage = value;
            NotifyOfPropertyChange();
            NotifyOfPropertyChange(nameof(HasError));
        }
    }

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public string? WarningMessage
    {
        get => _warningMessage;
        set
        {
            _warningMessage = value;
            NotifyOfPropertyChange();
            NotifyOfPropertyChange(nameof(HasWarning));
        }
    }

    public bool HasWarning => !string.IsNullOrWhiteSpace(WarningMessage);

    public IEnumerable<IGrouping<string, DuetAwardGroup>> GroupedByBucket => 
        Report?.Groups.GroupBy(g => g.Bucket) ?? Enumerable.Empty<IGrouping<string, DuetAwardGroup>>();

    public DuetAwardReport? Report
    {
        get => _report;
        set
        {
            _report = value;
            NotifyOfPropertyChange();
            NotifyOfPropertyChange(nameof(HasData));
            NotifyOfPropertyChange(nameof(GroupedByBucket));
        }
    }

    public bool HasData => Report?.Groups.Any() == true;

    public bool CanExportCsv => HasData && !IsLoading;

    protected override async Task OnActivatedAsync(CancellationToken cancellationToken)
    {
        // Auto-refresh when tab is activated
        await RefreshAsync();
    }

    public async Task GenerateReportAsync()
    {
        await RefreshAsync();
    }

    public async Task RefreshAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        WarningMessage = null;

        try
        {
            var report = await _reportService.GenerateReportAsync();
            Report = report;

            // Check for warnings
            if (report.Groups.Count == 0)
            {
                WarningMessage = "No duet routines found with scores. Please ensure routines are marked as 'Duet' and have been scored.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load report: {ex.Message}";
            Report = null;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void ExportCsv()
    {
        if (Report == null || !_shell.HasEventLoaded)
            return;

        try
        {
            // Create Reports subdirectory in event folder
            var reportsDir = Path.Combine(_shell.CurrentEventFolder!, "Reports");
            Directory.CreateDirectory(reportsDir);

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var defaultPath = Path.Combine(reportsDir, $"DuetAwards_{timestamp}.csv");

            var sfd = new SaveFileDialog
            {
                Title = "Export Duet Awards Report",
                FileName = Path.GetFileName(defaultPath),
                DefaultExt = ".csv",
                Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                InitialDirectory = reportsDir
            };

            if (sfd.ShowDialog() != true)
                return;

            ExportToCsv(Report, sfd.FileName);

            MessageBox.Show(
                $"Report exported successfully to:\n{sfd.FileName}",
                "Export Complete",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to export report:\n{ex.Message}",
                "Export Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private static void ExportToCsv(DuetAwardReport report, string filePath)
    {
        var csv = new StringBuilder();
        
        // Header
        csv.AppendLine("Bucket,Class,Place,Score,Program#,Participants,Studio,Routine");

        foreach (var group in report.Groups)
        {
            foreach (var entry in group.Entries)
            {
                csv.AppendLine($"{CsvEscape(group.Bucket)}," +
                             $"{CsvEscape(group.Class)}," +
                             $"{entry.Place}," +
                             $"{entry.FinalScore:F2}," +
                             $"{entry.ProgramNumber}," +
                             $"{CsvEscape(entry.Participants)}," +
                             $"{CsvEscape(entry.StudioName)}," +
                             $"{CsvEscape(entry.RoutineTitle)}");
            }
        }

        File.WriteAllText(filePath, csv.ToString(), Encoding.UTF8);
    }

    private static string CsvEscape(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}