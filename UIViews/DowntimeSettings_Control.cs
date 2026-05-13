using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TekstilScada.Repositories;
using TekstilScada.Services;
using TekstilScada.Core;

namespace TekstilScada.UIViews
{
    public partial class DowntimeSettings_Control : UserControl
    {
        private EfficiencyRepository _efficiencyRepository;
        private PlcPollingService _plcPollingService;

        public DowntimeSettings_Control()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Called to initialize this control from the settings page.
        /// </summary>
        public void InitializeControl(EfficiencyRepository efficiencyRepo, PlcPollingService pollingService)
        {
            _efficiencyRepository = efficiencyRepo;
            _plcPollingService = pollingService;
            LoadData();
        }

        private void DowntimeSettings_Control_Load(object sender, EventArgs e)
        {
            // If initialized via code instead of the designer, do not run LoadData here;
            // wait for InitializeControl.
        }

        private void LoadData()
        {
            if (_efficiencyRepository == null) return;

            try
            {
                var definitions = _efficiencyRepository.GetDowntimeDefinitions();

                dgvDowntime.Rows.Clear();
                foreach (var def in definitions.OrderBy(x => x.Key))
                {
                    dgvDowntime.Rows.Add(def.Key, def.Value);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            // Add a new empty row
            int rowIndex = dgvDowntime.Rows.Add();
            dgvDowntime.Rows[rowIndex].Cells[0].Value = 0; // Default bit
            dgvDowntime.Rows[rowIndex].Cells[1].Value = "New Downtime Reason";
            dgvDowntime.CurrentCell = dgvDowntime.Rows[rowIndex].Cells[1];
            dgvDowntime.BeginEdit(true);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvDowntime.SelectedRows.Count > 0)
            {
                var confirm = MessageBox.Show("Are you sure you want to delete the selected reason?", "Confirmation",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (confirm == DialogResult.Yes)
                {
                    foreach (DataGridViewRow row in dgvDowntime.SelectedRows)
                    {
                        dgvDowntime.Rows.Remove(row);
                    }
                }
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        private async void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                var list = new List<dynamic>();
                var bitIndices = new HashSet<int>();

                // Collect and validate data from the grid
                foreach (DataGridViewRow row in dgvDowntime.Rows)
                {
                    if (row.Cells[0].Value == null || row.Cells[1].Value == null) continue;

                    if (!int.TryParse(row.Cells[0].Value.ToString(), out int bitIndex))
                    {
                        MessageBox.Show("Bit address must be numeric!", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (bitIndices.Contains(bitIndex))
                    {
                        MessageBox.Show($"Duplicate Bit Address detected: {bitIndex}. Please provide a unique definition for each bit.",
                            "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    bitIndices.Add(bitIndex);
                    list.Add(new
                    {
                        BitIndex = bitIndex,
                        ReasonText = row.Cells[1].Value.ToString()
                    });
                }

                // 1. Save to Database (Truncate & Insert logic)
                await _efficiencyRepository.SaveDowntimeDefinitionsAsync(list);

                // 2. UPDATE the background service (PlcPollingService)
                if (_plcPollingService != null)
                {
                    _plcPollingService.LoadWaitingDefinitionsCache();
                }

                MessageBox.Show("Downtime reasons successfully saved and the system has been updated.", "Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadData(); // Refresh the list
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during save: {ex.Message}", "Critical Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}