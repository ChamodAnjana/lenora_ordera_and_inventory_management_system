using lenora_ordera_and_inventory_management_system.Common;
using lenora_ordera_and_inventory_management_system.Models;
using lenora_ordera_and_inventory_management_system.Services;

namespace lenora_ordera_and_inventory_management_system.Forms;

internal sealed class AccountsForm : Form
{
    private const int FormWidth = 760;
    private const int FormHeight = 580;
    private const int FieldX = 32;
    private const int ButtonTop = 500;
    private const int ButtonWidth = 100;
    private const int ButtonGap = 12;

    private readonly UserService _users;
    private readonly Panel pnlAccent = new();
    private readonly Label lblTitle = new();
    private readonly TextBox txtFilterFullName = new();
    private readonly TextBox txtFilterUsername = new();
    private readonly DataGridView dgvAccounts = new();
    private readonly Button btnNew = new();
    private readonly Button btnEdit = new();
    private readonly Button btnEnable = new();
    private readonly Button btnDisable = new();
    private readonly Button btnReset = new();
    private List<User> _allAccounts = new();

    public AccountsForm(UserService users)
    {
        _users = users;
        BuildChrome();
        AddAccent();
        AddTitle();
        AddGrid();
        AddActions();
        LoadAccounts();
    }

    private void BuildChrome()
    {
        Text = Messages.Accounts;
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
        lblTitle.Text = Messages.Accounts;
        lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
        lblTitle.ForeColor = Palette.Ink;
        lblTitle.Location = new Point(FieldX, 24);
        lblTitle.AutoSize = true;
        Controls.Add(lblTitle);

        var lblFilterFullName = new Label
        {
            Text = "Filter by Name:",
            Location = new Point(FieldX, 56),
            AutoSize = true
        };
        Controls.Add(lblFilterFullName);

        txtFilterFullName.Location = new Point(FieldX + 120, 53);
        txtFilterFullName.Size = new Size(150, 24);
        txtFilterFullName.TextChanged += (s, e) => ApplyFilters();
        Controls.Add(txtFilterFullName);

        var lblFilterUsername = new Label
        {
            Text = "Filter by Username:",
            Location = new Point(FieldX + 290, 56),
            AutoSize = true
        };
        Controls.Add(lblFilterUsername);

        txtFilterUsername.Location = new Point(FieldX + 430, 53);
        txtFilterUsername.Size = new Size(150, 24);
        txtFilterUsername.TextChanged += (s, e) => ApplyFilters();
        Controls.Add(txtFilterUsername);
    }

    private void AddGrid()
    {
        dgvAccounts.Location = new Point(FieldX, 90);
        dgvAccounts.Size = new Size(FormWidth - 64, 390);
        GridTheme.Apply(dgvAccounts);
        dgvAccounts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        AddColumns();
        Controls.Add(dgvAccounts);
    }

    private void AddColumns()
    {
        var colName = new DataGridViewTextBoxColumn
        {
            HeaderText = Messages.FullName,
            FillWeight = 170,
            MinimumWidth = 140,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        dgvAccounts.Columns.Add(colName);

        var colUser = new DataGridViewTextBoxColumn
        {
            HeaderText = Messages.Username,
            FillWeight = 120,
            MinimumWidth = 100,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        dgvAccounts.Columns.Add(colUser);

        var colRole = new DataGridViewTextBoxColumn
        {
            HeaderText = Messages.Role,
            FillWeight = 80,
            MinimumWidth = 75,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        colRole.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        colRole.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dgvAccounts.Columns.Add(colRole);

        var colStatus = new DataGridViewTextBoxColumn
        {
            HeaderText = Messages.Status,
            FillWeight = 80,
            MinimumWidth = 75,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        colStatus.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        colStatus.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dgvAccounts.Columns.Add(colStatus);
    }

    private void AddActions()
    {
        PlacePrimary(btnNew, Messages.NewAccount, FieldX);
        PlacePlain(btnEdit, "Edit", FieldX + ButtonWidth + ButtonGap);
        PlacePlain(btnEnable, Messages.Enable, FieldX + (ButtonWidth + ButtonGap) * 2);
        PlacePlain(btnDisable, Messages.Disable, FieldX + (ButtonWidth + ButtonGap) * 3);
        PlacePlain(btnReset, Messages.ResetPassword, FieldX + (ButtonWidth + ButtonGap) * 4);
        btnNew.Click += NewAccountClicked;
        btnEdit.Click += EditClicked;
        btnEnable.Click += EnableClicked;
        btnDisable.Click += DisableClicked;
        btnReset.Click += ResetClicked;
        Controls.Add(btnNew);
        Controls.Add(btnEdit);
        Controls.Add(btnEnable);
        Controls.Add(btnDisable);
        Controls.Add(btnReset);
    }

    private static void PlacePrimary(Button button, string text, int left)
    {
        PlaceButton(button, text, left);
        button.FlatAppearance.BorderSize = 0;
        button.BackColor = Palette.Accent;
        button.ForeColor = Palette.White;
    }

    private static void PlacePlain(Button button, string text, int left)
    {
        PlaceButton(button, text, left);
        button.FlatAppearance.BorderColor = Palette.Ink;
        button.FlatAppearance.BorderSize = 1;
        button.BackColor = Palette.White;
        button.ForeColor = Palette.Ink;
    }

    private static void PlaceButton(Button button, string text, int left)
    {
        button.Text = text;
        button.Location = new Point(left, ButtonTop);
        button.Size = new Size(ButtonWidth, 38);
        button.FlatStyle = FlatStyle.Flat;
        button.Cursor = Cursors.Hand;
    }

    private void LoadAccounts()
    {
        var accounts = new List<User>();
        var result = _users.ListAccounts(accounts);
        if (!result.Success)
        {
            ShowWarning(result.Message);
            return;
        }

        _allAccounts = accounts;
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        var filterName = txtFilterFullName.Text.Trim().ToLower();
        var filterUsername = txtFilterUsername.Text.Trim().ToLower();

        var filtered = _allAccounts
            .Where(a => a.FullName.ToLower().Contains(filterName) && 
                        a.Username.ToLower().Contains(filterUsername))
            .ToList();

        FillGrid(filtered);
    }

    private void FillGrid(List<User> accounts)
    {
        dgvAccounts.Rows.Clear();
        foreach (var account in accounts)
        {
            var index = dgvAccounts.Rows.Add(AccountCells(account));
            dgvAccounts.Rows[index].Tag = account;
        }
    }

    private static object[] AccountCells(User account)
    {
        var status = account.IsActive ? Messages.Active : Messages.Disabled;
        return [account.FullName, account.Username, account.Role.ToString(), status];
    }

    private void NewAccountClicked(object? sender, EventArgs e)
    {
        using var account = new AccountForm(_users);
        account.ShowDialog(this);
        LoadAccounts();
    }

    private void EditClicked(object? sender, EventArgs e)
    {
        var account = SelectedAccount();
        if (account is null)
        {
            ShowWarning(Messages.SelectAccount);
            return;
        }

        using var edit = new EditAccountForm(_users, account);
        if (edit.ShowDialog(this) == DialogResult.OK)
        {
            LoadAccounts();
        }
    }

    private void EnableClicked(object? sender, EventArgs e)
    {
        ChangeActive(true);
    }

    private void DisableClicked(object? sender, EventArgs e)
    {
        ChangeActive(false);
    }

    private void ChangeActive(bool active)
    {
        var account = SelectedAccount();
        if (account is null)
        {
            ShowWarning(Messages.SelectAccount);
            return;
        }

        ShowOutcome(_users.SetActive(account.UserId, active));
    }

    private void ResetClicked(object? sender, EventArgs e)
    {
        var account = SelectedAccount();
        if (account is null)
        {
            ShowWarning(Messages.SelectAccount);
            return;
        }

        using var reset = new ResetPasswordForm(_users, account.UserId);
        if (reset.ShowDialog(this) == DialogResult.OK)
        {
            LoadAccounts();
        }
    }

    private User? SelectedAccount()
    {
        return dgvAccounts.CurrentRow?.Tag as User;
    }

    private void ShowOutcome(Result result)
    {
        var icon = result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning;
        MessageBox.Show(this, result.Message, Messages.AppName, MessageBoxButtons.OK, icon);
        if (result.Success)
        {
            LoadAccounts();
        }
    }

    private void ShowWarning(string message)
    {
        MessageBox.Show(this, message, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
}
