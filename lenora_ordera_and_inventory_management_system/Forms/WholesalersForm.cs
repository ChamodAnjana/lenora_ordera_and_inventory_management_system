using lenora_ordera_and_inventory_management_system.Common;
using lenora_ordera_and_inventory_management_system.Models;
using lenora_ordera_and_inventory_management_system.Services;

namespace lenora_ordera_and_inventory_management_system.Forms;

internal sealed class WholesalersForm : Form
{
    private const int FormWidth = 760;
    private const int FormHeight = 580;
    private const int FieldX = 32;
    private const int ButtonWidth = 100;
    private const int ButtonGap = 12;

    private readonly WholesalerService _wholesalers;
    private readonly Panel pnlAccent = new();
    private readonly Label lblTitle = new();
    private readonly Label lblSearch = new();
    private readonly TextBox txtSearch = new();
    private readonly DataGridView dgvWholesalers = new();
    private readonly Button btnNew = new();
    private readonly Button btnEdit = new();
    private readonly Button btnDelete = new();

    public WholesalersForm(WholesalerService wholesalers)
    {
        _wholesalers = wholesalers;
        BuildChrome();
        AddAccent();
        AddTitle();
        AddFilter();
        AddGrid();
        AddActions();
        LoadWholesalers();
    }

    private void BuildChrome()
    {
        Text = Messages.Wholesalers;
        Font = new Font("Segoe UI", 10F);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(FormWidth, FormHeight);
        BackColor = Palette.Paper;
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
        lblTitle.Text = Messages.Wholesalers;
        lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
        lblTitle.ForeColor = Palette.Ink;
        lblTitle.Location = new Point(FieldX, 20);
        lblTitle.AutoSize = true;
        Controls.Add(lblTitle);
    }

    private void AddFilter()
    {
        lblSearch.Text = Messages.Search + ":";
        lblSearch.Location = new Point(FieldX, 60);
        lblSearch.AutoSize = true;
        Controls.Add(lblSearch);

        txtSearch.Location = new Point(FieldX + 60, 57);
        txtSearch.Size = new Size(220, 26);
        txtSearch.TextChanged += (s, e) => LoadWholesalers();
        Controls.Add(txtSearch);
    }

    private void AddGrid()
    {
        dgvWholesalers.Location = new Point(FieldX, 96);
        dgvWholesalers.Size = new Size(FormWidth - 64, 400);
        GridTheme.Apply(dgvWholesalers);
        dgvWholesalers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        AddColumns();
        Controls.Add(dgvWholesalers);
    }

    private void AddColumns()
    {
        dgvWholesalers.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "ID", Visible = false });

        var colName = new DataGridViewTextBoxColumn
        {
            HeaderText = Messages.Wholesaler,
            FillWeight = 180,
            MinimumWidth = 140,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        dgvWholesalers.Columns.Add(colName);

        var colPhone = new DataGridViewTextBoxColumn
        {
            HeaderText = Messages.ContactNo,
            FillWeight = 110,
            MinimumWidth = 100,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        colPhone.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        colPhone.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dgvWholesalers.Columns.Add(colPhone);

        var colAddress = new DataGridViewTextBoxColumn
        {
            HeaderText = Messages.Address,
            FillWeight = 220,
            MinimumWidth = 160,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        dgvWholesalers.Columns.Add(colAddress);
    }

    private void AddActions()
    {
        PlacePrimary(btnNew, Messages.NewWholesaler, FieldX);
        PlacePlain(btnEdit, Messages.Edit, FieldX + ButtonWidth + ButtonGap);
        PlacePlain(btnDelete, Messages.Delete, FieldX + (ButtonWidth + ButtonGap) * 2);

        btnNew.Click += NewClicked;
        btnEdit.Click += EditClicked;
        btnDelete.Click += DeleteClicked;

        Controls.Add(btnNew);
        Controls.Add(btnEdit);
        Controls.Add(btnDelete);
    }

    private static void PlacePrimary(Button button, string text, int left)
    {
        button.Text = text;
        button.Location = new Point(left, 514);
        button.Size = new Size(ButtonWidth, 36);
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.BackColor = Palette.Accent;
        button.ForeColor = Palette.White;
        button.Cursor = Cursors.Hand;
    }

    private static void PlacePlain(Button button, string text, int left)
    {
        button.Text = text;
        button.Location = new Point(left, 514);
        button.Size = new Size(ButtonWidth, 36);
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderColor = Palette.Ink;
        button.FlatAppearance.BorderSize = 1;
        button.BackColor = Palette.White;
        button.ForeColor = Palette.Ink;
        button.Cursor = Cursors.Hand;
    }

    private void LoadWholesalers()
    {
        dgvWholesalers.Rows.Clear();
        var list = new List<Wholesaler>();
        var result = _wholesalers.ListWholesalers(list, txtSearch.Text.Trim());
        if (!result.Success)
        {
            MessageBox.Show(this, result.Message, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        foreach (var item in list)
        {
            dgvWholesalers.Rows.Add(item.WholesalerId, item.Name, item.ContactNo, item.Address ?? string.Empty);
        }
    }

    private void NewClicked(object? sender, EventArgs e)
    {
        using var form = new WholesalerEntryForm(_wholesalers);
        if (form.ShowDialog(this) == DialogResult.OK)
        {
            LoadWholesalers();
        }
    }

    private void EditClicked(object? sender, EventArgs e)
    {
        if (SelectedWholesalerId() is not int id)
        {
            MessageBox.Show(this, Messages.SelectWholesaler, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var result = _wholesalers.GetWholesaler(id, out var wholesaler);
        if (!result.Success || wholesaler is null)
        {
            MessageBox.Show(this, result.Message, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var form = new WholesalerEntryForm(_wholesalers, wholesaler);
        if (form.ShowDialog(this) == DialogResult.OK)
        {
            LoadWholesalers();
        }
    }

    private void DeleteClicked(object? sender, EventArgs e)
    {
        if (SelectedWholesalerId() is not int id)
        {
            MessageBox.Show(this, Messages.SelectWholesaler, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var confirmation = MessageBox.Show(
            this,
            "Are you sure you want to delete this wholesaler?",
            Messages.AppName,
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirmation != DialogResult.Yes)
        {
            return;
        }

        var result = _wholesalers.DeleteWholesaler(id);
        if (!result.Success)
        {
            MessageBox.Show(this, result.Message, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        MessageBox.Show(this, result.Message, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Information);
        LoadWholesalers();
    }

    private int? SelectedWholesalerId()
    {
        if (dgvWholesalers.SelectedRows.Count == 0)
        {
            return null;
        }

        var value = dgvWholesalers.SelectedRows[0].Cells[0].Value;
        return value is int id ? id : null;
    }
}
