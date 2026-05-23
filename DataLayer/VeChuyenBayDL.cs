using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransferObject;

namespace DataLayer
{
    public class VeChuyenBayDL
    {
        private DataProvider provider = new DataProvider();
        private const string ConnectionString = "Data Source=.;Initial Catalog=FlightManagement;Integrated Security=True";

        public List<VeChuyenBayTO> GetVeChuyenBayList()
        {
            string sql = "sp_LayDSVeChuyenBay";
            DataTable dt = provider.MyExecuteReader(sql, CommandType.StoredProcedure);
            List<VeChuyenBayTO> list = new List<VeChuyenBayTO>();
            foreach (DataRow row in dt.Rows)
            {
                VeChuyenBayTO ve = new VeChuyenBayTO
                {
                    maVe = Convert.ToInt32(row["maVe"]),
                    tenHK = row["tenHK"].ToString(),
                    maHD = Convert.ToInt32(row["maHD"]),
                    maCB = Convert.ToInt32(row["maCB"]),
                    maGhe = Convert.ToInt32(row["maGhe"]),
                    gia = Convert.ToDouble(row["giaVe"]),
                    
                };
                list.Add(ve);
            }
            return list;
        }

        public List<string> GetEmailsByMaCB(int maCB)
        {
            string sql = "SELECT DISTINCT nd.mail FROM VeChuyenBay v " +
                         "JOIN HoaDon hd ON v.maHD = hd.maHD " +
                         "JOIN NguoiDung nd ON hd.maND = nd.maND " +
                         "WHERE v.maCB = @maCB AND nd.mail IS NOT NULL AND LTRIM(RTRIM(nd.mail)) <> ''";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@maCB", maCB)
            };

            DataTable dt = provider.MyExecuteReader(sql, CommandType.Text, parameters);
            List<string> emails = new List<string>();
            foreach (DataRow row in dt.Rows)
            {
                string email = row["mail"].ToString();
                if (!string.IsNullOrWhiteSpace(email))
                {
                    emails.Add(email.Trim());
                }
            }

            return emails;
        }

        public string sanBayDiCuaVe(int maCB)
        {
            string sql_sb = "sp_LaySanBayDi";
            SqlParameter[] param_sb = { new SqlParameter("@maCB", maCB) };
            var sb = provider.MyExecuteScalar(sql_sb, CommandType.StoredProcedure, param_sb);
            return sb.ToString();
        }
        public string sanBayDenCuaVe(int maCB)
        {
            string sql_sb = "sp_LaySanBayDen";
            SqlParameter[] param_sb = { new SqlParameter("@maCB", maCB) };
            var sb = provider.MyExecuteScalar(sql_sb, CommandType.StoredProcedure, param_sb);
            return sb.ToString();
        }
        public VeChuyenBayTO GetThongTinVeChuyenBayDL(int maVe)
        {
            string sql = "sp_TraCuuThongTinVe";
            SqlParameter[] param = {
                new SqlParameter("@maVe", maVe)
            };
            DataTable dt = provider.MyExecuteReader(sql, CommandType.StoredProcedure, param);
            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                string sb_di = sanBayDiCuaVe(Convert.ToInt32(row["maCB"]));
                string sb_den = sanBayDenCuaVe(Convert.ToInt32(row["maCB"]));
                return new VeChuyenBayTO
                {
                    maVe = Convert.ToInt32(row["maVe"]),
                    tenHK = row["tenHK"].ToString(),
                    maHD = Convert.ToInt32(row["maHD"]),
                    ngayLapHD = Convert.ToDateTime(row["ngayLapHD"]),
                    maCB = Convert.ToInt32(row["maCB"]),
                    ngayGioDi = Convert.ToDateTime(row["ngayGioDi"]),
                    tuyenBay = row["tuyenBay"].ToString(),
                    tenGhe = row["tenGhe"].ToString(),
                    hangGhe = row["hangGhe"].ToString(),
                    gia = Convert.ToDouble(row["giaVe"]),
                    trangThai = row["trangThai"].ToString(),
                    sanBayDi = sb_di,
                    sanBayDen = sb_den
                    

                };
            }

            return null;
        }
        public int GetTongSoVe (int thang,int nam)
        {
            string sql = "sp_LaySoLuongVeTheoNamThang";
            SqlParameter[] param =
            {
                new SqlParameter("@thang",thang == 0 ? (object)DBNull.Value : thang),
                new SqlParameter("@nam", nam)
            };
            var sl = provider.MyExecuteScalar(sql, CommandType.StoredProcedure, param);
            return Convert.ToInt32(sl);
        }

        //Xóa vé
        public bool DeleteVeCB(int maVe)
        {
            string sql = "sp_XoaVeTheoMaVe";
            SqlParameter[] param = { new SqlParameter("@maVe", maVe) };
            return provider.MyExecuteNonQuery(sql, CommandType.StoredProcedure, param) > 0;
        }

        //Xóa vé theo mã chuyến bay
        public bool DeleteVeByMaCB(int maCB)
        {
            try
            {
                string sql = "sp_XoaVeCuaChuyenBay";
                SqlParameter[] param = { new SqlParameter("@maCB", maCB) };
                return provider.MyExecuteNonQuery(sql, CommandType.StoredProcedure, param) > 0;
            }
            catch (SqlException ex)
            {

                throw ex;
            }

        }

        public bool SwitchMayBayForChuyenBay(int maCB, int maMB)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                SqlTransaction tx = conn.BeginTransaction();
                try
                {
                    DataTable veTable = ExecuteDataTable(conn, tx,
                        "SELECT v.maVe FROM VeChuyenBay v WHERE v.maCB = @maCB",
                        new SqlParameter("@maCB", maCB));

                    DataTable gheTable = ExecuteDataTable(conn, tx,
                        "SELECT maGhe, hangGhe FROM Ghe WHERE maMB = @maMB",
                        new SqlParameter("@maMB", maMB));

                    if (gheTable.Rows.Count == 0)
                    {
                        throw new Exception("May bay chua co ghe.");
                    }

                    if (gheTable.Rows.Count < veTable.Rows.Count)
                    {
                        throw new Exception("Khong du ghe de doi may bay.");
                    }

                    List<int> gheMoi = new List<int>();
                    foreach (DataRow row in gheTable.Rows)
                    {
                        gheMoi.Add(Convert.ToInt32(row["maGhe"]));
                    }

                    Dictionary<int, int> veToGhe = new Dictionary<int, int>();
                    for (int i = 0; i < veTable.Rows.Count; i++)
                    {
                        int maVe = Convert.ToInt32(veTable.Rows[i]["maVe"]);
                        veToGhe[maVe] = gheMoi[i];
                    }

                    foreach (DataRow row in gheTable.Rows)
                    {
                        int maGhe = Convert.ToInt32(row["maGhe"]);
                        ExecuteNonQuery(conn, tx,
                            "IF NOT EXISTS (SELECT 1 FROM Ghe_ChuyenBay WHERE maCB = @maCB AND maGhe = @maGhe) " +
                            "INSERT INTO Ghe_ChuyenBay (maGhe, maCB, trangThai) VALUES (@maGhe, @maCB, 0)",
                            new SqlParameter("@maGhe", maGhe),
                            new SqlParameter("@maCB", maCB));
                    }

                    ExecuteNonQuery(conn, tx,
                        "UPDATE Ghe_ChuyenBay SET trangThai = 0 WHERE maCB = @maCB",
                        new SqlParameter("@maCB", maCB));

                    foreach (KeyValuePair<int, int> item in veToGhe)
                    {
                        ExecuteNonQuery(conn, tx,
                            "UPDATE VeChuyenBay SET maGhe = @maGhe WHERE maVe = @maVe",
                            new SqlParameter("@maGhe", item.Value),
                            new SqlParameter("@maVe", item.Key));

                        ExecuteNonQuery(conn, tx,
                            "UPDATE Ghe_ChuyenBay SET trangThai = 1 WHERE maCB = @maCB AND maGhe = @maGhe",
                            new SqlParameter("@maCB", maCB),
                            new SqlParameter("@maGhe", item.Value));
                    }

                    ExecuteNonQuery(conn, tx,
                        "DELETE FROM Ghe_ChuyenBay WHERE maCB = @maCB AND maGhe IN (SELECT maGhe FROM Ghe WHERE maMB <> @maMB)",
                        new SqlParameter("@maCB", maCB),
                        new SqlParameter("@maMB", maMB));

                    tx.Commit();
                    return true;
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        private static DataTable ExecuteDataTable(SqlConnection conn, SqlTransaction tx, string sql, params SqlParameter[] parameters)
        {
            using (SqlCommand cmd = new SqlCommand(sql, conn, tx))
            {
                cmd.CommandType = CommandType.Text;
                if (parameters != null && parameters.Length > 0)
                {
                    cmd.Parameters.AddRange(parameters);
                }

                DataTable dt = new DataTable();
                dt.Load(cmd.ExecuteReader());
                return dt;
            }
        }

        private static int ExecuteNonQuery(SqlConnection conn, SqlTransaction tx, string sql, params SqlParameter[] parameters)
        {
            using (SqlCommand cmd = new SqlCommand(sql, conn, tx))
            {
                cmd.CommandType = CommandType.Text;
                if (parameters != null && parameters.Length > 0)
                {
                    cmd.Parameters.AddRange(parameters);
                }

                return cmd.ExecuteNonQuery();
            }
        }

    }
}
