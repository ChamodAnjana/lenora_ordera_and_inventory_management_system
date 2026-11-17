using lenora_ordera_and_inventory_management_system.Common;
using lenora_ordera_and_inventory_management_system.Models;
using lenora_ordera_and_inventory_management_system.Services;

namespace lenora_ordera_and_inventory_management_system.Forms;

internal sealed class DeliveryForm : Form
{
    private const int FormWidth = 960;
    private const int FormHeight = 640;
    private const int FieldX = 32;

    private readonly DeliveryService _delivery;
    private readonly OrderService _orders;
    private readonly List<CustomerOrder> _ordersList = new();
    private readonly List<FollowUpOrder> _followUpList = new();

    private readonly Panel pnlAccent = new();
    private readonly Label lblTitle = new();
    private readonly TabControl tabMain = new();
    private readonly TabPage tabDispatch = new();
    private readonly TabPage tabFollowUp = new();

    private readonly Label lblFilterStatus = new();
    private readonly ComboBox cmbFilterStatus = new();
    private readonly Button btnRefreshDispatch = new();
    private readonly Button btnPostOrder = new();
    private readonly Button btnMarkDelivered = new();
    private readonly Button btnMarkReturned = new();
    private readonly DataGridView dgvDispatch = new();

    private readonly Label lblFollowUpNotice = new();
    private readonly Button btnRefreshFollowUp = new();
    private readonly Button btnFollowUpDelivered = new();
    private readonly Button btnFollowUpReturned = new();
    private readonly DataGridView dgvFollowUp = new();

    public DeliveryForm(DeliveryService delivery, OrderService orders)
    {
        _delivery = delivery;
        _orders = orders;
        BuildChrome();
        AddAccent();
        AddTitle();
        AddTabs();
        LoadDispatchOrders();
        LoadFollowUpOrders();
    }

    private void BuildChrome()
    {
        Text = Messages.Delivery;
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
        lblTitle.Text = Messages.Delivery;
        lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
        lblTitle.ForeColor = Palette.Ink;
        lblTitle.Location = new Point(FieldX, 16);
        lblTitle.AutoSize = true;
        Controls.Add(lblTitle);
    }

    private void AddTabs()
    {
        tabMain.Location = new Point(FieldX, 55);
        tabMain.Size = new Size(FormWidth - (FieldX * 2), FormHeight - 75);

        tabDispatch.Text = "Dispatch & Status Updates";
        tabDispatch.BackColor = Palette.Paper;

        tabFollowUp.Text = Messages.FollowUpList;
        tabFollowUp.BackColor = Palette.Paper;

        tabMain.TabPages.Add(tabDispatch);
        tabMain.TabPages.Add(tabFollowUp);
        Controls.Add(tabMain);

        BuildDispatchTab();
        BuildFollowUpTab();
    }

    private void BuildDispatchTab()
    {
        AddDispatchControls();
        AddDispatchGrid();
    }

    private void AddDispatchControls()
    {
        lblFilterStatus.Text = Messages.Status + ":";
        lblFilterStatus.Location = new Point(12, 18);
        lblFilterStatus.AutoSize = true;
        tabDispatch.Controls.Add(lblFilterStatus);

        cmbFilterStatus.Location = new Point(70, 15);
        cmbFilterStatus.Size = new Size(130, 26);
        cmbFilterStatus.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbFilterStatus.Items.AddRange(["Placed or Posted", "Placed", "Posted"]);
        cmbFilterStatus.SelectedIndex = 0;
        cmbFilterStatus.SelectedIndexChanged += (_, _) => LoadDispatchOrders();
        tabDispatch.Controls.Add(cmbFilterStatus);

        btnRefreshDispatch.Text = "Refresh";
        btnRefreshDispatch.Location = new Point(210, 14);
        btnRefreshDispatch.Size = new Size(80, 28);
        btnRefreshDispatch.FlatStyle = FlatStyle.Flat;
        btnRefreshDispatch.BackColor = Palette.White;
        btnRefreshDispatch.Click += (_, _) => LoadDispatchOrders();
        tabDispatch.Controls.Add(btnRefreshDispatch);

        btnPostOrder.Text = Messages.PostOrder;
        btnPostOrder.Location = new Point(12, 54);
        btnPostOrder.Size = new Size(110, 32);
        btnPostOrder.FlatStyle = FlatStyle.Flat;
        btnPostOrder.BackColor = Palette.Accent;
        btnPostOrder.ForeColor = Palette.White;
        btnPostOrder.Click += PostOrderClicked;
        tabDispatch.Controls.Add(btnPostOrder);

        btnMarkDelivered.Text = Messages.MarkDelivered;
        btnMarkDelivered.Location = new Point(130, 54);
        btnMarkDelivered.Size = new Size(130, 32);
        btnMarkDelivered.FlatStyle = FlatStyle.Flat;
        btnMarkDelivered.BackColor = Palette.White;
        btnMarkDelivered.Click += MarkDeliveredClicked;
        tabDispatch.Controls.Add(btnMarkDelivered);

        btnMarkReturned.Text = Messages.MarkReturned;
        btnMarkReturned.Location = new Point(268, 54);
        btnMarkReturned.Size = new Size(130, 32);
        btnMarkReturned.FlatStyle = FlatStyle.Flat;
        btnMarkReturned.BackColor = Palette.White;
        btnMarkReturned.Click += MarkReturnedClicked;
        tabDispatch.Controls.Add(btnMarkReturned);
    }

    private void AddDispatchGrid()
    {
        var gridWidth = tabMain.Width - 32;
        dgvDispatch.Location = new Point(12, 95);
        dgvDispatch.Size = new Size(gridWidth, 440);
        dgvDispatch.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
        GridTheme.Apply(dgvDispatch);
        dgvDispatch.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

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
        dgvDispatch.Columns.Add(colId);

        var colName = new DataGridViewTextBoxColumn
        {
            Name = "CustomerName",
            HeaderText = Messages.CustomerName,
            FillWeight = 170,
            MinimumWidth = 140,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        dgvDispatch.Columns.Add(colName);

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
        dgvDispatch.Columns.Add(colPhone);

        var colStatus = new DataGridViewTextBoxColumn
        {
            Name = "Status",
            HeaderText = Messages.Status,
            FillWeight = 80,
            MinimumWidth = 75,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        colStatus.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        colStatus.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dgvDispatch.Columns.Add(colStatus);

        var colDate = new DataGridViewTextBoxColumn
        {
            Name = "OrderDate",
            HeaderText = Messages.OrderDate,
            FillWeight = 90,
            MinimumWidth = 85,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        colDate.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        colDate.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dgvDispatch.Columns.Add(colDate);

        var colCourier = new DataGridViewTextBoxColumn
        {
            Name = "Courier",
            HeaderText = Messages.Courier,
            FillWeight = 130,
            MinimumWidth = 110,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        dgvDispatch.Columns.Add(colCourier);

        var colPostDate = new DataGridViewTextBoxColumn
        {
            Name = "PostingDate",
            HeaderText = Messages.PostingDate,
            FillWeight = 95,
            MinimumWidth = 85,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        colPostDate.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        colPostDate.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dgvDispatch.Columns.Add(colPostDate);

        dgvDispatch.CellFormatting += FormatDispatchCell;
        tabDispatch.Controls.Add(dgvDispatch);
    }

    private static void FormatDispatchCell(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0)
        {
            return;
        }

        var grid = (DataGridView)sender!;
        if (grid.Columns[e.ColumnIndex].Name == "Status")
        {
            GridTheme.HighlightStatusCell(e, e.Value?.ToString());
        }
    }

    private void BuildFollowUpTab()
    {
        lblFollowUpNotice.Text = "Orders posted 2 or more days ago without delivery update (oldest first):";
        lblFollowUpNotice.Location = new Point(12, 18);
        lblFollowUpNotice.AutoSize = true;
        lblFollowUpNotice.Font = new Font("Segoe UI", 9.5F, FontStyle.Italic);
        lblFollowUpNotice.ForeColor = Palette.Ink;
        tabFollowUp.Controls.Add(lblFollowUpNotice);

        btnRefreshFollowUp.Text = "Refresh";
        btnRefreshFollowUp.Location = new Point(tabMain.Width - 110, 14);
        btnRefreshFollowUp.Size = new Size(80, 28);
        btnRefreshFollowUp.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnRefreshFollowUp.FlatStyle = FlatStyle.Flat;
        btnRefreshFollowUp.BackColor = Palette.White;
        btnRefreshFollowUp.Click += (_, _) => LoadFollowUpOrders();
        tabFollowUp.Controls.Add(btnRefreshFollowUp);

        btnFollowUpDelivered.Text = Messages.MarkDelivered;
        btnFollowUpDelivered.Location = new Point(12, 54);
        btnFollowUpDelivered.Size = new Size(130, 32);
        btnFollowUpDelivered.FlatStyle = FlatStyle.Flat;
        btnFollowUpDelivered.BackColor = Palette.Accent;
        btnFollowUpDelivered.ForeColor = Palette.White;
        btnFollowUpDelivered.Click += FollowUpDeliveredClicked;
        tabFollowUp.Controls.Add(btnFollowUpDelivered);

        btnFollowUpReturned.Text = Messages.MarkReturned;
        btnFollowUpReturned.Location = new Point(150, 54);
        btnFollowUpReturned.Size = new Size(130, 32);
        btnFollowUpReturned.FlatStyle = FlatStyle.Flat;
        btnFollowUpReturned.BackColor = Palette.White;
        btnFollowUpReturned.Click += FollowUpReturnedClicked;
        tabFollowUp.Controls.Add(btnFollowUpReturned);

        ConfigureFollowUpGrid();
        tabFollowUp.Controls.Add(dgvFollowUp);
    }

    private void ConfigureFollowUpGrid()
    {
        var gridWidth = tabMain.Width - 32;
        dgvFollowUp.Location = new Point(12, 95);
        dgvFollowUp.Size = new Size(gridWidth, 440);
        dgvFollowUp.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
        GridTheme.Apply(dgvFollowUp);
        dgvFollowUp.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

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
        dgvFollowUp.Columns.Add(colId);

        var colName = new DataGridViewTextBoxColumn
        {
            Name = "CustomerName",
            HeaderText = Messages.CustomerName,
            FillWeight = 180,
            MinimumWidth = 140,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        dgvFollowUp.Columns.Add(colName);

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
        dgvFollowUp.Columns.Add(colPhone);

        var colCourier = new DataGridViewTextBoxColumn
        {
            Name = "Courier",
            HeaderText = Messages.Courier,
            FillWeight = 135,
            MinimumWidth = 110,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        dgvFollowUp.Columns.Add(colCourier);

        var colDate = new DataGridViewTextBoxColumn
        {
            Name = "PostingDate",
            HeaderText = Messages.PostingDate,
            FillWeight = 95,
            MinimumWidth = 85,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        colDate.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        colDate.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dgvFollowUp.Columns.Add(colDate);

        var colDays = new DataGridViewTextBoxColumn
        {
            Name = "DaysElapsed",
            HeaderText = Messages.DaysElapsed,
            FillWeight = 75,
            MinimumWidth = 70,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        colDays.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        colDays.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dgvFollowUp.Columns.Add(colDays);
    }

    private void LoadDispatchOrders()
    {
        _ordersList.Clear();
        dgvDispatch.Rows.Clear();

        OrderStatus? status = cmbFilterStatus.SelectedIndex switch
        {
            1 => OrderStatus.Placed,
            2 => OrderStatus.Posted,
            _ => null
        };

        var result = _orders.ListOrders(_ordersList, null, null, null, status);
        if (!result.Success)
        {
            MessageBox.Show(this, result.Message, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var filtered = cmbFilterStatus.SelectedIndex == 0
            ? _ordersList.Where(o => o.Status == OrderStatus.Placed || o.Status == OrderStatus.Posted)
            : _ordersList;

        foreach (var o in filtered)
        {
            dgvDispatch.Rows.Add(
                o.OrderId,
                o.CustomerName,
                o.CustomerPhone,
                o.Status.ToString(),
                o.OrderDate.ToString("yyyy-MM-dd"),
                o.CourierName ?? string.Empty,
                o.PostingDate?.ToString("yyyy-MM-dd") ?? string.Empty);
        }
    }

    private void LoadFollowUpOrders()
    {
        _followUpList.Clear();
        dgvFollowUp.Rows.Clear();

        var result = _delivery.GetFollowUpList(_followUpList);
        if (!result.Success)
        {
            MessageBox.Show(this, result.Message, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        foreach (var f in _followUpList)
        {
            dgvFollowUp.Rows.Add(
                f.OrderId,
                f.CustomerName,
                f.CustomerPhone,
                f.CourierName,
                f.PostingDate.ToString("yyyy-MM-dd"),
                $"{f.DaysElapsed} days");
        }
    }

    private void PostOrderClicked(object? sender, EventArgs e)
    {
        if (dgvDispatch.SelectedRows.Count == 0)
        {
            MessageBox.Show(this, Messages.SelectOrder, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var orderId = Convert.ToInt32(dgvDispatch.SelectedRows[0].Cells["OrderId"].Value);
        var status = dgvDispatch.SelectedRows[0].Cells["Status"].Value?.ToString();
        if (status != OrderStatus.Placed.ToString())
        {
            MessageBox.Show(this, Messages.OnlyPlacedCanBePosted, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var form = new PostOrderForm(_delivery, orderId);
        if (form.ShowDialog(this) == DialogResult.OK)
        {
            LoadDispatchOrders();
            LoadFollowUpOrders();
        }
    }

    private void MarkDeliveredClicked(object? sender, EventArgs e)
    {
        if (dgvDispatch.SelectedRows.Count == 0)
        {
            MessageBox.Show(this, Messages.SelectOrder, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var orderId = Convert.ToInt32(dgvDispatch.SelectedRows[0].Cells["OrderId"].Value);
        var status = dgvDispatch.SelectedRows[0].Cells["Status"].Value?.ToString();
        if (status != OrderStatus.Posted.ToString())
        {
            MessageBox.Show(this, Messages.OnlyPostedCanBeUpdated, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        UpdateDelivery(orderId, OrderStatus.Delivered);
    }

    private void MarkReturnedClicked(object? sender, EventArgs e)
    {
        if (dgvDispatch.SelectedRows.Count == 0)
        {
            MessageBox.Show(this, Messages.SelectOrder, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var orderId = Convert.ToInt32(dgvDispatch.SelectedRows[0].Cells["OrderId"].Value);
        var status = dgvDispatch.SelectedRows[0].Cells["Status"].Value?.ToString();
        if (status != OrderStatus.Posted.ToString())
        {
            MessageBox.Show(this, Messages.OnlyPostedCanBeUpdated, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var confirm = MessageBox.Show(
            this,
            $"Mark order #{orderId} as Returned? All items will be added back to stock.",
            Messages.AppName,
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirm == DialogResult.Yes)
        {
            UpdateDelivery(orderId, OrderStatus.Returned);
        }
    }

    private void FollowUpDeliveredClicked(object? sender, EventArgs e)
    {
        if (dgvFollowUp.SelectedRows.Count == 0)
        {
            MessageBox.Show(this, Messages.SelectOrder, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var orderId = Convert.ToInt32(dgvFollowUp.SelectedRows[0].Cells["OrderId"].Value);
        UpdateDelivery(orderId, OrderStatus.Delivered);
    }

    private void FollowUpReturnedClicked(object? sender, EventArgs e)
    {
        if (dgvFollowUp.SelectedRows.Count == 0)
        {
            MessageBox.Show(this, Messages.SelectOrder, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var orderId = Convert.ToInt32(dgvFollowUp.SelectedRows[0].Cells["OrderId"].Value);
        var confirm = MessageBox.Show(
            this,
            $"Mark order #{orderId} as Returned? All items will be added back to stock.",
            Messages.AppName,
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirm == DialogResult.Yes)
        {
            UpdateDelivery(orderId, OrderStatus.Returned);
        }
    }

    private void UpdateDelivery(int orderId, OrderStatus newStatus)
    {
        var request = new DeliveryStatusRequest
        {
            OrderId = orderId,
            Status = newStatus,
            StatusDate = DateTime.Today
        };

        var result = _delivery.UpdateDeliveryStatus(request);
        MessageBox.Show(this, result.Message, Messages.AppName, MessageBoxButtons.OK, result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        if (result.Success)
        {
            LoadDispatchOrders();
            LoadFollowUpOrders();
        }
    }
}
