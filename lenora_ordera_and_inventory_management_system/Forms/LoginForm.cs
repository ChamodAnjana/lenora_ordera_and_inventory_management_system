using lenora_ordera_and_inventory_management_system.Common;
using lenora_ordera_and_inventory_management_system.Models;
using lenora_ordera_and_inventory_management_system.Services;

namespace lenora_ordera_and_inventory_management_system.Forms;

internal sealed class LoginForm : Form
{
    private const int BrandWidth = 300;
    private const int EntryWidth = 460;
    private const int FormHeight = 560;
    private const int FieldX = 48;
    private const int FieldWidth = 360;
    private const int LabelGap = 22;

    private readonly AuthService _auth;
    private bool _creatingAccount;
    private readonly Panel pnlBrand = new();
    private readonly Panel pnlEntry = new();
    private readonly Label lblBrand = new();
    private readonly Label lblTagline = new();
    private readonly Panel pnlAccent = new();
    private readonly Button btnSignInMode = new();
    private readonly Button btnCreateMode = new();
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
    private readonly Button btnForgotPassword = new();
    private readonly Button btnSubmit = new();
    private readonly Button btnExit = new();

    public LoginForm(AuthService auth)
    {
        _auth = auth;
        BuildChrome();
        BuildBrand();
        BuildChoices();
        BuildFields();
        BuildActions();
        ShowSignIn();
        HideCreateAccountIfAccountsExist();
    }

    private void HideCreateAccountIfAccountsExist()
    {
        if (_auth.DatabaseIsEmpty())
        {
            return;
        }

        btnSignInMode.Visible = false;
        btnCreateMode.Visible = false;
        MoveField(lblUsername, txtUsername, 48);
        MoveSecret(lblPassword, txtPassword, btnTogglePassword, 120);
        btnForgotPassword.Location = new Point(FieldX, 178);
        btnForgotPassword.Visible = true;
        MoveActions(212);
    }

    private void BuildChrome()
    {
        Text = Messages.AppName;
        Font = new Font("Segoe UI", 10F);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(BrandWidth + EntryWidth, FormHeight);
        BackColor = Palette.Paper;
        AcceptButton = btnSubmit;
        CancelButton = btnExit;
        pnlEntry.BackColor = Palette.Paper;
        pnlEntry.Dock = DockStyle.Fill;
        Controls.Add(pnlEntry);
    }

    private void BuildBrand()
    {
        pnlBrand.BackColor = Palette.Ink;
        pnlBrand.Dock = DockStyle.Left;
        pnlBrand.Width = BrandWidth;
        AddBrandTitle();
        AddBrandTagline();
        Controls.Add(pnlBrand);
    }

    private void AddBrandTitle()
    {
        lblBrand.Text = "lenora.lk";
        lblBrand.Font = new Font("Segoe Script", 24F, FontStyle.Bold);
        lblBrand.ForeColor = Palette.AccentPink;
        lblBrand.Location = new Point(28, 180);
        lblBrand.AutoSize = true;
        pnlAccent.BackColor = Palette.Accent;
        pnlAccent.Location = new Point(32, 246);
        pnlAccent.Size = new Size(52, 4);
        pnlBrand.Controls.Add(lblBrand);
        pnlBrand.Controls.Add(pnlAccent);
    }

    private void AddBrandTagline()
    {
        lblTagline.Text = "Online Bag Boutique & Reselling Orders System";
        lblTagline.Font = new Font("Segoe UI", 10.5F);
        lblTagline.ForeColor = Palette.Tagline;
        lblTagline.Location = new Point(32, 268);
        lblTagline.MaximumSize = new Size(236, 0);
        lblTagline.AutoSize = true;
        pnlBrand.Controls.Add(lblTagline);
    }

    private void BuildChoices()
    {
        StyleChoice(btnSignInMode, Messages.SignIn, FieldX);
        StyleChoice(btnCreateMode, Messages.CreateAccount, 232);
        btnSignInMode.Click += SignInModeClicked;
        btnCreateMode.Click += CreateModeClicked;
        btnSignInMode.TabIndex = 0;
        btnCreateMode.TabIndex = 1;
        pnlEntry.Controls.Add(btnSignInMode);
        pnlEntry.Controls.Add(btnCreateMode);
    }

    private static void StyleChoice(Button button, string text, int left)
    {
        button.Text = text;
        button.Location = new Point(left, 36);
        button.Size = new Size(176, 36);
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.Cursor = Cursors.Hand;
    }

    private void BuildFields()
    {
        AddIdentityFields();
        AddPasswordFields();
        BuildForgotPassword();
    }

    private void AddIdentityFields()
    {
        Place(lblFullName, txtFullName, Messages.FullName, 108);
        Place(lblUsername, txtUsername, Messages.Username, 108);
        txtFullName.MaxLength = Rules.FullNameMaxLength;
        txtUsername.MaxLength = Rules.UsernameMaxLength;
        pnlEntry.Controls.Add(lblFullName);
        pnlEntry.Controls.Add(txtFullName);
        pnlEntry.Controls.Add(lblUsername);
        pnlEntry.Controls.Add(txtUsername);
    }

    private void AddPasswordFields()
    {
        PlaceSecret(lblPassword, txtPassword, btnTogglePassword, Messages.Password, 180);
        PlaceSecret(lblConfirm, txtConfirm, btnToggleConfirm, Messages.ConfirmPassword, 312);
        pnlEntry.Controls.Add(lblPassword);
        pnlEntry.Controls.Add(txtPassword);
        pnlEntry.Controls.Add(btnTogglePassword);
        pnlEntry.Controls.Add(lblConfirm);
        pnlEntry.Controls.Add(txtConfirm);
        pnlEntry.Controls.Add(btnToggleConfirm);
    }

    private static void Place(Label label, TextBox box, string caption, int top)
    {
        label.Text = caption;
        label.AutoSize = true;
        label.ForeColor = Palette.Ink;
        label.Location = new Point(FieldX, top);
        box.Location = new Point(FieldX, top + LabelGap);
        box.Size = new Size(FieldWidth, 30);
        box.BorderStyle = BorderStyle.FixedSingle;
    }

    private static void PlaceSecret(Label label, TextBox box, Button toggle, string caption, int top)
    {
        label.Text = caption;
        label.AutoSize = true;
        label.ForeColor = Palette.Ink;
        label.Location = new Point(FieldX, top);

        box.Location = new Point(FieldX, top + LabelGap);
        box.Size = new Size(FieldWidth - 44, 30);
        box.BorderStyle = BorderStyle.FixedSingle;
        box.UseSystemPasswordChar = true;

        toggle.Text = "👁";
        toggle.Location = new Point(FieldX + FieldWidth - 40, top + LabelGap);
        toggle.Size = new Size(40, 26);
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

    private void BuildForgotPassword()
    {
        btnForgotPassword.Text = "Forgot password?";
        btnForgotPassword.Font = new Font("Segoe UI", 9F, FontStyle.Underline);
        btnForgotPassword.ForeColor = Palette.MutedText;
        btnForgotPassword.BackColor = Palette.Paper;
        btnForgotPassword.FlatStyle = FlatStyle.Flat;
        btnForgotPassword.FlatAppearance.BorderSize = 0;
        btnForgotPassword.AutoSize = true;
        btnForgotPassword.Cursor = Cursors.Hand;
        btnForgotPassword.MouseEnter += (_, _) => btnForgotPassword.ForeColor = Palette.AccentPink;
        btnForgotPassword.MouseLeave += (_, _) => btnForgotPassword.ForeColor = Palette.MutedText;
        btnForgotPassword.Click += ForgotPasswordClicked;
        pnlEntry.Controls.Add(btnForgotPassword);
    }

    private void ForgotPasswordClicked(object? sender, EventArgs e)
    {
        var message = "To reset an account password:\n\n" +
                      "• Assistant Accounts: Please contact your business Owner. The Owner can reset your password and assign a temporary login password from the Accounts Management screen.\n\n" +
                      "• Owner Accounts: Contact your technical support administrator.";
        MessageBox.Show(this, message, "Password Reset Assistance", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void BuildActions()
    {
        StyleSubmit();
        StyleExit();
        pnlEntry.Controls.Add(btnSubmit);
        pnlEntry.Controls.Add(btnExit);
    }

    private void StyleSubmit()
    {
        btnSubmit.Size = new Size(160, 38);
        btnSubmit.FlatStyle = FlatStyle.Flat;
        btnSubmit.FlatAppearance.BorderSize = 0;
        btnSubmit.BackColor = Palette.Accent;
        btnSubmit.ForeColor = Palette.White;
        btnSubmit.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        btnSubmit.Cursor = Cursors.Hand;
        btnSubmit.Click += SubmitClicked;
    }

    private void StyleExit()
    {
        btnExit.Text = Messages.Exit;
        btnExit.Size = new Size(120, 38);
        btnExit.FlatStyle = FlatStyle.Flat;
        btnExit.FlatAppearance.BorderColor = Palette.Border;
        btnExit.FlatAppearance.BorderSize = 1;
        btnExit.BackColor = Palette.White;
        btnExit.ForeColor = Palette.Ink;
        btnExit.DialogResult = DialogResult.Cancel;
    }

    private void ShowSignIn()
    {
        _creatingAccount = false;
        MarkChoice(btnSignInMode, btnCreateMode);
        SetPairVisible(lblFullName, txtFullName, false);
        SetSecretPairVisible(lblConfirm, txtConfirm, btnToggleConfirm, false);
        MoveField(lblUsername, txtUsername, 108);
        MoveSecret(lblPassword, txtPassword, btnTogglePassword, 180);
        btnForgotPassword.Location = new Point(FieldX, 238);
        btnForgotPassword.Visible = true;
        MoveActions(274);
        btnSubmit.Text = Messages.SignIn;
        txtUsername.TabIndex = 2;
        txtPassword.TabIndex = 3;
        btnSubmit.TabIndex = 4;
        btnExit.TabIndex = 5;
    }

    private void ShowCreate()
    {
        _creatingAccount = true;
        MarkChoice(btnCreateMode, btnSignInMode);
        SetPairVisible(lblFullName, txtFullName, true);
        SetSecretPairVisible(lblConfirm, txtConfirm, btnToggleConfirm, true);
        MoveField(lblFullName, txtFullName, 108);
        MoveField(lblUsername, txtUsername, 176);
        MoveSecret(lblPassword, txtPassword, btnTogglePassword, 244);
        MoveSecret(lblConfirm, txtConfirm, btnToggleConfirm, 312);
        btnForgotPassword.Visible = false;
        MoveActions(388);
        btnSubmit.Text = Messages.CreateAccount;
        txtFullName.TabIndex = 2;
        txtUsername.TabIndex = 3;
        txtPassword.TabIndex = 4;
        txtConfirm.TabIndex = 5;
        btnSubmit.TabIndex = 6;
        btnExit.TabIndex = 7;
    }

    private static void MarkChoice(Button selected, Button other)
    {
        selected.BackColor = Palette.Accent;
        selected.ForeColor = Palette.White;
        other.BackColor = Palette.White;
        other.ForeColor = Palette.Ink;
    }

    private static void SetPairVisible(Label label, TextBox box, bool visible)
    {
        label.Visible = visible;
        box.Visible = visible;
    }

    private static void SetSecretPairVisible(Label label, TextBox box, Button toggle, bool visible)
    {
        label.Visible = visible;
        box.Visible = visible;
        toggle.Visible = visible;
    }

    private static void MoveField(Label label, TextBox box, int top)
    {
        label.Location = new Point(FieldX, top);
        box.Location = new Point(FieldX, top + LabelGap);
    }

    private static void MoveSecret(Label label, TextBox box, Button toggle, int top)
    {
        label.Location = new Point(FieldX, top);
        box.Location = new Point(FieldX, top + LabelGap);
        toggle.Location = new Point(FieldX + FieldWidth - 40, top + LabelGap);
    }

    private void MoveActions(int top)
    {
        btnSubmit.Location = new Point(FieldX, top);
        btnExit.Location = new Point(FieldX + 176, top);
    }

    private void SignInModeClicked(object? sender, EventArgs e)
    {
        ClearSecrets();
        ShowSignIn();
    }

    private void CreateModeClicked(object? sender, EventArgs e)
    {
        ClearSecrets();
        ShowCreate();
    }

    private void ClearSecrets()
    {
        txtPassword.Clear();
        txtConfirm.Clear();
    }

    private void SubmitClicked(object? sender, EventArgs e)
    {
        if (_creatingAccount)
        {
            RegisterOwner();
            return;
        }

        SignIn();
    }

    private void SignIn()
    {
        var result = _auth.Login(txtUsername.Text, txtPassword.Text);
        if (!result.Success)
        {
            txtPassword.Clear();
            ShowWarning(result.Message);
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }

    private void RegisterOwner()
    {
        var result = _auth.RegisterFirstOwner(ReadAccount());
        if (!result.Success)
        {
            ShowWarning(result.Message);
            return;
        }

        ShowInfo(result.Message);
        ClearSecrets();
        ShowSignIn();
    }

    private AccountRequest ReadAccount()
    {
        return new AccountRequest
        {
            FullName = txtFullName.Text,
            Username = txtUsername.Text,
            Password = txtPassword.Text,
            ConfirmPassword = txtConfirm.Text
        };
    }

    private void ShowWarning(string message)
    {
        MessageBox.Show(this, message, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    private void ShowInfo(string message)
    {
        MessageBox.Show(this, message, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
