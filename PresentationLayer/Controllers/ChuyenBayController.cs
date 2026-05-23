using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TransferObject;
using BusinessLayer;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using PresentationLayer;
using System.Globalization;
namespace PresentationLayer.Controllers
{
    public partial class ChuyenBayController : UserControl
    {
        private ChuyenBayBL chuyenBayBL = new ChuyenBayBL();
        private TuyenBayBL tuyenBayBL = new TuyenBayBL();
        private TienTrinhBL tienTrinhBL = new TienTrinhBL();
        private HoaDonBL hoaDonBL = new HoaDonBL();
        private VeChuyenBayBL veChuyenBayBL = new VeChuyenBayBL();
        private MayBayBL mayBayBL = new MayBayBL();
        public ChuyenBayController()
        {
            InitializeComponent();
        }

        public void ChuyenBayDisplay()
        {
            dgvChuyenBay.DataSource = chuyenBayBL.GetChuyenBayList();
            dgvChuyenBay.Columns["MaTB"].Visible = false;
            dgvChuyenBay.Columns["TienTrinhID"].Visible = false;
            dgvChuyenBay.Columns["Delete"].DisplayIndex = dgvChuyenBay.Columns.Count - 1;

            // Load Tuyến bay
            cbMaTuyenBay.DataSource = tuyenBayBL.GetTuyenBayList();
            cbMaTuyenBay.DisplayMember = "TenTB";
            cbMaTuyenBay.ValueMember = "MaTB";
            cbMaTuyenBay.SelectedIndex = -1;

            // Load Tiến trình
            cbTienTrinh.DataSource = tienTrinhBL.GetTienTrinhList();
            cbTienTrinh.DisplayMember = "Ten";
            cbTienTrinh.ValueMember = "Id";
            cbTienTrinh.SelectedIndex = -1;

            // Load TuyenBay
            cbTuyenBaySearch.DataSource = tuyenBayBL.GetTuyenBayList();
            cbTuyenBaySearch.DisplayMember = "TenTB";
            cbTuyenBaySearch.ValueMember = "MaTB";
            cbTuyenBaySearch.SelectedIndex = -1;

            // Load MayBay for switching
            cbMayBayDoi.DataSource = mayBayBL.GetMayBayList();
            cbMayBayDoi.DisplayMember = "tenMB";
            cbMayBayDoi.ValueMember = "maMB";
            cbMayBayDoi.SelectedIndex = -1;


        }

        private void SelectChuyenBayRow(int maCB)
        {
            foreach (DataGridViewRow row in dgvChuyenBay.Rows)
            {
                if (row.Cells["MaCB"].Value == null)
                {
                    continue;
                }

                if (Convert.ToInt32(row.Cells["MaCB"].Value) == maCB)
                {
                    row.Selected = true;
                    dgvChuyenBay.CurrentCell = row.Cells["MaCB"];

                    cbMaTuyenBay.SelectedValue = row.Cells["MaTB"].Value;
                    cbTienTrinh.SelectedValue = row.Cells["TienTrinhID"].Value;

                    DateTime ngayGioDi = Convert.ToDateTime(row.Cells["NgayGioDi"].Value);
                    datetimeThemTB.Value = ngayGioDi.Date;
                    txtGioDi.Text = ngayGioDi.ToString("HH:mm");
                    txtThoiGianBay.Text = row.Cells["ThoiGianBay"].Value.ToString();
                    string tenMB = row.Cells["TenMB"].Value?.ToString();
                    SetMayBaySelection(tenMB);
                    return;
                }
            }
        }

        private void ChuyenBayController_Load(object sender, EventArgs e)
        {
            ChuyenBayDisplay();
            txtGioDi.Text = DateTime.Now.ToString("HH:mm");
        }

        private void btnThemCB_Click(object sender, EventArgs e)
        {
            var maTB = cbMaTuyenBay.SelectedValue;
            var tienTrinh = cbTienTrinh.SelectedValue;
            var thoiGianBay = txtThoiGianBay.Text;
            string tenMBCu = dgvChuyenBay.CurrentRow.Cells["TenMB"].Value?.ToString();

            if (maTB == null || tienTrinh == null || string.IsNullOrEmpty(thoiGianBay))
            {
                MessageBox.Show("Vui lòng chọn đầy đủ thông tin.");
                return;
            }
            if (!TryGetNgayGioDi(out DateTime ngayGioDi))
            {
                return;
            }
            if (chuyenBayBL.CheckChuyenBayExists(Convert.ToInt32(maTB), ngayGioDi))
            {
                MessageBox.Show("Đã tồn tại chuyến bay");
                return;
            }
            bool result = chuyenBayBL.AddChuyenBay((int)maTB, ngayGioDi, Convert.ToInt32(thoiGianBay), Convert.ToByte(tienTrinh));
            if (result)
            {
                MessageBox.Show("Thêm chuyến bay thành công.");
                ChuyenBayDisplay();
                this.Clear();
            }
            else
            {
                MessageBox.Show("Thêm chuyến bay thất bại.");
            }




        }

       

        private void Clear()
        {

            cbMaTuyenBay.SelectedIndex = -1;
            cbTienTrinh.SelectedIndex = -1;
            txtThoiGianBay.Clear();
            txtGioDi.Text = DateTime.Now.ToString("HH:mm");
            datetimeThemTB.Value = DateTime.Now;
            dgvChuyenBay.ClearSelection();
        }
        private void btnHuyThemCB_Click(object sender, EventArgs e)
        {
            this.Clear();

        }

        private async void btnCapNhatCB_Click(object sender, EventArgs e)
        {
            if(dgvChuyenBay.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn một dòng để cập nhật.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var maCB = dgvChuyenBay.CurrentRow.Cells["MaCB"].Value;
            var oldNgayGioDiObj = dgvChuyenBay.CurrentRow.Cells["NgayGioDi"].Value;
            var maTB = cbMaTuyenBay.SelectedValue;
            var tienTrinh = cbTienTrinh.SelectedValue;
            var thoiGianBay = txtThoiGianBay.Text;
            string tenMBCu = dgvChuyenBay.CurrentRow.Cells["TenMB"].Value?.ToString();
            string tenTB = dgvChuyenBay.CurrentRow.Cells["TenTB"].Value?.ToString();
            if (!TryGetNgayGioDi(out DateTime datetime))
            {
                return;
            }

            DateTime oldNgayGioDi = oldNgayGioDiObj == null
                ? DateTime.MinValue
                : Convert.ToDateTime(oldNgayGioDiObj);


            try
            {
                LoadingForm loadingForm = new LoadingForm();
                loadingForm.StartPosition = FormStartPosition.CenterScreen;
                loadingForm.Show();

                bool result = chuyenBayBL.UpdateChuyenBay(Convert.ToInt32(maCB), Convert.ToInt32(maTB), datetime,
                Convert.ToInt32(thoiGianBay), Convert.ToByte(tienTrinh));
                if (!result)
                {
                    if (loadingForm.InvokeRequired)
                    {
                        loadingForm.Invoke(new Action(() => loadingForm.Close()));
                    }
                    else
                    {
                        loadingForm.Close();
                    }
                    MessageBox.Show("Cập nhật chuyến bay thất bại.");
                    return;
                }

                bool aircraftChanged = false;
                string tenMBMoi = string.Empty;
                if (cbMayBayDoi.SelectedIndex != -1 && !string.IsNullOrWhiteSpace(tenMBCu))
                {
                    tenMBMoi = cbMayBayDoi.Text;
                    if (!tenMBMoi.Equals(tenMBCu, StringComparison.OrdinalIgnoreCase))
                    {
                        veChuyenBayBL.SwitchMayBayForChuyenBay(Convert.ToInt32(maCB), Convert.ToInt32(cbMayBayDoi.SelectedValue));
                        aircraftChanged = true;
                    }
                }

                bool timeChanged = oldNgayGioDi != DateTime.MinValue && oldNgayGioDi != datetime;
                bool mailResultTime = true;
                bool mailResultAircraft = true;

                if (aircraftChanged || timeChanged)
                {
                    try
                    {
                        if (aircraftChanged)
                        {
                            mailResultAircraft = await Task.Run(() => SendAircraftChangeEmails(Convert.ToInt32(maCB), tenTB, tenMBCu, tenMBMoi));
                        }
                        if (timeChanged)
                        {
                            string tenMB = aircraftChanged ? tenMBMoi : tenMBCu;
                            mailResultTime = await Task.Run(() => SendRescheduleEmails(Convert.ToInt32(maCB), tenTB, tenMB, oldNgayGioDi, datetime));
                        }
                    }
                    finally
                    {
                        if (loadingForm.InvokeRequired)
                        {
                            loadingForm.Invoke(new Action(() => loadingForm.Close()));
                        }
                        else
                        {
                            loadingForm.Close();
                        }
                    }

                    if (mailResultTime && mailResultAircraft)
                    {
                        MessageBox.Show("Cập nhật chuyến bay thành công và đã gửi thông báo cho khách hàng.");
                    }
                    else
                    {
                        MessageBox.Show("Cập nhật chuyến bay thành công nhưng gửi thông báo thất bại hoặc không có email khách hàng.");
                    }
                }
                else
                {
                    if (loadingForm.InvokeRequired)
                    {
                        loadingForm.Invoke(new Action(() => loadingForm.Close()));
                    }
                    else
                    {
                        loadingForm.Close();
                    }
                    MessageBox.Show("Cập nhật chuyến bay thành công.");
                }

                ChuyenBayDisplay();
                SelectChuyenBayRow(Convert.ToInt32(maCB));
            }
            catch (SqlException ex)
            {

                MessageBox.Show(ex.Message);
            }

            
            // Update
            dgvChuyenBay.DataSource = chuyenBayBL.GetChuyenBayList();
            this.Clear();

        }

        private bool SendRescheduleEmails(int maCB, string tenTB, string tenMB, DateTime oldNgayGioDi, DateTime newNgayGioDi)
        {
            List<string> emails = veChuyenBayBL.GetEmailsByMaCB(maCB);
            if (emails == null || emails.Count == 0)
            {
                return false;
            }

            string fromAdd = "shopnro247combot@gmail.com";
            string fromPassword = "zgjk hwvj poye djvz";
            string subject = "Thong bao doi gio chuyen bay";
            string tenTuyenBay = string.IsNullOrWhiteSpace(tenTB) ? "chuyen bay" : tenTB;
            string tenMayBay = string.IsNullOrWhiteSpace(tenMB) ? "khong ro" : tenMB;
            string oldTimeText = oldNgayGioDi.ToString("dd/MM/yyyy HH:mm");
            string newTimeText = newNgayGioDi.ToString("dd/MM/yyyy HH:mm");
            string body = $"Xin chao,\n\nChuyen bay {tenTuyenBay} (MaCB: {maCB}) da duoc thay doi thoi gian.\n" +
                          $"May bay: {tenMayBay}\n" +
                          $"Thoi gian cu: {oldTimeText}\n" +
                          $"Thoi gian moi: {newTimeText}\n\n" +
                          "Vui long lien he tong dai neu can ho tro.\n\nTran trong.";

            using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
            {
                smtp.Credentials = new NetworkCredential(fromAdd, fromPassword);
                smtp.EnableSsl = true;

                foreach (string email in emails.Distinct())
                {
                    if (string.IsNullOrWhiteSpace(email))
                    {
                        continue;
                    }

                    using (MailMessage mail = new MailMessage(fromAdd, email, subject, body))
                    {
                        mail.IsBodyHtml = false;
                        smtp.Send(mail);
                    }
                }
            }

            return true;
        }

        private bool SendAircraftChangeEmails(int maCB, string tenTB, string tenMBCu, string tenMBMoi)
        {
            List<string> emails = veChuyenBayBL.GetEmailsByMaCB(maCB);
            if (emails == null || emails.Count == 0)
            {
                return false;
            }

            string fromAdd = "shopnro247combot@gmail.com";
            string fromPassword = "zgjk hwvj poye djvz";
            string subject = "Thong bao doi may bay";
            string tenTuyenBay = string.IsNullOrWhiteSpace(tenTB) ? "chuyen bay" : tenTB;
            string mayBayCu = string.IsNullOrWhiteSpace(tenMBCu) ? "khong ro" : tenMBCu;
            string mayBayMoi = string.IsNullOrWhiteSpace(tenMBMoi) ? "khong ro" : tenMBMoi;
            string body = $"Xin chao,\n\nChuyen bay {tenTuyenBay} (MaCB: {maCB}) da duoc thay doi may bay.\n" +
                          $"May bay cu: {mayBayCu}\n" +
                          $"May bay moi: {mayBayMoi}\n\n" +
                          "Vui long lien he tong dai neu can ho tro.\n\nTran trong.";

            using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
            {
                smtp.Credentials = new NetworkCredential(fromAdd, fromPassword);
                smtp.EnableSsl = true;

                foreach (string email in emails.Distinct())
                {
                    if (string.IsNullOrWhiteSpace(email))
                    {
                        continue;
                    }

                    using (MailMessage mail = new MailMessage(fromAdd, email, subject, body))
                    {
                        mail.IsBodyHtml = false;
                        smtp.Send(mail);
                    }
                }
            }

            return true;
        }

        private void btnTimKiemTB_Click(object sender, EventArgs e)
        {
            dgvChuyenBay.DataSource = chuyenBayBL.GetViewChuyenBayByMaTB(Convert.ToInt32(cbTuyenBaySearch.SelectedValue));

        }

        private void dgvChuyenBay_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dgvChuyenBay.Columns["Delete"].Index && e.RowIndex >= 0)
            {
                var maCB = dgvChuyenBay.Rows[e.RowIndex].Cells["MaCB"].Value;
                if (maCB != null)
                {
                    DialogResult dialogResult = MessageBox.Show($"Bạn có chắc chắn muốn xóa chuyến {maCB} này không?", "Xác nhận", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                       // xử lý
                       // Tạo list maHD của Tuyen
                       var x = from ve in veChuyenBayBL.GetVeChuyenBayList()
                               join hd in hoaDonBL.GetHoaDonList()
                                 on ve.maHD equals hd.maHD
                               where ve.maCB == Convert.ToInt32(maCB)
                               select new
                               {
                                 
                                   hd.maHD,
                               };
                        // Xóa vé
                        veChuyenBayBL.DeleteVeByMaCB(Convert.ToInt32(maCB));
                        // Xóa hóa đơn
                        foreach (var item in x)
                        {
                            hoaDonBL.DeleteHoaDon(item.maHD);
                        }

                        // Cập nhật tiến trình
                        // Tiến trình 4: Hủy chuyến
                        chuyenBayBL.UpdateTienTrinh(Convert.ToInt32(maCB), 4);
                        MessageBox.Show("Xóa chuyến bay thành công.");
                        dgvChuyenBay.DataSource = chuyenBayBL.GetChuyenBayList();
                    }
                }
            }
            else
            {
                if (e.RowIndex < 0) return;
            }

            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvChuyenBay.Rows[e.RowIndex];
                // Hiển thị thông tin chuyến bay được chọn
                cbMaTuyenBay.SelectedValue = row.Cells["MaTB"].Value;
                cbTienTrinh.SelectedValue = row.Cells["TienTrinhID"].Value;
                DateTime ngayGioDi = Convert.ToDateTime(row.Cells["NgayGioDi"].Value);
                datetimeThemTB.Value = ngayGioDi.Date;
                txtGioDi.Text = ngayGioDi.ToString("HH:mm");
                txtThoiGianBay.Text = row.Cells["ThoiGianBay"].Value.ToString();
                string tenMB = row.Cells["TenMB"].Value?.ToString();
                SetMayBaySelection(tenMB);
            }

        }

        private void SetMayBaySelection(string tenMB)
        {
            if (string.IsNullOrWhiteSpace(tenMB))
            {
                cbMayBayDoi.SelectedIndex = -1;
                return;
            }

            foreach (DataRowView item in cbMayBayDoi.Items)
            {
                if (item["tenMB"].ToString().Equals(tenMB, StringComparison.OrdinalIgnoreCase))
                {
                    cbMayBayDoi.SelectedValue = item["maMB"];
                    return;
                }
            }

            cbMayBayDoi.SelectedIndex = -1;
        }

        private bool TryGetNgayGioDi(out DateTime ngayGioDi)
        {
            ngayGioDi = DateTime.MinValue;
            string timeText = txtGioDi.Text.Trim();
            if (string.IsNullOrWhiteSpace(timeText))
            {
                MessageBox.Show("Vui lòng nhập giờ đi (HH:mm).", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!DateTime.TryParseExact(timeText, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime timePart))
            {
                MessageBox.Show("Giờ đi không đúng định dạng HH:mm.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            DateTime datePart = datetimeThemTB.Value.Date;
            ngayGioDi = datePart.AddHours(timePart.Hour).AddMinutes(timePart.Minute);
            return true;
        }
    }
}
