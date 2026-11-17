using lenora_ordera_and_inventory_management_system.Common;
using lenora_ordera_and_inventory_management_system.Models;
using lenora_ordera_and_inventory_management_system.Services;

namespace lenora_ordera_and_inventory_management_system.Forms;

internal sealed class StockOrdersForm : Form
{
    private const int FormWidth = 880;
    private const int FormHeight = 640;
    private const int FieldX = 32;

    private readonly StockService _stock;
    private readonly WholesalerService _wholesalers;
    private readonly ProductService _products;
    private readonly List<Wholesaler> _allWholesalers = new();

    private readonly Panel pnlAccent = new();
    private readonly Label lblTitle = new();
    private readonly Label lblFrom = new();
    private readonly DateTimePicker dtpFrom = new();
    private readonly Label lblTo = new();
    private readonly DateTimePicker dtpTo = new();
    private readonly Label lblWholesaler = new();
    private readonly ComboBox cmbWholesaler = new();
    private readonly Button btnFilter = new();
    private readonly Button btnNewOrder = new();
    private readonly TabControl tabMain = new();
    private readonly TabPage tabOrders = new();
    private readonly TabPage tabSpending = new();
    private readonly DataGridView dgvOrders = new();
    private readonly DataGridView dgvItems = new();
    private readonly Label lblItemsTitle = new();
    private readonly DataGridView dgvSpending = new();
    private readonly Label lblSpendingGrandTotal = new();

    public StockOrdersForm(StockService stock, WholesalerService wholesalers, ProductService products)
    {
        _stock = stock;
        _wholesalers = wholesalers;
        _products = products;
        BuildChrome();
        AddAccent();
        AddTitle();
        AddFilters();
        AddTabs();
        LoadWholesalers();
        LoadOrders();
    }

    private void BuildChrome()
    {
        Text = Messages.StockOrders;
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
        lblTitle.Text = Messages.StockOrders;
        lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
        lblTitle.ForeColor = Palette.Ink;
        lblTitle.Location = new Point(FieldX, 16);
        lblTitle.AutoSize = true;
        Controls.Add(lblTitle);
    }

    private void AddFilters()
    {
        lblFrom.Text = Messages.DateFrom + ":";
        lblFrom.Location = new Point(FieldX, 58);
        lblFrom.AutoSize = true;
        Controls.Add(lblFrom);

        dtpFrom.Location = new Point(FieldX + 44, 55);
        dtpFrom.Size = new Size(110, 26);
        dtpFrom.Format = DateTimePickerFormat.Short;
        dtpFrom.Value = DateTime.Today.AddMonths(-1);
        Controls.Add(dtpFrom);

        lblTo.Text = Messages.DateTo + ":";
        lblTo.Location = new Point(FieldX + 168, 58);
        lblTo.AutoSize = true;
        Controls.Add(lblTo);

        dtpTo.Location = new Point(FieldX + 198, 55);
        dtpTo.Size = new Size(110, 26);
        dtpTo.Format = DateTimePickerFormat.Short;
        dtpTo.Value = DateTime.Today;
        Controls.Add(dtpTo);

        lblWholesaler.Text = Messages.Wholesaler + ":";
        lblWholesaler.Location = new Point(FieldX + 322, 58);
        lblWholesaler.AutoSize = true;
        Controls.Add(lblWholesaler);

        cmbWholesaler.Location = new Point(FieldX + 408, 55);
        cmbWholesaler.Size = new Size(150, 26);
        cmbWholesaler.DropDownStyle = ComboBoxStyle.DropDownList;
        Controls.Add(cmbWholesaler);

        btnFilter.Text = Messages.Filter;
        btnFilter.Location = new Point(FieldX + 572, 54);
        btnFilter.Size = new Size(76, 28);
        btnFilter.FlatStyle = FlatStyle.Flat;
        btnFilter.FlatAppearance.BorderColor = Palette.Ink;
        btnFilter.FlatAppearance.BorderSize = 1;
        btnFilter.BackColor = Palette.White;
        btnFilter.ForeColor = Palette.Ink;
        btnFilter.Click += (s, e) => RefreshActiveTab();
        Controls.Add(btnFilter);

        btnNewOrder.Text = Messages.NewStockOrder;
        btnNewOrder.Location = new Point(FormWidth - FieldX - 150, 52);
        btnNewOrder.Size = new Size(150, 32);
        btnNewOrder.FlatStyle = FlatStyle.Flat;
        btnNewOrder.FlatAppearance.BorderSize = 0;
        btnNewOrder.BackColor = Palette.Accent;
        btnNewOrder.ForeColor = Palette.White;
        btnNewOrder.Cursor = Cursors.Hand;
        btnNewOrder.Click += NewOrderClicked;
        Controls.Add(btnNewOrder);
    }

    private void AddTabs()
    {
        tabMain.Location = new Point(FieldX, 94);
        tabMain.Size = new Size(FormWidth - 64, 520);
        tabMain.SelectedIndexChanged += (s, e) => RefreshActiveTab();

        tabOrders.Text = Messages.StockOrders;
        tabOrders.BackColor = Palette.White;

        tabSpending.Text = Messages.StockSpending;
        tabSpending.BackColor = Palette.White;

        tabMain.TabPages.Add(tabOrders);
        tabMain.TabPages.Add(tabSpending);
        Controls.Add(tabMain);

        BuildOrdersTab();
        BuildSpendingTab();
    }

    private void BuildOrdersTab()
    {
        var gridWidth = tabMain.Width - 32;
        StyleGrid(dgvOrders);
        dgvOrders.Location = new Point(12, 12);
        dgvOrders.Size = new Size(gridWidth, 255);
        dgvOrders.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

        var colId = new DataGridViewTextBoxColumn
        {
            HeaderText = "ID",
            FillWeight = 50,
            MinimumWidth = 50,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        colId.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        colId.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dgvOrders.Columns.Add(colId);

        var colWholesaler = new DataGridViewTextBoxColumn
        {
            HeaderText = Messages.Wholesaler,
            FillWeight = 190,
            MinimumWidth = 150,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        dgvOrders.Columns.Add(colWholesaler);

        var colDate = new DataGridViewTextBoxColumn
        {
            HeaderText = Messages.OrderDate,
            FillWeight = 95,
            MinimumWidth = 90,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        colDate.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        colDate.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dgvOrders.Columns.Add(colDate);

        var colTotal = new DataGridViewTextBoxColumn
        {
            HeaderText = Messages.TotalAmount,
            FillWeight = 110,
            MinimumWidth = 100,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        colTotal.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        colTotal.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
        dgvOrders.Columns.Add(colTotal);

        var colUser = new DataGridViewTextBoxColumn
        {
            HeaderText = Messages.RecordedBy,
            FillWeight = 110,
            MinimumWidth = 100,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        dgvOrders.Columns.Add(colUser);

        dgvOrders.SelectionChanged += OrdersSelectionChanged;
        tabOrders.Controls.Add(dgvOrders);

        lblItemsTitle.Text = "Order Items:";
        lblItemsTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblItemsTitle.ForeColor = Palette.Ink;
        lblItemsTitle.Location = new Point(12, 278);
        lblItemsTitle.AutoSize = true;
        tabOrders.Controls.Add(lblItemsTitle);

        StyleGrid(dgvItems);
        dgvItems.Location = new Point(12, 304);
        dgvItems.Size = new Size(gridWidth, 172);
        dgvItems.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

        var colItemName = new DataGridViewTextBoxColumn
        {
            HeaderText = Messages.ProductName,
            FillWeight = 220,
            MinimumWidth = 180,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        dgvItems.Columns.Add(colItemName);

        var colItemQty = new DataGridViewTextBoxColumn
        {
            HeaderText = Messages.Quantity,
            FillWeight = 70,
            MinimumWidth = 65,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        colItemQty.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        colItemQty.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dgvItems.Columns.Add(colItemQty);

        var colItemCost = new DataGridViewTextBoxColumn
        {
            HeaderText = Messages.UnitCost,
            FillWeight = 95,
            MinimumWidth = 90,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        colItemCost.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        colItemCost.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
        dgvItems.Columns.Add(colItemCost);

        var colItemTotal = new DataGridViewTextBoxColumn
        {
            HeaderText = Messages.TotalAmount,
            FillWeight = 110,
            MinimumWidth = 100,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        colItemTotal.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        colItemTotal.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
        dgvItems.Columns.Add(colItemTotal);

        tabOrders.Controls.Add(dgvItems);
    }

    private void BuildSpendingTab()
    {
        var gridWidth = tabMain.Width - 32;
        StyleGrid(dgvSpending);
        dgvSpending.Location = new Point(12, 12);
        dgvSpending.Size = new Size(gridWidth, 420);
        dgvSpending.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

        var colWholesaler = new DataGridViewTextBoxColumn
        {
            HeaderText = Messages.Wholesaler,
            FillWeight = 220,
            MinimumWidth = 180,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        dgvSpending.Columns.Add(colWholesaler);

        var colCount = new DataGridViewTextBoxColumn
        {
            HeaderText = "Orders Count",
            FillWeight = 80,
            MinimumWidth = 80,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        colCount.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        colCount.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dgvSpending.Columns.Add(colCount);

        var colTotal = new DataGridViewTextBoxColumn
        {
            HeaderText = Messages.TotalSpending,
            FillWeight = 120,
            MinimumWidth = 110,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        colTotal.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        colTotal.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
        dgvSpending.Columns.Add(colTotal);

        tabSpending.Controls.Add(dgvSpending);

        lblSpendingGrandTotal.Text = $"{Messages.TotalSpending}: Rs. 0.00";
        lblSpendingGrandTotal.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        lblSpendingGrandTotal.ForeColor = Palette.Ink;
        lblSpendingGrandTotal.Location = new Point(12, 442);
        lblSpendingGrandTotal.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        lblSpendingGrandTotal.AutoSize = true;
        tabSpending.Controls.Add(lblSpendingGrandTotal);
    }

    private static void StyleGrid(DataGridView grid)
    {
        GridTheme.Apply(grid);
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
    }

    private void LoadWholesalers()
    {
        _allWholesalers.Clear();
        _wholesalers.ListWholesalers(_allWholesalers);
        cmbWholesaler.Items.Clear();
        cmbWholesaler.Items.Add(Messages.All);
        foreach (var w in _allWholesalers)
        {
            cmbWholesaler.Items.Add(w.Name);
        }
        cmbWholesaler.SelectedIndex = 0;
    }

    private void RefreshActiveTab()
    {
        if (tabMain.SelectedTab == tabOrders)
        {
            LoadOrders();
        }
        else
        {
            LoadSpending();
        }
    }

    private void LoadOrders()
    {
        dgvOrders.Rows.Clear();
        dgvItems.Rows.Clear();
        int? wholesalerId = SelectedWholesalerId();
        var list = new List<StockOrder>();
        var result = _stock.ListStockOrders(list, dtpFrom.Value.Date, dtpTo.Value.Date, wholesalerId);
        if (!result.Success)
        {
            MessageBox.Show(this, result.Message, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        foreach (var item in list)
        {
            dgvOrders.Rows.Add(item.StockOrderId, item.WholesalerName, item.OrderDate.ToString("yyyy-MM-dd"), item.TotalAmount.ToString("F2"), item.RecordedByUserName);
        }
    }

    private void LoadSpending()
    {
        dgvSpending.Rows.Clear();
        int? wholesalerId = SelectedWholesalerId();
        var list = new List<WholesalerSpendingSummary>();
        var result = _stock.GetSpendingSummary(list, dtpFrom.Value.Date, dtpTo.Value.Date, wholesalerId);
        if (!result.Success)
        {
            MessageBox.Show(this, result.Message, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        decimal grandTotal = 0;
        foreach (var item in list)
        {
            grandTotal += item.TotalSpending;
            dgvSpending.Rows.Add(item.WholesalerName, item.OrderCount, item.TotalSpending.ToString("F2"));
        }
        lblSpendingGrandTotal.Text = $"{Messages.TotalSpending}: Rs. {grandTotal:F2}";
    }

    private void OrdersSelectionChanged(object? sender, EventArgs e)
    {
        dgvItems.Rows.Clear();
        if (dgvOrders.SelectedRows.Count == 0)
        {
            return;
        }

        var orderIdValue = dgvOrders.SelectedRows[0].Cells[0].Value;
        if (orderIdValue is not int orderId)
        {
            return;
        }

        var items = new List<StockOrderItem>();
        var result = _stock.ListOrderItems(orderId, items);
        if (!result.Success)
        {
            return;
        }

        foreach (var item in items)
        {
            var lineTotal = item.Quantity * item.UnitCost;
            dgvItems.Rows.Add(item.ProductName, item.Quantity, item.UnitCost.ToString("F2"), lineTotal.ToString("F2"));
        }
    }

    private int? SelectedWholesalerId()
    {
        if (cmbWholesaler.SelectedIndex <= 0)
        {
            return null;
        }

        var index = cmbWholesaler.SelectedIndex - 1;
        return index < _allWholesalers.Count ? _allWholesalers[index].WholesalerId : null;
    }

    private void NewOrderClicked(object? sender, EventArgs e)
    {
        using var form = new StockOrderEntryForm(_stock, _wholesalers, _products);
        if (form.ShowDialog(this) == DialogResult.OK)
        {
            RefreshActiveTab();
        }
    }
}
