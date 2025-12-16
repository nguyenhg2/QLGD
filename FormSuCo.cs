using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace QLGD_WinForm
{
    public class FormSuCo : BaseManagementForm
    {
        private TextBox txtTim;
        private Button btnThem;
        private Button btnCapNhat;
        private Button btnXoa;

        public FormSuCo() : base("Quản Lý Sự Cố & Bảo Trì", "SuCo", new Size(1200, 700))
        {
            InitializeCustomToolbar();
            LoadData();
        }

        #region UI Setup
        private void InitializeCustomToolbar()
        {
            txtTim = new TextBox { Width = 250, PlaceholderText = "Tìm theo Mã, Tên TB, Mô tả..." };
            Button btnTim = new Button { Text = "Tìm Kiếm", BackColor = Color.SteelBlue, ForeColor = Color.White };

            btnThem = new Button { Text = "Báo Hỏng Mới", BackColor = Color.Firebrick, ForeColor = Color.White, Width = 120, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            btnCapNhat = new Button { Text = "Cập Nhật / Xử Lý", BackColor = Color.ForestGreen, ForeColor = Color.White, Width = 130, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            btnXoa = new Button { Text = "Xóa", BackColor = Color.Gray, ForeColor = Color.White };
            Button btnLamMoi = new Button { Text = "Làm Mới", BackColor = Color.White };

            btnTim.Click += (s, e) => LoadData(txtTim.Text);
            txtTim.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { LoadData(txtTim.Text); e.SuppressKeyPress = true; } };
            btnLamMoi.Click += (s, e) => { txtTim.Clear(); LoadData(); };

            btnThem.Click += (s, e) =>
            {
                FormThemSuCo frm = new FormThemSuCo();
                if (frm.ShowDialog() == DialogResult.OK) LoadData(txtTim.Text);
            };

            btnCapNhat.Click += BtnCapNhat_Click;
            btnXoa.Click += BtnXoa_Click;

            pnlTop.Controls.Clear();
            int x = 20, y = 15, h = 35;
            Control[] ctrls = { txtTim, btnTim, btnThem, btnCapNhat, btnXoa, btnLamMoi };
            foreach (var c in ctrls)
            {
                c.Location = new Point(x, y);
                c.Height = h;
                if (c is Button && c.Width < 100) c.Width = 100;
                pnlTop.Controls.Add(c);
                x += c.Width + 10;
            }
        }
        #endregion

        #region Data Loading
        protected override void LoadData(string search = "")
        {
            try
            {
                using (var conn = new SqlConnection(AppConfig.ConnectionString))
                {
                    conn.Open();
                    var cmd = new SqlCommand("sp_GetAllSuCo", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@TuKhoa", search.Trim());

                    var dt = new DataTable();
                    new SqlDataAdapter(cmd).Fill(dt);

                    dgvMain.DataSource = dt;
                    ConfigureGridColumns();
                }
            }
            catch { }
        }

        private void ConfigureGridColumns()
        {
            if (dgvMain.Columns.Count == 0) return;

            dgvMain.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvMain.ColumnHeadersHeight = 50;
            dgvMain.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvMain.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            void SetCol(string dbCol, string header, int w)
            {
                if (dgvMain.Columns.Contains(dbCol))
                {
                    dgvMain.Columns[dbCol].HeaderText = header;
                    dgvMain.Columns[dbCol].Width = w;
                }
            }

            SetCol("MaSuKien", "Mã SK", 100);
            SetCol("MaTB", "Mã TB", 100);
            SetCol("TenTB", "Tên Thiết Bị", 200);
            SetCol("LoaiSuKien", "Loại", 120);
            SetCol("NgayPhatSinh", "Ngày Ghi Nhận", 140);
            SetCol("MoTa", "Mô Tả & Kết Quả", 300);
            SetCol("NguoiXuLy", "Người Xử Lý", 120);
            SetCol("TrangThai", "Trạng Thái Xử Lý", 140);
            SetCol("ChiPhi", "Chi Phí", 120);

            if (dgvMain.Columns.Contains("NgayPhatSinh"))
                dgvMain.Columns["NgayPhatSinh"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";

            if (dgvMain.Columns.Contains("ChiPhi"))
            {
                dgvMain.Columns["ChiPhi"].DefaultCellStyle.Format = "N0";
                dgvMain.Columns["ChiPhi"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            if (dgvMain.Columns.Contains("TrangThai"))
            {
                foreach (DataGridViewRow row in dgvMain.Rows)
                {
                    if (row.IsNewRow) continue;
                    var cell = row.Cells["TrangThai"];
                    if (cell.Value == null) continue;

                    string status = cell.Value.ToString();
                    if (status == "Chờ xử lý")
                    {
                        cell.Style.ForeColor = Color.Red;
                        cell.Style.Font = new Font(dgvMain.Font, FontStyle.Bold);
                    }
                    else if (status == "Đang sửa chữa")
                    {
                        cell.Style.ForeColor = Color.DarkOrange;
                    }
                    else if (status == "Đã xử lý")
                    {
                        cell.Style.ForeColor = Color.Green;
                    }
                }
            }
        }
        #endregion

        #region Actions
        private void BtnCapNhat_Click(object sender, EventArgs e)
        {
            if (dgvMain.CurrentRow == null || dgvMain.CurrentRow.IsNewRow)
            {
                MessageBox.Show("Vui lòng chọn sự cố cần cập nhật!");
                return;
            }

            string maSK = dgvMain.CurrentRow.Cells["MaSuKien"].Value?.ToString();
            string tenTB = dgvMain.CurrentRow.Cells["TenTB"].Value?.ToString();
            string trangThai = dgvMain.CurrentRow.Cells["TrangThai"].Value?.ToString();

            decimal chiPhi = 0;
            if (dgvMain.CurrentRow.Cells["ChiPhi"].Value != DBNull.Value)
                decimal.TryParse(dgvMain.CurrentRow.Cells["ChiPhi"].Value.ToString(), out chiPhi);

            if (string.IsNullOrEmpty(maSK)) return;

            FormCapNhatSuCo frm = new FormCapNhatSuCo(maSK, tenTB, trangThai, chiPhi);
            if (frm.ShowDialog() == DialogResult.OK)
            {
                LoadData(txtTim.Text);
            }
        }

        private void BtnXoa_Click(object sender, EventArgs e)
        {
            if (dgvMain.CurrentRow == null || dgvMain.CurrentRow.IsNewRow) return;

            string maSK = dgvMain.CurrentRow.Cells["MaSuKien"].Value.ToString();

            if (MessageBox.Show($"Bạn có chắc muốn xóa sự kiện {maSK}?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    using (var conn = new SqlConnection(AppConfig.ConnectionString))
                    {
                        conn.Open();
                        var cmd = new SqlCommand("DELETE FROM SU_CO_BAO_TRI WHERE MaSuKien = @id", conn);
                        cmd.Parameters.AddWithValue("@id", maSK);
                        cmd.ExecuteNonQuery();

                        MessageBox.Show("Đã xóa thành công!");
                        LoadData(txtTim.Text);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi xóa: " + ex.Message);
                }
            }
        }
        #endregion
    }
}