using lenora_ordera_and_inventory_management_system.Common;
using lenora_ordera_and_inventory_management_system.Models;
using lenora_ordera_and_inventory_management_system.Services;

namespace lenora_ordera_and_inventory_management_system.Forms;

internal sealed class ExportForm : Form
{
    private const int FormWidth = 540;
    private const int FormHeight = 320;
    private const int FieldX = 32;

    private readonly ExportService _export;

    private readonly Panel pnlAccent = new();
    private readonly Label lblTitle = new();
    private readonly Label lblType = new();
    private readonly ComboBox cmbType = new();
    private readonly Label lblDescription = new();
    private readonly Label lblPath = new();
    private readonly TextBox txtPath = new();
    private readonly Button btnBrowse = new();
    private readonly Button btnExport = new();
    private readonly Button btnCancel = new();

    public ExportForm(ExportService export)
    {
        _export = export;
        BuildChrome();
        AddAccent();
        AddTitle();
        AddFields();
        AddButtons();
    }

    private void BuildChrome()
    {
        Text = Messages.ExportData;
        Font = new Font("Segoe UI", 10F);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(FormWidth, FormHeight);
        BackColor = Palette.Paper;
        CancelButton = btnCancel;
    }

    private void AddAccent()
    {
        pnlAccent.BackColor = Palette.Accent;
        pnlAccent.Location = new Point(0, 0);
        pnlAccent.Size = new Size(FormWidth, 6);
        Controls.Add(pnlAccent);
    }

    private void AddTitle()
    {
        lblTitle.Text = Messages.ExportData;
        lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
        lblTitle.ForeColor = Palette.Ink;
        lblTitle.Location = new Point(FieldX, 16);
        lblTitle.AutoSize = true;
        Controls.Add(lblTitle);
    }

    private void AddFields()
    {
        lblType.Text = Messages.ExportType + ":";
        lblType.Location = new Point(FieldX, 64);
        lblType.AutoSize = true;
        Controls.Add(lblType);

        cmbType.Location = new Point(FieldX + 130, 61);
        cmbType.Size = new Size(240, 26);
        cmbType.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbType.Items.AddRange([Messages.StockData, Messages.OrdersData, Messages.PaymentsData]);
        cmbType.SelectedIndex = 0;
        cmbType.SelectedIndexChanged += TypeChanged;
        Controls.Add(cmbType);

        lblDescription.Location = new Point(FieldX + 130, 94);
        lblDescription.Size = new Size(340, 36);
        lblDescription.ForeColor = Color.DimGray;
        lblDescription.Font = new Font("Segoe UI", 9F);
        UpdateDescription();
        Controls.Add(lblDescription);

        lblPath.Text = "Destination:";
        lblPath.Location = new Point(FieldX, 140);
        lblPath.AutoSize = true;
        Controls.Add(lblPath);

        txtPath.Location = new Point(FieldX + 130, 137);
        txtPath.Size = new Size(240, 26);
        txtPath.ReadOnly = true;
        Controls.Add(txtPath);

        btnBrowse.Text = "Browse...";
        btnBrowse.Location = new Point(FieldX + 380, 136);
        btnBrowse.Size = new Size(90, 28);
        btnBrowse.FlatStyle = FlatStyle.Flat;
        btnBrowse.BackColor = Palette.White;
        btnBrowse.Click += BrowseClicked;
        Controls.Add(btnBrowse);
    }

    private void AddButtons()
    {
        btnExport.Text = Messages.Export;
        btnExport.Location = new Point(FormWidth - 230, FormHeight - 55);
        btnExport.Size = new Size(90, 32);
        btnExport.FlatStyle = FlatStyle.Flat;
        btnExport.BackColor = Palette.Accent;
        btnExport.ForeColor = Palette.White;
        btnExport.Click += ExportClicked;
        Controls.Add(btnExport);

        btnCancel.Text = Messages.Cancel;
        btnCancel.Location = new Point(FormWidth - 130, FormHeight - 55);
        btnCancel.Size = new Size(90, 32);
        btnCancel.FlatStyle = FlatStyle.Flat;
        btnCancel.BackColor = Palette.White;
        btnCancel.Click += (_, _) => Close();
        Controls.Add(btnCancel);
    }

    private void TypeChanged(object? sender, EventArgs e)
    {
        UpdateDescription();
        txtPath.Clear();
    }

    private void UpdateDescription()
    {
        lblDescription.Text = cmbType.SelectedIndex switch
        {
            0 => "Exports all products with names, categories, cost prices, selling prices, and current stock quantities.",
            1 => "Exports all customer orders with customer info, order dates, statuses, total amounts, couriers, and dates.",
            2 => "Exports all payment records with delivery dates, COD payment statuses, amounts received, and payment dates.",
            _ => string.Empty
        };
    }

    private void BrowseClicked(object? sender, EventArgs e)
    {
        using var sfd = new SaveFileDialog();
        sfd.Filter = "CSV files (*.csv)|*.csv";
        var prefix = cmbType.SelectedIndex switch
        {
            0 => "Stock",
            1 => "Orders",
            2 => "Payments",
            _ => "Export"
        };
        sfd.FileName = $"Lenora_{prefix}_{DateTime.Today:yyyyMMdd}.csv";

        if (sfd.ShowDialog(this) == DialogResult.OK)
        {
            txtPath.Text = sfd.FileName;
        }
    }

    private void ExportClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtPath.Text))
        {
            MessageBox.Show(this, Messages.FilePathRequired, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var exportType = cmbType.SelectedIndex switch
        {
            0 => ExportType.Stock,
            1 => ExportType.Orders,
            2 => ExportType.Payments,
            _ => ExportType.Stock
        };

        var request = new ExportRequest
        {
            Type = exportType,
            FilePath = txtPath.Text.Trim()
        };

        var result = _export.ExportToFile(request);
        if (!result.Success)
        {
            MessageBox.Show(this, result.Message, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        MessageBox.Show(this, result.Message, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Information);
        DialogResult = DialogResult.OK;
        Close();
    }
}
