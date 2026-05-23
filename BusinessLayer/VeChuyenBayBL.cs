using DataLayer;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransferObject;

namespace BusinessLayer
{
    public class VeChuyenBayBL
    {
        private VeChuyenBayDL veCBDL = new VeChuyenBayDL();

        public List<VeChuyenBayTO> GetVeChuyenBayList()
        {
            return veCBDL.GetVeChuyenBayList();
        }

        public List<string> GetEmailsByMaCB(int maCB)
        {
            return veCBDL.GetEmailsByMaCB(maCB);
        }
        public VeChuyenBayTO GetThongTinVeChuyenBayBL(int maVe)
        {
            return veCBDL.GetThongTinVeChuyenBayDL(maVe);
        }

        public int GetTongSoVe (int thang, int nam)
        {
            return veCBDL.GetTongSoVe(thang, nam);
        }

        public bool DeleteVeCB(int maVe)
        {
            return veCBDL.DeleteVeCB(maVe);
        }

        public bool DeleteVeByMaCB(int maCB)
        {
            try
            {
                return veCBDL.DeleteVeByMaCB(maCB);
            }
            catch (SqlException ex)
            {

                throw ex;
            }
        }

        public bool SwitchMayBayForChuyenBay(int maCB, int maMB)
        {
            try
            {
                return veCBDL.SwitchMayBayForChuyenBay(maCB, maMB);
            }
            catch (SqlException ex)
            {
                throw ex;
            }
        }
    }
}
