using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace QLGD_WinForm
{
    public class FormThemSuCo : Form
    {
        private ComboBox cboThietBi;
        private ComboBox cboLoaiSuKien;
        private DateTimePicker dtpNgay;
        private TextBox txtMoTa;
        private TextBox txtNguoiBao;
        private Button btnLuu;
        private Button btnHuy;

        public FormThemSuCo()
        {
            InitializeUI();
            LoadDanhSachThietBi();
        }

        #region UI Setup
        private void InitializeUI()
        {
            this.Text = "KHAI BÁO SỰ CỐ / BẢO TRÌ";
            this.Size = new Size(550, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            Label lblHeader = new Label
            {
                Text = "THÔNG TIN SỰ CỐ",
                Dock = DockStyle.Top,
                Height = 60,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.Firebrick
            };

            TableLayoutPanel table = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 320,
                ColumnCount = 2,
                Padding = new Padding(20),
                RowCount = 5
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));

            cboThietBi = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Height = 30, Font = new Font("Segoe UI", 10) };

            // Fix lỗi NotSupportedException: Source phải gán TRƯỚC Mode
            cboThietBi.AutoCompleteSource = AutoCompleteSource.ListItems;
            cboThietBi.AutoCompleteMode = AutoCompleteMode.SuggestAppend;

            AddRow(table, "Chọn Thiết Bị (*):", cboThietBi);

            cboLoaiSuKien = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Height = 30, Font = new Font("Segoe UI", 10) };
            cboLoaiSuKien.Items.AddRange(new object[] { "Sự cố", "Bảo trì định kỳ", "Bảo trì đột xuất" });
            cboLoaiSuKien.SelectedIndex = 0;
            AddRow(table, "Loại Sự Kiện:", cboLoaiSuKien);

            dtpNgay = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy HH:mm", Width = 200 };
            AddRow(table, "Thời Gian:", dtpNgay);

            txtNguoiBao = new TextBox { Height = 30, Font = new Font("Segoe UI", 10) };
            AddRow(table, "Người Báo/Xử Lý:", txtNguoiBao);

            txtMoTa = new TextBox { Multiline = true, Height = 100, ScrollBars = ScrollBars.Vertical, Font = new Font("Segoe UI", 10) };
            Label lblMoTa = new Label { Text = "Mô Tả Chi Tiết:", AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Left, Font = new Font("Segoe UI", 10) };
            table.Controls.Add(lblMoTa);
            table.Controls.Add(txtMoTa);

            btnLuu = new Button { Text = "LƯU SỰ CỐ", DialogResult = DialogResult.None, BackColor = Color.Firebrick, ForeColor = Color.White, Height = 45, Width = 120, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            btnHuy = new Button { Text = "Hủy", DialogResult = DialogResult.Cancel, Height = 45, Width = 100 };

            btnLuu.Click += BtnLuu_Click;

            FlowLayoutPanel pnlBtn = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Bottom, Height = 80, Padding = new Padding(20) };
            pnlBtn.Controls.Add(btnHuy);
            pnlBtn.Controls.Add(btnLuu);

            this.Controls.Add(table);
            this.Controls.Add(pnlBtn);
            this.Controls.Add(lblHeader);
        }

        private void AddRow(TableLayoutPanel table, string label, Control ctrl)
        {
            Label lbl = new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left, Font = new Font("Segoe UI", 10) };
            ctrl.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            table.Controls.Add(lbl);
            table.Controls.Add(ctrl);
        }
        #endregion

        #region Actions
        private void LoadDanhSachThietBi()
        {
            try
            {
                using (var conn = new SqlConnection(AppConfig.ConnectionString))
                {
                    conn.Open();
                    string sql = "SELECT MaTB, TenTB + ' (' + MaTB + ')' as DisplayName FROM THIET_BI ORDER BY TenTB";
                    var da = new SqlDataAdapter(sql, conn);
                    var dt = new DataTable();
                    da.Fill(dt);

                    cboThietBi.DataSource = dt;
                    cboThietBi.DisplayMember = "DisplayName";
                    cboThietBi.ValueMember = "MaTB";
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi load thiết bị: " + ex.Message); }
        }

        private void BtnLuu_Click(object sender, EventArgs e)
        {
            if (cboThietBi.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn thiết bị!");
                return;
            }
            if (string.IsNullOrWhiteSpace(txtMoTa.Text))
            {
                MessageBox.Show("Vui lòng nhập mô tả lỗi!");
                return;
            }

            try
            {
                using (var conn = new SqlConnection(AppConfig.ConnectionString))
                {
                    conn.Open();
                    var cmd = new SqlCommand("sp_ThemSuCoChiTiet", conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@MaTB", cboThietBi.SelectedValue);
                    cmd.Parameters.AddWithValue("@LoaiSuKien", cboLoaiSuKien.SelectedItem.ToString());
                    cmd.Parameters.AddWithValue("@MoTa", txtMoTa.Text);
                    cmd.Parameters.AddWithValue("@NguoiXuLy", txtNguoiBao.Text);
                    cmd.Parameters.AddWithValue("@NgayPhatSinh", dtpNgay.Value);

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Đã ghi nhận sự cố thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu dữ liệu: " + ex.Message);
            }
        }
        #endregion
    }
}