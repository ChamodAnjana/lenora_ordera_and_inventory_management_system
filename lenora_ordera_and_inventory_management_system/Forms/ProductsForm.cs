using lenora_ordera_and_inventory_management_system.Common;
using lenora_ordera_and_inventory_management_system.Models;
using lenora_ordera_and_inventory_management_system.Security;
using lenora_ordera_and_inventory_management_system.Services;

namespace lenora_ordera_and_inventory_management_system.Forms;

internal sealed class ProductsForm : Form
{
    private const int FormWidth = 840;
    private const int FormHeight = 600;
    private const int FieldX = 32;
    private const int ButtonWidth = 100;
    private const int ButtonGap = 12;

    private readonly ProductService _products;
    private readonly bool _isOwner;
    private readonly Panel pnlAccent = new();
    private readonly Label lblTitle = new();
    private readonly Label lblSearch = new();
    private readonly TextBox txtSearch = new();
    private readonly Label lblCategory = new();
    private readonly ComboBox cmbCategory = new();
    private readonly DataGridView dgvProducts = new();
    private readonly Button btnNew = new();
    private readonly Button btnEdit = new();
    private readonly Button btnDelete = new();

    public ProductsForm(ProductService products)
    {
        _products = products;
        _isOwner = CurrentSession.Current?.Role == UserRole.Owner;
        BuildChrome();
        AddAccent();
        AddTitle();
        AddFilters();
        AddGrid();
        AddActions();
        LoadCategories();
        LoadProducts();
    }

    private void BuildChrome()
    {
        Text = Messages.Products;
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
        lblTitle.Text = Messages.Products;
        lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
        lblTitle.ForeColor = Palette.Ink;
        lblTitle.Location = new Point(FieldX, 20);
        lblTitle.AutoSize = true;
        Controls.Add(lblTitle);
    }

    private void AddFilters()
    {
        lblSearch.Text = Messages.Search + ":";
        lblSearch.Location = new Point(FieldX, 60);
        lblSearch.AutoSize = true;
        Controls.Add(lblSearch);

        txtSearch.Location = new Point(FieldX + 60, 57);
        txtSearch.Size = new Size(180, 26);
        txtSearch.TextChanged += (s, e) => LoadProducts();
        Controls.Add(txtSearch);

        lblCategory.Text = Messages.Category + ":";
        lblCategory.Location = new Point(FieldX + 270, 60);
        lblCategory.AutoSize = true;
        Controls.Add(lblCategory);

        cmbCategory.Location = new Point(FieldX + 350, 57);
        cmbCategory.Size = new Size(180, 26);
        cmbCategory.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbCategory.SelectedIndexChanged += (s, e) => LoadProducts();
        Controls.Add(cmbCategory);
    }

    private void AddGrid()
    {
        dgvProducts.Location = new Point(FieldX, 96);
        dgvProducts.Size = new Size(FormWidth - 64, 420);
        GridTheme.Apply(dgvProducts);
        dgvProducts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        AddColumns();
        Controls.Add(dgvProducts);
    }

    private void AddColumns()
    {
        dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "ID", Visible = false });

        var colName = new DataGridViewTextBoxColumn
        {
            HeaderText = Messages.ProductName,
            FillWeight = 220,
            MinimumWidth = 180,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        dgvProducts.Columns.Add(colName);

        var colCategory = new DataGridViewTextBoxColumn
        {
            HeaderText = Messages.Category,
            FillWeight = 110,
            MinimumWidth = 100,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        dgvProducts.Columns.Add(colCategory);

        if (_isOwner)
        {
            var colCost = new DataGridViewTextBoxColumn
            {
                HeaderText = Messages.CostPrice,
                FillWeight = 90,
                MinimumWidth = 85,
                SortMode = DataGridViewColumnSortMode.NotSortable
            };
            colCost.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            colCost.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvProducts.Columns.Add(colCost);
        }

        var colPrice = new DataGridViewTextBoxColumn
        {
            HeaderText = Messages.SellingPrice,
            FillWeight = 90,
            MinimumWidth = 85,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        colPrice.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        colPrice.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
        dgvProducts.Columns.Add(colPrice);

        var colQty = new DataGridViewTextBoxColumn
        {
            HeaderText = Messages.Quantity,
            FillWeight = 70,
            MinimumWidth = 65,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        colQty.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        colQty.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dgvProducts.Columns.Add(colQty);
    }

    private void AddActions()
    {
        if (!_isOwner)
        {
            return;
        }

        PlacePrimary(btnNew, Messages.NewProduct, FieldX);
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
        button.Location = new Point(left, 532);
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
        button.Location = new Point(left, 532);
        button.Size = new Size(ButtonWidth, 36);
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderColor = Palette.Ink;
        button.FlatAppearance.BorderSize = 1;
        button.BackColor = Palette.White;
        button.ForeColor = Palette.Ink;
        button.Cursor = Cursors.Hand;
    }

    private void LoadCategories()
    {
        var categories = new List<string>();
        _products.ListCategories(categories);
        cmbCategory.Items.Clear();
        cmbCategory.Items.Add(Messages.All);
        foreach (var category in categories)
        {
            cmbCategory.Items.Add(category);
        }
        cmbCategory.SelectedIndex = 0;
    }

    private void LoadProducts()
    {
        dgvProducts.Rows.Clear();
        var search = txtSearch.Text.Trim();
        var category = cmbCategory.SelectedItem?.ToString() == Messages.All ? null : cmbCategory.SelectedItem?.ToString();

        if (_isOwner)
        {
            var list = new List<Product>();
            var result = _products.ListProductsForOwner(list, search, category);
            if (!result.Success)
            {
                MessageBox.Show(this, result.Message, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            foreach (var item in list)
            {
                dgvProducts.Rows.Add(item.ProductId, item.Name, item.Category, item.CostPrice.ToString("F2"), item.SellingPrice.ToString("F2"), item.Quantity);
            }
        }
        else
        {
            var list = new List<ProductCatalogItem>();
            var result = _products.ListProductsForAssistant(list, search, category);
            if (!result.Success)
            {
                MessageBox.Show(this, result.Message, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            foreach (var item in list)
            {
                dgvProducts.Rows.Add(item.ProductId, item.Name, item.Category, item.SellingPrice.ToString("F2"), item.Quantity);
            }
        }
    }

    private void NewClicked(object? sender, EventArgs e)
    {
        using var form = new ProductEntryForm(_products);
        if (form.ShowDialog(this) == DialogResult.OK)
        {
            LoadCategories();
            LoadProducts();
        }
    }

    private void EditClicked(object? sender, EventArgs e)
    {
        if (SelectedProductId() is not int id)
        {
            MessageBox.Show(this, Messages.SelectProduct, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var result = _products.GetProduct(id, out var product);
        if (!result.Success || product is null)
        {
            MessageBox.Show(this, result.Message, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var form = new ProductEntryForm(_products, product);
        if (form.ShowDialog(this) == DialogResult.OK)
        {
            LoadCategories();
            LoadProducts();
        }
    }

    private void DeleteClicked(object? sender, EventArgs e)
    {
        if (SelectedProductId() is not int id)
        {
            MessageBox.Show(this, Messages.SelectProduct, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var confirmation = MessageBox.Show(
            this,
            "Are you sure you want to delete this product?",
            Messages.AppName,
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirmation != DialogResult.Yes)
        {
            return;
        }

        var result = _products.DeleteProduct(id);
        if (!result.Success)
        {
            MessageBox.Show(this, result.Message, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        MessageBox.Show(this, result.Message, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Information);
        LoadCategories();
        LoadProducts();
    }

    private int? SelectedProductId()
    {
        if (dgvProducts.SelectedRows.Count == 0)
        {
            return null;
        }

        var value = dgvProducts.SelectedRows[0].Cells[0].Value;
        return value is int id ? id : null;
    }
}
