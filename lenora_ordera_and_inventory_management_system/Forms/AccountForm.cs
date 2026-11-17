using lenora_ordera_and_inventory_management_system.Common;
using lenora_ordera_and_inventory_management_system.Models;
using lenora_ordera_and_inventory_management_system.Services;

namespace lenora_ordera_and_inventory_management_system.Forms;

internal sealed class AccountForm : Form
{
    private const int FormWidth = 440;
    private const int FormHeight = 560;
    private const int FieldX = 32;
    private const int FieldWidth = 376;
    private const int LabelGap = 22;

    private readonly UserService _users;
    private readonly Panel pnlAccent = new();
    private readonly Label lblTitle = new();
    private readonly Label lblFullName = new();
    private readonly TextBox txtFullName = new();
    private readonly Label lblUsername = new();
    private readonly TextBox txtUsername = new();
    private readonly Label lblPassword = new();
    private readonly TextBox txtPassword = new();
    private readonly Button btnTogglePassword = new();
    private readonly Label lblConfirm = new();
    private readonly TextBox txtConfirm = new();
    private readonly Button btnToggleConfirm = new();
    private readonly Label lblRole = new();
    private readonly ComboBox cmbRole = new();
    private readonly Button btnSave = new();
    private readonly Button btnCancel = new();

    public AccountForm(UserService users)
    {
        _users = users;
        BuildChrome();
        AddAccent();
        AddTitle();
        AddIdentityFields();
        AddPasswordFields();
        AddRole();
        AddActions();
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
        AcceptButton = btnSave;
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
        lblTitle.Text = Messages.NewAccount;
        lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
        lblTitle.ForeColor = Palette.Ink;
        lblTitle.Location = new Point(FieldX, 24);
        lblTitle.AutoSize = true;
        Controls.Add(lblTitle);
    }

    private void AddIdentityFields()
    {
        Place(lblFullName, txtFullName, Messages.FullName, 72, false);
        Place(lblUsername, txtUsername, Messages.Username, 140, false);
        txtFullName.MaxLength = Rules.FullNameMaxLength;
        txtUsername.MaxLength = Rules.UsernameMaxLength;
        txtFullName.TabIndex = 0;
        txtUsername.TabIndex = 1;
        Controls.Add(lblFullName);
        Controls.Add(txtFullName);
        Controls.Add(lblUsername);
        Controls.Add(txtUsername);
    }

    private void AddPasswordFields()
    {
        PlaceSecret(lblPassword, txtPassword, btnTogglePassword, Messages.Password, 208);
        PlaceSecret(lblConfirm, txtConfirm, btnToggleConfirm, Messages.ConfirmPassword, 276);
        txtPassword.TabIndex = 2;
        txtConfirm.TabIndex = 3;
        Controls.Add(lblPassword);
        Controls.Add(txtPassword);
        Controls.Add(btnTogglePassword);
        Controls.Add(lblConfirm);
        Controls.Add(txtConfirm);
        Controls.Add(btnToggleConfirm);
    }

    private static void PlaceSecret(Label label, TextBox box, Button toggle, string caption, int top)
    {
        label.Text = caption;
        label.AutoSize = true;
        label.ForeColor = Palette.Ink;
        label.Location = new Point(FieldX, top);

        box.Location = new Point(FieldX, top + LabelGap);
        box.Size = new Size(FieldWidth - 42, 30);
        box.BorderStyle = BorderStyle.FixedSingle;
        box.UseSystemPasswordChar = true;

        toggle.Text = "👁";
        toggle.Location = new Point(FieldX + FieldWidth - 38, top + LabelGap);
        toggle.Size = new Size(38, 28);
        toggle.FlatStyle = FlatStyle.Flat;
        toggle.FlatAppearance.BorderSize = 0;
        toggle.BackColor = Palette.PaperLight;
        toggle.ForeColor = Palette.Ink;
        toggle.Cursor = Cursors.Hand;
        toggle.Click += (_, _) =>
        {
            box.UseSystemPasswordChar = !box.UseSystemPasswordChar;
            toggle.ForeColor = box.UseSystemPasswordChar ? Palette.Ink : Palette.Accent;
        };
    }

    private void AddRole()
    {
        lblRole.Text = Messages.Role;
        lblRole.AutoSize = true;
        lblRole.ForeColor = Palette.Ink;
        lblRole.Location = new Point(FieldX, 344);
        cmbRole.Location = new Point(FieldX, 366);
        cmbRole.Size = new Size(FieldWidth, 28);
        cmbRole.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbRole.FlatStyle = FlatStyle.Flat;
        cmbRole.TabIndex = 4;
        cmbRole.Items.Add(UserRole.Owner);
        cmbRole.Items.Add(UserRole.Assistant);
        Controls.Add(lblRole);
        Controls.Add(cmbRole);
    }

    private static void Place(Label label, TextBox box, string caption, int top, bool secret)
    {
        label.Text = caption;
        label.AutoSize = true;
        label.ForeColor = Palette.Ink;
        label.Location = new Point(FieldX, top);
        box.Location = new Point(FieldX, top + LabelGap);
        box.Size = new Size(FieldWidth, 30);
        box.BorderStyle = BorderStyle.FixedSingle;
        box.UseSystemPasswordChar = secret;
    }

    private void AddActions()
    {
        StyleSave();
        StyleCancel();
        Controls.Add(btnSave);
        Controls.Add(btnCancel);
    }

    private void StyleSave()
    {
        btnSave.Text = Messages.Save;
        btnSave.Location = new Point(FieldX, 430);
        btnSave.Size = new Size(140, 38);
        btnSave.FlatStyle = FlatStyle.Flat;
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.BackColor = Palette.Accent;
        btnSave.ForeColor = Palette.White;
        btnSave.Cursor = Cursors.Hand;
        btnSave.TabIndex = 5;
        btnSave.Click += SaveClicked;
    }

    private void StyleCancel()
    {
        btnCancel.Text = Messages.Cancel;
        btnCancel.Location = new Point(188, 430);
        btnCancel.Size = new Size(120, 38);
        btnCancel.FlatStyle = FlatStyle.Flat;
        btnCancel.FlatAppearance.BorderColor = Palette.Ink;
        btnCancel.FlatAppearance.BorderSize = 1;
        btnCancel.BackColor = Palette.White;
        btnCancel.ForeColor = Palette.Ink;
        btnCancel.TabIndex = 6;
        btnCancel.DialogResult = DialogResult.Cancel;
    }

    private void SaveClicked(object? sender, EventArgs e)
    {
        var result = _users.CreateAccount(ReadAccount());
        if (!result.Success)
        {
            MessageBox.Show(this, result.Message, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        MessageBox.Show(this, result.Message, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Information);
        Close();
    }

    private AccountRequest ReadAccount()
    {
        return new AccountRequest
        {
            FullName = txtFullName.Text,
            Username = txtUsername.Text,
            Password = txtPassword.Text,
            ConfirmPassword = txtConfirm.Text,
            Role = cmbRole.SelectedItem is UserRole role ? role : null
        };
    }
}
