namespace lenora_ordera_and_inventory_management_system.Models;

internal sealed class ExportRequest
{
    public ExportType Type { get; init; }

    public string FilePath { get; init; } = "";
}
