using System.Reflection;

namespace lenora_ordera_and_inventory_management_system.Forms;

internal static class GridTheme
{
    public static void Apply(DataGridView dgv)
    {
        EnableDoubleBuffering(dgv);
        ConfigureGeneral(dgv);
        ConfigureHeaders(dgv);
        ConfigureRows(dgv);
    }

    private static void EnableDoubleBuffering(DataGridView dgv)
    {
        typeof(Control)
            .GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)?
            .SetValue(dgv, true, null);
    }

    private static void ConfigureGeneral(DataGridView dgv)
    {
        dgv.EnableHeadersVisualStyles = false;
        dgv.BorderStyle = BorderStyle.None;
        dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        dgv.GridColor = Palette.Border;
        dgv.BackgroundColor = Palette.White;
        dgv.RowHeadersVisible = false;
        dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgv.MultiSelect = false;
        dgv.AllowUserToResizeRows = false;
        dgv.ReadOnly = true;
        dgv.AllowUserToAddRows = false;
        dgv.AllowUserToDeleteRows = false;
        dgv.Font = new Font("Segoe UI", 9.5F);
    }

    private static void ConfigureHeaders(DataGridView dgv)
    {
        dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        dgv.ColumnHeadersDefaultCellStyle.BackColor = Palette.Ink;
        dgv.ColumnHeadersDefaultCellStyle.ForeColor = Palette.White;
        dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
        dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(10, 0, 10, 0);
        dgv.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
        dgv.ColumnHeadersHeight = 40;
        dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
    }

    private static void ConfigureRows(DataGridView dgv)
    {
        dgv.RowTemplate.Height = 36;
        dgv.DefaultCellStyle.BackColor = Palette.White;
        dgv.DefaultCellStyle.ForeColor = Palette.Ink;
        dgv.DefaultCellStyle.SelectionBackColor = Palette.AccentLight;
        dgv.DefaultCellStyle.SelectionForeColor = Palette.Ink;
        dgv.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
        dgv.DefaultCellStyle.Padding = new Padding(10, 0, 10, 0);
        dgv.DefaultCellStyle.WrapMode = DataGridViewTriState.False;

        dgv.AlternatingRowsDefaultCellStyle.BackColor = Palette.PaperLight;
        dgv.AlternatingRowsDefaultCellStyle.ForeColor = Palette.Ink;
        dgv.AlternatingRowsDefaultCellStyle.SelectionBackColor = Palette.AccentLight;
        dgv.AlternatingRowsDefaultCellStyle.SelectionForeColor = Palette.Ink;
    }

    public static void HighlightStatusCell(DataGridViewCellFormattingEventArgs e, string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return;
        }

        e.FormattingApplied = true;
        switch (status)
        {
            case "Placed":
                e.CellStyle!.ForeColor = Palette.Accent;
                e.CellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                break;
            case "Posted":
                e.CellStyle!.ForeColor = Palette.Warning;
                e.CellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                break;
            case "Delivered":
                e.CellStyle!.ForeColor = Palette.Success;
                e.CellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                break;
            case "Returned":
                e.CellStyle!.ForeColor = Color.FromArgb(194, 24, 91);
                e.CellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                break;
            case "Cancelled":
                e.CellStyle!.ForeColor = Palette.MutedText;
                break;
        }
    }
}
