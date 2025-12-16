using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace QLGD_WinForm
{
    public class FormGiangDuong : BaseManagementForm
    {
        private TextBox txtTim;

        public FormGiangDuong() : base("Quản Lý Giảng Đường (Tổng Hợp)", "GiangDuong", new Size(1100, 650))
        {
            InitializeCustomToolbar();
            LoadData();
        }

        #region UI Setup
        private void InitializeCustomToolbar()
        {
            txtTim = new TextBox();
            var btnSearch = new Button();
            var btnAdd = new Button();
            var btnEdit = new Button();
            var btnDelete = new Button();

            SetupToolbar(txtTim, btnSearch, btnAdd, btnEdit, btnDelete, "Tìm Giảng đường:");

            btnSearch.Click += (s, e) => LoadData(txtTim.Text);
            btnDelete.Click += (s, e) => DeleteSelected();

            btnAdd.Click += (s, e) => {
                string maGD = Microsoft.VisualBasic.Interaction.InputBox("Nhập mã Giảng đường mới (VD: H9):", "Thêm Giảng Đường");
                if (!string.IsNullOrEmpty(maGD)) ThemGiangDuongMoi(maGD);
            };

            btnEdit.Visible = false;
        }
        #endregion

        #region Data Logic
        private void ThemGiangDuongMoi(string maGD)
        {
            try
            {
                using (var conn = new SqlConnection(AppConfig.ConnectionString))
                {
                    conn.Open();
                    var checkCmd = new SqlCommand("SELECT COUNT(*) FROM GIANG_DUONG WHERE MaGD = @m", conn);
                    checkCmd.Parameters.AddWithValue("@m", maGD);
                    int count = (int)checkCmd.ExecuteScalar();

                    if (count > 0)
                    {
                        MessageBox.Show("Giảng đường này đã tồn tại!");
                        return;
                    }

                    string sql = "INSERT INTO GIANG_DUONG (MaGD, MaPhong) VALUES (@m, '000')";
                    var cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@m", maGD);
                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Thêm thành công!");
                    LoadData();
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        protected override void LoadData(string search = "")
        {
            try
            {
                using (var conn = new SqlConnection(AppConfig.ConnectionString))
                {
                    conn.Open();
                    string sql = @"
                        SELECT 
                            gd.MaGD AS [Khu Vực],
                            COUNT(tb.MaTB) AS [Tổng TB],
                            SUM(CASE WHEN tb.TrangThai = N'Tốt' THEN 1 ELSE 0 END) AS [Tốt],
                            SUM(CASE WHEN tb.TrangThai = N'Hỏng' THEN 1 ELSE 0 END) AS [Hỏng],
                            SUM(CASE WHEN tb.TrangThai = N'Đang sử dụng' THEN 1 ELSE 0 END) AS [Đang Dùng],
                            SUM(CASE WHEN tb.TrangThai = N'Đang sửa chữa' THEN 1 ELSE 0 END) AS [Sửa Chữa]
                        FROM (SELECT DISTINCT MaGD FROM GIANG_DUONG) gd
                        LEFT JOIN THIET_BI tb ON gd.MaGD = tb.MaGD
                        GROUP BY gd.MaGD";

                    var cmd = new SqlCommand(sql, conn);
                    var dt = new DataTable();
                    new SqlDataAdapter(cmd).Fill(dt);

                    if (!string.IsNullOrEmpty(search))
                    {
                        dt.DefaultView.RowFilter = $"[Khu Vực] LIKE '%{search}%'";
                        dgvMain.DataSource = dt.DefaultView;
                    }
                    else
                    {
                        dgvMain.DataSource = dt;
                    }

                    ConfigureGridColumns();
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message); }
        }

        private void ConfigureGridColumns()
        {
            if (dgvMain.Columns.Count == 0) return;

            dgvMain.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvMain.EnableHeadersVisualStyles = false;
            dgvMain.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
            dgvMain.ColumnHeadersHeight = 50;
            dgvMain.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvMain.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvMain.ColumnHeadersDefaultCellStyle.BackColor = Color.WhiteSmoke;

            void SetCol(string dbCol, string headerText, int width)
            {
                if (dgvMain.Columns.Contains(dbCol))
                {
                    dgvMain.Columns[dbCol].HeaderText = headerText;
                    dgvMain.Columns[dbCol].Width = width;
                    dgvMain.Columns[dbCol].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }
            }

            SetCol("Khu Vực", "Giảng Đường", 180);
            SetCol("Tổng TB", "Tổng Số Thiết Bị", 180);
            SetCol("Tốt", "Tốt", 180);
            SetCol("Hỏng", "Hỏng", 180);
            SetCol("Đang Dùng", "Đang Cho Mượn", 180);
            SetCol("Sửa Chữa", "Sửa Chữa", 150);

            foreach (DataGridViewRow row in dgvMain.Rows)
            {
                if (row.Cells["Hỏng"].Value != null &&
                    int.TryParse(row.Cells["Hỏng"].Value.ToString(), out int hong) && hong > 0)
                {
                    row.Cells["Hỏng"].Style.ForeColor = Color.Red;
                    row.Cells["Hỏng"].Style.Font = new Font(dgvMain.Font, FontStyle.Bold);
                    row.Cells["Hỏng"].Style.BackColor = Color.MistyRose;
                }
            }
        }
        #endregion

        #region Actions
        protected override void DeleteSelected()
        {
            if (dgvMain.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn Giảng đường cần xóa!");
                return;
            }

            string maGD = dgvMain.CurrentRow.Cells["Khu Vực"].Value.ToString();

            if (MessageBox.Show($"CẢNH BÁO QUAN TRỌNG:\n" +
                $"Bạn đang chọn xóa toàn bộ Giảng đường {maGD}.\n" +
                $"Hành động này sẽ xóa TẤT CẢ các phòng (101, 102...) thuộc {maGD}.\n\n" +
                "Bạn có chắc chắn muốn tiếp tục?",
                "Xác nhận xóa Giảng đường", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                using (var conn = new SqlConnection(AppConfig.ConnectionString))
                {
                    conn.Open();
                    string sql = "DELETE FROM GIANG_DUONG WHERE MaGD = @m";
                    var cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@m", maGD);

                    try
                    {
                        cmd.ExecuteNonQuery();
                        MessageBox.Show($"Đã xóa giảng đường {maGD} và các phòng liên quan!");
                        LoadData(txtTim.Text);
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number == 547)
                            MessageBox.Show($"Không thể xóa {maGD} vì vẫn còn Thiết bị hoặc dữ liệu Mượn Trả.\nVui lòng xóa hoặc di chuyển thiết bị trước.", "Lỗi ràng buộc dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        else
                            MessageBox.Show("Lỗi: " + ex.Message);
                    }
                }
            }
        }
        #endregion
    }
}