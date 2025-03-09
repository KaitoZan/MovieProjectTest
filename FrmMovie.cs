using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
namespace MovieProjectTest
{
    public partial class FrmMovie : Form
    {
        public FrmMovie()
        {
            InitializeComponent();
        }
        byte[] mImg, mDiImg;

        private static string conStr = "Server=DESKTOP-F17LCK5\\SQLEXPRESS;Database=movie_record_db;Trusted_connection=True";


        public static void showWarningMSG(string msg)
        {
            MessageBox.Show(msg, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }


        private void getMovieDbToDgv()
        {
            SqlConnection connt = new SqlConnection(conStr);
            try
            {
                if (connt.State == ConnectionState.Open)
                {
                    connt.Close();
                }
                connt.Open();

                string strSql = "SELECT movieId, movieName, movieDetail, movieDateSale, movieTypeName FROM movie_tb " +
                                "INNER JOIN movie_type_tb ON movie_tb.movieTypeId = movie_type_tb.movieTypeId";

                SqlDataAdapter dtAdap = new SqlDataAdapter(strSql, connt);
                DataTable dtTb = new DataTable();

                dtAdap.Fill(dtTb);

                dgvMovieShowAll.Rows.Clear();

                var thaiCulture = new System.Globalization.CultureInfo("th-TH");

                foreach (DataRow row in dtTb.Rows)
                {
                    DateTime movieDateSale = Convert.ToDateTime(row["movieDateSale"]);
                    string dateOnly = movieDateSale.ToString("d MMMM yyyy", thaiCulture);

                    dgvMovieShowAll.Rows.Add(row["movieId"], row["movieName"], row["movieDetail"], dateOnly, row["movieTypeName"]);
                    dgvMovieShowAll.ClearSelection();
                    dgvMovieShowAll.EnableHeadersVisualStyles = false;
                }
            }
            catch (Exception ex)
            {
                showWarningMSG("เกิดข้อผิดพลาด: " + ex.Message);
            }
            finally
            {
                connt.Close();
            }
        }
        private void loadDtToComboBox()
        {

            try
            {
                using (SqlConnection connt = new SqlConnection(conStr))
                {
                    connt.Open();

                    string strSql = "SELECT movieTypeName FROM movie_type_tb";

                    using (SqlCommand sqlCom = new SqlCommand(strSql, connt))
                    {

                        SqlDataReader reader = sqlCom.ExecuteReader();

                        cbbMovieType.Items.Clear();

                        while (reader.Read())
                        {
                            cbbMovieType.Items.Add(reader["movieTypeName"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                showWarningMSG("เกิดข้อผิดพลาด: " + ex.Message);
            }

        }
        private void cancelClearFrm()
        {
            rdMovieId.Checked = true;
            btAdd.Enabled = true;
            btEdit.Enabled = false;
            btDel.Enabled = false;
            btSaveAddEdit.Enabled = false;
            groupBox2.Enabled = false;
            lbMovieId.Text = "";
            tbMovieSearch.Clear();
            tbMovieName.Clear();
            tbMovieDetail.Clear();
            tbMovieDVDTotal.Clear();
            tbMovieDVDPrice.Clear();
            lsMovieShow.Items.Clear();
            nudMovieHour.Value = 0;
            nudMovieMinute.Value = 0;
            cbbMovieType.SelectedIndex = 0;
            dtpMovieDateSale.Value = DateTime.Now;
            pcbMovieImg.Image = null;
            pcbDirMovie.Image = null;

        }
      
        private void FrmMovie_Load(object sender, EventArgs e)
        {
            loadDtToComboBox();
            cancelClearFrm();
            getMovieDbToDgv();
            createDeletedMovieIdsTableIfNotExists();
        }
        private void createDeletedMovieIdsTableIfNotExists()
        {
            using (SqlConnection connt = new SqlConnection(conStr))
            {
                try
                {
                    connt.Open();

                    // ตรวจสอบว่าตาราง deleted_movie_ids มีอยู่หรือไม่
                    string checkTableQuery = "IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'deleted_movie_ids' AND type = 'U') " +
                                             "CREATE TABLE deleted_movie_ids (movieId VARCHAR(10) PRIMARY KEY)";

                    using (SqlCommand cmd = new SqlCommand(checkTableQuery, connt))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("เกิดข้อผิดพลาดในการตรวจสอบหรือสร้างตาราง deleted_movie_ids: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void rdMovieId_Click(object sender, EventArgs e)
        {
            lsMovieShow.Items.Clear();
            tbMovieSearch.Clear();
        }
        private void rdMovieName_Click(object sender, EventArgs e)
        {
            lsMovieShow.Items.Clear();
            tbMovieSearch.Clear();
        }
        private void SearchByMovieID(string movieId)
        {
            using (SqlConnection connt = new SqlConnection(conStr))
            {
                try
                {
                    connt.Open();
                    string query = "SELECT movieId, movieName FROM movie_tb WHERE movieId = @movieId";
                    SqlCommand command = new SqlCommand(query, connt);
                    command.Parameters.AddWithValue("@movieId", movieId);

                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        ListViewItem item = new ListViewItem("1");
                        item.SubItems.Add(reader["movieName"].ToString());
                        item.Tag = reader["movieId"].ToString();
                        lsMovieShow.Items.Add(item);
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message);
                }
            }
        }

        private void SearchByMovieName(string movieName)
        {
            using (SqlConnection connt = new SqlConnection(conStr))
            {
                try
                {
                    connt.Open();
                    string query = "SELECT movieId, movieName FROM movie_tb WHERE movieName LIKE @movieName";
                    SqlCommand command = new SqlCommand(query, connt);
                    command.Parameters.AddWithValue("@movieName", "%" + movieName + "%");

                    SqlDataReader reader = command.ExecuteReader();
                    int count = 1; 
                    while (reader.Read())
                    {
                        ListViewItem item = new ListViewItem(count.ToString());
                        item.SubItems.Add(reader["movieName"].ToString());
                        item.Tag = reader["movieId"].ToString(); 
                        lsMovieShow.Items.Add(item);
                        count++; 
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message);
                }
            }
        }
        private void btMovieSearch_Click(object sender, EventArgs e)
        {
            string searchText = tbMovieSearch.Text.Trim();
            if (string.IsNullOrEmpty(searchText))
            {
                MessageBox.Show("กรุณาป้อนคำค้นหา");
                return;
            }
            lsMovieShow.Items.Clear(); 
            if (rdMovieId.Checked)
            {
                SearchByMovieID(searchText);
                groupBox2.Enabled = false;
            }
            else if (rdMovieName.Checked)
            {
                SearchByMovieName(searchText);
                groupBox2.Enabled = false;

            }

            if (lsMovieShow.Items.Count == 0)
            {
                MessageBox.Show("ไม่พบข้อมูลที่ค้นหา");
            }



        }
        private (string movieId, bool isFromDeleted) genNewMovieId()
{
    string newMovieId = "mv001";
    string lastMovieId = "";
    bool isFromDeleted = false; // เพิ่มตัวแปร isFromDeleted

    using (SqlConnection connt = new SqlConnection(conStr))
    {
        connt.Open();

        // 1. ตรวจสอบ movieId ที่ถูกลบ
        string strSqlDeleted = "SELECT TOP 1 movieId FROM deleted_movie_ids ORDER BY movieId ASC";
        using (SqlCommand cmdDeleted = new SqlCommand(strSqlDeleted, connt))
        {
            object resultDeleted = cmdDeleted.ExecuteScalar();
            if (resultDeleted != null)
            {
                newMovieId = resultDeleted.ToString();
                isFromDeleted = true; // ตั้งค่า isFromDeleted เป็น true

                return (newMovieId, isFromDeleted); // ส่งคืน movieId และ isFromDeleted
            }
        }

        // 2. ถ้าไม่มี movieId ที่ถูกลบ ให้สร้าง movieId ต่อจาก movieId ล่าสุด
        string strSqlLast = "SELECT TOP 1 movieId FROM movie_tb ORDER BY movieId DESC";
        using (SqlCommand cmdLast = new SqlCommand(strSqlLast, connt))
        {
            object resultLast = cmdLast.ExecuteScalar();
            if (resultLast != null)
            {
                lastMovieId = resultLast.ToString();

                int numberPart = int.Parse(lastMovieId.Substring(2));
                numberPart++;
                newMovieId = "mv" + numberPart.ToString("D3");
            }
        }
    }
    return (newMovieId, isFromDeleted); // ส่งคืน movieId และ isFromDeleted
}


        private void deleteMovie(string movieId)
        {
            using (SqlConnection connt = new SqlConnection(conStr))
            {
                connt.Open();


                string strSqlDeleteMovie = "DELETE FROM movie_tb WHERE movieId = @movieId";
                using (SqlCommand cmdDeleteMovie = new SqlCommand(strSqlDeleteMovie, connt))
                {
                    cmdDeleteMovie.Parameters.AddWithValue("@movieId", movieId);
                    cmdDeleteMovie.ExecuteNonQuery();
                }

                string strSqlInsertDeleted = "INSERT INTO deleted_movie_ids (movieId) VALUES (@movieId)";
                using (SqlCommand cmdInsertDeleted = new SqlCommand(strSqlInsertDeleted, connt))
                {
                    cmdInsertDeleted.Parameters.AddWithValue("@movieId", movieId);
                    cmdInsertDeleted.ExecuteNonQuery();
                }
            }
        }
        private void btAdd_Click(object sender, EventArgs e)
        {
            btAdd.Enabled = false;
            btSaveAddEdit.Enabled = true;

            groupBox2.Enabled = true;
            lbMovieId.Text = "";
            tbMovieSearch.Clear();
            tbMovieName.Clear();
            tbMovieDetail.Clear();
            tbMovieDVDTotal.Clear();
            tbMovieDVDPrice.Clear();

            // รับค่า tuple และแยก movieId ออกมาใช้
            var newMovieIdInfo = genNewMovieId();
            lbMovieId.Text = newMovieIdInfo.movieId;
        }
        private bool checkForInsertOrUpdate(string movieId)
        {
            using (SqlConnection connt = new SqlConnection(conStr))
            {
                connt.Open();
                string strSql = "SELECT COUNT(*) FROM movie_tb WHERE movieId = @movieId";
                using (SqlCommand sqlCom = new SqlCommand(strSql, connt))
                {
                    sqlCom.Parameters.AddWithValue("@movieId", movieId);
                    int count = Convert.ToInt32(sqlCom.ExecuteScalar());
                    return count == 0;
                }
            }
        }
        private void btSelectMImg_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files (*.jpg, *.jpeg, *.png) | *.jpg; *.jpeg; *.png";
            if (ofd.ShowDialog() == DialogResult.OK)
            {

                pcbMovieImg.Image = Image.FromFile(ofd.FileName);

                string extFile = Path.GetExtension(ofd.FileName);
   
                using (MemoryStream ms = new MemoryStream())
                {
                    if (extFile == ".jpg" || extFile == ".jpeg")
                    {
                        pcbMovieImg.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    }
                    else
                    {
                        pcbMovieImg.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    }
                    mImg = ms.ToArray();
                }
            }
        }


        private void btSelectMDiImg_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files (*.jpg, *.jpeg, *.png) | *.jpg; *.jpeg; *.png";
            if (ofd.ShowDialog() == DialogResult.OK)
            {

                pcbDirMovie.Image = Image.FromFile(ofd.FileName);

                string extFile = Path.GetExtension(ofd.FileName);
            
                using (MemoryStream ms = new MemoryStream())
                {
                    if (extFile == ".jpg" || extFile == ".jpeg")
                    {
                        pcbDirMovie.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    }
                    else
                    {
                        pcbDirMovie.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    }
                    mDiImg = ms.ToArray();
                }
            }
        }

        private void btSaveAddEdit_Click(object sender, EventArgs e)
        {
            // ส่วนของการตรวจสอบข้อมูล

            // 1. ตรวจสอบว่าชื่อภาพยนตร์ถูกป้อนหรือไม่
            if (tbMovieName.Text.Trim().Length == 0)
            {
                showWarningMSG("ป้อนชื่อภาพยนต์ด้วย");
                return;
            }

            // 2. ตรวจสอบว่ารายละเอียดภาพยนตร์ถูกป้อนหรือไม่
            if (tbMovieDetail.Text.Trim().Length == 0)
            {
                showWarningMSG("ป้อนรายละเอียดภาพยนต์ด้วย");
                return;
            }

            // 3. ตรวจสอบว่าชั่วโมงความยาวภาพยนตร์มากกว่า 0 หรือไม่
            if (nudMovieHour.Value <= 0)
            {
                showWarningMSG("ชั่วโมงต้องมากกว่า 0");
                return;
            }

            // 4. ตรวจสอบว่าจำนวน DVD ถูกป้อนหรือไม่
            if (tbMovieDVDTotal.Text.Trim().Length == 0)
            {
                showWarningMSG("ป้อนจำนวน DVD ด้วย");
                return;
            }

            // 5. ตรวจสอบว่าราคา DVD ถูกป้อนหรือไม่
            if (tbMovieDVDPrice.Text.Trim().Length == 0)
            {
                showWarningMSG("ป้อนราคา DVD ด้วย");
                return;
            }

            // 6. ตรวจสอบว่ารูปภาพภาพยนตร์ถูกเลือกหรือไม่
            if (mImg == null)
            {
                showWarningMSG("เลือกรูปตัวอย่างภาพยนต์ด้วย");
                return;
            }

            // 7. ตรวจสอบว่ารูปภาพผู้กำกับถูกเลือกหรือไม่
            if (mDiImg == null)
            {
                showWarningMSG("เลือกรูปผู้กำกับภาพยนต์ด้วย");
                return;
            }

            // ส่วนของการบันทึกข้อมูล
            using (SqlConnection connt = new SqlConnection(conStr))
            {
                connt.Open();
                SqlTransaction sqlTran = connt.BeginTransaction();
                SqlCommand sqlCom = new SqlCommand();
                sqlCom.Connection = connt;
                sqlCom.Transaction = sqlTran;
                try
                {
                    string strSql;

                    // ตรวจสอบว่าเป็นการเพิ่มหรือแก้ไขข้อมูล
                    if (checkForInsertOrUpdate(lbMovieId.Text))
                    {
                        // ถ้าเป็นการเพิ่มข้อมูล
                        strSql = "INSERT INTO movie_tb (movieId, movieName, movieDetail, movieDateSale, movieLengthHour, movieLengthMinute, movieTypeId, movieDVDTotal, movieDVDPrice, movieImg, movieDirImg) " +
                                 "VALUES (@movieId, @movieName, @movieDetail, @movieDateSale, @movieLengthHour, @movieLengthMinute, @movieTypeId, @movieDVDTotal, @movieDVDPrice, @movieImg, @movieDirImg)";
                    }
                    else
                    {
                        // ถ้าเป็นการแก้ไขข้อมูล
                        strSql = "UPDATE movie_tb SET movieName=@movieName, movieDetail=@movieDetail, movieDateSale=@movieDateSale, movieLengthHour=@movieLengthHour, " +
                                 "movieLengthMinute=@movieLengthMinute, movieTypeId=@movieTypeId, movieDVDTotal=@movieDVDTotal, movieDVDPrice=@movieDVDPrice, movieImg=@movieImg , movieDirImg=@movieDirImg " +
                                 "WHERE movieId=@movieId";
                    }

                    sqlCom.CommandText = strSql;

                    // กำหนดค่าพารามิเตอร์
                    sqlCom.Parameters.AddWithValue("@movieId", lbMovieId.Text.Trim());
                    sqlCom.Parameters.AddWithValue("@movieName", tbMovieName.Text.Trim());
                    sqlCom.Parameters.AddWithValue("@movieDetail", tbMovieDetail.Text.Trim());
                    sqlCom.Parameters.AddWithValue("@movieDateSale", dtpMovieDateSale.Value);
                    sqlCom.Parameters.AddWithValue("@movieLengthHour", nudMovieHour.Value);
                    sqlCom.Parameters.AddWithValue("@movieLengthMinute", nudMovieMinute.Value);
                    sqlCom.Parameters.AddWithValue("@movieTypeId", cbbMovieType.SelectedIndex + 1);
                    sqlCom.Parameters.AddWithValue("@movieDVDTotal", Convert.ToInt32(tbMovieDVDTotal.Text));
                    sqlCom.Parameters.AddWithValue("@movieDVDPrice", Convert.ToDecimal(tbMovieDVDPrice.Text));
                    sqlCom.Parameters.AddWithValue("@movieImg", mImg);
                    sqlCom.Parameters.AddWithValue("@movieDirImg", mDiImg);

                    // บันทึกข้อมูลลงในฐานข้อมูล
                    sqlCom.ExecuteNonQuery();
                    sqlTran.Commit();

                    // ส่วนของการลบ movieId ออกจาก deleted_movie_ids ถ้าถูกดึงมาจากตารางนั้น
                    var newMovieIdInfo = genNewMovieId();
                    if (newMovieIdInfo.isFromDeleted)
                    {
                        string strSqlDelete = "DELETE FROM deleted_movie_ids WHERE movieId = @movieId";
                        using (SqlCommand cmdDelete = new SqlCommand(strSqlDelete, connt))
                        {
                            cmdDelete.Parameters.AddWithValue("@movieId", newMovieIdInfo.movieId);
                            cmdDelete.ExecuteNonQuery();
                        }
                    }

                    // แสดงข้อความและโหลดข้อมูลใหม่
                    showWarningMSG("บันทึกข้อมูลสำเร็จ!");
                    cancelClearFrm();
                    FrmMovie_Load(sender, e);
                }
                catch (Exception ex)
                {
                    // ถ้าเกิดข้อผิดพลาด ให้ rollback transaction และแสดงข้อความผิดพลาด
                    sqlTran.Rollback();
                    showWarningMSG("เกิดข้อผิดพลาด: " + ex.Message);
                }
            }
        }
        private void btEdit_Click(object sender, EventArgs e)
        {
            groupBox2.Enabled = true;
            btSaveAddEdit.Enabled = true;
            btAdd.Enabled = false;
            btEdit.Enabled = false;
            btDel.Enabled = false;
        }
        private void btDel_Click(object sender, EventArgs e)
        {
            if (lsMovieShow.SelectedItems.Count > 0)
            {
                string movieId = lsMovieShow.SelectedItems[0].Tag.ToString();

                DialogResult result = MessageBox.Show("คุณต้องการลบข้อมูลภาพยนตร์นี้ใช่หรือไม่?", "ยืนยัน", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    try
                    {
                        deleteMovie(movieId);
                        showWarningMSG("ลบข้อมูลภาพยนตร์สำเร็จ");
                        FrmMovie_Load(sender, e);
                    }
                    catch (Exception ex)
                    {
                        showWarningMSG("เกิดข้อผิดพลาด: " + ex.Message);
                    }
                }
            }
            else
            {
                showWarningMSG("กรุณาเลือกภาพยนตร์ที่ต้องการลบ");
            }
        }
        private void btCancel_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("ต้องการล้างข้อมูลหรือไม่", "ยืนยัน",
                   MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                cancelClearFrm();
            }

        }
        private void lsMovieShow_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lsMovieShow.SelectedItems.Count > 0)
            {
                btAdd.Enabled = false;
                btEdit.Enabled = true;
                btDel.Enabled = true;
                btSaveAddEdit.Enabled = false;
                string movieId = lsMovieShow.SelectedItems[0].Tag.ToString(); // ดึง movieId จาก Tag

                // ดึงข้อมูลภาพยนตร์จากฐานข้อมูลและแสดงในฟอร์ม
                using (SqlConnection connection = new SqlConnection(conStr))
                {
                    try
                    {
                        connection.Open();
                        string query = "SELECT * FROM movie_tb WHERE movieId = @movieId";
                        SqlCommand command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@movieId", movieId);
                        DataTable dt = new DataTable();
                        SqlDataReader reader = command.ExecuteReader();
                        if (reader.Read())
                        {
                            lbMovieId.Text = movieId;
                            tbMovieName.Text = reader["movieName"].ToString();
                            tbMovieDetail.Text = reader["movieDetail"].ToString();
                            dtpMovieDateSale.Value = Convert.ToDateTime(reader["movieDateSale"]);
                            nudMovieHour.Value = Convert.ToInt32(reader["movieLengthHour"]);
                            nudMovieMinute.Value = Convert.ToInt32(reader["movieLengthMinute"]);
                            cbbMovieType.SelectedIndex = Convert.ToInt32(reader["movieTypeId"]) - 1; // แสดง Index-1
                            tbMovieDVDTotal.Text = reader["movieDVDTotal"].ToString();
                            tbMovieDVDPrice.Text = reader["movieDVDPrice"].ToString();

                            // ดึงรูปภาพจากฐานข้อมูลและแสดงใน PictureBox
                            byte[] imageBytes = reader["movieImg"] as byte[];
                            if (imageBytes != null)
                            {
                                using (MemoryStream ms = new MemoryStream(imageBytes))
                                {
                                    pcbMovieImg.Image = Image.FromStream(ms);  // ใส่ PictureBox ที่คุณใช้แสดงรูปภาพ
                                    mImg = imageBytes;
                                }
                            }

                            imageBytes = reader["movieDirImg"] as byte[];
                            if (imageBytes != null)
                            {
                                using (MemoryStream ms = new MemoryStream(imageBytes))
                                {
                                    pcbDirMovie.Image = Image.FromStream(ms);  // ใส่ PictureBox ที่คุณใช้แสดงรูปภาพ
                                    mDiImg = imageBytes;
                                }
                            }


                        }
                        reader.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message);
                    }
                }

            }
        }
        private void btExit_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("ต้องการออกจากแอปหรือไม่", "ยืนยัน",
                   MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                Application.Exit();
            }
        }
        private void tbMovieDVDTotal_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true; 
            }
        }

        private void tbMovieDVDPrice_KeyPress(object sender, KeyPressEventArgs e)
        {
           
            if (char.IsControl(e.KeyChar))
            {
                return;
            }

            
            if (char.IsDigit(e.KeyChar))
            {
                return;
            }

            if (e.KeyChar == '.')
            {
                if (tbMovieDVDPrice.Text.Length == 0)
                {
                    e.Handled = true;
                    return;
                }

                if (tbMovieDVDPrice.Text.Contains("."))
                {
                    e.Handled = true;
                    return;
                }

                return;
            }

            e.Handled = true;
        }

    }
    }

