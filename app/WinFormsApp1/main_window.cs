using System;
using System.IO;
using System.Windows.Forms;
using Npgsql;
using Newtonsoft.Json;
using System.Drawing;

namespace LoginApp
{
    public partial class LoginForm : Form
    {
        private string configFile = "config.json";
        private Config config; // Cambiado de dynamic a un tipo específico

        public LoginForm()
        {
            InitializeComponent();
            LoadConfig();
            // Asegúrate de que config no sea null
            if (config == null)
            {
                config = new Config();
            }
        }

        private void InitializeComponent()
        {
            this.Text = "Login";
            this.Size = new Size(600, 300);

            TableLayoutPanel mainLayout = new TableLayoutPanel();
            mainLayout.ColumnCount = 2;
            mainLayout.Dock = DockStyle.Fill;

            FlowLayoutPanel formLayout = new FlowLayoutPanel();
            formLayout.FlowDirection = FlowDirection.TopDown;
            formLayout.Dock = DockStyle.Fill;

            TextBox ip = new TextBox();
            ip.PlaceholderText = "IP de la base de datos";
            ip.Text = config?.Host ?? "";
            formLayout.Controls.Add(ip);

            TextBox port = new TextBox();
            port.PlaceholderText = "Puerto";
            port.Text = config?.Port ?? "";
            formLayout.Controls.Add(port);

            TextBox user = new TextBox();
            user.PlaceholderText = "Usuario";
            user.Text = config?.User ?? "";
            formLayout.Controls.Add(user);

            TextBox password = new TextBox();
            password.PlaceholderText = "Contraseña";
            password.PasswordChar = '*';
            password.Text = config?.Password ?? "";
            formLayout.Controls.Add(password);

            TextBox dbname = new TextBox();
            dbname.PlaceholderText = "Nombre de la base de datos";
            dbname.Text = config?.DbName ?? "";
            formLayout.Controls.Add(dbname);

            Button loginBtn = new Button();
            loginBtn.Text = "Login";
            loginBtn.Click += (sender, e) => CheckLogin(ip.Text, port.Text, user.Text, password.Text, dbname.Text);
            formLayout.Controls.Add(loginBtn);

            mainLayout.Controls.Add(formLayout, 0, 0);

            PictureBox imageLabel = new PictureBox();
            imageLabel.Image = Image.FromFile(ResourcePath("resources/cotaxomil.jpg"));
            imageLabel.SizeMode = PictureBoxSizeMode.StretchImage;
            mainLayout.Controls.Add(imageLabel, 1, 0);

            this.Controls.Add(mainLayout);
        }

        private void LoadConfig()
        {
            try
            {
                if (File.Exists(configFile))
                {
                    string json = File.ReadAllText(configFile);
                    config = JsonConvert.DeserializeObject<Config>(json) ?? new Config();
                }
                else
                {
                    config = new Config();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar el archivo de configuración: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                config = new Config();
            }
        }

        private void SaveConfig(string host, string port, string user, string password, string dbname)
        {
            config = new Config { Host = host, Port = port, User = user, Password = password, DbName = dbname };
            File.WriteAllText(configFile, JsonConvert.SerializeObject(config));
        }

        private void CheckLogin(string host, string port, string user, string password, string dbname)
        {
            string connString = $"Host={host};Port={port};Username={user};Password={password};Database={dbname}";

            try
            {
                using (var conn = new NpgsqlConnection(connString))
                {
                    conn.Open();
                    // Simula la validación del rol de usuario
                    var roles = GetUserRoles(conn, user);
                    if (roles != null)
                    {
                        SaveConfig(host, port, user, password, dbname);
                        // Aquí abrirías tu ventana principal
                        MessageBox.Show("Login exitoso", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Usuario o contraseña incorrectos", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                MessageBox.Show($"Error de conexión a la base de datos: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string[] GetUserRoles(NpgsqlConnection conn, string user)
        {
            // Simula la obtención de roles, aquí podrías hacer una consulta real a la base de datos
            return new string[] { "admin", "user" };
        }

        private string ResourcePath(string relativePath)
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(basePath, relativePath);
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LoginForm());
        }
    }

    // Clase Config para mantener los datos de configuración
    public class Config
    {
        public string Host { get; set; } = ""; // Inicializa con una cadena vacía
        public string Port { get; set; } = ""; // Inicializa con una cadena vacía
        public string User { get; set; } = ""; // Inicializa con una cadena vacía
        public string Password { get; set; } = ""; // Inicializa con una cadena vacía
        public string DbName { get; set; } = ""; // Inicializa con una cadena vacía
    }
}
