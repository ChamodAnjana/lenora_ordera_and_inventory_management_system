using lenora_ordera_and_inventory_management_system.Common;
using lenora_ordera_and_inventory_management_system.Models;
using lenora_ordera_and_inventory_management_system.Services;

namespace lenora_ordera_and_inventory_management_system.Forms;

internal sealed class EditAccountForm : Form
{
    private const int FormWidth = 440;
    private const int FormHeight = 280;
    private const int FieldX = 32;
    private const int FieldWidth = 376;
    private const int LabelGap = 22;

    private readonly UserService _users;
    private readonly User _account;

    private readonly Panel pnlAccent = new();
    private readonly Label lblTitle = new();
    private readonly Label lblFullName = new();
    private readonly TextBox txtFullName = new();
    private readonly Label lblUsername = new();
    private readonly TextBox txtUsername = new();
    private readonly Button btnSave = new();
    private readonly Button btnCancel = new();

    public EditAccountForm(UserService users, User account)
    {
        _users = users;
        _account = account;
        BuildChrome();
        AddAccent();
        AddTitle();
        AddFields();
        AddActions();
        LoadAccount();
    }

    private void BuildChrome()
    {
        Text = "Edit Account";
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
        lblTitle.Text = "Edit Account";
        lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
        lblTitle.ForeColor = Palette.Ink;
        lblTitle.Location = new Point(FieldX, 20);
        lblTitle.AutoSize = true;
        Controls.Add(lblTitle);
    }

    private void AddFields()
    {
        Place(lblFullName, txtFullName, Messages.FullName, 68);
        Place(lblUsername, txtUsername, Messages.Username, 136);

        txtFullName.MaxLength = Rules.FullNameMaxLength;
        txtUsername.MaxLength = Rules.UsernameMaxLength;

        txtFullName.TabIndex = 0;
        txtUsername.TabIndex = 1;

        Controls.Add(lblFullName);
        Controls.Add(txtFullName);
        Controls.Add(lblUsername);
        Controls.Add(txtUsername);
    }

    private static void Place(Label label, TextBox box, string caption, int top)
    {
        label.Text = caption;
        label.AutoSize = true;
        label.ForeColor = Palette.Ink;
        label.Location = new Point(FieldX, top);
        box.Location = new Point(FieldX, top + LabelGap);
        box.Size = new Size(FieldWidth, 28);
        box.BorderStyle = BorderStyle.FixedSingle;
    }

    private void AddActions()
    {
        btnSave.Text = Messages.Save;
        btnSave.Location = new Point(FieldX, 214);
        btnSave.Size = new Size(130, 36);
        btnSave.FlatStyle = FlatStyle.Flat;
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.BackColor = Palette.Accent;
        btnSave.ForeColor = Palette.White;
        btnSave.Cursor = Cursors.Hand;
        btnSave.TabIndex = 2;
        btnSave.Click += SaveClicked;

        btnCancel.Text = Messages.Cancel;
        btnCancel.Location = new Point(FieldX + 142, 214);
        btnCancel.Size = new Size(110, 36);
        btnCancel.FlatStyle = FlatStyle.Flat;
        btnCancel.FlatAppearance.BorderColor = Palette.Ink;
        btnCancel.FlatAppearance.BorderSize = 1;
        btnCancel.BackColor = Palette.White;
        btnCancel.ForeColor = Palette.Ink;
        btnCancel.Cursor = Cursors.Hand;
        btnCancel.TabIndex = 3;
        btnCancel.Click += (_, _) => Close();

        Controls.Add(btnSave);
        Controls.Add(btnCancel);
    }

    private void LoadAccount()
    {
        txtFullName.Text = _account.FullName;
        txtUsername.Text = _account.Username;
    }

    private void SaveClicked(object? sender, EventArgs e)
    {
        var result = _users.UpdateAccount(_account.UserId, txtFullName.Text.Trim(), txtUsername.Text.Trim());
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
