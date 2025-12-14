// Core/ExcelExporter.cs
using ClosedXML.Excel;
using ScottPlot.WinForms;
using System;
using System.Data;
using System.Windows.Forms;
using TekstilScada.Models;
using ScottPlot; // ERROR FIXED: Added for ScottPlot
using System.IO; // Added for MemoryStream
using System.Drawing.Imaging; // ERROR FIXED: Added for ImageFormat
using System.Linq; // Added for Any() method
using ScottPlot;
namespace TekstilScada.Core
{
    public static class ExcelExporter
    {
        public static void ExportDataGridViewToExcel(DataGridView dgv)
        {
            if (dgv.Rows.Count == 0)
            {
                MessageBox.Show("No data was found to export.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SaveFileDialog sfd = new SaveFileDialog() { Filter = "Excel Workbook|*.xlsx" })
                {
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        using (var workbook = new XLWorkbook())
                        {
                            var worksheet = workbook.Worksheets.Add("Report");

                            // Görünür sütunların listesini ve index eşlemesini tutmak için
                            var visibleColumns = dgv.Columns.Cast<DataGridViewColumn>()
                                .Where(c => c.Visible)
                                .OrderBy(c => c.DisplayIndex) // Görüntülenme sırasına göre sırala
                                .ToList();

                            // 1. Başlıkları yazdır (Sadece görünür sütunlar)
                            for (int i = 0; i < visibleColumns.Count; i++)
                            {
                                // i: Excel sütun indexi
                                worksheet.Cell(1, i + 1).Value = visibleColumns[i].HeaderText;
                            }

                            // 2. Verileri yazdır (Sadece görünür sütunlar)
                            for (int i = 0; i < dgv.Rows.Count; i++)
                            {
                                for (int j = 0; j < visibleColumns.Count; j++)
                                {
                                    // visibleColumns[j].Index: DataGridView'daki gerçek sütun indexi
                                    worksheet.Cell(i + 2, j + 1).Value = dgv.Rows[i].Cells[visibleColumns[j].Index].Value?.ToString();
                                }
                            }
                            // 3. Kaydet ve bilgi ver
                            worksheet.Columns().AdjustToContents();
                            workbook.SaveAs(sfd.FileName);
                            MessageBox.Show("The report has been successfully exported to Excel.", "Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while exporting to Excel: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        // NEW: Method for exporting detailed production report to Excel
        public static void ExportProductionDetailToExcel(ProductionReportItem headerData, DataGridView dgvSteps, DataGridView dgvAlarms, FormsPlot formsPlot)
        {
            try
            {
                using (SaveFileDialog sfd = new SaveFileDialog() { Filter = "Excel Workbook|*.xlsx", FileName = $"{headerData.BatchId}_Report.xlsx" })
                {
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        using (var workbook = new XLWorkbook())
                        {
                            // --- 1. Sheet: DATA ---
                            var worksheet = workbook.Worksheets.Add("Production Detail Report");

                            // --- HESAPLAMALAR ---
                            // Teorik süreyi TimeSpan formatına çevir
                            var theoreticalDuration = TimeSpan.FromSeconds(headerData.TheoreticalCycleTimeSeconds);

                            // Gerçekleşen süreyi hesapla (EndTime - StartTime)
                            // Not: CycleTime string geldiği için matematiksel işlemde EndTime-StartTime kullanıyoruz.
                            var actualDuration = headerData.EndTime - headerData.StartTime;

                            // Farkı hesapla (Gerçekleşen - Teorik)
                            var difference = actualDuration - theoreticalDuration;
                            string differenceString = $"{(difference.TotalSeconds < 0 ? "-" : "")}{difference.Duration():hh\\:mm\\:ss}";

                            // --- BAŞLIK BİLGİLERİ (1-13. Satırlar) ---

                            // 1. Machine Name
                            worksheet.Cell("A1").Value = "Machine Name:";
                            worksheet.Cell("B1").Value = headerData.MachineName;

                            // 2. Recipe Name
                            worksheet.Cell("A2").Value = "Recipe Name:";
                            worksheet.Cell("B2").Value = headerData.RecipeName;

                            // 3. Operator
                            worksheet.Cell("A3").Value = "Operator:";
                            worksheet.Cell("B3").Value = headerData.OperatorName;

                            // 4. Order No (YENİ)
                            worksheet.Cell("A4").Value = "Order No:";
                            worksheet.Cell("B4").Value = headerData.SiparisNo;

                            // 5. Customer Number (YENİ)
                            worksheet.Cell("A5").Value = "Customer Number:";
                            worksheet.Cell("B5").Value = headerData.MusteriNo;

                            // 6. Start Date
                            worksheet.Cell("A6").Value = "Start Date:";
                            worksheet.Cell("B6").Value = headerData.StartTime;

                            // 7. End Date
                            worksheet.Cell("A7").Value = "End Date:";
                            worksheet.Cell("B7").Value = headerData.EndTime;

                            // 8. Total Duration
                            worksheet.Cell("A8").Value = "Total Duration:";
                            worksheet.Cell("B8").Value = headerData.CycleTime; // veya actualDuration.ToString(@"hh\:mm\:ss");

                            // 9. Theoretical Duration (YENİ)
                            worksheet.Cell("A9").Value = "Theoretical Duration:";
                            worksheet.Cell("B9").Value = theoreticalDuration.ToString(@"hh\:mm\:ss");

                            // 10. Difference (Act-Theo) (YENİ)
                            worksheet.Cell("A10").Value = "Difference (Act-Theo):";
                            worksheet.Cell("B10").Value = differenceString;

                            // 11. Electricity Consumption
                            worksheet.Cell("A11").Value = "Electricity Consumption (kWh):";
                            worksheet.Cell("B11").Value = headerData.TotalElectricity;
                            worksheet.Cell("B11").Style.NumberFormat.Format = "0.00";

                            // 12. Water Consumption
                            worksheet.Cell("A12").Value = "Water Consumption (m³):";
                            worksheet.Cell("B12").Value = headerData.TotalWater;
                            worksheet.Cell("B12").Style.NumberFormat.Format = "0.00";

                            // 13. Steam Consumption
                            worksheet.Cell("A13").Value = "Steam Consumption (m³):";
                            worksheet.Cell("B13").Value = headerData.TotalSteam;
                            worksheet.Cell("B13").Style.NumberFormat.Format = "0.00";

                            // --- STİL AYARLARI ---
                            // A1'den A13'e kadar başlıkları kalın yap
                            worksheet.Range("A1:A13").Style.Font.SetBold(true);
                            worksheet.Range("A1:B13").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);

                            // Tabloların başlangıç satırını 16'ya kaydırdık
                            int currentRow = 16;

                            // --- STEP DETAILS TABLE ---
                            worksheet.Cell(currentRow, 1).Value = "STEP DETAILS";
                            worksheet.Range(currentRow, 1, currentRow, dgvSteps.Columns.Count).Merge().Style.Font.SetBold(true).Fill.SetBackgroundColor(XLColor.LightGray);

                            currentRow++;
                            for (int i = 0; i < dgvSteps.Columns.Count; i++)
                            {
                                worksheet.Cell(currentRow, i + 1).Value = dgvSteps.Columns[i].HeaderText;
                            }
                            worksheet.Row(currentRow).Style.Font.SetBold(true);

                            currentRow++;
                            for (int i = 0; i < dgvSteps.Rows.Count; i++)
                            {
                                for (int j = 0; j < dgvSteps.Columns.Count; j++)
                                {
                                    worksheet.Cell(currentRow + i, j + 1).Value = dgvSteps.Rows[i].Cells[j].Value?.ToString();
                                }
                            }

                            // --- PROCESS ALARMS TABLE ---
                            currentRow += dgvSteps.Rows.Count + 2;
                            worksheet.Cell(currentRow, 1).Value = "PROCESS ALARMS";
                            worksheet.Range(currentRow, 1, currentRow, dgvAlarms.Columns.Count).Merge().Style.Font.SetBold(true).Fill.SetBackgroundColor(XLColor.LightGray);

                            currentRow++;
                            for (int i = 0; i < dgvAlarms.Columns.Count; i++)
                            {
                                worksheet.Cell(currentRow, i + 1).Value = dgvAlarms.Columns[i].HeaderText;
                            }
                            worksheet.Row(currentRow).Style.Font.SetBold(true);

                            currentRow++;
                            for (int i = 0; i < dgvAlarms.Rows.Count; i++)
                            {
                                for (int j = 0; j < dgvAlarms.Columns.Count; j++)
                                {
                                    worksheet.Cell(currentRow + i, j + 1).Value = dgvAlarms.Rows[i].Cells[j].Value?.ToString();
                                }
                            }

                            worksheet.Columns().AdjustToContents();

                            // --- CHART IMAGE ---
                            if (formsPlot != null && formsPlot.Plot.GetPlottables().Any())
                            {
                                // 1. En geniş tablo sütununu bul
                                int lastUsedColumn = Math.Max(dgvSteps.Columns.Count, dgvAlarms.Columns.Count);

                                // 2. Grafiği tablonun 3 sütun sağına koy
                                int chartStartColumn = lastUsedColumn + 3;

                                var imageBytes = formsPlot.Plot.GetImageBytes(1200, 800, ScottPlot.ImageFormat.Png);
                                using (var ms = new MemoryStream(imageBytes))
                                {
                                    var image = worksheet.AddPicture(ms, "ProcessChart")
                                        .MoveTo(worksheet.Cell(1, chartStartColumn))
                                        .Scale(0.75);
                                }
                            }

                            workbook.SaveAs(sfd.FileName);
                            MessageBox.Show("The report (Data and Chart) has been successfully exported to Excel.", "Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while exporting to Excel: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}