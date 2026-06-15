using AutoShop.Core.Entities;
using AutoShop.Core.Enums;
using AutoShop.MainApp.Helpers;
using AutoShop.Services;
using System.CodeDom;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace AutoShop.MainApp.ViewModels;

public class InspectionChecklistViewModel : INotifyPropertyChanged
{
    private readonly InspectionService _inspectionService = new();

    public ObservableCollection<WorkOrderInspectionItem> Items { get; } = new();

    public int WorkOrderId { get; }

    private string? _technicianName;
    public string? TechnicianName
    {
        get => _technicianName;
        set
        {
            _technicianName = value;
            OnPropertyChanged();
        }
    }

    private string? _overallNotes;
    public string? OverallNotes
    {
        get => _overallNotes;
        set
        {
            _overallNotes = value;
            OnPropertyChanged();
        }
    }

    public Array StatusOptions => Enum.GetValues(typeof(InspectionStatus));

    public ICommand SaveCommand { get; }
    public ICommand ReloadCommand { get; }

    public InspectionChecklistViewModel(int workOrderId)
    {
        WorkOrderId = workOrderId;

        SaveCommand = new RelayCommand(Save);
        ReloadCommand = new RelayCommand(Load);

        Load();
    }

    private void Load()
    {
        Items.Clear();

        var inspection = _inspectionService.GetOrCreateInspection(WorkOrderId);

        TechnicianName = inspection.TechnicianName;
        OverallNotes = inspection.OverallNotes;

        foreach (var item in inspection.Items.OrderBy(i => i.Section).ThenBy(i => i.SortOrder))
        {
            Items.Add(item);
        }
    }

    private void Save()
    {
        var inspection = _inspectionService.GetOrCreateInspection(WorkOrderId);

        inspection.TechnicianName = TechnicianName;
        inspection.OverallNotes = OverallNotes;
        inspection.Items = Items.ToList();

        _inspectionService.SaveInspection(inspection);
        Load();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}