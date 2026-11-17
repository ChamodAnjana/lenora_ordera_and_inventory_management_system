using lenora_ordera_and_inventory_management_system.Common;
using lenora_ordera_and_inventory_management_system.Models;
using lenora_ordera_and_inventory_management_system.Security;
using lenora_ordera_and_inventory_management_system.Services;

namespace lenora_ordera_and_inventory_management_system.Forms;

internal sealed class OrdersForm : Form
{
    private const int FormWidth = 980;
    private const int FormHeight = 680;
    private const int FieldX = 32;

    private readonly OrderService _orders;
    private readonly ProductService _products;
    private readonly List<CustomerOrder> _ordersList = new();
    private readonly List<Customer> _repeatList = new();

    private readonly Panel pnlAccent = new();
    private readonly Label lblTitle = new();
    private readonly TabControl tabMain = new();
    private readonly TabPage tabOrders = new();
    private readonly TabPage tabRepeat = new();

    private readonly Label lblSearch = new();
    private readonly TextBox txtSearch = new();
    private readonly Label lblFrom = new();
    private readonly DateTimePicker dtpFrom = new();
    private readonly Label lblTo = new();
    private readonly DateTimePicker dtpTo = new();
    private readonly Label lblStatus = new();
    private readonly ComboBox cmbStatus = new();
    private readonly Button btnFilter = new();

    private readonly Button btnNewOrder = new();
    private readonly Button btnEditOrder = new();
    private readonly Button btnCancelOrder = new();
    private readonly DataGridView dgvOrders = new();
    private readonly DataGridView dgvItems = new();
    private readonly Label lblItemsTitle = new();

    private readonly Label lblRepeatSearch = new();
    private readonly TextBox txtRepeatSearch = new();
    private readonly Button btnRepeatSearch = new();
    private readonly DataGridView dgvRepeat = new();

    public OrdersForm(OrderService orders, ProductService products)
    {
        _orders = orders;
        _products = products;
        BuildChrome();
        AddAccent();
        AddTitle();
        AddTabs();
        LoadOrders();
        LoadRepeatCustomers();
    }

    private void BuildChrome()
    {
        Text = Messages.Orders;
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
        lblTitle.Text = Messages.Orders;
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
        tabOrders.Text = Messages.Orders;
        tabOrders.BackColor = Palette.Paper;
        tabRepeat.Text = Messages.RepeatCustomers;
        tabRepeat.BackColor = Palette.Paper;

        tabMain.TabPages.Add(tabOrders);
        tabMain.TabPages.Add(tabRepeat);
        Controls.Add(tabMain);

        BuildOrdersTab();
        BuildRepeatTab();
    }

    private void BuildOrdersTab()
    {
        AddFilterControls();
        AddActionButtons();
        AddOrdersGrids();
    }

    private void AddFilterControls()
    {
        lblSearch.Text = Messages.Search + ":";
        lblSearch.Location = new Point(12, 18);
        lblSearch.AutoSize = true;
        tabOrders.Controls.Add(lblSearch);

        txtSearch.Location = new Point(70, 15);
        txtSearch.Size = new Size(130, 26);
        tabOrders.Controls.Add(txtSearch);

        lblFrom.Text = Messages.DateFrom + ":";
        lblFrom.Location = new Point(210, 18);
        lblFrom.AutoSize = true;
        tabOrders.Controls.Add(lblFrom);

        dtpFrom.Location = new Point(255, 15);
        dtpFrom.Size = new Size(120, 26);
        dtpFrom.Format = DateTimePickerFormat.Short;
        dtpFrom.Value = DateTime.Today.AddDays(-30);
        tabOrders.Controls.Add(dtpFrom);

        lblTo.Text = Messages.DateTo + ":";
        lblTo.Location = new Point(385, 18);
        lblTo.AutoSize = true;
        tabOrders.Controls.Add(lblTo);

        dtpTo.Location = new Point(415, 15);
        dtpTo.Size = new Size(120, 26);
        dtpTo.Format = DateTimePickerFormat.Short;
        dtpTo.Value = DateTime.Today;
        tabOrders.Controls.Add(dtpTo);

        lblStatus.Text = Messages.Status + ":";
        lblStatus.Location = new Point(545, 18);
        lblStatus.AutoSize = true;
        tabOrders.Controls.Add(lblStatus);

        cmbStatus.Location = new Point(600, 15);
        cmbStatus.Size = new Size(110, 26);
        cmbStatus.DropDownStyle = ComboBoxStyle.DropDownList;
        PopulateStatusFilter();
        tabOrders.Controls.Add(cmbStatus);

        btnFilter.Text = Messages.Filter;
        btnFilter.Location = new Point(720, 14);
        btnFilter.Size = new Size(75, 28);
        btnFilter.FlatStyle = FlatStyle.Flat;
        btnFilter.BackColor = Palette.Accent;
        btnFilter.ForeColor = Palette.White;
        btnFilter.Click += (_, _) => LoadOrders();
        tabOrders.Controls.Add(btnFilter);
    }

    private void PopulateStatusFilter()
    {
        cmbStatus.Items.Add(Messages.All);
        foreach (var status in Enum.GetNames<OrderStatus>())
        {
            cmbStatus.Items.Add(status);
        }
        cmbStatus.SelectedIndex = 0;
    }

    private void AddActionButtons()
    {
        btnNewOrder.Text = Messages.NewOrder;
        btnNewOrder.Location = new Point(12, 54);
        btnNewOrder.Size = new Size(110, 32);
        btnNewOrder.FlatStyle = FlatStyle.Flat;
        btnNewOrder.BackColor = Palette.Accent;
        btnNewOrder.ForeColor = Palette.White;
        btnNewOrder.Click += NewOrderClicked;
        tabOrders.Controls.Add(btnNewOrder);

        btnEditOrder.Text = Messages.EditOrder;
        btnEditOrder.Location = new Point(130, 54);
        btnEditOrder.Size = new Size(110, 32);
        btnEditOrder.FlatStyle = FlatStyle.Flat;
        btnEditOrder.BackColor = Palette.White;
        btnEditOrder.Click += EditOrderClicked;
        tabOrders.Controls.Add(btnEditOrder);

        btnCancelOrder.Text = Messages.CancelOrder;
        btnCancelOrder.Location = new Point(248, 54);
        btnCancelOrder.Size = new Size(120, 32);
        btnCancelOrder.FlatStyle = FlatStyle.Flat;
        btnCancelOrder.BackColor = Palette.White;
        btnCancelOrder.Visible = CurrentSession.Current?.Role == UserRole.Owner;
        btnCancelOrder.Click += CancelOrderClicked;
        tabOrders.Controls.Add(btnCancelOrder);
    }

    private void AddOrdersGrids()
    {
        ConfigureOrdersGrid();
        tabOrders.Controls.Add(dgvOrders);

        lblItemsTitle.Text = Messages.OrderItems;
        lblItemsTitle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        lblItemsTitle.ForeColor = Palette.Ink;
        lblItemsTitle.Location = new Point(12, 360);
        lblItemsTitle.AutoSize = true;
        tabOrders.Controls.Add(lblItemsTitle);

        ConfigureItemsGrid();
        tabOrders.Controls.Add(dgvItems);
    }

    private void ConfigureOrdersGrid()
    {
        var gridWidth = tabMain.Width - 32;
        dgvOrders.Location = new Point(12, 95);
        dgvOrders.Size = new Size(gridWidth, 255);
        dgvOrders.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        GridTheme.Apply(dgvOrders);
        dgvOrders.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

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
        dgvOrders.Columns.Add(colId);

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
        dgvOrders.Columns.Add(colDate);

        var colName = new DataGridViewTextBoxColumn
        {
            Name = "CustomerName",
            HeaderText = Messages.CustomerName,
            FillWeight = 160,
            MinimumWidth = 140,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        dgvOrders.Columns.Add(colName);

        var colPhone = new DataGridViewTextBoxColumn
        {
            Name = "CustomerPhone",
            HeaderText = Messages.CustomerPhone,
            FillWeight = 110,
            MinimumWidth = 95,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        colPhone.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        colPhone.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dgvOrders.Columns.Add(colPhone);

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
        dgvOrders.Columns.Add(colStatus);

        var colTotal = new DataGridViewTextBoxColumn
        {
            Name = "TotalAmount",
            HeaderText = Messages.TotalAmount,
            FillWeight = 95,
            MinimumWidth = 90,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        colTotal.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        colTotal.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
        dgvOrders.Columns.Add(colTotal);

        var colRepeat = new DataGridViewTextBoxColumn
        {
            Name = "Repeat",
            HeaderText = Messages.RepeatCustomer,
            FillWeight = 75,
            MinimumWidth = 70,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        colRepeat.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        colRepeat.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dgvOrders.Columns.Add(colRepeat);

        var colUser = new DataGridViewTextBoxColumn
        {
            Name = "RecordedBy",
            HeaderText = Messages.RecordedBy,
            FillWeight = 100,
            MinimumWidth = 90,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        dgvOrders.Columns.Add(colUser);

        dgvOrders.SelectionChanged += OrdersSelectionChanged;
        dgvOrders.CellFormatting += FormatOrderCell;
    }

    private static void FormatOrderCell(object? sender, DataGridViewCellFormattingEventArgs e)
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

    private void ConfigureItemsGrid()
    {
        var gridWidth = tabMain.Width - 32;
        dgvItems.Location = new Point(12, 388);
        dgvItems.Size = new Size(gridWidth, 160);
        dgvItems.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        GridTheme.Apply(dgvItems);
        dgvItems.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        var colProduct = new DataGridViewTextBoxColumn
        {
            Name = "ProductName",
            HeaderText = Messages.Product,
            FillWeight = 220,
            MinimumWidth = 180,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        dgvItems.Columns.Add(colProduct);

        var colQty = new DataGridViewTextBoxColumn
        {
            Name = "Quantity",
            HeaderText = Messages.Quantity,
            FillWeight = 70,
            MinimumWidth = 65,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        colQty.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        colQty.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dgvItems.Columns.Add(colQty);

        var colPrice = new DataGridViewTextBoxColumn
        {
            Name = "SellingPrice",
            HeaderText = Messages.SellingPrice,
            FillWeight = 95,
            MinimumWidth = 90,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        colPrice.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        colPrice.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
        dgvItems.Columns.Add(colPrice);

        var colSubtotal = new DataGridViewTextBoxColumn
        {
            Name = "Subtotal",
            HeaderText = "Subtotal",
            FillWeight = 105,
            MinimumWidth = 100,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        colSubtotal.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        colSubtotal.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
        dgvItems.Columns.Add(colSubtotal);
    }

    private void BuildRepeatTab()
    {
        lblRepeatSearch.Text = Messages.Search + ":";
        lblRepeatSearch.Location = new Point(12, 18);
        lblRepeatSearch.AutoSize = true;
        tabRepeat.Controls.Add(lblRepeatSearch);

        txtRepeatSearch.Location = new Point(70, 15);
        txtRepeatSearch.Size = new Size(220, 26);
        tabRepeat.Controls.Add(txtRepeatSearch);

        btnRepeatSearch.Text = Messages.Search;
        btnRepeatSearch.Location = new Point(300, 14);
        btnRepeatSearch.Size = new Size(80, 28);
        btnRepeatSearch.FlatStyle = FlatStyle.Flat;
        btnRepeatSearch.BackColor = Palette.Accent;
        btnRepeatSearch.ForeColor = Palette.White;
        btnRepeatSearch.Click += (_, _) => LoadRepeatCustomers();
        tabRepeat.Controls.Add(btnRepeatSearch);

        ConfigureRepeatGrid();
        tabRepeat.Controls.Add(dgvRepeat);
    }

    private void ConfigureRepeatGrid()
    {
        var gridWidth = tabMain.Width - 32;
        dgvRepeat.Location = new Point(12, 55);
        dgvRepeat.Size = new Size(gridWidth, 490);
        dgvRepeat.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
        GridTheme.Apply(dgvRepeat);
        dgvRepeat.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        var colId = new DataGridViewTextBoxColumn
        {
            Name = "CustomerId",
            HeaderText = "Customer ID",
            FillWeight = 60,
            MinimumWidth = 60,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        colId.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        colId.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dgvRepeat.Columns.Add(colId);

        var colName = new DataGridViewTextBoxColumn
        {
            Name = "Name",
            HeaderText = Messages.CustomerName,
            FillWeight = 160,
            MinimumWidth = 130,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        dgvRepeat.Columns.Add(colName);

        var colPhone = new DataGridViewTextBoxColumn
        {
            Name = "Phone",
            HeaderText = Messages.CustomerPhone,
            FillWeight = 110,
            MinimumWidth = 100,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        colPhone.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        colPhone.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dgvRepeat.Columns.Add(colPhone);

        var colAddress = new DataGridViewTextBoxColumn
        {
            Name = "Address",
            HeaderText = Messages.Address,
            FillWeight = 230,
            MinimumWidth = 180,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        dgvRepeat.Columns.Add(colAddress);

        var colOrders = new DataGridViewTextBoxColumn
        {
            Name = "OrderCount",
            HeaderText = "Total Orders",
            FillWeight = 75,
            MinimumWidth = 70,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        colOrders.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        colOrders.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dgvRepeat.Columns.Add(colOrders);
    }

    private void LoadOrders()
    {
        _ordersList.Clear();
        dgvOrders.Rows.Clear();
        dgvItems.Rows.Clear();

        var search = txtSearch.Text.Trim();
        var from = dtpFrom.Value.Date;
        var to = dtpTo.Value.Date;
        OrderStatus? status = cmbStatus.SelectedIndex > 0
            ? Enum.Parse<OrderStatus>(cmbStatus.SelectedItem!.ToString()!)
            : null;

        var result = _orders.ListOrders(_ordersList, search, from, to, status);
        if (!result.Success)
        {
            MessageBox.Show(this, result.Message, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        foreach (var o in _ordersList)
        {
            var repeatText = o.IsRepeatCustomer ? "Yes ♥" : "No";
            dgvOrders.Rows.Add(
                o.OrderId,
                o.OrderDate.ToString("yyyy-MM-dd"),
                o.CustomerName,
                o.CustomerPhone,
                o.Status.ToString(),
                $"Rs. {o.TotalAmount:F2}",
                repeatText,
                o.CreatedByUserName);
        }
    }

    private void OrdersSelectionChanged(object? sender, EventArgs e)
    {
        dgvItems.Rows.Clear();
        if (dgvOrders.SelectedRows.Count == 0)
        {
            return;
        }

        var orderId = Convert.ToInt32(dgvOrders.SelectedRows[0].Cells["OrderId"].Value);
        var items = new List<OrderItem>();
        var result = _orders.GetOrderDetails(orderId, out _, items);
        if (!result.Success)
        {
            return;
        }

        foreach (var item in items)
        {
            dgvItems.Rows.Add(item.ProductName, item.Quantity, $"Rs. {item.SellingPrice:F2}", $"Rs. {item.Subtotal:F2}");
        }
    }

    private void LoadRepeatCustomers()
    {
        _repeatList.Clear();
        dgvRepeat.Rows.Clear();
        var search = txtRepeatSearch.Text.Trim();
        var result = _orders.ListRepeatCustomers(_repeatList, search);
        if (!result.Success)
        {
            MessageBox.Show(this, result.Message, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        foreach (var c in _repeatList)
        {
            dgvRepeat.Rows.Add(c.CustomerId, c.Name, c.Phone, c.Address, c.OrderCount);
        }
    }

    private void NewOrderClicked(object? sender, EventArgs e)
    {
        using var form = new OrderEntryForm(_orders, _products);
        if (form.ShowDialog(this) == DialogResult.OK)
        {
            LoadOrders();
            LoadRepeatCustomers();
        }
    }

    private void EditOrderClicked(object? sender, EventArgs e)
    {
        if (dgvOrders.SelectedRows.Count == 0)
        {
            MessageBox.Show(this, Messages.SelectOrder, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var orderId = Convert.ToInt32(dgvOrders.SelectedRows[0].Cells["OrderId"].Value);
        var statusStr = dgvOrders.SelectedRows[0].Cells["Status"].Value?.ToString();
        if (statusStr != OrderStatus.Placed.ToString())
        {
            MessageBox.Show(this, Messages.OnlyPlacedOrdersCanBeUpdated, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var form = new OrderEntryForm(_orders, _products, orderId);
        if (form.ShowDialog(this) == DialogResult.OK)
        {
            LoadOrders();
            LoadRepeatCustomers();
        }
    }

    private void CancelOrderClicked(object? sender, EventArgs e)
    {
        if (dgvOrders.SelectedRows.Count == 0)
        {
            MessageBox.Show(this, Messages.SelectOrder, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var orderId = Convert.ToInt32(dgvOrders.SelectedRows[0].Cells["OrderId"].Value);
        var statusStr = dgvOrders.SelectedRows[0].Cells["Status"].Value?.ToString();
        if (statusStr != OrderStatus.Placed.ToString())
        {
            MessageBox.Show(this, Messages.OnlyPlacedOrdersCanBeCancelled, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var confirm = MessageBox.Show(
            this,
            $"Are you sure you want to cancel order #{orderId}? Items will be returned to stock.",
            Messages.AppName,
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirm != DialogResult.Yes)
        {
            return;
        }

        var result = _orders.CancelOrder(orderId);
        MessageBox.Show(this, result.Message, Messages.AppName, MessageBoxButtons.OK, result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        if (result.Success)
        {
            LoadOrders();
            LoadRepeatCustomers();
        }
    }
}
