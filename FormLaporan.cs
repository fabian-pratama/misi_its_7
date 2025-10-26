using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;


namespace ManajemenToko
{
    public partial class FormLaporan : Form
    {
        public FormLaporan()
        {
            InitializeComponent();
            dtpDari = new DateTimePicker();
            dtpSampai = new DateTimePicker();
            btnFilter = new Button();

            dtpDari.Location = new Point(30, 20);
            dtpSampai.Location = new Point(200, 20);
            btnFilter.Location = new Point(370, 20);
            btnFilter.Text = "Filter";

            btnFilter.Click += btnFilter_Click;

            Controls.Add(dtpDari);
            Controls.Add(dtpSampai);
            Controls.Add(btnFilter);

            btnCetak = new Button();
            btnCetak.Text = "Cetak Laporan";
            btnCetak.Location = new Point(500, 20);
            btnCetak.Click += btnCetak_Click;
            Controls.Add(btnCetak);

            printDoc.PrintPage += printDoc_PrintPage;

         
        }

        private void TampilkanPenjualanPerHari()
        {
            chartPenjualan.Series.Clear();
            chartPenjualan.ChartAreas.Clear();
            ChartArea area = new ChartArea("AreaUtama");
            chartPenjualan.ChartAreas.Add(area);
            Series series = new Series("Penjualan Per Hari");
            series.ChartType = SeriesChartType.Column;
            series.XValueType = ChartValueType.Date;
            chartPenjualan.Series.Add(series);
            using (SqlConnection conn = Koneksi.GetConnection())
            {
                conn.Open();
                string query = @"SELECT CAST(Tanggal AS DATE) AS Tgl, SUM(TotalHarga) AS Total FROM Penjualan
                GROUP BY CAST(Tanggal AS DATE)
                ORDER BY Tgl";

                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    DateTime tanggal = Convert.ToDateTime(reader["Tgl"]);
                    decimal total = Convert.ToDecimal(reader["Total"]);
                    series.Points.AddXY(tanggal.ToString("dd-MM"), total);
                }
                reader.Close();
            }
            chartPenjualan.Titles.Clear();
            chartPenjualan.Titles.Add("Grafik Penjualan per Hari");
        }

        private void TampilkanPenjualanPerHari(DateTime dari, DateTime sampai)
        {
            chartPenjualan.Series.Clear();
            chartPenjualan.ChartAreas.Clear();
            ChartArea area = new ChartArea("AreaUtama");
            chartPenjualan.ChartAreas.Add(area);
            Series series = new Series("Penjualan Per Hari");
            series.ChartType = SeriesChartType.Column;
            series.XValueType = ChartValueType.Date;
            chartPenjualan.Series.Add(series);

            using (SqlConnection conn = Koneksi.GetConnection())
            {
                conn.Open();
                string query = @"SELECT CAST(Tanggal AS DATE) AS Tgl, SUM(TotalHarga) AS Total 
                         FROM Penjualan
                         WHERE CAST(Tanggal AS DATE) BETWEEN @Dari AND @Sampai
                         GROUP BY CAST(Tanggal AS DATE)
                         ORDER BY Tgl";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Dari", dari);
                cmd.Parameters.AddWithValue("@Sampai", sampai);

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    DateTime tanggal = Convert.ToDateTime(reader["Tgl"]);
                    decimal total = Convert.ToDecimal(reader["Total"]);
                    series.Points.AddXY(tanggal.ToString("dd-MM"), total);
                }
                reader.Close();
            }

            chartPenjualan.Titles.Clear();
            chartPenjualan.Titles.Add($"Penjualan ({dari:dd/MM} - {sampai:dd/MM})");
        }
        private void HitungTotalMingguan()
        {
            using (SqlConnection conn = Koneksi.GetConnection())
            {
                conn.Open();
                string query = @"SELECT SUM(TotalHarga) AS TotalMingguan 
                         FROM Penjualan 
                         WHERE Tanggal >= DATEADD(DAY, -7, GETDATE())";
                SqlCommand cmd = new SqlCommand(query, conn);
                object result = cmd.ExecuteScalar();
                decimal total = (result != DBNull.Value) ? Convert.ToDecimal(result) : 0;
                lblTotalMingguan.Text = $"Total Penjualan Mingguan: Rp {total:N0}";
            }
        }

        private void FormLaporan_Load(object sender, EventArgs e)
        {
            TampilkanPenjualanPerHari();
            HitungTotalMingguan();
        }

        private void TampilkanPenjualanPerKategori()
        {
            chartPenjualan.Series.Clear();
            chartPenjualan.ChartAreas.Clear();
            ChartArea area = new ChartArea("AreaKategori");
            chartPenjualan.ChartAreas.Add(area);
            Series series = new Series("Penjualan per Kategori");
            series.ChartType = SeriesChartType.Pie;
            chartPenjualan.Series.Add(series);
            using (SqlConnection conn = Koneksi.GetConnection())
            {
                conn.Open();
                string query = @"SELECT k.NamaKategori, SUM(pd.Subtotal) AS Total FROM PenjualanDetail pd
                JOIN Produk p ON p.Id = pd.ProdukId
                JOIN Kategori k ON k.Id = p.KategoriId
                GROUP BY k.NamaKategori";
                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string kategori = reader["NamaKategori"].ToString();
                    decimal total = Convert.ToDecimal(reader["Total"]);
                    series.Points.AddXY(kategori, total);
                }
                reader.Close();
            }
            chartPenjualan.Titles.Clear();
            chartPenjualan.Titles.Add("Grafik Penjualan per Kategori");
        }

        private void chartPenjualan_Click(object sender, EventArgs e)
        {

        }

        private void cmbTipeLaporan_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbTipeLaporan.SelectedItem.ToString() == "Harian")
                TampilkanPenjualanPerHari();
            else
                TampilkanPenjualanPerKategori();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            TampilkanPenjualanPerHari();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void btnFilter_Click(object sender, EventArgs e)
        {
            DateTime dari = dtpDari.Value.Date;
            DateTime sampai = dtpSampai.Value.Date;
            TampilkanPenjualanPerHari(dari, sampai);
        }

        private void btnCetak_Click(object sender, EventArgs e)
        {
            printPreview.Document = printDoc;
            printPreview.ShowDialog();
        }

        private void printDoc_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            e.Graphics.DrawString("Laporan Penjualan", new Font("Segoe UI", 16, FontStyle.Bold), Brushes.Black, new Point(100, 50));
            chartPenjualan.Printing.PrintPaint(e.Graphics, new Rectangle(50, 100, 700, 400));

            chartPenjualan.DrawToBitmap(new Bitmap(chartPenjualan.Width, chartPenjualan.Height),
                            new Rectangle(0, 0, chartPenjualan.Width, chartPenjualan.Height));
        }

        private void printPreview_Load(object sender, EventArgs e)
        {

        }

        private void lblTotalMingguan_Click(object sender, EventArgs e)
        {
            
        }
    }
}
