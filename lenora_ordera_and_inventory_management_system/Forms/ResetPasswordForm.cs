using lenora_ordera_and_inventory_management_system.Common;
using lenora_ordera_and_inventory_management_system.Services;

namespace lenora_ordera_and_inventory_management_system.Forms;

internal sealed class ResetPasswordForm : Form
{
    private const int FormWidth = 440;
    private const int FormHeight = 280;
    private const int FieldX = 32;
    private const int FieldWidth = 376;
    private const int LabelGap = 22;

    private readonly UserService _users;
    private readonly int _userId;
    private readonly Panel pnlAccent = new();
    private readonly Label lblTitle = new();
    private readonly Label lblPassword = new();
    private readonly TextBox txtPassword = new();
    private readonly Button btnTogglePassword = new();
    private readonly Label lblConfirm = new();
    private readonly TextBox txtConfirm = new();
    private readonly Button btnToggleConfirm = new();
    private readonly Button btnSave = new();
    private readonly Button btnCancel = new();

    public ResetPasswordForm(UserService users, int userId)
    {
        _users = users;
        _userId = userId;
        BuildChrome();
        AddAccent();
        AddTitle();
        AddPasswordFields();
        AddActions();
    }

    private void BuildChrome()
    {
        Text = Messages.ResetPassword;
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
        lblTitle.Text = Messages.ResetPassword;
        lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
        lblTitle.ForeColor = Palette.Ink;
        lblTitle.Location = new Point(FieldX, 24);
        lblTitle.AutoSize = true;
        Controls.Add(lblTitle);
    }

    private void AddPasswordFields()
    {
        PlaceSecret(lblPassword, txtPassword, btnTogglePassword, Messages.Password, 72);
        PlaceSecret(lblConfirm, txtConfirm, btnToggleConfirm, Messages.ConfirmPassword, 140);
        txtPassword.TabIndex = 0;
        txtConfirm.TabIndex = 1;
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
        btnSave.Location = new Point(FieldX, 220);
        btnSave.Size = new Size(140, 38);
        btnSave.FlatStyle = FlatStyle.Flat;
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.BackColor = Palette.Accent;
        btnSave.ForeColor = Palette.White;
        btnSave.Cursor = Cursors.Hand;
        btnSave.TabIndex = 2;
        btnSave.Click += SaveClicked;
    }

    private void StyleCancel()
    {
        btnCancel.Text = Messages.Cancel;
        btnCancel.Location = new Point(188, 220);
        btnCancel.Size = new Size(120, 38);
        btnCancel.FlatStyle = FlatStyle.Flat;
        btnCancel.FlatAppearance.BorderColor = Palette.Ink;
        btnCancel.FlatAppearance.BorderSize = 1;
        btnCancel.BackColor = Palette.White;
        btnCancel.ForeColor = Palette.Ink;
        btnCancel.TabIndex = 3;
        btnCancel.DialogResult = DialogResult.Cancel;
    }

    private void SaveClicked(object? sender, EventArgs e)
    {
        var result = _users.ResetPassword(_userId, txtPassword.Text, txtConfirm.Text);
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
