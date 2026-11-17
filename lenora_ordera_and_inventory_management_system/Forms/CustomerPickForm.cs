using lenora_ordera_and_inventory_management_system.Common;
using lenora_ordera_and_inventory_management_system.Models;

namespace lenora_ordera_and_inventory_management_system.Forms;

internal sealed class CustomerPickForm : Form
{
    private const int FormWidth = 640;
    private const int FormHeight = 420;
    private const int FieldX = 24;

    private readonly IReadOnlyList<Customer> _customers;
    private readonly Panel pnlAccent = new();
    private readonly Label lblTitle = new();
    private readonly DataGridView dgvCustomers = new();
    private readonly Button btnSelect = new();
    private readonly Button btnCancel = new();

    public CustomerPickForm(IReadOnlyList<Customer> customers)
    {
        _customers = customers;
        BuildChrome();
        AddAccent();
        AddTitle();
        AddGrid();
        AddActions();
        FillGrid();
    }

    public Customer? SelectedCustomer { get; private set; }

    private void BuildChrome()
    {
        Text = Messages.SelectCustomer;
        Font = new Font("Segoe UI", 10F);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(FormWidth, FormHeight);
        BackColor = Palette.Paper;
        AcceptButton = btnSelect;
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
        lblTitle.Text = Messages.SelectCustomer;
        lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
        lblTitle.ForeColor = Palette.Ink;
        lblTitle.Location = new Point(FieldX, 16);
        lblTitle.AutoSize = true;
        Controls.Add(lblTitle);
    }

    private void AddGrid()
    {
        dgvCustomers.Location = new Point(FieldX, 58);
        dgvCustomers.Size = new Size(FormWidth - (FieldX * 2), 290);
        GridTheme.Apply(dgvCustomers);
        dgvCustomers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvCustomers.CellDoubleClick += (_, _) => SelectClicked(this, EventArgs.Empty);
        AddColumns();
        Controls.Add(dgvCustomers);
    }

    private void AddColumns()
    {
        dgvCustomers.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "CustomerId",
            HeaderText = "ID",
            Visible = false
        });
        dgvCustomers.Columns.Add(TextColumn("Name", Messages.CustomerName, 180));
        dgvCustomers.Columns.Add(TextColumn("Phone", Messages.CustomerPhone, 120));
        dgvCustomers.Columns.Add(TextColumn("Address", Messages.Address, 220));
    }

    private static DataGridViewTextBoxColumn TextColumn(string name, string header, int minimumWidth)
    {
        return new DataGridViewTextBoxColumn
        {
            Name = name,
            HeaderText = header,
            MinimumWidth = minimumWidth,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
    }

    private void AddActions()
    {
        btnSelect.Text = Messages.SelectCustomer;
        btnSelect.Location = new Point(FormWidth - 310, FormHeight - 58);
        btnSelect.Size = new Size(170, 34);
        btnSelect.FlatStyle = FlatStyle.Flat;
        btnSelect.BackColor = Palette.Accent;
        btnSelect.ForeColor = Palette.White;
        btnSelect.Click += SelectClicked;
        Controls.Add(btnSelect);

        btnCancel.Text = Messages.Cancel;
        btnCancel.Location = new Point(FormWidth - 130, FormHeight - 58);
        btnCancel.Size = new Size(100, 34);
        btnCancel.FlatStyle = FlatStyle.Flat;
        btnCancel.BackColor = Palette.White;
        btnCancel.DialogResult = DialogResult.Cancel;
        Controls.Add(btnCancel);
    }

    private void FillGrid()
    {
        dgvCustomers.Rows.Clear();
        foreach (var customer in _customers)
        {
            dgvCustomers.Rows.Add(customer.CustomerId, customer.Name, customer.Phone, customer.Address);
        }
    }

    private void SelectClicked(object? sender, EventArgs e)
    {
        if (dgvCustomers.SelectedRows.Count == 0)
        {
            MessageBox.Show(this, Messages.SelectCustomer, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var customerId = Convert.ToInt32(dgvCustomers.SelectedRows[0].Cells["CustomerId"].Value);
        SelectedCustomer = _customers.FirstOrDefault(customer => customer.CustomerId == customerId);
        if (SelectedCustomer is null)
        {
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }
}
