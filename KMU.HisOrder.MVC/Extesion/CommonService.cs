using KMU.HisOrder.MVC.Models;
using System;

namespace KMU.HisOrder.MVC.Extesion
{
    public class CommonService:IDisposable
    {
        private readonly KMUContext _context;


        public CommonService(KMUContext context)
        {
            _context = context;
        }

        public void Dispose() => Dispose(disposing: true);

        /// <summary>
        /// Releases all resources currently used by this <see cref="Controller"/> instance.
        /// </summary>
        /// <param name="disposing"><c>true</c> if this method is being invoked by the <see cref="Dispose()"/> method,
        /// otherwise <c>false</c>.</param>
        protected virtual void Dispose(bool disposing)
        {
        }

        public List<KmuCoderef> GetCodeRef(string CodeType)
        {
            #region Variable Setting
            
            List<KmuCoderef> coderefs = new List<KmuCoderef>();
            
            #endregion
            try
            {
                coderefs = _context.KmuCoderefs.Where(c => c.RefCodetype == CodeType).ToList();
            }
            catch(Exception ex)
            {

            }

            return coderefs;
        }

        public string GetEmpName(string UserID)
        {
            #region Variable Setting

            string UserName = "";
            List<KmuUser> kmuUsers = new List<KmuUser>();
            #endregion

            try
            {
                kmuUsers = _context.KmuUsers.Where(c => c.UserIdno == UserID).ToList();
                if (kmuUsers.Any())
                {
                    UserName = kmuUsers.FirstOrDefault().UserNameFirstname + " " +
                               kmuUsers.FirstOrDefault().UserNameMidname + " " +
                               kmuUsers.FirstOrDefault().UserNameLastname;
                }
            }
            catch (Exception ex)
            {

            }

            return UserName;
        }

        public List<EmployeeList> GetEmpList(string[] Type = null)
        {
            #region Variable Setting

            List<EmployeeList> empList = new List<EmployeeList>();
            List<KmuUser> kmuUsers = new List<KmuUser>();
            #endregion

            try
            {
                kmuUsers = _context.KmuUsers.ToList();

                //if (Type != null)
                //{
                //    foreach (var type in Type)
                //    {
                //        users = users.Where(c=>c.)
                //    }
                //}                

                foreach (var user in kmuUsers)
                {
                    EmployeeList emp = new EmployeeList();
                    emp.UserID = user.UserIdno;
                    emp.UserName = user.UserNameFirstname + " " + user.UserNameMidname + " " + user.UserNameLastname;
                    emp.Category = user.UserCategory;
                    empList.Add(emp);
                }
            }
            catch (Exception ex)
            {

            }

            return empList;
        }

        public string GetDepartmentName(string DptCode)
        {
            #region Variable Setting

            string DptName = "";
            List<KmuDepartment> KmuDepartments = new List<KmuDepartment>();
            #endregion

            try
            {
                KmuDepartments = _context.KmuDepartments.Where(c => c.DptCode == DptCode).ToList();
                if (KmuDepartments.Any())
                {
                    DptName = KmuDepartments.FirstOrDefault().DptName;
                }
            }
            catch (Exception ex)
            {

            }

            return DptName;
        }

        public List<DepartmentList> GetDptList(string Category)
        {
            #region Variable Setting

            List<DepartmentList> DptList = new List<DepartmentList>();
            List<KmuDepartment> kmuDepartments = new List<KmuDepartment>();
            #endregion

            try
            {
                //2023.05.02 若Category無指定, 則讀出全部資料 update by elain.
                kmuDepartments = _context.KmuDepartments.Where(c => c.DptDepth == 2 && c.DptStatus == "Y" &&
                                 c.DptCategory == (string.IsNullOrWhiteSpace(Category) ? c.DptCategory : Category)).ToList();
 
                foreach (var department in kmuDepartments)
                {
                    DepartmentList dpt = new DepartmentList();
                    dpt.DptCode = department.DptCode;
                    dpt.DptName = department.DptName;
                    dpt.DptCategory = department.DptCategory;
                    DptList.Add(dpt);
                }
            }
            catch
            {

            }

            return DptList;
        }


        /// <summary>
        /// MAP_INFO 運算式比較
        /// </summary>
        /// <param name="CompareValue1"></param>
        /// <param name="CompareValue2"></param>
        /// <param name="OperatorType"></param>
        /// <returns></returns>
        public bool MAPINFO_Compare(string CompareValue1, string CompareValue2, string OperatorType)
        {
            double CPV1 = Convert.ToDouble(CompareValue1);
            double CPV2 = Convert.ToDouble(CompareValue2);
            switch (OperatorType.Trim())
            {
                case ">=":
                    if (CPV1 >= CPV2)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case "<=":
                    if (CPV1 <= CPV2)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case "=":
                    if (CPV1 == CPV2)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case ">":
                    if (CPV1 > CPV2)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case "<":
                    if (CPV1 < CPV2)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case "<>":
                    if (CPV1 != CPV2)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case "":
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// MAP_INFO 運算式比較
        /// </summary>
        /// <param name="CompareValue1"></param>
        /// <param name="CompareValue2"></param>
        /// <param name="OperatorType"></param>
        /// <returns></returns>
        public bool MAPINFO_CompareForString(string CompareValue1, string CompareValue2, string OperatorType)
        {
            switch (OperatorType.Trim())
            {
                case ">=":
                    if (string.Compare(CompareValue1, CompareValue2) > 0 || string.Compare(CompareValue1, CompareValue2) == 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case "<=":
                    if (string.Compare(CompareValue1, CompareValue2) < 0 || string.Compare(CompareValue1, CompareValue2) == 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case "=":
                    if (string.Compare(CompareValue1, CompareValue2) == 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case ">":
                    if (string.Compare(CompareValue1, CompareValue2) > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case "<":
                    if (string.Compare(CompareValue1, CompareValue2) < 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case "<>":
                    if (string.Compare(CompareValue1, CompareValue2) != 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case "":
                    return true;
                default:
                    return false;
            }
        }
    }
}
