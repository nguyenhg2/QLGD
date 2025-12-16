using System;
using System.Drawing;
using System.Windows.Forms;

namespace QLGD_WinForm
{
    public partial class FormMain : Form
    {
        private Color primaryColor = Color.FromArgb(0, 122, 204);
        private Color headerColor = Color.FromArgb(45, 45, 48);
        private Color bgColor = Color.FromArgb(240, 240, 240);

        public FormMain()
        {
            InitializeComponent();
            SetupUI();
        }

        #region UI Initialization
        private void SetupUI()
        {
            this.Text = "QLGD";
            this.Size = new Size(1100, 700);
            this.MinimumSize = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = bgColor;

            Panel pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = headerColor
            };

            Label lblTitle = new Label
            {
                Text = "HỆ THỐNG QUẢN LÝ TRANG THIẾT BỊ VẬT TƯ GIẢNG ĐƯỜNG",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(180, 25)
            };


            pnlHeader.Controls.Add(lblTitle);

            Panel pnlFooter = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 30,
                BackColor = Color.LightGray
            };

            Label lblCopyright = new Label
            {
                Text = "© 2025 K59 - HVKTQS. All rights reserved.",
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = Color.DimGray,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlFooter.Controls.Add(lblCopyright);

            TableLayoutPanel pnlMenu = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                BackColor = Color.Transparent,
                Padding = new Padding(100, 50, 100, 50)
            };

            pnlMenu.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            pnlMenu.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            pnlMenu.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
            pnlMenu.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
            pnlMenu.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));

            Button btnThietBi = CreateDashboardButton("QUẢN LÝ THIẾT BỊ", "📦", "Danh sách, nhập mới, sửa, xóa thiết bị", Color.Teal);
            Button btnMuonTra = CreateDashboardButton("MƯỢN / TRẢ ĐỒ", "📝", "Quản lý phiếu mượn, trả thiết bị", Color.DarkOrange);
            Button btnGiangDuong = CreateDashboardButton("QUỸ PHÒNG HỌC", "🏫", "Quản lý danh sách giảng đường, phòng học", Color.SteelBlue);
            Button btnSuCo = CreateDashboardButton("SỰ CỐ & BẢO TRÌ", "🔧", "Báo hỏng, theo dõi sửa chữa", Color.Firebrick);

            Button btnThoat = new Button
            {
                Text = "Thoát Hệ Thống",
                Size = new Size(200, 45),
                Anchor = AnchorStyles.None,
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnThoat.FlatAppearance.BorderSize = 0;

            btnThietBi.Click += (s, e) => OpenForm(new FormThietBi());
            btnMuonTra.Click += (s, e) => OpenForm(new FormMuonTra());
            btnGiangDuong.Click += (s, e) => OpenForm(new FormGiangDuong());
            btnSuCo.Click += (s, e) => OpenForm(new FormSuCo());
            btnThoat.Click += (s, e) => Application.Exit();

            btnThoat.MouseEnter += (s, e) => btnThoat.BackColor = Color.FromArgb(192, 0, 0);
            btnThoat.MouseLeave += (s, e) => btnThoat.BackColor = Color.Gray;

            pnlMenu.Controls.Add(btnThietBi, 0, 0);
            pnlMenu.Controls.Add(btnMuonTra, 1, 0);
            pnlMenu.Controls.Add(btnGiangDuong, 0, 1);
            pnlMenu.Controls.Add(btnSuCo, 1, 1);

            pnlMenu.Controls.Add(btnThoat, 0, 2);
            pnlMenu.SetColumnSpan(btnThoat, 2);

            this.Controls.Add(pnlMenu);
            this.Controls.Add(pnlFooter);
            this.Controls.Add(pnlHeader);
        }

        private Button CreateDashboardButton(string title, string iconSymbol, string subTitle, Color baseColor)
        {
            Button btn = new Button();
            btn.Text = $"{iconSymbol}\n\n{title}\n\n{subTitle}";
            btn.Dock = DockStyle.Fill;
            btn.Margin = new Padding(15);
            btn.BackColor = Color.White;
            btn.ForeColor = baseColor;
            btn.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            btn.FlatStyle = FlatStyle.Flat;
            btn.Cursor = Cursors.Hand;

            btn.FlatAppearance.BorderColor = baseColor;
            btn.FlatAppearance.BorderSize = 2;

            btn.MouseEnter += (s, e) =>
            {
                btn.BackColor = baseColor;
                btn.ForeColor = Color.White;
            };
            btn.MouseLeave += (s, e) =>
            {
                btn.BackColor = Color.White;
                btn.ForeColor = baseColor;
            };

            return btn;
        }
        #endregion

        #region Helpers
        private void OpenForm(Form f)
        {
            this.Hide();
            f.ShowDialog();
            this.Show();
        }
        #endregion
    }
}