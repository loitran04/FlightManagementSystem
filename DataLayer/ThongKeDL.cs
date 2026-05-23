using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransferObject;

namespace DataLayer
{
    public class ThongKeDL
    {
        private readonly DataProvider provider = new DataProvider();

        public double TongDoanhThu(DataTable dt)
        {
            if (dt == null)
                throw new ArgumentNullException(nameof(dt));

            double Tong = 0;
            foreach (DataRow row in dt.Rows)
            {
                if (row["doanhThu"] != DBNull.Value && row["doanhThu"] != null)
                {
                    double value;
                    if (double.TryParse(row["doanhThu"].ToString(), out value))
                        Tong += value;
                }
            }
            return Tong;
        }

        public DataTable GetThongKeTheoThangNam(int thang, int nam)
        {
            string sql = "sp_ThongKeTheoThangNam";
            SqlParameter[] param = {
                new SqlParameter("@thang", thang == 0 ? (object)DBNull.Value : (object)thang),
                new SqlParameter("@nam", nam)
            };
            DataTable dt = provider.MyExecuteReader(sql, CommandType.StoredProcedure, param);
            if (dt == null)
                return new DataTable();
            return dt;

        }
    }
}
