using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonitorSystem
{
    public partial class FrmTImeMonitor : Form
    {
        #region private members
        List<TimeData> _data = new List<TimeData>();
        TimeData _editRow;
        #endregion

        #region constructor
        public FrmTImeMonitor()
        {
            InitializeComponent();
            grdData.AutoGenerateColumns = false;
            grdData.AllowUserToDeleteRows = true;
            var col = (DataGridViewComboBoxColumn)grdData.Columns[2];
            col.DataSource = Enum.GetValues(typeof(TimeEntryType));
            col.ValueType = typeof(TimeEntryType);
            
            _data.Add(new TimeData() { State = DataState.Submitted, Title = "Task1", EntryType = TimeEntryType.TelephoneCall, Duration = new DateTime()+ new TimeSpan(10, 30, 0), HourlyRate = Convert.ToDouble(txtDefaultRate.Text)});
            RefreshGrid();
            grdData.CellContentClick += GrdData_CellContentClick;
            grdData.RowValidated += GrdData_RowValidated;
            grdData.DataError += GrdData_DataError;
            grdData.CellPainting += GrdData_CellPainting;
        }

        #endregion

        #region private methods
        private void RefreshGrid()
        {
            grdData.DataSource = null;
            grdData.DataSource = _data;
            SetButtonsVisibility();
        }

        private void SetButtonsVisibility()
        {
            foreach(var datarow in grdData.Rows)
            {
                var row = datarow as DataGridViewRow;
                var edit = (DataGridViewButtonCell)row.Cells[6];
                var delete = (DataGridViewButtonCell)row.Cells[7];
                var timeData = row.DataBoundItem as TimeData;
                if (timeData != null)
                {
                    if (timeData.State == DataState.New)
                    {
                        delete.Value = "Cancel";
                        edit.Value = "Save";
                    }
                    else if (timeData.State == DataState.Active)
                    {
                        delete.Value = "delete";
                        edit.Value = "Edit";
                    }
                }
            }
            grdData.RefreshEdit();
            grdData.Invalidate();
            grdData.Refresh();
        }

        private void SetVisibility(DataGridViewButtonCell cell, bool visible)
        {
            cell.Style = visible ?
              new DataGridViewCellStyle { Padding = new Padding(0, 0, 0, 0) } :
              new DataGridViewCellStyle { Padding = new Padding(1000, 1000, 0, 0) };
        }

        private void EditMode(DataGridViewRow row, DataGridViewButtonCell edit)
        {
            var title = (DataGridViewTextBoxCell)row.Cells[1];
            var eventType = (DataGridViewComboBoxCell)row.Cells[2];
            var duration = (DataGridViewTextBoxCell)row.Cells[3];
            var hourlyRate = (DataGridViewTextBoxCell)row.Cells[4];
            var delete = (DataGridViewButtonCell)row.Cells[7];
            delete.Value = "Cancel";
            edit.Value = "Save";
            title.ReadOnly = false;
            eventType.ReadOnly = false;
            duration.ReadOnly = false;
            hourlyRate.ReadOnly = false;
            grdData.CurrentCell = title;
            grdData.BeginEdit(true);
        }

        private void ExitEditMode(DataGridViewRow row, DataGridViewButtonCell edit1)
        {
            var item = row.DataBoundItem as TimeData;
            if(item?.State == DataState.New)
            {
                item.State = DataState.Active;
            }
            if (item?.State == DataState.Active)
            {
                var title = (DataGridViewTextBoxCell)row.Cells[1];
                var eventType = (DataGridViewComboBoxCell)row.Cells[2];
                var duration = (DataGridViewTextBoxCell)row.Cells[3];
                var hourlyRate = (DataGridViewTextBoxCell)row.Cells[4];
                var edit = (DataGridViewButtonCell)row.Cells[6];
                var delete = (DataGridViewButtonCell)row.Cells[7];
                delete.Value = "delete";
                edit.Value = "Edit";
                title.ReadOnly = true;
                eventType.ReadOnly = true;
                duration.ReadOnly = true;
                hourlyRate.ReadOnly = true;
            }
            grdData.DataSource = _data;
            grdData.RefreshEdit();
            grdData.Invalidate();
        }

        #endregion

        #region eventhandlers
        private void GrdData_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0 && _data.Count() > e.RowIndex)
                {
                    var data = _data[e.RowIndex];
                    if ((e.ColumnIndex == 6 || e.ColumnIndex == 7) && data.State == DataState.Submitted)
                    {
                        e.PaintBackground(e.ClipBounds, true);
                        e.Handled = true;
                    }
                }
            }
            catch
            {

            }
        }

        private void GrdData_RowValidated(object sender, DataGridViewCellEventArgs e)
        {
            var row = grdData.Rows[e.RowIndex];
            var edit = (DataGridViewButtonCell)row.Cells[6];
            if ((string)edit.Value == "Save")
                ExitEditMode(row, edit);
        }

        private void GrdData_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 6)
            {
                var row = grdData.Rows[e.RowIndex];
                var edit = (DataGridViewButtonCell)row.Cells[6];
                if (row != null && (string)edit.Value == "Edit")
                {
                    _editRow = _data[e.RowIndex].Clone() as TimeData;
                    EditMode(row, edit);
                }
                else if (grdData.EndEdit())
                {
                    ExitEditMode(row, edit);
                }
            }
            else if (e.ColumnIndex == 7)
            {
                var row = grdData.Rows[e.RowIndex];
                var remove = _data[e.RowIndex];
                var delete = (DataGridViewButtonCell)row.Cells[7];
                bool deleted = false;
                if (row != null && (string)delete.Value == "Cancel")
                {
                    if (remove.State == DataState.New)
                    {
                        _data.Remove(remove);
                        deleted = true;
                    }
                    else
                    {
                        _data[e.RowIndex] = _editRow;
                        ExitEditMode(row, delete);
                    }
                }
                else if (row != null && (string)delete.Value == "delete")
                {
                    var result = MessageBox.Show("Are you sure you want to delete this timesheet entry?", "Warning", MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        _data.Remove(remove);
                        deleted = true;
                    }
                }
                if (deleted)
                    RefreshGrid();
            }
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            _data.Insert(0, new TimeData() { State = DataState.New, HourlyRate = Convert.ToDouble(txtDefaultRate.Text), HourString = "0:0" });
            RefreshGrid();
            var row = grdData.Rows[0];
            var edit = (DataGridViewButtonCell)row.Cells[6];
            var delete = (DataGridViewButtonCell)row.Cells[7];
            edit.Value = "Save";
            delete.Value = "Cancel";
            SetButtonsVisibility();
            EditMode(row, edit);
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            if (grdData.SelectedRows.Count != 0)
            {
                foreach (var selectedrow in grdData.SelectedRows)
                {
                    DataGridViewRow row = selectedrow as DataGridViewRow;
                    var item = row.DataBoundItem as TimeData;
                    if (item != null && item.State == DataState.Active)
                        item.State = DataState.Submitted;
                    var edit = (DataGridViewButtonCell)row.Cells[6];
                    var delete = (DataGridViewButtonCell)row.Cells[7];
                    SetVisibility(edit, false);
                    SetVisibility(delete, false);
                }
                RefreshGrid();
            }
        }

        private void GrdData_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {

        }
        #endregion
    }
}
