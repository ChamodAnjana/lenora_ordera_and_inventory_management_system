using lenora_ordera_and_inventory_management_system.Common;
using lenora_ordera_and_inventory_management_system.Models;
using lenora_ordera_and_inventory_management_system.Services;

namespace lenora_ordera_and_inventory_management_system.Forms;

internal sealed class StockOrderEntryForm : Form
{
    private const int FormWidth = 720;
    private const int FormHeight = 640;
    private const int FieldX = 32;

    private readonly StockService _stock;
    private readonly WholesalerService _wholesalers;
    private readonly ProductService _products;
    private readonly List<StockOrderItemRequest> _items = new();
    private readonly List<Wholesaler> _allWholesalers = new();
    private readonly List<Product> _allProducts = new();

    private readonly Panel pnlAccent = new();
    private readonly Label lblTitle = new();
    private readonly Label lblWholesaler = new();
    private readonly ComboBox cmbWholesaler = new();
    private readonly Label lblOrderDate = new();
    private readonly DateTimePicker dtpOrderDate = new();
    private readonly GroupBox grpItems = new();
    private readonly Label lblFindProduct = new();
    private readonly TextBox txtFindProduct = new();
    private readonly Label lblProduct = new();
    private readonly ComboBox cmbProduct = new();
    private readonly Label lblQuantity = new();
    private readonly TextBox txtQuantity = new();
    private readonly Label lblUnitCost = new();
    private readonly TextBox txtUnitCost = new();
    private readonly Button btnAddItem = new();
    private readonly Button btnRemoveItem = new();
    private readonly DataGridView dgvItems = new();
    private readonly Label lblTotal = new();
    private readonly Button btnSave = new();
    private readonly Button btnCancel = new();

    public StockOrderEntryForm(StockService stock, WholesalerService wholesalers, ProductService products)
    {
        _stock = stock;
        _wholesalers = wholesalers;
        _products = products;
        BuildChrome();
        AddAccent();
        AddTitle();
        AddHeaderFields();
        AddItemsSection();
        AddFooter();
        LoadLookups();
    }

    private void BuildChrome()
    {
        Text = Messages.NewStockOrder;
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
        lblTitle.Text = Messages.NewStockOrder;
        lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
        lblTitle.ForeColor = Palette.Ink;
        lblTitle.Location = new Point(FieldX, 16);
        lblTitle.AutoSize = true;
        Controls.Add(lblTitle);
    }

    private void AddHeaderFields()
    {
        lblWholesaler.Text = Messages.Wholesaler + ":";
        lblWholesaler.Location = new Point(FieldX, 58);
        lblWholesaler.AutoSize = true;
        Controls.Add(lblWholesaler);

        cmbWholesaler.Location = new Point(FieldX + 90, 55);
        cmbWholesaler.Size = new Size(240, 26);
        cmbWholesaler.DropDownStyle = ComboBoxStyle.DropDownList;
        Controls.Add(cmbWholesaler);

        lblOrderDate.Text = Messages.OrderDate + ":";
        lblOrderDate.Location = new Point(FieldX + 360, 58);
        lblOrderDate.AutoSize = true;
        Controls.Add(lblOrderDate);

        dtpOrderDate.Location = new Point(FieldX + 450, 55);
        dtpOrderDate.Size = new Size(180, 26);
        dtpOrderDate.Format = DateTimePickerFormat.Short;
        Controls.Add(dtpOrderDate);
    }

    private void AddItemsSection()
    {
        grpItems.Text = "Order Items";
        grpItems.Location = new Point(FieldX, 94);
        grpItems.Size = new Size(FormWidth - 64, 426);
        grpItems.ForeColor = Palette.Ink;

        lblFindProduct.Text = Messages.FindProduct + ":";
        lblFindProduct.Location = new Point(16, 28);
        lblFindProduct.AutoSize = true;
        grpItems.Controls.Add(lblFindProduct);

        txtFindProduct.Location = new Point(110, 25);
        txtFindProduct.Size = new Size(220, 26);
        txtFindProduct.TextChanged += FindProductChanged;
        grpItems.Controls.Add(txtFindProduct);

        lblProduct.Text = Messages.Product + ":";
        lblProduct.Location = new Point(16, 64);
        lblProduct.AutoSize = true;
        grpItems.Controls.Add(lblProduct);

        cmbProduct.Location = new Point(78, 61);
        cmbProduct.Size = new Size(180, 26);
        cmbProduct.DropDownStyle = ComboBoxStyle.DropDown;
        cmbProduct.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
        cmbProduct.AutoCompleteSource = AutoCompleteSource.ListItems;
        cmbProduct.SelectedIndexChanged += ProductSelectionChanged;
        grpItems.Controls.Add(cmbProduct);

        lblQuantity.Text = Messages.Quantity + ":";
        lblQuantity.Location = new Point(270, 64);
        lblQuantity.AutoSize = true;
        grpItems.Controls.Add(lblQuantity);

        txtQuantity.Location = new Point(336, 61);
        txtQuantity.Size = new Size(60, 26);
        grpItems.Controls.Add(txtQuantity);

        lblUnitCost.Text = Messages.UnitCost + ":";
        lblUnitCost.Location = new Point(410, 64);
        lblUnitCost.AutoSize = true;
        grpItems.Controls.Add(lblUnitCost);

        txtUnitCost.Location = new Point(476, 61);
        txtUnitCost.Size = new Size(80, 26);
        grpItems.Controls.Add(txtUnitCost);

        btnAddItem.Text = Messages.AddItem;
        btnAddItem.Location = new Point(570, 59);
        btnAddItem.Size = new Size(76, 30);
        btnAddItem.FlatStyle = FlatStyle.Flat;
        btnAddItem.FlatAppearance.BorderSize = 0;
        btnAddItem.BackColor = Palette.Accent;
        btnAddItem.ForeColor = Palette.White;
        btnAddItem.Click += AddItemClicked;
        grpItems.Controls.Add(btnAddItem);

        BuildItemsGrid();
        grpItems.Controls.Add(dgvItems);

        btnRemoveItem.Text = Messages.RemoveItem;
        btnRemoveItem.Location = new Point(16, 384);
        btnRemoveItem.Size = new Size(110, 30);
        btnRemoveItem.FlatStyle = FlatStyle.Flat;
        btnRemoveItem.FlatAppearance.BorderColor = Palette.Ink;
        btnRemoveItem.FlatAppearance.BorderSize = 1;
        btnRemoveItem.BackColor = Palette.White;
        btnRemoveItem.ForeColor = Palette.Ink;
        btnRemoveItem.Click += RemoveItemClicked;
        grpItems.Controls.Add(btnRemoveItem);

        Controls.Add(grpItems);
    }

    private void BuildItemsGrid()
    {
        dgvItems.Location = new Point(16, 100);
        dgvItems.Size = new Size(FormWidth - 96, 274);
        GridTheme.Apply(dgvItems);
        dgvItems.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        dgvItems.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "ProductID", Visible = false });

        var colProduct = new DataGridViewTextBoxColumn
        {
            HeaderText = Messages.ProductName,
            FillWeight = 220,
            MinimumWidth = 180,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        dgvItems.Columns.Add(colProduct);

        var colQty = new DataGridViewTextBoxColumn
        {
            HeaderText = Messages.Quantity,
            FillWeight = 70,
            MinimumWidth = 65,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        colQty.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        colQty.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dgvItems.Columns.Add(colQty);

        var colCost = new DataGridViewTextBoxColumn
        {
            HeaderText = Messages.UnitCost,
            FillWeight = 95,
            MinimumWidth = 90,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        colCost.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        colCost.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
        dgvItems.Columns.Add(colCost);

        var colTotal = new DataGridViewTextBoxColumn
        {
            HeaderText = Messages.TotalAmount,
            FillWeight = 110,
            MinimumWidth = 100,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        colTotal.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        colTotal.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
        dgvItems.Columns.Add(colTotal);
    }

    private void AddFooter()
    {
        lblTotal.Text = $"{Messages.TotalAmount}: Rs. 0.00";
        lblTotal.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        lblTotal.ForeColor = Palette.Ink;
        lblTotal.Location = new Point(FieldX, 534);
        lblTotal.AutoSize = true;
        Controls.Add(lblTotal);

        btnSave.Text = Messages.Save;
        btnSave.Location = new Point(FieldX, 572);
        btnSave.Size = new Size(130, 36);
        btnSave.FlatStyle = FlatStyle.Flat;
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.BackColor = Palette.Accent;
        btnSave.ForeColor = Palette.White;
        btnSave.Cursor = Cursors.Hand;
        btnSave.Click += SaveClicked;

        btnCancel.Text = Messages.Cancel;
        btnCancel.Location = new Point(176, 572);
        btnCancel.Size = new Size(110, 36);
        btnCancel.FlatStyle = FlatStyle.Flat;
        btnCancel.FlatAppearance.BorderColor = Palette.Ink;
        btnCancel.FlatAppearance.BorderSize = 1;
        btnCancel.BackColor = Palette.White;
        btnCancel.ForeColor = Palette.Ink;
        btnCancel.DialogResult = DialogResult.Cancel;

        Controls.Add(btnSave);
        Controls.Add(btnCancel);
    }

    private void LoadLookups()
    {
        _allWholesalers.Clear();
        _wholesalers.ListWholesalers(_allWholesalers);
        cmbWholesaler.Items.Clear();
        foreach (var w in _allWholesalers)
        {
            cmbWholesaler.Items.Add(w.Name);
        }

        _allProducts.Clear();
        _products.ListProductsForOwner(_allProducts);
        ShowProducts(MatchingProducts(txtFindProduct.Text));
    }

    private void FindProductChanged(object? sender, EventArgs e)
    {
        ShowProducts(MatchingProducts(txtFindProduct.Text));
    }

    private IEnumerable<Product> MatchingProducts(string term)
    {
        var query = term.Trim();
        if (query.Length == 0)
        {
            return _allProducts;
        }

        return _allProducts.Where(product => product.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
    }

    private void ShowProducts(IEnumerable<Product> products)
    {
        cmbProduct.AutoCompleteMode = AutoCompleteMode.None;
        cmbProduct.BeginUpdate();
        cmbProduct.Items.Clear();
        foreach (var product in products)
        {
            cmbProduct.Items.Add(product.Name);
        }

        cmbProduct.EndUpdate();
        cmbProduct.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
        cmbProduct.SelectedIndex = cmbProduct.Items.Count > 0 ? 0 : -1;
        if (cmbProduct.SelectedIndex < 0)
        {
            txtUnitCost.Clear();
        }
    }

    private Product? SelectedStockProduct()
    {
        var name = cmbProduct.Text.Trim();
        if (name.Length == 0)
        {
            return null;
        }

        return _allProducts.FirstOrDefault(product => product.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    private void ProductSelectionChanged(object? sender, EventArgs e)
    {
        var product = SelectedStockProduct();
        txtUnitCost.Text = product is null ? string.Empty : product.CostPrice.ToString("F2");
    }

    private void AddItemClicked(object? sender, EventArgs e)
    {
        var product = SelectedStockProduct();
        if (product is null)
        {
            MessageBox.Show(this, Messages.SelectProduct, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (!int.TryParse(txtQuantity.Text.Trim(), out var quantity) || quantity <= 0)
        {
            MessageBox.Show(this, Messages.QuantityInvalid, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtQuantity.Focus();
            return;
        }

        if (!decimal.TryParse(txtUnitCost.Text.Trim(), out var unitCost) || unitCost < 0)
        {
            MessageBox.Show(this, Messages.UnitCostInvalid, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtUnitCost.Focus();
            return;
        }

        if (_items.Any(i => i.ProductId == product.ProductId))
        {
            MessageBox.Show(this, Messages.ProductAlreadyAdded, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _items.Add(new StockOrderItemRequest
        {
            ProductId = product.ProductId,
            ProductName = product.Name,
            Quantity = quantity,
            UnitCost = unitCost
        });

        RefreshItemsGrid();
        txtQuantity.Clear();
    }

    private void RemoveItemClicked(object? sender, EventArgs e)
    {
        if (dgvItems.SelectedRows.Count == 0)
        {
            return;
        }

        var index = dgvItems.SelectedRows[0].Index;
        if (index >= 0 && index < _items.Count)
        {
            _items.RemoveAt(index);
            RefreshItemsGrid();
        }
    }

    private void RefreshItemsGrid()
    {
        dgvItems.Rows.Clear();
        decimal total = 0;
        foreach (var item in _items)
        {
            var lineTotal = item.Quantity * item.UnitCost;
            total += lineTotal;
            dgvItems.Rows.Add(item.ProductId, item.ProductName, item.Quantity, item.UnitCost.ToString("F2"), lineTotal.ToString("F2"));
        }
        lblTotal.Text = $"{Messages.TotalAmount}: Rs. {total:F2}";
    }

    private void SaveClicked(object? sender, EventArgs e)
    {
        if (cmbWholesaler.SelectedIndex < 0 || cmbWholesaler.SelectedIndex >= _allWholesalers.Count)
        {
            MessageBox.Show(this, Messages.SelectWholesaler, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            cmbWholesaler.Focus();
            return;
        }

        if (_items.Count == 0)
        {
            MessageBox.Show(this, Messages.StockOrderRequiresItems, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var wholesaler = _allWholesalers[cmbWholesaler.SelectedIndex];
        var request = new StockOrderRequest
        {
            WholesalerId = wholesaler.WholesalerId,
            OrderDate = dtpOrderDate.Value.Date,
            Items = _items
        };

        var result = _stock.RecordStockOrder(request);
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
