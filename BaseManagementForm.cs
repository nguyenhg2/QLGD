using System;
using System.Drawing;
using System.Windows.Forms;

namespace QLGD_WinForm
{
    public class BaseManagementForm : Form
    {
        protected DataGridView dgvMain;
        protected Panel pnlTop;

        #region Constructor & UI Init
        public BaseManagementForm(string title, string configName, Size size)
        {
            this.Text = title;
            this.Size = size;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 10F);

            dgvMain = new DataGridView { Dock = DockStyle.Fill };
            pnlTop = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.WhiteSmoke };

            this.Controls.Add(dgvMain);
            this.Controls.Add(pnlTop);
        }

        protected void SetupToolbar(TextBox txtSearch, Button btnSearch, Button btnAdd, Button btnEdit, Button btnDel, string lblText)
        {
            Label lbl = new Label { Text = lblText, Location = new Point(20, 20), AutoSize = true };

            txtSearch.Location = new Point(lbl.Right + 10, 18);
            txtSearch.Width = 200;

            btnSearch.Text = "Tìm kiếm";
            btnSearch.Location = new Point(txtSearch.Right + 10, 16);

            btnAdd.Text = "Thêm";
            btnAdd.Location = new Point(btnSearch.Right + 20, 16);
            btnAdd.BackColor = Color.Teal;
            btnAdd.ForeColor = Color.White;

            btnEdit.Text = "Sửa";
            btnEdit.Location = new Point(btnAdd.Right + 10, 16);

            btnDel.Text = "Xóa";
            btnDel.Location = new Point(btnEdit.Right + 10, 16);
            btnDel.BackColor = Color.IndianRed;
            btnDel.ForeColor = Color.White;

            foreach (var btn in new[] { btnSearch, btnAdd, btnEdit, btnDel })
            {
                btn.Size = new Size(90, 32);
            }

            pnlTop.Controls.AddRange(new Control[] { lbl, txtSearch, btnSearch, btnAdd, btnEdit, btnDel });
        }
        #endregion

        #region Virtual Methods
        protected virtual void LoadData(string search = "") { }
        protected virtual void AddNew() { }
        protected virtual void EditSelected() { }
        protected virtual void DeleteSelected() { }
        #endregion
    }
}