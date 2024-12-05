using System;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace RSADigitalSignature
{
    public partial class SignatureForm : Form
    {
        private string? _privateKey;
        private string? _publicKey;

        public SignatureForm()
        {
            InitializeComponent();
            BuildUI();
        }

        // Побудова інтерфейсу з дизайном
        private void BuildUI()
        {
            Text = "RSA Digital Signature Tool";
            ClientSize = new System.Drawing.Size(500, 600);
            BackColor = System.Drawing.Color.FromArgb(245, 245, 245);

            // Поля для ключів
            var privateKeyLabel = CreateLabel("Private Key:", 20, 20);
            var privateKeyBox = CreateTextBox(20, 50, 450, 60, true);

            var publicKeyLabel = CreateLabel("Public Key:", 20, 130);
            var publicKeyBox = CreateTextBox(20, 160, 450, 60, true);

            // Поле для повідомлення
            var messageLabel = CreateLabel("Message:", 20, 240);
            var messageBox = CreateTextBox(20, 270, 450, 80, false);

            // Поле для підпису
            var signatureLabel = CreateLabel("Signature:", 20, 370);
            var signatureBox = CreateTextBox(20, 400, 450, 80, true);

            // Кнопки
            var generateKeysButton = CreateButton("Generate Keys", 20, 500, System.Drawing.Color.FromArgb(52, 152, 219));
            generateKeysButton.Click += (_, _) => GenerateKeys(privateKeyBox, publicKeyBox);

            var signMessageButton = CreateButton("Sign Message", 180, 500, System.Drawing.Color.FromArgb(46, 204, 113));
            signMessageButton.Click += (_, _) => SignMessage(messageBox, signatureBox);

            var verifySignatureButton = CreateButton("Verify Signature", 340, 500, System.Drawing.Color.FromArgb(231, 76, 60));
            verifySignatureButton.Click += (_, _) => VerifySignature(messageBox, signatureBox);

            // Додавання елементів
            Controls.Add(privateKeyLabel);
            Controls.Add(privateKeyBox);
            Controls.Add(publicKeyLabel);
            Controls.Add(publicKeyBox);
            Controls.Add(messageLabel);
            Controls.Add(messageBox);
            Controls.Add(signatureLabel);
            Controls.Add(signatureBox);
            Controls.Add(generateKeysButton);
            Controls.Add(signMessageButton);
            Controls.Add(verifySignatureButton);
        }

        // Утилітні методи для створення елементів
        private Label CreateLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Location = new System.Drawing.Point(x, y),
                AutoSize = true,
                Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.FromArgb(50, 50, 50)
            };
        }

        private TextBox CreateTextBox(int x, int y, int width, int height, bool readOnly)
        {
            return new TextBox
            {
                Location = new System.Drawing.Point(x, y),
                Width = width,
                Height = height,
                Multiline = true,
                ReadOnly = readOnly,
                Font = new System.Drawing.Font("Consolas", 10),
                BackColor = readOnly ? System.Drawing.Color.FromArgb(240, 240, 240) : System.Drawing.Color.White,
                ForeColor = System.Drawing.Color.Black,
                BorderStyle = BorderStyle.FixedSingle
            };
        }

        private Button CreateButton(string text, int x, int y, System.Drawing.Color backgroundColor)
        {
            return new Button
            {
                Text = text,
                Location = new System.Drawing.Point(x, y),
                Width = 140,
                Height = 40,
                BackColor = backgroundColor,
                ForeColor = System.Drawing.Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold)
            };
        }

        // Генерація ключів
        private void GenerateKeys(TextBox privateKeyBox, TextBox publicKeyBox)
        {
            using var rsa = new RSACryptoServiceProvider(2048);
            _privateKey = rsa.ToXmlString(true); // Приватний ключ
            _publicKey = rsa.ToXmlString(false); // Публічний ключ

            privateKeyBox.Text = _privateKey;
            publicKeyBox.Text = _publicKey;

            MessageBox.Show("Keys generated successfully!");
        }

        // Підписування повідомлення
        private void SignMessage(TextBox messageBox, TextBox signatureBox)
        {
            if (string.IsNullOrEmpty(_privateKey))
            {
                MessageBox.Show("Generate keys first!");
                return;
            }

            var message = messageBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(message))
            {
                MessageBox.Show("Please enter a message to sign.");
                return;
            }

            using var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(_privateKey);

            var messageBytes = Encoding.UTF8.GetBytes(message);
            var hash = SHA256.Create().ComputeHash(messageBytes);
            var signature = rsa.SignHash(hash, CryptoConfig.MapNameToOID("SHA256"));

            signatureBox.Text = Convert.ToBase64String(signature);
            MessageBox.Show("Message signed successfully!");
        }

        // Перевірка підпису
        private void VerifySignature(TextBox messageBox, TextBox signatureBox)
        {
            if (string.IsNullOrEmpty(_publicKey))
            {
                MessageBox.Show("Generate keys first!");
                return;
            }

            var message = messageBox.Text.Trim();
            var signature = signatureBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(message) || string.IsNullOrWhiteSpace(signature))
            {
                MessageBox.Show("Please enter both message and signature to verify.");
                return;
            }

            using var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(_publicKey);

            var messageBytes = Encoding.UTF8.GetBytes(message);
            var hash = SHA256.Create().ComputeHash(messageBytes);
            var signatureBytes = Convert.FromBase64String(signature);

            var isValid = rsa.VerifyHash(hash, CryptoConfig.MapNameToOID("SHA256"), signatureBytes);
            MessageBox.Show(isValid ? "Signature is valid!" : "Signature is invalid!");
        }
    }
}
