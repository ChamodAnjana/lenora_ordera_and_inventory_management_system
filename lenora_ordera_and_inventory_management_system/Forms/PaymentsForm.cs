using lenora_ordera_and_inventory_management_system.Common;
using lenora_ordera_and_inventory_management_system.Models;
using lenora_ordera_and_inventory_management_system.Services;

namespace lenora_ordera_and_inventory_management_system.Forms;

internal sealed class PaymentsForm : Form
{
    private const int FormWidth = 880;
    private const int FormHeight = 600;
    private const int FieldX = 32;

    private readonly PaymentService _payments;
    private readonly List<PendingPaymentOrder> _pendingList = new();

    private readonly Panel pnlAccent = new();
    private readonly Label lblTitle = new();
    private readonly Label lblSubtitle = new();
    private readonly Button btnRefresh = new();
    private readonly Button btnConfirmPayment = new();
    private readonly DataGridView dgvPending = new();
    private readonly Label lblTotalPending = new();

    public PaymentsForm(PaymentService payments)
    {
        _payments = payments;
        BuildChrome();
        AddAccent();
        AddTitle();
        AddActionButtons();
        AddGrid();
        AddFooter();
        LoadPendingPayments();
    }

    private void BuildChrome()
    {
        Text = Messages.PendingPayments;
        Font = new Font("Segoe UI", 10F);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
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
        lblTitle.Text = Messages.PendingPayments;
        lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
        lblTitle.ForeColor = Palette.Ink;
        lblTitle.Location = new Point(FieldX, 16);
        lblTitle.AutoSize = true;
        Controls.Add(lblTitle);

        lblSubtitle.Text = "Delivered orders awaiting courier COD remittance:";
        lblSubtitle.Location = new Point(FieldX, 48);
        lblSubtitle.AutoSize = true;
        lblSubtitle.ForeColor = Color.DimGray;
        Controls.Add(lblSubtitle);
    }

    private void AddActionButtons()
    {
        btnConfirmPayment.Text = Messages.ConfirmPayment;
        btnConfirmPayment.Location = new Point(FieldX, 78);
        btnConfirmPayment.Size = new Size(150, 32);
        btnConfirmPayment.FlatStyle = FlatStyle.Flat;
        btnConfirmPayment.BackColor = Palette.Accent;
        btnConfirmPayment.ForeColor = Palette.White;
        btnConfirmPayment.Click += ConfirmPaymentClicked;
        Controls.Add(btnConfirmPayment);

        btnRefresh.Text = "Refresh";
        btnRefresh.Location = new Point(FieldX + 160, 78);
        btnRefresh.Size = new Size(90, 32);
        btnRefresh.FlatStyle = FlatStyle.Flat;
        btnRefresh.BackColor = Palette.White;
        btnRefresh.Click += (_, _) => LoadPendingPayments();
        Controls.Add(btnRefresh);
    }

    private void AddGrid()
    {
        dgvPending.Location = new Point(FieldX, 120);
        dgvPending.Size = new Size(FormWidth - (FieldX * 2), 400);
        GridTheme.Apply(dgvPending);
        dgvPending.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        var colId = new DataGridViewTextBoxColumn
        {
            Name = "OrderId",
            HeaderText = "Order ID",
            FillWeight = 55,
            MinimumWidth = 55,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        colId.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        colId.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dgvPending.Columns.Add(colId);

        var colName = new DataGridViewTextBoxColumn
        {
            Name = "CustomerName",
            HeaderText = Messages.CustomerName,
            FillWeight = 180,
            MinimumWidth = 140,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        dgvPending.Columns.Add(colName);

        var colPhone = new DataGridViewTextBoxColumn
        {
            Name = "CustomerPhone",
            HeaderText = Messages.CustomerPhone,
            FillWeight = 110,
            MinimumWidth = 100,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        colPhone.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        colPhone.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dgvPending.Columns.Add(colPhone);

        var colOrderDate = new DataGridViewTextBoxColumn
        {
            Name = "OrderDate",
            HeaderText = Messages.OrderDate,
            FillWeight = 95,
            MinimumWidth = 85,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        colOrderDate.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        colOrderDate.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dgvPending.Columns.Add(colOrderDate);

        var colDelivDate = new DataGridViewTextBoxColumn
        {
            Name = "DeliveryDate",
            HeaderText = "Delivery Date",
            FillWeight = 95,
            MinimumWidth = 85,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        colDelivDate.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        colDelivDate.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dgvPending.Columns.Add(colDelivDate);

        var colTotal = new DataGridViewTextBoxColumn
        {
            Name = "TotalAmount",
            HeaderText = Messages.TotalAmount,
            FillWeight = 105,
            MinimumWidth = 95,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        colTotal.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        colTotal.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
        dgvPending.Columns.Add(colTotal);

        Controls.Add(dgvPending);
    }

    private void AddFooter()
    {
        lblTotalPending.Text = "Total Outstanding: Rs. 0.00";
        lblTotalPending.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        lblTotalPending.ForeColor = Palette.Ink;
        lblTotalPending.Location = new Point(FieldX, 535);
        lblTotalPending.Size = new Size(FormWidth - (FieldX * 2), 30);
        lblTotalPending.TextAlign = ContentAlignment.MiddleRight;
        Controls.Add(lblTotalPending);
    }

    private void LoadPendingPayments()
    {
        _pendingList.Clear();
        dgvPending.Rows.Clear();

        var result = _payments.ListPendingPayments(_pendingList);
        if (!result.Success)
        {
            MessageBox.Show(this, result.Message, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        decimal totalOutstanding = 0;
        foreach (var p in _pendingList)
        {
            totalOutstanding += p.TotalAmount;
            dgvPending.Rows.Add(
                p.OrderId,
                p.CustomerName,
                p.CustomerPhone,
                p.OrderDate.ToString("yyyy-MM-dd"),
                p.StatusDate?.ToString("yyyy-MM-dd") ?? string.Empty,
                $"Rs. {p.TotalAmount:F2}");
        }

        lblTotalPending.Text = $"Total Outstanding: Rs. {totalOutstanding:F2} ({_pendingList.Count} orders)";
    }

    private void ConfirmPaymentClicked(object? sender, EventArgs e)
    {
        if (dgvPending.SelectedRows.Count == 0)
        {
            MessageBox.Show(this, Messages.SelectOrder, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var orderId = Convert.ToInt32(dgvPending.SelectedRows[0].Cells["OrderId"].Value);
        var selected = _pendingList.FirstOrDefault(p => p.OrderId == orderId);
        var suggestedAmount = selected?.TotalAmount ?? 0;

        using var form = new ConfirmPaymentForm(_payments, orderId, suggestedAmount);
        if (form.ShowDialog(this) == DialogResult.OK)
        {
            LoadPendingPayments();
        }
    }
}
