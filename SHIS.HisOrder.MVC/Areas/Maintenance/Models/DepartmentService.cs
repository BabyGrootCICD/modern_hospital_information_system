using SHIS.HisOrder.MVC.Models;

namespace SHIS.HisOrder.MVC.Areas.Maintenance.Models
{
    public class DepartmentService : IDisposable
    {
        private readonly SHISContext _context;


        public DepartmentService(SHISContext context)
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

        public string getDefaultAttribute(string DptCode)
        {
            string Attr = "000";
            List<ShisDepartment> departmentList = new List<ShisDepartment>();

            try
            {
                departmentList = _context.ShisDepartments.Where(c => c.DptCode == DptCode).ToList();
                if (departmentList.Any())
                {
                    Attr = departmentList.FirstOrDefault().DptDefaultAttr;
                }
            }
            catch (Exception ex)
            {

            }

            return Attr;

        }

        public string getDeptName(string DptCode)
        {
            string DptName = "";
            try
            {
                var Dtos = _context.ShisDepartments.Where(c => c.DptCode == DptCode);
                if (Dtos.Any())
                {
                    DptName = Dtos.FirstOrDefault().DptName;
                }
            }
            catch (Exception ex)
            {

            }

            return DptName;
        }


        public string getParentDpt(string DptCode)
        {
            string parentDpt = "";
            List<ShisDepartment> departmentList = new List<ShisDepartment>();

            try
            {
                departmentList = _context.ShisDepartments.Where(c => c.DptCode == DptCode).ToList();
                if (departmentList.Any())
                {
                    parentDpt = departmentList.FirstOrDefault().DptParent;
                }
            }
            catch (Exception ex)
            {

            }

            return parentDpt;

        }

        public string getCategory(string DptCode)
        {
            string category = "";
            List<ShisDepartment> departmentList = new List<ShisDepartment>();

            try
            {
                departmentList = _context.ShisDepartments.Where(c => c.DptCode == DptCode).ToList();
                if (departmentList.Any())
                {
                    category = departmentList.FirstOrDefault().DptCategory;
                }
            }
            catch (Exception ex)
            {

            }

            return category;
        }
    }
}
