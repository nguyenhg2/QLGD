using System.Windows.Forms;
using System.Drawing;

namespace QLGD_WinForm
{
    public static class ColumnWidthHelper
    {
        public static void Load(DataGridView dgv, string configName, int[] defaultWidths)
        {
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgv.AllowUserToResizeColumns = true;
            dgv.ReadOnly = true;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.BackgroundColor = Color.White;
            dgv.RowTemplate.Height = 35;
            dgv.ColumnHeadersHeight = 60;

            for (int i = 0; i < defaultWidths.Length && i < dgv.Columns.Count; i++)
            {
                dgv.Columns[i].Width = defaultWidths[i];
            }
        }
    }
}