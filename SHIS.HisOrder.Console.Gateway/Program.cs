
////if (args.Length > 0)
////{
////    foreach (var arg in args)
////    {
////        Console.WriteLine($"Argument={arg}");
////    }
////}
////else
////{
////    Console.WriteLine("No arguments");
////}


using SHIS.HisOrder.Console.Gateway.Models;
using Microsoft.VisualBasic;
using System;
using System.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Numerics;
using Newtonsoft.Json;
using System.Reflection.Emit;
using System.Reflection;
using System.Net.Http.Json;
using System.Text;
using NLog;
using NLog.Config;
using System.Text.Json.Nodes;
using System.Net.NetworkInformation;
using NLog.Fluent;
using Microsoft.EntityFrameworkCore;
using Npgsql.PostgresTypes;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using SHIS.HisOrder.Console.Gateway.Models_MOHD;
using Microsoft.Extensions.Configuration;

class Program
{
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private static string localIP;
    private static string userChoice;
    private static string jwtToken = "";
    private static string hosp_code = "";
    private static string version = "";
    private static ExportData exportData = new ExportData();
    private static List<Root> roots = new List<Root>();
    private static List<PatientInfo> patientInfos = new List<PatientInfo>();
    private static List<MergePatient> mergePatients = new List<MergePatient>();
    private static List<string> summary_errorMsg = new List<string>();
    public static IConfiguration _configuration;
    static async Task Main(string[] args)
    {
        // 初始化 NLog 配置 
        LogManager.Configuration = new XmlLoggingConfiguration("NLog/nlog.config");
        // 初始化連線字串
        string test = Directory.GetCurrentDirectory();
        _configuration = new ConfigurationBuilder().
               SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
              .AddJsonFile("appsettings.json")
              .Build();


        Console.WriteLine(" _    _   _____    _____         _____            _______  ______ __          __          __     __\r\n| |  | | |_   _|  / ____|       / ____|    /\\    |__   __||  ____|\\ \\        / /    /\\    \\ \\   / /\r\n| |__| |   | |   | (___        | |  __    /  \\      | |   | |__    \\ \\  /\\  / /    /  \\    \\ \\_/ /\r\n|  __  |   | |    \\___ \\       | | |_ |  / /\\ \\     | |   |  __|    \\ \\/  \\/ /    / /\\ \\    \\   /\r\n| |  | |  _| |_   ____) |      | |__| | / ____ \\    | |   | |____    \\  /\\  /    / ____ \\    | |\r\n|_|  |_| |_____| |_____/        \\_____|/_/    \\_\\   |_|   |______|    \\/  \\/    /_/    \\_\\   |_|\r\n");

        try
        {
            using (var context = new GatewayContext())
            {
                //院區
                var hospdata = context.ShisCoderefs.Where(c => c.RefCodetype == "HospitalCode").FirstOrDefault();
                if (hospdata != null && !string.IsNullOrWhiteSpace(hospdata.RefCode)) { hosp_code = hospdata.RefCode; }
                localIP = GetLocalIPAddress();
            }
        }
        catch (Exception ex)
        {
            hosp_code = "unknown";
            localIP = GetLocalIPAddress();
        }


        Version version = Assembly.GetEntryAssembly().GetName().Version;
        Console.WriteLine("〔Version〕: " + version + " | " + "〔Hospital〕: " + hosp_code + " | " + "〔IP〕: " + localIP + " | " + "〔Login〕: " + "Admin" + " | " + "\r\n");


        Console.WriteLine("  ");


    start:
        if (hosp_code != "unknown")
        {
            Console.WriteLine("Welcome to HIS GATEWAY! ");
            Console.WriteLine("Please enter numbers for respective functions: \r\n");
            Console.WriteLine(" 〔0〕 One-click upload.");
            Console.WriteLine(" 〔1〕 Daily outpatient and emergency medical record upload task.");
            Console.WriteLine(" 〔2〕 Patient basic information upload task.");
            Console.WriteLine(" 〔3〕 Medical records merge upload task.");
            Console.WriteLine(" 〔4〕 Offline file export operation.");
            Console.WriteLine(" 〔5〕 Offline file import operation.  ");
            do
            {

                Console.Write(" \r\n Select a function option ( 0 , 1 , 2 , 3 , etc...): ");
                if (args == null || args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
                {
                    userChoice = Console.ReadLine();
                }
                else
                {
                    userChoice = args[0];
                    Console.WriteLine(args[0]);
                    args = null;
                }

                switch (userChoice)
                {
                    case "0":
                    restart_0:
                        logger.Warn("You chose to 〔0〕 One-click upload.");
                        var t1 = upload_daily_medical_record(ConnectionStatus.Online);
                        if (t1.Result == "Q")
                        {
                            goto start;
                        }
                        else if (t1.Result == "00")
                        {
                            logger.Warn(" Task finished! ");
                        }
                        else if (t1.Result == "999" || t1.Result == "R")
                        {
                            logger.Warn(" Task exception! ");
                            await StartCountdown();
                            goto restart_0;
                        }

                        var t2 = upload_patient_basic_info(ConnectionStatus.Online);
                        if (t2.Result == "Q")
                        {
                            goto start;
                        }
                        else if (t2.Result == "00")
                        {
                            logger.Warn(" Task finished! ");
                        }
                        else if (t2.Result == "999" || t2.Result == "R")
                        {
                            logger.Warn(" Task exception! ");
                            await StartCountdown();
                            goto restart_0;
                        }
                        var t3 = upload_merge_patient();
                        if (t3.Result == "Q")
                        {
                            goto start;
                        }
                        else if (t3.Result == "00")
                        {
                            logger.Warn(" Task finished! ");
                        }
                        else if (t3.Result == "999" || t3.Result == "R")
                        {
                            logger.Warn(" Task exception! ");
                            await StartCountdown();
                            goto restart_0;
                        }
                        goto Exit;


                    case "1":
                    restart_1:
                        logger.Warn("You chose to 〔1〕 Daily outpatient and emergency medical record upload task.");
                        // Perform relevant operations here
                        var tk = upload_daily_medical_record(ConnectionStatus.Online);
                        if (tk.Result == "Q")
                        {
                            goto start;
                        }
                        else if (tk.Result == "00")
                        {
                            logger.Warn(" Task finished! ");
                        }
                        else if (tk.Result == "999" || tk.Result == "R")
                        {
                            logger.Warn(" Task exception! ");

                            await StartCountdown();

                            goto restart_1;
                        }
                        goto start;
                    case "2":
                    restart_2:
                        logger.Warn("You chose to 〔2〕 Patient basic information upload task.");
                        // Perform relevant operations here
                        var tk_2 = upload_patient_basic_info();
                        if (tk_2.Result == "Q")
                        {
                            goto start;
                        }
                        else if (tk_2.Result == "00")
                        {
                            logger.Warn(" Task finished! ");
                        }
                        else if (tk_2.Result == "999" || tk_2.Result == "R")
                        {
                            logger.Warn(" Task exception! ");

                            await StartCountdown();

                            goto restart_2;
                        }
                        goto start;
                    case "3":
                    restart_3:
                        logger.Warn("You chose to 〔3〕 Medical records merge upload task.");
                        // Perform relevant operations here
                        var tk_3 = upload_merge_patient();
                        if (tk_3.Result == "Q")
                        {
                            goto start;
                        }
                        else if (tk_3.Result == "00")
                        {
                            logger.Warn(" Task finished! ");
                        }
                        else if (tk_3.Result == "999" || tk_3.Result == "R")
                        {
                            logger.Warn(" Task exception! ");

                            await StartCountdown();

                            goto restart_3;
                        }
                        goto start;

                    case "4":
                    restart_4:
                        logger.Warn("You chose to 〔4〕 Offline file export operation.");
                        // Perform relevant operations here
                        var tk_4 = export_Files_For_Offline_Use();
                        if (tk_4.Result == "Q")
                        {
                            goto start;
                        }
                        else if (tk_4.Result == "00")
                        {
                            logger.Warn(" Task finished! ");
                        }
                        else if (tk_4.Result == "999" || tk_4.Result == "R")
                        {
                            logger.Warn(" Task exception! ");

                            await StartCountdown();

                            goto restart_4;
                        }
                        goto start;


                    case "5":
                    restart_5:
                        logger.Warn("You chose to 〔5〕 Offline file import operation.");
                        // Perform relevant operations here
                        var tk_5 = import_Files_For_Offline_Use();
                        if (tk_5.Result == "Q")
                        {
                            goto start;
                        }
                        else if (tk_5.Result == "00")
                        {
                            logger.Warn(" Task finished! ");
                        }
                        else if (tk_5.Result == "999" || tk_5.Result == "R")
                        {
                            logger.Warn(" Task exception! ");

                            await StartCountdown();

                            goto restart_5;
                        }
                        goto start;

                    default:
                        if (args != null && args.Length > 0) args[0] = "";
                        logger.Warn("Invalid option. Please enter 1, 2, or 3 as the option.");
                        goto start;
                }
            } while (userChoice != "1" && userChoice != "2" && userChoice != ""); //日後擴充
        }
        else
        {
            Console.WriteLine("Welcome to HIS GATEWAY! ");
            Console.WriteLine("Please enter numbers for respective functions: \r\n");
            Console.WriteLine(" 〔5〕 Offline file import operation.");

            do
            {

                Console.Write("\r\nSelect a function option : ");
                if (args == null || args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
                {
                    userChoice = Console.ReadLine();
                }
                else
                {
                    userChoice = args[0];
                    Console.WriteLine(args[0]);
                    args = null;
                }

                switch (userChoice)
                {
                    case "5":
                    restart_offline_5:
                        logger.Warn("You chose to 〔5〕 Offline file import operation.");
                        // Perform relevant operations here
                        var tk_5 = import_Files_For_Offline_Use();
                        if (tk_5.Result == "Q")
                        {
                            goto start;
                        }
                        else if (tk_5.Result == "00")
                        {
                            logger.Warn(" Task finished! ");
                        }
                        else if (tk_5.Result == "999" || tk_5.Result == "R")
                        {
                            logger.Warn(" Task exception! ");

                            await StartCountdown();

                            goto restart_offline_5;
                        }
                        goto start;

                    default:
                        if (args != null && args.Length > 0) args[0] = "";
                        logger.Warn("Invalid option. Please enter 5 as the option.");
                        goto start;
                }
            } while (userChoice != "5" && userChoice != "");

        }

    Exit:
        Environment.Exit(0); // 0 表示成功退出
    }
    private static bool isPaused = false;

    static async Task StartCountdown()
    {
        for (int i = 5; i > 0; i--)
        {
            Console.WriteLine($"Executing again in {i} seconds...");
            await Task.Delay(1000); // wait 1 sec
        }
    }

    static async Task<string> upload_daily_medical_record(ConnectionStatus inConnSatus = ConnectionStatus.Online)
    {
        try
        {
            //2024.01.10 update by 1050325 offline work
            var _inConnSatus = inConnSatus;
            //var _inFilePath = inFilePath;


            using (var context = new GatewayContext())
            {
                if (string.IsNullOrWhiteSpace(hosp_code))
                {
                    var hospdata = context.ShisCoderefs.Where(c => c.RefCodetype == "HospitalCode").FirstOrDefault();
                    if (hospdata != null &&
                        !string.IsNullOrWhiteSpace(hospdata.RefCode))
                    {
                        hosp_code = hospdata.RefCode;
                    }
                    else
                    {
                        throw new ArgumentNullException(nameof(upload_daily_medical_record), "hosp_code cannot be null.");
                    }
                }

                //搜尋需要上傳的Case
                var uploadCase = context.
                    Registrations
                    .Where(d => (d.RegStatus == "*" || d.RegStatus == "O" || d.RegStatus == "T") && d.UploadStatus == "N")
                    .OrderBy(d => d.Inhospid)
                    .ToList();

                var duplicateInhospids = uploadCase
                    .GroupBy(d => d.Inhospid)
                    .Where(g => g.Count() >= 2)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicateInhospids.Any())
                {
                    duplicateInhospids.ForEach(c =>
                    {
                        using (var context_du = new GatewayContext())
                        {
                            logger.Fatal($"Duplicate inhospid: {c}. Upload action cannot be performed.");
                            ShisUploadLog uploadlog = new ShisUploadLog();
                            uploadlog.Logid = -1;
                            uploadlog.RegDate = null;
                            uploadlog.Option = userChoice != null ? Convert.ToChar(userChoice) : ' ';
                            uploadlog.Inhospid = c;
                            uploadlog.ExecDatetime = DateTime.Now;
                            uploadlog.LocalIp = GetLocalIPAddress();
                            uploadlog.LocalLoginUser = "Admin";
                            uploadlog.TargetAgency = "MOHD";
                            uploadlog.TargetUrl = null;
                            uploadlog.ResultSuccess = false;
                            uploadlog.ResultMessage = $"Duplicate inhospid: {c}. Upload action cannot be performed.";
                            uploadlog.ResultStatusCode = "999";
                            uploadlog.ResultStatusDesc = "Duplicate";

                            context_du.ShisUploadLogs.Add(uploadlog);
                            context_du.SaveChanges();

                            uploadCase.RemoveAll(d => d.Inhospid == c);
                        }
                    });
                }




                if (uploadCase.Any())
                {
                    logger.Info($"This Task is expected to upload {uploadCase.Count} medical records.");
                    foreach (var item in uploadCase)
                    {
                        using (var context_sub = new GatewayContext())
                        {

                            // 監聽使用者輸入
                            while (true)
                            {
                                if (Console.KeyAvailable)
                                {
                                    var key = Console.ReadKey(intercept: true).Key;
                                    if (key == ConsoleKey.P)
                                    {
                                        isPaused = !isPaused; // 暫停
                                        Console.WriteLine(isPaused ? "\nExecution paused. Press 'P' to resume..." : "\nResuming execution...");
                                    }
                                    else if (key == ConsoleKey.Q)
                                    {
                                        Console.WriteLine("\nExecution halted by user.");
                                        isPaused = false;
                                        return "Q";
                                    }
                                    else if (key == ConsoleKey.Escape)
                                    {
                                        isPaused = false;
                                        return "Esc";
                                    }
                                }

                                if (!isPaused)
                                {
                                    break; // 繼續執行
                                }
                            }

                            #region 必要資訊
                            var pInfo = context_sub.ShisCharts.Where(c => c.ChrHealthId == item.RegHealthId).FirstOrDefault();
                            if (pInfo == null)
                            {
                                using (var _errorContext = new GatewayContext())
                                {
                                    logger.Fatal($"shis_chart No patient found: {item.RegHealthId}");
                                    ShisUploadLog _uploadlog = new ShisUploadLog();
                                    _uploadlog.Logid = -1;
                                    _uploadlog.RegDate = null;
                                    _uploadlog.Option = userChoice != null ? Convert.ToChar(userChoice) : ' ';
                                    _uploadlog.Inhospid = item.RegHealthId;
                                    _uploadlog.ExecDatetime = DateTime.Now;
                                    _uploadlog.LocalIp = GetLocalIPAddress();
                                    _uploadlog.LocalLoginUser = "Admin";
                                    _uploadlog.TargetAgency = "MOHD";
                                    _uploadlog.TargetUrl = null;
                                    _uploadlog.ResultSuccess = false;
                                    _uploadlog.ResultMessage = "shis_chart No patient found: " + item.RegHealthId;
                                    _uploadlog.ResultStatusCode = "999";
                                    _uploadlog.ResultStatusDesc = "Exception";

                                    _errorContext.ShisUploadLogs.Add(_uploadlog);
                                    _errorContext.SaveChanges();
                                }

                                continue;
                            }



                            var rInfo = item;
                            var soapInfo = context_sub.Hisordersoas.Where(c => c.Inhospid == item.Inhospid && c.Status == 'V').ToList();
                            var dxInfo = context_sub.Hisorderplans.Where(c => c.Inhospid == item.Inhospid && c.HplanType == "ICD" && c.DcDate == null).ToList();
                            var medInfo = context_sub.Hisorderplans.Where(c => c.Inhospid == item.Inhospid && c.HplanType == "Med" && c.DcDate == null).ToList();
                            var othersInfo = context_sub.Hisorderplans.Where(c => c.Inhospid == item.Inhospid && c.HplanType != "Med" && c.HplanType != "ICD" && c.DcDate == null).ToList();
                            var orderPlanInfo = context_sub.Hisorderplans.Where(c => c.Inhospid == item.Inhospid && c.DcDate == null).ToList();
                            #endregion

                            var root = new Root();
                            root.HospInfo = new HospInfo()
                            {
                                HospCode = hosp_code,
                                HospName = "",
                                HospAddress = "",
                                HospTel = ""
                            };
                            root.SenderInfo = new SenderInfo()
                            {
                                IP = "",
                                Name = ""
                            };
                            var regdptparent = context_sub.ShisDepartments.Where(d => d.DptCode == rInfo.RegDepartment).Select(d => d.DptParent).SingleOrDefault();
                            root.RegInfo = new RegInfo()
                            {
                                HospCode = hosp_code,
                                RegDate = rInfo.RegDate,
                                DeptCode = rInfo.RegDepartment,
                                DeptName = context_sub.ShisDepartments.Where(d => d.DptCode == rInfo.RegDepartment).Select(e => e.DptName).FirstOrDefault(),
                                Noon = rInfo.RegNoon,
                                SeqNo = rInfo.RegSeqNo,
                                HealthId = rInfo.RegHealthId,
                                Inhospid = rInfo.Inhospid,
                                Triage = rInfo.RegTriage,
                                BedNo = rInfo.RegBedNo,
                                RegAttribute = rInfo.RegAttribute,
                                AttrDesc = rInfo.RegAttrDesc,
                                DoctorId = rInfo.RegDoctorId,
                                DoctorName = context_sub.ShisUsers.Where(c => c.UserIdno == rInfo.RegDoctorId).Select(d => d.UserNameFirstname + d.UserNameMidname + d.UserNameLastname).FirstOrDefault(),
                                RoomNo = rInfo.RegRoomNo,
                                Status = rInfo.RegStatus,
                                CallTime = rInfo.RegCallTime,
                                StartTime = rInfo.RegStartTime,
                                EndTime = rInfo.RegEndTime,
                                ExamStartTime = rInfo.RegExamStartTime,
                                ExamEndTime = rInfo.RegExamEndTime,
                                CreateTime = rInfo.RegCreateTime,
                                FollowCode = rInfo.RegFollowCode,
                                FollowDesc = rInfo.RegFollowDesc,
                                dept_parent = context_sub.ShisDepartments.Where(d => d.DptCode == rInfo.RegDepartment).Select(d => d.DptParent).SingleOrDefault(),
                                dept_parent_name = context.ShisDepartments.Where(d => d.DptCode == regdptparent).Select(r => r.DptName).SingleOrDefault(),


                            };
                            root.PatientInfo = new PatientInfo()
                            {
                                FromHosp = hosp_code,
                                HealthId = pInfo.ChrHealthId,
                                NationalId = pInfo.ChrNationalId,
                                FName = pInfo.ChrPatientFirstname,
                                MName = pInfo.ChrPatientMidname,
                                LName = pInfo.ChrPatientLastname,
                                Sex = pInfo.ChrSex,
                                BirthDate = pInfo.ChrBirthDate,
                                Mobile = pInfo.ChrMobilePhone,
                                Address = pInfo.ChrAddress,
                                EmgCont = pInfo.ChrEmgContact,
                                ContRel = pInfo.ChrContactRelation,
                                ContPhone = pInfo.ChrContactPhone,
                                CombineFlag = pInfo.ChrCombineFlag,
                                Remark = pInfo.ChrRemark,
                                ModifyTime = pInfo.ModifyTime,
                                RefugeeFlag = pInfo.ChrRefugeeFlag
                            };
                            root.SOAP = new List<SOAP>() { };
                            root.Med = new List<OrderPlan> { };
                            root.Dx = new List<OrderPlan> { };
                            root.Others = new List<OrderPlan> { };


                            if (soapInfo != null && soapInfo.Count > 0)
                            {
                                foreach (Hisordersoa _soap in soapInfo)
                                {
                                    root.SOAP.Add(new SOAP()
                                    {
                                        Soaid = _soap.Soaid,
                                        Inhospid = _soap.Inhospid,
                                        HealthId = _soap.HealthId,
                                        Kind = _soap.Kind,
                                        Context = _soap.Context,
                                        CreateTime = _soap.CreateDate,
                                        ModifyTime = _soap.ModifyDate,
                                        SourceType = _soap.SourceType,
                                        VersionCode = _soap.VersionCode,
                                        Status = _soap.Status,
                                        HospCode = hosp_code
                                    });
                                }
                            }
                            if (orderPlanInfo != null && orderPlanInfo.Count > 0)
                            {
                                foreach (Hisorderplan _plan in orderPlanInfo)
                                {
                                    var orderplan = new OrderPlan()
                                    {
                                        HospCode = hosp_code,
                                        Orderplanid = _plan.Orderplanid,
                                        Inhospid = _plan.Inhospid,
                                        HealthId = _plan.HealthId,
                                        HplanType = _plan.HplanType,
                                        SeqNo = _plan.SeqNo,
                                        PlanCode = _plan.PlanCode,
                                        PlanDes = _plan.PlanDes,
                                        FreeCharge = _plan.FreeCharge,
                                        ExecDateFrom = _plan.ExecDateFrom,
                                        ExecDateTo = _plan.ExecDateTo,
                                        OrderDept = _plan.OrderDept,
                                        OrderDr = _plan.OrderDr,
                                        PlanDays = _plan.PlanDays,
                                        QtyDose = _plan.QtyDose,
                                        QtyDaily = _plan.QtyDaily,
                                        UnitDose = _plan.UnitDose,
                                        FreqCode = _plan.FreqCode,
                                        DoseIndi = _plan.DoseIndication,
                                        DosePath = _plan.DosePath,
                                        MadeType = _plan.MadeType,
                                        TotalQty = _plan.TotalQty,
                                        ExamLoc = _plan.ExamLoc,
                                        UrgFlag = _plan.UrgFlag,
                                        PreopFlag = _plan.PreopFlag,
                                        AddFlag = _plan.AddFlag,
                                        KeepspecFlag = _plan.KeepspecFlag,
                                        LocationCode = _plan.LocationCode,
                                        TriggerTablecode = _plan.TriggerTablecode,
                                        TriggerRecid = _plan.TriggerRecid,
                                        Status = _plan.Status,
                                        ExecStatus = _plan.ExecStatus,
                                        ChargeStatus = _plan.ChargeStatus,
                                        CreateDate = _plan.CreateDate,
                                        ModifyDate = _plan.ModifyDate,
                                        Remark = _plan.Remark,
                                        MedBag = _plan.MedBag

                                    };

                                    switch (orderplan.HplanType)
                                    {
                                        case "ICD":
                                            root.Dx.Add(orderplan);
                                            break;
                                        case "Med":
                                            root.Med.Add(orderplan);
                                            break;
                                        default:
                                            root.Others.Add(orderplan);
                                            break;
                                    }
                                }
                            }



                            if (_inConnSatus == ConnectionStatus.Online)
                            {

                                #region authentication token
                                if (string.IsNullOrWhiteSpace(jwtToken))
                                {
                                    var result = await GetNewJwtToken();
                                    if (result.isSuccess == true)
                                    {
                                        jwtToken = result.returnValue;
                                    }
                                    else
                                    {
                                        return "R";
                                    }
                                }
                                #endregion
                                #region call web api 
                                //string apiUrl = "https://172.18.8.188/api/UploadTask/MOHD_TK_01"; // 你的目标 Web API 地址
                                //string apiUrl = "https://localhost:7287/api/UploadTask/MOHD_TK_01"; // 你的目标 Web API 地址

                                string apiUrl = context.ShisCoderefs
                                    .Where(c => c.RefCodetype == "MOHD_TK" && c.RefCode == "TK_01").Select(c => c.RefName).FirstOrDefault();

                                if (string.IsNullOrWhiteSpace(apiUrl)) { throw new ArgumentNullException(nameof(upload_daily_medical_record), "api Url cannot be null."); }

                                ShisUploadLog uploadlog = new ShisUploadLog();
                                uploadlog.Logid = -1;
                                uploadlog.RegDate = rInfo.RegDate;
                                uploadlog.Option = userChoice != null ? Convert.ToChar(userChoice) : ' ';
                                uploadlog.Inhospid = rInfo.Inhospid;
                                uploadlog.ExecDatetime = DateTime.Now;
                                uploadlog.LocalIp = GetLocalIPAddress();
                                uploadlog.LocalLoginUser = "Admin";
                                uploadlog.TargetAgency = "MOHD";
                                uploadlog.TargetUrl = apiUrl;

                                try
                                {

                                    var handler = new HttpClientHandler
                                    {
                                        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                                    };

                                    var json = JsonConvert.SerializeObject(root);
                                    var content = new StringContent(json, Encoding.UTF8, "application/json");


                                    var httpClient = new HttpClient(handler);
                                    httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwtToken);



                                    // 发送POST请求到Web API
                                    var response = await httpClient.PostAsync(apiUrl, content);

                                    // 检查响应是否成功
                                    if (response.IsSuccessStatusCode)
                                    {
                                        // 从响应中获取内容
                                        string responseContent = await response.Content.ReadAsStringAsync();
                                        ResultDTO result = JsonConvert.DeserializeObject<ResultDTO>(responseContent);


                                        uploadlog.ResultSuccess = result != null ? result.isSuccess : false;
                                        uploadlog.ResultMessage = result.Message;
                                        uploadlog.ResultStatusCode = ((int)response.StatusCode).ToString();
                                        uploadlog.ResultStatusDesc = response.StatusCode.ToString();
                                        if (result.isSuccess == true)
                                        {
                                            //logger.Info(responseContent);

                                            logger.Info(
                                                $"request status code: {response.StatusCode}({(int)(response.StatusCode)}) | inhospID: " + root.RegInfo.Inhospid);

                                            //update registration、shischart
                                            pInfo.UploadStatus = "Y";
                                            pInfo.UploadTime = DateTime.Now;
                                            rInfo.UploadStatus = "Y";
                                            rInfo.UploadTime = DateTime.Now;
                                            context_sub.ShisCharts.Update(pInfo);
                                            context_sub.Registrations.Update(rInfo);
                                        }
                                        else
                                        {
                                            //logger.Error(responseContent);
                                            logger.Info(
                                                $"request status code: {response.StatusCode}({(int)(response.StatusCode)}) | inhospID: {root.RegInfo.Inhospid}");
                                        }

                                    }
                                    else
                                    {
                                        logger.Fatal(
                                            $"request fail，status code: {response.StatusCode}({(int)(response.StatusCode)}) | inhospID: {root.RegInfo.Inhospid}");

                                        uploadlog.ResultSuccess = false;
                                        uploadlog.ResultMessage = response.ReasonPhrase.ToString();
                                        uploadlog.ResultStatusCode = ((int)response.StatusCode).ToString();
                                        uploadlog.ResultStatusDesc = response.StatusCode.ToString();
                                    }


                                    context_sub.ShisUploadLogs.Add(uploadlog);
                                    context_sub.SaveChanges();


                                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                                    {
                                        //The token will be cleared when it expires and restart function
                                        jwtToken = "";
                                        return "R";
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logger.Fatal($"Exception error，status code: {ex.Message}");

                                    uploadlog.ResultSuccess = false;
                                    uploadlog.ResultMessage = ex.Message.ToString();
                                    uploadlog.ResultStatusCode = "999";
                                    uploadlog.ResultStatusDesc = "Exception";

                                    context_sub.ShisUploadLogs.Add(uploadlog);
                                    context_sub.SaveChanges();
                                    return "R";
                                }


                                #endregion
                            }
                            else
                            {
                                roots.Add(root);
                                logger.Info(
                                                $"roots count: {roots.Count} | inhospID: " + root.RegInfo.Inhospid);
                            }
                        }

                    }
                }

                return "00";
            }
        }
        catch (ArgumentNullException anex)
        {
            using (var context_sub = new GatewayContext())
            {
                logger.Fatal($"ArgumentNullException error: {anex}");
                ShisUploadLog uploadlog = new ShisUploadLog();
                uploadlog.Logid = -1;
                uploadlog.RegDate = null;
                uploadlog.Option = userChoice != null ? Convert.ToChar(userChoice) : ' ';
                uploadlog.Inhospid = null;
                uploadlog.ExecDatetime = DateTime.Now;
                uploadlog.LocalLoginUser = "Admin";
                uploadlog.TargetAgency = "MOHD";
                uploadlog.LocalIp = GetLocalIPAddress();
                uploadlog.TargetUrl = null;
                uploadlog.ResultSuccess = false;
                uploadlog.ResultMessage = $"ArgumentNullException error: {anex}";
                uploadlog.ResultStatusCode = "999";
                uploadlog.ResultStatusDesc = "ArgumentNullException";
                context_sub.ShisUploadLogs.Add(uploadlog);
                context_sub.SaveChanges();
            }
            return "999";
        }
        catch (Exception ex)
        {
            using (var context_sub = new GatewayContext())
            {
                logger.Fatal($"Exception error: {ex.Message}");
                ShisUploadLog uploadlog = new ShisUploadLog();
                uploadlog.Logid = -1;
                uploadlog.RegDate = null;
                uploadlog.Option = userChoice != null ? Convert.ToChar(userChoice) : ' ';
                uploadlog.Inhospid = null;
                uploadlog.ExecDatetime = DateTime.Now;
                uploadlog.LocalIp = GetLocalIPAddress();
                uploadlog.LocalLoginUser = "Admin";
                uploadlog.TargetAgency = "MOHD";
                uploadlog.TargetUrl = null;
                uploadlog.ResultSuccess = false;
                uploadlog.ResultMessage = ex.Message.ToString();
                uploadlog.ResultStatusCode = "999";
                uploadlog.ResultStatusDesc = "Exception";

                context_sub.ShisUploadLogs.Add(uploadlog);
                context_sub.SaveChanges();
            }


            return "999";
        }

    }

    static async Task<string> upload_merge_patient(ConnectionStatus inConnSatus = ConnectionStatus.Online)
    {
        try
        {
            //2024.01.10 update by 1050325 offline work
            var _inConnSatus = inConnSatus;

            using (var context = new GatewayContext())
            {
                if (string.IsNullOrWhiteSpace(hosp_code))
                {
                    var hospdata = context.ShisCoderefs.Where(c => c.RefCodetype == "HospitalCode").FirstOrDefault();
                    if (hospdata != null &&
                        !string.IsNullOrWhiteSpace(hospdata.RefCode))
                    {
                        hosp_code = hospdata.RefCode;
                    }
                    else
                    {
                        throw new ArgumentNullException(nameof(upload_patient_basic_info), "hosp_code cannot be null.");
                    }
                }


                //step1 搜尋需要上傳的Case
                var uploadCase = context.
                    ShisChartMergeHistories.Where(d => d.UploadStatus == "N").ToList();


                if (uploadCase.Any())
                {
                    logger.Info($"This Task is expected to upload {uploadCase.Count} medical records.");
                    foreach (var item in uploadCase)
                    {
                        using (var context_sub = new GatewayContext())
                        {
                            // 监听用户输入
                            while (true)
                            {
                                if (Console.KeyAvailable)
                                {
                                    var key = Console.ReadKey(intercept: true).Key;
                                    if (key == ConsoleKey.P)
                                    {
                                        isPaused = !isPaused; // 切换暂停状态
                                        Console.WriteLine(isPaused ? "\nExecution paused. Press 'P' to resume..." : "\nResuming execution...");
                                    }
                                    else if (key == ConsoleKey.Q)
                                    {
                                        Console.WriteLine("\nExecution halted by user.");
                                        isPaused = false;
                                        return "Q";
                                    }
                                    else if (key == ConsoleKey.Escape)
                                    {
                                        isPaused = false;
                                        return "Esc";
                                    }
                                }

                                if (!isPaused)
                                {
                                    break; // 继续执行
                                }
                            }

                            #region 必要資訊
                            if (item == null)
                            {
                                using (var _errorContext = new GatewayContext())
                                {
                                    logger.Fatal($"shis_chart No patient found: {item.ChrHalthId}");
                                    ShisUploadLog _uploadlog = new ShisUploadLog();
                                    _uploadlog.Logid = -1;
                                    _uploadlog.RegDate = null;
                                    _uploadlog.Option = userChoice != null ? Convert.ToChar(userChoice) : ' ';
                                    _uploadlog.Inhospid = item.ChrHalthId;
                                    _uploadlog.ExecDatetime = DateTime.Now;
                                    _uploadlog.LocalIp = GetLocalIPAddress();
                                    _uploadlog.LocalLoginUser = "Admin";
                                    _uploadlog.TargetAgency = "MOHD";
                                    _uploadlog.TargetUrl = null;
                                    _uploadlog.ResultSuccess = false;
                                    _uploadlog.ResultMessage = "shis_chart No patient found: " + item.ChrHalthId;
                                    _uploadlog.ResultStatusCode = "999";
                                    _uploadlog.ResultStatusDesc = "Exception";

                                    _errorContext.ShisUploadLogs.Add(_uploadlog);
                                    _errorContext.SaveChanges();
                                }

                                continue;
                            }
                            #endregion


                            //var rInfo = item;
                            var MergePatient = new MergePatient()
                            {
                                FromHosp = hosp_code,
                                Id = item.Id,
                                ChrHalthId = item.ChrHalthId,
                                MhHealthId = item.MhHealthId,
                            };

                            if (inConnSatus == ConnectionStatus.Online)
                            {
                                #region authentication token
                                if (string.IsNullOrWhiteSpace(jwtToken))
                                {
                                    var result = await GetNewJwtToken();
                                    if (result.isSuccess == true)
                                    {
                                        jwtToken = result.returnValue;
                                    }
                                    else
                                    {
                                        return "R";
                                    }
                                }
                                #endregion
                                #region call web api 
                                // string apiUrl = "https://172.18.8.188/api/UploadTask/MOHD_TK_03"; // 你的目标 Web API 地址
                                //string apiUrl = "https://localhost:7287/api/UploadTask/MOHD_TK_02"; // 你的目标 Web API 地址

                                string apiUrl = context.ShisCoderefs
                                   .Where(c => c.RefCodetype == "MOHD_TK" && c.RefCode == "TK_03").Select(c => c.RefName).FirstOrDefault();

                                if (string.IsNullOrWhiteSpace(apiUrl)) { throw new ArgumentNullException(nameof(upload_patient_basic_info), "api Url cannot be null."); }


                                ShisUploadLog uploadlog = new ShisUploadLog();
                                uploadlog.Logid = -1;
                                uploadlog.RegDate = null;
                                uploadlog.Option = userChoice != null ? Convert.ToChar(userChoice) : ' ';
                                uploadlog.Inhospid = MergePatient.ChrHalthId;
                                uploadlog.ExecDatetime = DateTime.Now;
                                uploadlog.LocalIp = GetLocalIPAddress();
                                uploadlog.LocalLoginUser = "Admin";
                                uploadlog.TargetAgency = "MOHD";
                                uploadlog.TargetUrl = apiUrl;

                                try
                                {

                                    var handler = new HttpClientHandler
                                    {
                                        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                                    };

                                    var json = JsonConvert.SerializeObject(MergePatient);
                                    var content = new StringContent(json, Encoding.UTF8, "application/json");


                                    var httpClient = new HttpClient(handler);
                                    httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwtToken);

                                    // 发送POST请求到Web API
                                    var response = await httpClient.PutAsync(apiUrl, content);

                                    // 检查响应是否成功
                                    if (response.IsSuccessStatusCode)
                                    {
                                        // 从响应中获取内容
                                        string responseContent = await response.Content.ReadAsStringAsync();
                                        ResultDTO result = JsonConvert.DeserializeObject<ResultDTO>(responseContent);


                                        uploadlog.ResultSuccess = result != null ? result.isSuccess : false;
                                        uploadlog.ResultMessage = result.Message;
                                        uploadlog.ResultStatusCode = ((int)response.StatusCode).ToString();
                                        uploadlog.ResultStatusDesc = response.StatusCode.ToString();
                                        if (result.isSuccess == true)
                                        {
                                            //logger.Info(responseContent);

                                            logger.Info(
                                                $"request status code: {response.StatusCode}({(int)(response.StatusCode)}) | HealthID: " + MergePatient.ChrHalthId);

                                            //update SHIS_MergeHistory
                                            item.UploadStatus = "Y";
                                            //item.upload_time = DateTime.Now;
                                            context_sub.ShisChartMergeHistories.Update(item);
                                        }
                                        else
                                        {
                                            //logger.Error(responseContent);
                                            logger.Info(
                                                $"request status code: {response.StatusCode}({(int)(response.StatusCode)}) | HealthID: {MergePatient.ChrHalthId}");
                                        }

                                    }
                                    else
                                    {
                                        logger.Fatal(
                                            $"request fail，status code: {response.StatusCode}({(int)(response.StatusCode)}) | HealthID: {MergePatient.ChrHalthId}");

                                        uploadlog.ResultSuccess = false;
                                        uploadlog.ResultMessage = response.ReasonPhrase.ToString();
                                        uploadlog.ResultStatusCode = ((int)response.StatusCode).ToString();
                                        uploadlog.ResultStatusDesc = response.StatusCode.ToString();
                                    }


                                    context_sub.ShisUploadLogs.Add(uploadlog);
                                    context_sub.SaveChanges();


                                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                                    {
                                        //The token will be cleared when it expires and restart function
                                        jwtToken = "";
                                        return "R";
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logger.Fatal($"Exception error，status code: {ex.Message}");

                                    uploadlog.ResultSuccess = false;
                                    uploadlog.ResultMessage = ex.Message.ToString();
                                    uploadlog.ResultStatusCode = "999";
                                    uploadlog.ResultStatusDesc = "Exception";

                                    context_sub.ShisUploadLogs.Add(uploadlog);
                                    context_sub.SaveChanges();
                                    return "R";
                                }
                                #endregion
                            }
                            else
                            {
                                mergePatients.Add(MergePatient);
                                logger.Info($"mergePatients count: {mergePatients.Count} | HealthId: " + MergePatient.MhHealthId);
                            }
                        }
                    }
                }

                return "00";



            }
        }
        catch (ArgumentNullException anex)
        {
            using (var context_sub = new GatewayContext())
            {
                logger.Fatal($"ArgumentNullException error: {anex}");
                ShisUploadLog uploadlog = new ShisUploadLog();
                uploadlog.Logid = -1;
                uploadlog.RegDate = null;
                uploadlog.Option = userChoice != null ? Convert.ToChar(userChoice) : ' ';
                uploadlog.Inhospid = null;
                uploadlog.ExecDatetime = DateTime.Now;
                uploadlog.LocalIp = GetLocalIPAddress();
                uploadlog.LocalLoginUser = "Admin";
                uploadlog.TargetAgency = "MOHD";
                uploadlog.TargetUrl = null;
                uploadlog.ResultSuccess = false;
                uploadlog.ResultMessage = $"ArgumentNullException {anex}";
                uploadlog.ResultStatusCode = "999";
                uploadlog.ResultStatusDesc = "ArgumentNullException";
                context_sub.ShisUploadLogs.Add(uploadlog);
                context_sub.SaveChanges();
            }
            return "999";
        }
        catch (Exception ex)
        {
            using (var context_sub = new GatewayContext())
            {
                logger.Fatal($"Exception error: {ex.Message}");
                ShisUploadLog uploadlog = new ShisUploadLog();
                uploadlog.Logid = -1;
                uploadlog.RegDate = null;
                uploadlog.Option = userChoice != null ? Convert.ToChar(userChoice) : ' ';
                uploadlog.Inhospid = null;
                uploadlog.ExecDatetime = DateTime.Now;
                uploadlog.LocalIp = GetLocalIPAddress();
                uploadlog.LocalLoginUser = "Admin";
                uploadlog.TargetAgency = "MOHD";
                uploadlog.TargetUrl = null;
                uploadlog.ResultSuccess = false;
                uploadlog.ResultMessage = ex.Message.ToString();
                uploadlog.ResultStatusCode = "999";
                uploadlog.ResultStatusDesc = "Exception";

                context_sub.ShisUploadLogs.Add(uploadlog);
                context_sub.SaveChanges();
            }


            return "999";
        }
    }
    static async Task<string> upload_patient_basic_info(ConnectionStatus inConnSatus = ConnectionStatus.Online)
    {
        try
        {
            //2024.01.10 update by 1050325 offline work
            var _inConnSatus = inConnSatus;

            using (var context = new GatewayContext())
            {
                if (string.IsNullOrWhiteSpace(hosp_code))
                {
                    var hospdata = context.ShisCoderefs.Where(c => c.RefCodetype == "HospitalCode").FirstOrDefault();
                    if (hospdata != null &&
                        !string.IsNullOrWhiteSpace(hospdata.RefCode))
                    {
                        hosp_code = hospdata.RefCode;
                    }
                    else
                    {
                        throw new ArgumentNullException(nameof(upload_patient_basic_info), "hosp_code cannot be null.");
                    }
                }


                //step1 搜尋需要上傳的Case
                var uploadCase = context.
                    ShisCharts

                    .Where(d => d.UploadStatus == "N")
                    .OrderBy(d => d.ChrHealthId)
                    .ToList();


                var duplicateChrHealthIds = uploadCase
                    .GroupBy(d => d.ChrHealthId)
                    .Where(g => g.Count() >= 2)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicateChrHealthIds.Any())
                {
                    duplicateChrHealthIds.ForEach(c =>
                    {
                        using (var context_du = new GatewayContext())
                        {
                            logger.Fatal($"Duplicate HealthId: {c}. Upload action cannot be performed.");
                            ShisUploadLog uploadlog = new ShisUploadLog();
                            uploadlog.Logid = -1;
                            uploadlog.RegDate = null;
                            uploadlog.Option = userChoice != null ? Convert.ToChar(userChoice) : ' ';
                            uploadlog.Inhospid = c;
                            uploadlog.ExecDatetime = DateTime.Now;
                            uploadlog.LocalIp = GetLocalIPAddress();
                            uploadlog.LocalLoginUser = "Admin";
                            uploadlog.TargetAgency = "MOHD";
                            uploadlog.TargetUrl = null;
                            uploadlog.ResultSuccess = false;
                            uploadlog.ResultMessage = $"Duplicate HealthId: {c}. Upload action cannot be performed.";
                            uploadlog.ResultStatusCode = "999";
                            uploadlog.ResultStatusDesc = "Duplicate";

                            context_du.ShisUploadLogs.Add(uploadlog);
                            context_du.SaveChanges();

                            uploadCase.RemoveAll(d => d.ChrHealthId == c);
                        }
                    });
                }


                if (uploadCase.Any())
                {
                    logger.Info($"This Task is expected to upload {uploadCase.Count} medical records.");
                    foreach (var item in uploadCase)
                    {
                        using (var context_sub = new GatewayContext())
                        {
                            // 监听用户输入
                            while (true)
                            {
                                if (Console.KeyAvailable)
                                {
                                    var key = Console.ReadKey(intercept: true).Key;
                                    if (key == ConsoleKey.P)
                                    {
                                        isPaused = !isPaused; // 切换暂停状态
                                        Console.WriteLine(isPaused ? "\nExecution paused. Press 'P' to resume..." : "\nResuming execution...");
                                    }
                                    else if (key == ConsoleKey.Q)
                                    {
                                        Console.WriteLine("\nExecution halted by user.");
                                        isPaused = false;
                                        return "Q";
                                    }
                                    else if (key == ConsoleKey.Escape)
                                    {
                                        isPaused = false;
                                        return "Esc";
                                    }
                                }

                                if (!isPaused)
                                {
                                    break; // 继续执行
                                }
                            }

                            #region 必要資訊
                            if (item == null)
                            {
                                using (var _errorContext = new GatewayContext())
                                {
                                    logger.Fatal($"shis_chart No patient found: {item.ChrHealthId}");
                                    ShisUploadLog _uploadlog = new ShisUploadLog();
                                    _uploadlog.Logid = -1;
                                    _uploadlog.RegDate = null;
                                    _uploadlog.Option = userChoice != null ? Convert.ToChar(userChoice) : ' ';
                                    _uploadlog.Inhospid = item.ChrHealthId;
                                    _uploadlog.ExecDatetime = DateTime.Now;
                                    _uploadlog.LocalIp = GetLocalIPAddress();
                                    _uploadlog.LocalLoginUser = "Admin";
                                    _uploadlog.TargetAgency = "MOHD";
                                    _uploadlog.TargetUrl = null;
                                    _uploadlog.ResultSuccess = false;
                                    _uploadlog.ResultMessage = "shis_chart No patient found: " + item.ChrHealthId;
                                    _uploadlog.ResultStatusCode = "999";
                                    _uploadlog.ResultStatusDesc = "Exception";

                                    _errorContext.ShisUploadLogs.Add(_uploadlog);
                                    _errorContext.SaveChanges();
                                }

                                continue;
                            }
                            #endregion


                            //var rInfo = item;
                            var patientInfo = new PatientInfo()
                            {
                                FromHosp = hosp_code,
                                HealthId = item.ChrHealthId,
                                NationalId = item.ChrNationalId,
                                FName = item.ChrPatientFirstname,
                                MName = item.ChrPatientMidname,
                                LName = item.ChrPatientLastname,
                                Sex = item.ChrSex,
                                BirthDate = item.ChrBirthDate,
                                Mobile = item.ChrMobilePhone,
                                Address = item.ChrAddress,
                                EmgCont = item.ChrEmgContact,
                                ContRel = item.ChrContactRelation,
                                ContPhone = item.ChrContactPhone,
                                CombineFlag = item.ChrCombineFlag,
                                Remark = item.ChrRemark,
                                ModifyTime = item.ModifyTime,
                                RefugeeFlag = item.ChrRefugeeFlag,
                                AreaCode = item.ChrAreaCode,
                            };


                            if (_inConnSatus == ConnectionStatus.Online)
                            {
                                #region authentication token
                                if (string.IsNullOrWhiteSpace(jwtToken))
                                {
                                    var result = await GetNewJwtToken();
                                    if (result.isSuccess == true)
                                    {
                                        jwtToken = result.returnValue;
                                    }
                                    else
                                    {
                                        return "R";
                                    }
                                }
                                #endregion
                                #region call web api 
                                //string apiUrl = "https://172.18.8.188/api/UploadTask/MOHD_TK_02"; // 你的目标 Web API 地址
                                //string apiUrl = "https://localhost:7287/api/UploadTask/MOHD_TK_02"; // 你的目标 Web API 地址

                                string apiUrl = context.ShisCoderefs
                                   .Where(c => c.RefCodetype == "MOHD_TK" && c.RefCode == "TK_02").Select(c => c.RefName).FirstOrDefault();

                                if (string.IsNullOrWhiteSpace(apiUrl)) { throw new ArgumentNullException(nameof(upload_patient_basic_info), "api Url cannot be null."); }


                                ShisUploadLog uploadlog = new ShisUploadLog();
                                uploadlog.Logid = -1;
                                uploadlog.RegDate = null;
                                uploadlog.Option = userChoice != null ? Convert.ToChar(userChoice) : ' ';
                                uploadlog.Inhospid = patientInfo.HealthId;
                                uploadlog.ExecDatetime = DateTime.Now;
                                uploadlog.LocalIp = GetLocalIPAddress();
                                uploadlog.LocalLoginUser = "Admin";
                                uploadlog.TargetAgency = "MOHD";
                                uploadlog.TargetUrl = apiUrl;

                                try
                                {

                                    var handler = new HttpClientHandler
                                    {
                                        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                                    };

                                    var json = JsonConvert.SerializeObject(patientInfo);
                                    var content = new StringContent(json, Encoding.UTF8, "application/json");


                                    var httpClient = new HttpClient(handler);
                                    httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwtToken);

                                    // 发送POST请求到Web API
                                    var response = await httpClient.PutAsync(apiUrl, content);

                                    // 检查响应是否成功
                                    if (response.IsSuccessStatusCode)
                                    {
                                        // 从响应中获取内容
                                        string responseContent = await response.Content.ReadAsStringAsync();
                                        ResultDTO result = JsonConvert.DeserializeObject<ResultDTO>(responseContent);


                                        uploadlog.ResultSuccess = result != null ? result.isSuccess : false;
                                        uploadlog.ResultMessage = result.Message;
                                        uploadlog.ResultStatusCode = ((int)response.StatusCode).ToString();
                                        uploadlog.ResultStatusDesc = response.StatusCode.ToString();
                                        if (result.isSuccess == true)
                                        {
                                            //logger.Info(responseContent);

                                            logger.Info(
                                                $"request status code: {response.StatusCode}({(int)(response.StatusCode)}) | HealthID: " + patientInfo.HealthId);

                                            //update shischart
                                            item.UploadStatus = "Y";
                                            item.UploadTime = DateTime.Now;
                                            context_sub.ShisCharts.Update(item);
                                        }
                                        else
                                        {
                                            //logger.Error(responseContent);
                                            logger.Info(
                                                $"request status code: {response.StatusCode}({(int)(response.StatusCode)}) | HealthID: {patientInfo.HealthId}");
                                        }

                                    }
                                    else
                                    {
                                        logger.Fatal(
                                            $"request fail，status code: {response.StatusCode}({(int)(response.StatusCode)}) | HealthID: {patientInfo.HealthId}");

                                        uploadlog.ResultSuccess = false;
                                        uploadlog.ResultMessage = response.ReasonPhrase.ToString();
                                        uploadlog.ResultStatusCode = ((int)response.StatusCode).ToString();
                                        uploadlog.ResultStatusDesc = response.StatusCode.ToString();
                                    }


                                    context_sub.ShisUploadLogs.Add(uploadlog);
                                    context_sub.SaveChanges();


                                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                                    {
                                        //The token will be cleared when it expires and restart function
                                        jwtToken = "";
                                        return "R";
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logger.Fatal($"Exception error，status code: {ex.Message}");

                                    uploadlog.ResultSuccess = false;
                                    uploadlog.ResultMessage = ex.Message.ToString();
                                    uploadlog.ResultStatusCode = "999";
                                    uploadlog.ResultStatusDesc = "Exception";

                                    context_sub.ShisUploadLogs.Add(uploadlog);
                                    context_sub.SaveChanges();
                                    return "R";
                                }


                                #endregion
                            }
                            else
                            {
                                patientInfos.Add(patientInfo);
                                logger.Info($"patientInfos count: {patientInfos.Count} | HealthId: " + patientInfo.HealthId);
                            }
                        }

                    }
                }

                return "00";



            }
        }
        catch (ArgumentNullException anex)
        {
            using (var context_sub = new GatewayContext())
            {
                logger.Fatal($"ArgumentNullException error: {anex}");
                ShisUploadLog uploadlog = new ShisUploadLog();
                uploadlog.Logid = -1;
                uploadlog.RegDate = null;
                uploadlog.Option = userChoice != null ? Convert.ToChar(userChoice) : ' ';
                uploadlog.Inhospid = null;
                uploadlog.ExecDatetime = DateTime.Now;
                uploadlog.LocalIp = GetLocalIPAddress();
                uploadlog.LocalLoginUser = "Admin";
                uploadlog.TargetAgency = "MOHD";
                uploadlog.TargetUrl = null;
                uploadlog.ResultSuccess = false;
                uploadlog.ResultMessage = $"ArgumentNullException {anex}";
                uploadlog.ResultStatusCode = "999";
                uploadlog.ResultStatusDesc = "ArgumentNullException";
                context_sub.ShisUploadLogs.Add(uploadlog);
                context_sub.SaveChanges();
            }
            return "999";
        }
        catch (Exception ex)
        {
            using (var context_sub = new GatewayContext())
            {
                logger.Fatal($"Exception error: {ex.Message}");
                ShisUploadLog uploadlog = new ShisUploadLog();
                uploadlog.Logid = -1;
                uploadlog.RegDate = null;
                uploadlog.Option = userChoice != null ? Convert.ToChar(userChoice) : ' ';
                uploadlog.Inhospid = null;
                uploadlog.ExecDatetime = DateTime.Now;
                uploadlog.LocalIp = GetLocalIPAddress();
                uploadlog.LocalLoginUser = "Admin";
                uploadlog.TargetAgency = "MOHD";
                uploadlog.TargetUrl = null;
                uploadlog.ResultSuccess = false;
                uploadlog.ResultMessage = ex.Message.ToString();
                uploadlog.ResultStatusCode = "999";
                uploadlog.ResultStatusDesc = "Exception";

                context_sub.ShisUploadLogs.Add(uploadlog);
                context_sub.SaveChanges();
            }


            return "999";
        }
    }
    static async Task<string> import_Files_For_Offline_Use()
    {
        try
        {
            using (var context = new GatewayContext())
            {
                logger.Info($"import_Files_For_Offline_Use in progress, please wait.");

                string JsonData = "";
                // 設定匯入路徑
                //string importDirectoryPath = @"D:\SHIS_IMPORT\IMPORT";
                string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SHIS_import");

                if (!Directory.Exists(folderPath))
                {
                    logger.Warn($"The local path directory does not exist:{folderPath}");
                    // 若不存在，則創建資料夾
                    try
                    {
                        Directory.CreateDirectory(folderPath);
                        logger.Info($"The folder has been successfully created：{folderPath}");
                    }
                    catch (Exception ex)
                    {
                        logger.Fatal($"The folder has been failed created：{ex.Message}");
                    }
                }

                if (Directory.Exists(folderPath))
                {
                    // 判斷目錄下是否存在 txt 檔案
                    string[] txtFiles = Directory.GetFiles(folderPath, "*.txt");

                    if (txtFiles.Length > 0)
                    {
                        foreach (var filePath in txtFiles)
                        {
                            summary_errorMsg.Clear();

                            string jsonData = File.ReadAllText(filePath);
                            //List<Root> data = JsonConvert.DeserializeObject<List<Root>>(jsonData);
                            ExportData data = new ExportData();
                            data = JsonConvert.DeserializeObject<ExportData>(jsonData);
                            ImportResult _importResult = new ImportResult();


                            logger.Info($"Imported data from: {filePath}");

                            if (data.roots != null && data.roots.Count > 0)
                            {
                                foreach (Root _root in data.roots)
                                {
                                    #region 先ban
                                    //#region 監聽使用者輸入
                                    //// 監聽使用者輸入
                                    //while (true)
                                    //{
                                    //    if (Console.KeyAvailable)
                                    //    {
                                    //        var key = Console.ReadKey(intercept: true).Key;
                                    //        if (key == ConsoleKey.P)
                                    //        {
                                    //            isPaused = !isPaused; // 切换暂停状态
                                    //            Console.WriteLine(isPaused ? "\nExecution paused. Press 'P' to resume..." : "\nResuming execution...");
                                    //        }
                                    //        else if (key == ConsoleKey.Q)
                                    //        {
                                    //            Console.WriteLine("\nExecution halted by user.");
                                    //            isPaused = false;
                                    //            return "Q";
                                    //        }
                                    //        else if (key == ConsoleKey.Escape)
                                    //        {
                                    //            isPaused = false;
                                    //            return "Esc";
                                    //        }
                                    //    }

                                    //    if (!isPaused)
                                    //    {
                                    //        break; // 继续执行
                                    //    }
                                    //}
                                    //#endregion
                                    //#region authentication token
                                    //if (string.IsNullOrWhiteSpace(jwtToken) || IsJwtExpired(jwtToken))
                                    //{
                                    //    int maxRetries = 3; // 設定最大重試次數
                                    //    int retryCount = 0;
                                    //    var result = await GetNewJwtToken();

                                    //    // 如果取得 JWT Token 失敗，就等待一段時間後重新嘗試
                                    //    while (!result.isSuccess && retryCount < maxRetries)
                                    //    {
                                    //        #region 監聽使用者輸入
                                    //        // 監聽使用者輸入
                                    //        while (true)
                                    //        {
                                    //            if (Console.KeyAvailable)
                                    //            {
                                    //                var key = Console.ReadKey(intercept: true).Key;
                                    //                if (key == ConsoleKey.P)
                                    //                {
                                    //                    isPaused = !isPaused; // 切换暂停状态
                                    //                    Console.WriteLine(isPaused ? "\nExecution paused. Press 'P' to resume..." : "\nResuming execution...");
                                    //                }
                                    //                else if (key == ConsoleKey.Q)
                                    //                {
                                    //                    Console.WriteLine("\nExecution halted by user.");
                                    //                    isPaused = false;
                                    //                    return "Q";
                                    //                }
                                    //                else if (key == ConsoleKey.Escape)
                                    //                {
                                    //                    isPaused = false;
                                    //                    return "Esc";
                                    //                }
                                    //            }

                                    //            if (!isPaused)
                                    //            {
                                    //                break; // 继续执行
                                    //            }
                                    //        }
                                    //        #endregion
                                    //        // 等待一段時間再重新嘗試
                                    //        await Task.Delay(TimeSpan.FromSeconds(5)); // 可以根據需要調整等待的秒數

                                    //        // 再次呼叫 GetNewJwtToken()
                                    //        result = await GetNewJwtToken();
                                    //        retryCount++;
                                    //    }

                                    //    jwtToken = result.returnValue;
                                    //}
                                    //#endregion
                                    //#region call web api 
                                    ////string apiUrl = "https://172.18.8.188/api/UploadTask/MOHD_TK_01"; // 你的目标 Web API 地址
                                    ////string apiUrl = "https://localhost:7287/api/UploadTask/MOHD_TK_01"; // 你的目标 Web API 地址


                                    //string apiUrl = "https://127.0.0.1/api/UploadTask/MOHD_TK_01";
                                    ////string apiUrl = context.ShisCoderefs
                                    ////    .Where(c => c.RefCodetype == "MOHD_TK" && c.RefCode == "TK_01").Select(c => c.RefName).FirstOrDefault();

                                    //if (string.IsNullOrWhiteSpace(apiUrl)) { throw new ArgumentNullException(nameof(upload_daily_medical_record), "api Url cannot be null."); }
                                    //try
                                    //{

                                    //    var handler = new HttpClientHandler
                                    //    {
                                    //        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                                    //    };

                                    //    var json = JsonConvert.SerializeObject(_root);
                                    //    var content = new StringContent(json, Encoding.UTF8, "application/json");


                                    //    var httpClient = new HttpClient(handler);
                                    //    httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwtToken);



                                    //    // 发送POST请求到Web API
                                    //    var response = await httpClient.PostAsync(apiUrl, content);

                                    //    // 检查响应是否成功
                                    //    if (response.IsSuccessStatusCode)
                                    //    {
                                    //        // 从响应中获取内容
                                    //        string responseContent = await response.Content.ReadAsStringAsync();
                                    //        ResultDTO result = JsonConvert.DeserializeObject<ResultDTO>(responseContent);
                                    //        if (result.isSuccess == true)
                                    //        {
                                    //            //logger.Info(responseContent);

                                    //            logger.Info(
                                    //                $"【Import】| request status code: {response.StatusCode}({(int)(response.StatusCode)}) | inhospID: " + _root.RegInfo.Inhospid);

                                    //            _importResult.SuccessfulImports_Root.Add(_root);

                                    //        }
                                    //        else
                                    //        {
                                    //            //logger.Error(responseContent);
                                    //            logger.Info(
                                    //                $"【Import】| request status code: {response.StatusCode}({(int)(response.StatusCode)}) | inhospID: {_root.RegInfo.Inhospid}");

                                    //            _importResult.FailedImports_Root.Add(_root);
                                    //        }

                                    //    }
                                    //    else
                                    //    {
                                    //        logger.Fatal(
                                    //            $"【Import】| request fail，status code: {response.StatusCode}({(int)(response.StatusCode)}) | inhospID: {_root.RegInfo.Inhospid}");

                                    //        _importResult.FailedImports_Root.Add(_root);
                                    //    }

                                    //}
                                    //catch (Exception ex)
                                    //{
                                    //    logger.Fatal($"Exception error，status code: {ex.Message}");
                                    //    _importResult.FailedImports_Root.Add(_root);
                                    //    //return "R";
                                    //    continue;
                                    //}
                                    //#endregion

                                    #endregion

                                    var result = await getJWTokenAndCallAPI(_root, _importResult);
                                    _importResult = result.Item2;
                                    if (result.Item1 == "00")
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        return result.Item1;
                                    }
                                }

                                #region 先ban
                                //#region 匯出作業
                                ////以上做完全部匯入動作，以下做檔案異動
                                ////產生資料夾
                                //// 檢查exec資料夾是否存在
                                ////string exec_path = @"D:\SHIS_IMPORT\EXECUTION_LOG";
                                ////string exec_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SHIS_IMPORT");
                                //string exec_path = Path.Combine(folderPath, "execution_log");


                                //if (!Directory.Exists(exec_path))
                                //{
                                //    // 若不存在，則創建資料夾
                                //    try
                                //    {
                                //        Directory.CreateDirectory(exec_path);
                                //        logger.Info($"The folder has been successfully created：{exec_path}");
                                //    }
                                //    catch (Exception ex)
                                //    {
                                //        logger.Fatal($"The folder has been failed created：{ex.Message}");
                                //    }
                                //}

                                //// 產生當次執行的資料夾
                                //var smy_path = CreateUniqueFolder(exec_path, GetFileNameWithoutExtension(filePath));

                                ////匯出錯誤文件
                                //if (_importResult.FailedImports.Count > 0)
                                //{
                                //    var jsonErrorData = JsonConvert.SerializeObject(_importResult.FailedImports);
                                //    //匯出準備
                                //    string fileName_new = "";
                                //    //避免重複出現_FAIL
                                //    if (GetFileNameWithoutExtension(filePath).Contains("_FAIL"))
                                //    {
                                //        fileName_new = $"{GetFileNameWithoutExtension(filePath)}.txt";
                                //    }
                                //    else
                                //    {
                                //        fileName_new = $"{GetFileNameWithoutExtension(filePath)}_FAIL.txt";
                                //    }
                                //    //string fileName_new = $"{GetFileNameWithoutExtension(filePath)}_FAIL.txt";
                                //    string filePath_new = Path.Combine(smy_path, fileName_new);
                                //    // 檢查路徑是否存在，如果不存在則創建該路徑
                                //    //if (!Directory.Exists(filePath_new))
                                //    //{
                                //    //    Directory.CreateDirectory(filePath_new);
                                //    //    logger.Info($"Directory created successfully. ({filePath_new})");
                                //    //}

                                //    var exportResult = ExportJsonToTextFile(jsonErrorData, filePath_new);
                                //    if (exportResult.isSuccess == true)
                                //    {
                                //        logger.Info($"ExportJsonToTextFile successfully.");
                                //    }
                                //    else
                                //    {
                                //        logger.Info($"ExportJsonToTextFile fail.");
                                //    }
                                //}

                                ////匯出summary
                                //string fileName_summary = $"{GetFileNameWithoutExtension(filePath)}_summary.txt";
                                //string filePath_summary = Path.Combine(smy_path, fileName_summary);
                                //string summaryData = "";
                                //FileInfo fileInfo = new FileInfo(filePath);
                                //long fileSizeBytes = fileInfo.Length;
                                //double fileSizeKB = Math.Floor(fileSizeBytes / (1024.0)); // 將字節轉換為 MB
                                //string fileType = Path.GetExtension(filePath);

                                //summaryData =
                                //$"Upload Summary - {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}" + Environment.NewLine + Environment.NewLine +
                                //$"-- Uploaded Files:" + Environment.NewLine +
                                //$"File: {GetFileNameWithoutExtension(fileInfo.Name)}" + Environment.NewLine +
                                //$"Size: {fileSizeKB} KB" + Environment.NewLine +
                                //$"Type: {fileType}" + Environment.NewLine + Environment.NewLine +
                                //$"-- Upload Statistics:" + Environment.NewLine +
                                //$"Total Uploads: {_importResult.totalRoot}" + Environment.NewLine +
                                //$"Successful Uploads: {_importResult.SuccessfulImports.Count}" + Environment.NewLine +
                                //$"Failed Uploads: {_importResult.FailedImports.Count}";



                                //var summaryResult = ExportJsonToTextFile(summaryData, filePath_summary);
                                //if (summaryResult.isSuccess == true)
                                //{
                                //    logger.Info($"ExportJsonToTextFile successfully.");
                                //}
                                //else
                                //{
                                //    logger.Info($"ExportJsonToTextFile fail.");
                                //}


                                ////backup
                                //string fileName_bk = $"{GetFileNameWithoutExtension(filePath)}_backup.txt";
                                //string sourceFilePath = filePath;
                                //string destinationFilePath = Path.Combine(smy_path, fileName_bk);
                                ////調用 File.Copy 備份文件
                                //BackupFile(sourceFilePath, destinationFilePath);


                                ////刪除原始文件
                                //DeleteFile(filePath);

                                //#endregion
                                #endregion
                            }

                            if (data.patientinfos != null && data.patientinfos.Count > 0)
                            {
                                foreach (PatientInfo _patientInfo in data.patientinfos)
                                {
                                    var result = await getJWTokenAndCallAPI(_patientInfo, _importResult);
                                    _importResult = result.Item2;
                                    if (result.Item1 == "00")
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        return result.Item1;
                                    }
                                }

                            }

                            if (data.mergerecords != null && data.mergerecords.Count > 0)
                            {
                                foreach (MergePatient _mergePatient in data.mergerecords)
                                {
                                    var result = await getJWTokenAndCallAPI(_mergePatient, _importResult);
                                    _importResult = result.Item2;
                                    if (result.Item1 == "00")
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        return result.Item1;
                                    }
                                }

                            }

                            #region 匯出作業
                            //以上做完全部匯入動作，以下做檔案異動
                            //產生資料夾
                            // 檢查exec資料夾是否存在
                            //string exec_path = @"D:\SHIS_IMPORT\EXECUTION_LOG";
                            //string exec_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SHIS_IMPORT");
                            string exec_path = Path.Combine(folderPath, "execution_log");


                            if (!Directory.Exists(exec_path))
                            {
                                // 若不存在，則創建資料夾
                                try
                                {
                                    Directory.CreateDirectory(exec_path);
                                    logger.Info($"The folder has been successfully created：{exec_path}");
                                }
                                catch (Exception ex)
                                {
                                    logger.Fatal($"The folder has been failed created：{ex.Message}");
                                }
                            }

                            // 產生當次執行的資料夾
                            var smy_path = CreateUniqueFolder(exec_path, GetFileNameWithoutExtension(filePath));

                            //匯出錯誤文件
                            if (_importResult.FailedImports_Root.Count > 0 ||
                                _importResult.FailedImports_Pinfo.Count > 0 ||
                                _importResult.FailedImports_MP.Count > 0)
                            {
                                ExportData failData = new ExportData();
                                failData.roots = _importResult.FailedImports_Root;
                                failData.patientinfos = _importResult.FailedImports_Pinfo;
                                failData.mergerecords = _importResult.FailedImports_MP;

                                var jsonErrorData = JsonConvert.SerializeObject(failData);
                                //匯出準備
                                string fileName_new = "";
                                //避免重複出現_FAIL
                                if (GetFileNameWithoutExtension(filePath).Contains("_fail"))
                                {
                                    fileName_new = $"{GetFileNameWithoutExtension(filePath)}.txt";
                                }
                                else
                                {
                                    fileName_new = $"{GetFileNameWithoutExtension(filePath)}_fail.txt";
                                }
                                //string fileName_new = $"{GetFileNameWithoutExtension(filePath)}_FAIL.txt";
                                string filePath_new = Path.Combine(smy_path, fileName_new);
                                // 檢查路徑是否存在，如果不存在則創建該路徑
                                //if (!Directory.Exists(filePath_new))
                                //{
                                //    Directory.CreateDirectory(filePath_new);
                                //    logger.Info($"Directory created successfully. ({filePath_new})");
                                //}

                                var exportResult = ExportJsonToTextFile(jsonErrorData, filePath_new);
                                if (exportResult.isSuccess == true)
                                {
                                    logger.Info($"ExportJsonToTextFile successfully.");
                                }
                                else
                                {
                                    logger.Info($"ExportJsonToTextFile fail.");
                                }
                            }

                            //匯出summary
                            string fileName_summary = $"{GetFileNameWithoutExtension(filePath)}_summary.txt";
                            string filePath_summary = Path.Combine(smy_path, fileName_summary);
                            string summaryData = "";
                            FileInfo fileInfo = new FileInfo(filePath);
                            long fileSizeBytes = fileInfo.Length;
                            double fileSizeKB = Math.Floor(fileSizeBytes / (1024.0)); // 將字節轉換為 MB
                            string fileType = Path.GetExtension(filePath);

                            summaryData =
                            $"Upload Summary - {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}" + Environment.NewLine + Environment.NewLine +
                            $"-- Uploaded Files:" + Environment.NewLine +
                            $"File: {GetFileNameWithoutExtension(fileInfo.Name)}" + Environment.NewLine +
                            $"Size: {fileSizeKB} KB" + Environment.NewLine +
                            $"Type: {fileType}" + Environment.NewLine + Environment.NewLine +
                            $"-- Upload Statistics:" + Environment.NewLine +
                            $"Total Uploads: {_importResult.TotalCount} | " +
                            $"Total Successful Uploads: {_importResult.TotalSuccessfulCount} | " +
                            $"Total Failed Uploads: {_importResult.TotalFailedCount}" + Environment.NewLine + Environment.NewLine +

                            $"-- Daily outpatient and emergency medical record upload task:" + Environment.NewLine +
                            $"Successful Uploads: {_importResult.SuccessfulImports_RootCount} | " +
                            $"Failed Uploads: {_importResult.FailedImports_RootCount}" + Environment.NewLine + Environment.NewLine +
                            $"-- Patient basic information upload task:" + Environment.NewLine +
                            $"Successful Uploads: {_importResult.SuccessfulImports_PinfoCount}  | " +
                            $"Failed Uploads: {_importResult.FailedImports_PinfoCount}" + Environment.NewLine + Environment.NewLine +
                            $"-- Medical records merge upload task:" + Environment.NewLine +
                            $"Successful Uploads: {_importResult.SuccessfulImports_MPCount}  | " +
                            $"Failed Uploads: {_importResult.FailedImports_MPCount}" + Environment.NewLine + Environment.NewLine;

                            if (summary_errorMsg.Count > 0 && !string.IsNullOrWhiteSpace(summaryData))
                            {
                                summaryData += $"-- Error Message:" + Environment.NewLine;
                                foreach (string msg in summary_errorMsg)
                                {
                                    summaryData += $"{msg}" + Environment.NewLine + Environment.NewLine;

                                }
                                summaryData += "End.";
                            }

                            var summaryResult = ExportJsonToTextFile(summaryData, filePath_summary);
                            if (summaryResult.isSuccess == true)
                            {
                                logger.Info($"ExportJsonToTextFile successfully.");
                            }
                            else
                            {
                                logger.Info($"ExportJsonToTextFile fail.");
                            }


                            //backup
                            string fileName_bk = $"{GetFileNameWithoutExtension(filePath)}_backup.txt";
                            string sourceFilePath = filePath;
                            string destinationFilePath = Path.Combine(smy_path, fileName_bk);
                            //調用 File.Copy 備份文件
                            BackupFile(sourceFilePath, destinationFilePath);


                            //刪除原始文件
                            DeleteFile(filePath);

                            #endregion

                        }
                    }
                    else
                    {
                        logger.Warn(".txt files under the SHIS_import path.");
                    }
                }
                else
                {
                    logger.Warn("Directory (SHIS_import) does not exist.");
                }

                return "00";
            }

        }
        catch (Exception ex)
        {
            logger.Fatal($"Exception error: {ex.Message}");
            return "999";
        }
    }

    static async Task<(string, ImportResult)> getJWTokenAndCallAPI<T>(T data, ImportResult _importResult)
    {
        HttpMethod httpMethod = HttpMethod.Post;
        string dataType = "";
        dynamic dyData = null;
        string apiUrl = "";
        string targetFun = "";
        // 使用泛型參數 T 來操作傳入的不同類型的參數
        if (typeof(T) == typeof(Root))
        {
            dyData = data as Root;
            targetFun = "MOHD_TK_01";
#if DEBUG
            apiUrl = _configuration.GetSection("ApiUrls")["Debug"];
#else
            apiUrl = _configuration.GetSection("ApiUrls")["Release"];
#endif

            apiUrl = $"{apiUrl}/{targetFun}";

            // 在這裡處理 Root 類型的 data
        }
        else if (typeof(T) == typeof(PatientInfo))
        {
            dyData = data as PatientInfo;
            targetFun = "MOHD_TK_02";
#if DEBUG
            apiUrl = _configuration.GetSection("ApiUrls")["Debug"];
#else
            apiUrl = _configuration.GetSection("ApiUrls")["Release"];
#endif
            apiUrl = $"{apiUrl}/{targetFun}";
            httpMethod = HttpMethod.Put;
        }
        else if (typeof(T) == typeof(MergePatient))
        {
            dyData = data as MergePatient;
            targetFun = "MOHD_TK_03";
#if DEBUG
            apiUrl = _configuration.GetSection("ApiUrls")["Debug"];
#else
            apiUrl = _configuration.GetSection("ApiUrls")["Release"];
#endif
            apiUrl = $"{apiUrl}/{targetFun}";
            httpMethod = HttpMethod.Put;
        }
        else
        {
            // 若傳入的類型不是 Root、PatientInfo 或 MergePatient，執行相應的錯誤處理邏輯
            throw new ArgumentException("Unsupported data type.");
        }



        #region authentication token
        if (string.IsNullOrWhiteSpace(jwtToken) || IsJwtExpired(jwtToken))
        {
            int maxRetries = 3; // 設定最大重試次數
            int retryCount = 0;
            var result = await GetNewJwtToken();

            // 如果取得 JWT Token 失敗，就等待一段時間後重新嘗試
            while (!result.isSuccess && retryCount < maxRetries)
            {
                #region 監聽使用者輸入
                // 監聽使用者輸入
                while (true)
                {
                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(intercept: true).Key;
                        if (key == ConsoleKey.P)
                        {
                            isPaused = !isPaused; // 切换暂停状态
                            Console.WriteLine(isPaused ? "\nExecution paused. Press 'P' to resume..." : "\nResuming execution...");
                        }
                        else if (key == ConsoleKey.Q)
                        {
                            Console.WriteLine("\nExecution halted by user.");
                            isPaused = false;
                            return ("Q", _importResult);
                        }
                        else if (key == ConsoleKey.Escape)
                        {
                            isPaused = false;
                            return ("Esc", _importResult);
                        }
                    }

                    if (!isPaused)
                    {
                        break; // 继续执行
                    }
                }
                #endregion
                // 等待一段時間再重新嘗試
                await Task.Delay(TimeSpan.FromSeconds(5)); // 可以根據需要調整等待的秒數

                // 再次呼叫 GetNewJwtToken()
                result = await GetNewJwtToken();
                retryCount++;
            }

            jwtToken = result.returnValue;
        }
        #endregion
        #region call web api 

        if (string.IsNullOrWhiteSpace(apiUrl)) { throw new ArgumentNullException(nameof(getJWTokenAndCallAPI), "api Url cannot be null."); }
        try
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            var json = JsonConvert.SerializeObject(dyData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");


            var httpClient = new HttpClient(handler);
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwtToken);

            // 发送POST请求到Web API
            HttpResponseMessage response = null;

            if (httpMethod == HttpMethod.Post)
            {
                response = await httpClient.PostAsync(apiUrl, content);
            }
            else if (httpMethod == HttpMethod.Put)
            {
                response = await httpClient.PutAsync(apiUrl, content);
            }

            // 检查响应是否成功
            if (response.IsSuccessStatusCode)
            {
                // 从响应中获取内容
                string responseContent = await response.Content.ReadAsStringAsync();
                ResultDTO result = JsonConvert.DeserializeObject<ResultDTO>(responseContent);
                if (result.isSuccess == true)
                {
                    //logger.Info(responseContent);

                    if (typeof(T) == typeof(Root))
                    {
                        logger.Info(
                        $"【Import】| request status code: {response.StatusCode}({(int)(response.StatusCode)}) | inhospID: " + dyData.RegInfo.Inhospid);

                        _importResult.SuccessfulImports_Root.Add(dyData);
                    }
                    else if (typeof(T) == typeof(PatientInfo))
                    {
                        logger.Info(
                       $"【Import】| request status code: {response.StatusCode}({(int)(response.StatusCode)}) | HealthId: " + dyData.HealthId);

                        _importResult.SuccessfulImports_Pinfo.Add(dyData);
                    }
                    else if (typeof(T) == typeof(MergePatient))
                    {
                        logger.Info(
                       $"【Import】| request status code: {response.StatusCode}({(int)(response.StatusCode)}) | MhHealthId: " + dyData.MhHealthId);

                        _importResult.SuccessfulImports_MP.Add(dyData);
                    }
                }
                else
                {

                    if (typeof(T) == typeof(Root))
                    {
                        string errormsg = $"【Import】| request status code: {response.StatusCode}({(int)(response.StatusCode)}) | inhospID: {dyData.RegInfo.Inhospid}";

                        summary_errorMsg.Add(errormsg);
                        logger.Info(errormsg);

                        _importResult.FailedImports_Root.Add(dyData);
                    }
                    else if (typeof(T) == typeof(PatientInfo))
                    {
                        string errormsg = $"【Import】| request status code: {response.StatusCode}({(int)(response.StatusCode)}) | inhospID: {dyData.HealthId}";

                        summary_errorMsg.Add(errormsg);
                        logger.Info(errormsg);

                        _importResult.FailedImports_Pinfo.Add(dyData);

                    }
                    else if (typeof(T) == typeof(MergePatient))
                    {
                        string errormsg = $"【Import】| request status code: {response.StatusCode}({(int)(response.StatusCode)}) | inhospID: {dyData.MhHealthId}";

                        summary_errorMsg.Add(errormsg);
                        logger.Info(errormsg);

                        _importResult.FailedImports_MP.Add(dyData);
                    }


                    //logger.Error(responseContent);

                }

            }
            else
            {
                if (typeof(T) == typeof(Root))
                {
                    string errormsg = $"【Import】| request status code: {response.StatusCode}({(int)(response.StatusCode)}) | inhospID: {dyData.RegInfo.Inhospid}";

                    summary_errorMsg.Add(errormsg);
                    logger.Info(errormsg);

                    _importResult.FailedImports_Root.Add(dyData);
                }
                else if (typeof(T) == typeof(PatientInfo))
                {
                    string errormsg = $"【Import】| request status code: {response.StatusCode}({(int)(response.StatusCode)}) | HealthId: {dyData.HealthId}";

                    summary_errorMsg.Add(errormsg);
                    logger.Info(errormsg);

                    _importResult.FailedImports_Pinfo.Add(dyData);

                }
                else if (typeof(T) == typeof(MergePatient))
                {
                    string errormsg = $"【Import】| request status code: {response.StatusCode}({(int)(response.StatusCode)}) | MhHealthId: {dyData.MhHealthId}";

                    summary_errorMsg.Add(errormsg);
                    logger.Info(errormsg);

                    _importResult.FailedImports_MP.Add(dyData);
                }
            }

            return ("00", _importResult);
        }
        catch (Exception ex)
        {
            logger.Fatal($"Exception error，status code: {ex.Message}");
            if (typeof(T) == typeof(Root))
            {
                string errormsg = $"Exception error，status code: {ex.Message}";
                summary_errorMsg.Add(errormsg);
                logger.Info(errormsg);

                _importResult.FailedImports_Root.Add(dyData);
            }
            else if (typeof(T) == typeof(PatientInfo))
            {
                string errormsg = $"Exception error，status code: {ex.Message}";
                summary_errorMsg.Add(errormsg);
                logger.Info(errormsg);

                _importResult.FailedImports_Pinfo.Add(dyData);
            }
            else if (typeof(T) == typeof(MergePatient))
            {
                string errormsg = $"Exception error，status code: {ex.Message}";
                summary_errorMsg.Add(errormsg);
                logger.Info(errormsg);

                _importResult.FailedImports_MP.Add(dyData);
            }
            return ("00", _importResult);
        }
        #endregion

    }

    static string CreateUniqueFolder(string baseFolderPath, string infolderName)
    {
        string folderName = infolderName; // 新文件夹的基础名称

        string newFolderPath = Path.Combine(baseFolderPath, folderName);

        // 检查文件夹是否已存在，如果存在则添加后缀
        int suffix = 1;
        while (Directory.Exists(newFolderPath))
        {
            folderName = $"{infolderName} ({suffix})";
            newFolderPath = Path.Combine(baseFolderPath, folderName);
            suffix++;
        }

        // 创建文件夹
        Directory.CreateDirectory(newFolderPath);

        return newFolderPath;
    }

    static string RemoveUnwantedSuffix(string folderName)
    {
        // 在这里添加逻辑以去除不需要的后缀，例如 "(2)"
        // 这里假设后缀是以括号括起来的，你可以根据实际情况调整逻辑
        int startIndex = folderName.IndexOf("(");
        int endIndex = folderName.IndexOf(")");

        if (startIndex != -1 && endIndex != -1)
        {
            folderName = folderName.Remove(startIndex, endIndex - startIndex + 1);
        }

        return folderName;
    }


    static string GetFolderPath(string filePath)
    {
        // 使用 Path.GetDirectoryName 取得文件夾路徑
        string folderPath = Path.GetDirectoryName(filePath);
        return folderPath;
    }

    static string GetFileNameWithExtension(string filePath)
    {
        // 使用 Path.GetFileName 取得文件名
        string fileName = Path.GetFileName(filePath);
        return fileName;
    }

    static string GetFileNameWithoutExtension(string filePath)
    {
        // 使用 Path.GetFileNameWithoutExtension 取得不帶副檔名的文件名
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
        return fileNameWithoutExtension;
    }

    static void BackupFile(string sourceFilePath, string destinationFilePath)
    {
        try
        {
            // 使用 File.Copy 將文件從原路徑複製到目標路徑
            File.Copy(sourceFilePath, destinationFilePath, true);
            Console.WriteLine($"File backup successful.: {destinationFilePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"File backup failed.: {ex.Message}");
        }
    }

    static void DeleteFile(string filePath)
    {
        try
        {
            // 使用 File.Delete 删除文件
            File.Delete(filePath);
            Console.WriteLine($"File deletion successful.: {filePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"File deletion failed.: {ex.Message}");
        }
    }

    static void RenameFile(string oldFilePath, string newFilePath)
    {
        try
        {
            // 使用 File.Move 重命名文件
            File.Move(oldFilePath, newFilePath);
            Console.WriteLine($"File rename successful.: {newFilePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"File rename failed.: {ex.Message}");
        }
    }


    static bool IsJwtExpired(string jwtToken)
    {
        var handler = new JwtSecurityTokenHandler();

        // 解析 JWT Token，不驗證簽章
        var jsonToken = handler.ReadToken(jwtToken) as JwtSecurityToken;

        if (jsonToken == null)
        {
            // 無法解析 JWT Token
            return true;
        }

        // 取得 exp 欄位
        var expirationTime = jsonToken.ValidTo;

        // 判斷 JWT 是否已過期
        return expirationTime <= DateTime.UtcNow;
    }

    static ResultDTO ExportJsonToTextFile(string jsonContent, string filePath)
    {
        var result = new ResultDTO() { isSuccess = false };

        try
        {
            // 使用 StreamWriter 將JSON寫入文件
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                sw.WriteLine(jsonContent);

                result.isSuccess = true;

                sw.Close();
            }

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"export fail : {ex.Message}");
            result.Message = ex.Message;
            result.isSuccess = false;
            return result;
        }
    }

    static List<Root> ImportFromJson(string directoryPath)
    {
        List<Root> importedData = new List<Root>();

        if (Directory.Exists(directoryPath))
        {
            foreach (var filePath in Directory.GetFiles(directoryPath, "*.txt"))
            {
                string jsonData = File.ReadAllText(filePath);
                List<Root> data = JsonConvert.DeserializeObject<List<Root>>(jsonData);
                importedData.AddRange(data);
                Console.WriteLine($"Imported data from: {filePath}");
            }
        }
        else
        {
            Console.WriteLine("Directory does not exist.");
        }

        return importedData;
    }

    static async Task<string> export_Files_For_Offline_Use()
    {
        try
        {
            using (var context = new GatewayContext())
            {
                logger.Info($"export_Files_For_Offline_Use in progress, please wait.");
                string JsonData = "";
                string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                //string folderPath = @"D:\SHIS_EXPORT";
                string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SHIS_export");

                // 檢查資料夾是否存在
                if (!Directory.Exists(folderPath))
                {
                    logger.Warn($"The local path directory does not exist:{folderPath}");
                    // 若不存在，則創建資料夾
                    try
                    {
                        Directory.CreateDirectory(folderPath);
                        Console.WriteLine($"The folder has been successfully created：{folderPath}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"The folder has been failed created：{ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"The folder already exists：{folderPath}");
                }

                //string fileName = $"{hosp_code}__{timestamp}.txt";
                //string filePath = Path.Combine(folderPath, fileName);
                //資料處理與匯集
                int totalCount = 0;

                roots.Clear();
                Task<string> result = upload_daily_medical_record(ConnectionStatus.Export);
                if (result.Result != "00") return result.Result;

                patientInfos.Clear();
                Task<string> result_pInfo = upload_patient_basic_info(ConnectionStatus.Export);
                if (result_pInfo.Result != "00") return result_pInfo.Result;

                mergePatients.Clear();
                Task<string> result_m_pInfo = upload_merge_patient(ConnectionStatus.Export);
                if (result_m_pInfo.Result != "00") return result_m_pInfo.Result;

                if (roots.Any() || patientInfos.Any() || mergePatients.Any())
                {

                    if (roots.Any())
                    {
                        //JsonData = JsonConvert.SerializeObject(roots);
                        exportData.roots = roots;
                        totalCount += roots.Count;
                    }

                    if (patientInfos.Any())
                    {
                        exportData.patientinfos = patientInfos;
                        totalCount += patientInfos.Count;
                    }

                    if (mergePatients.Any())
                    {
                        exportData.mergerecords = mergePatients;
                        totalCount += mergePatients.Count;
                    }

                    JsonData = JsonConvert.SerializeObject(exportData);
                }


                //匯出準備
                string fileName = $"{hosp_code}_{timestamp}_{totalCount}.txt";
                string filePath = Path.Combine(folderPath, fileName);
                // 檢查路徑是否存在，如果不存在則創建該路徑
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                    Console.WriteLine("Directory created successfully.");
                }

                var exportResult = ExportJsonToTextFile(JsonData, filePath);
                if (exportResult.isSuccess == true)
                {
                    //寫success log (逐筆)
                    foreach (var root in roots)
                    {
                        var pInfo = context.ShisCharts.Where(c => c.ChrHealthId == root.PatientInfo.HealthId).FirstOrDefault();
                        var rInfo = context.Registrations.Where(c => c.Inhospid == root.RegInfo.Inhospid).FirstOrDefault();
                        //update registration、shischart
                        pInfo.UploadStatus = "Y";
                        pInfo.UploadTime = DateTime.Now;
                        rInfo.UploadStatus = "Y";
                        rInfo.UploadTime = DateTime.Now;
                        context.ShisCharts.Update(pInfo);
                        context.Registrations.Update(rInfo);

                        using (var context_sub = new GatewayContext())
                        {
                            ShisUploadLog uploadlog = new ShisUploadLog();
                            uploadlog.Logid = -1;
                            uploadlog.RegDate = root.RegInfo.RegDate;
                            uploadlog.Option = userChoice != null ? Convert.ToChar(userChoice) : ' ';
                            uploadlog.Inhospid = root.RegInfo.Inhospid;
                            uploadlog.ExecDatetime = DateTime.Now;
                            uploadlog.LocalIp = GetLocalIPAddress();
                            uploadlog.LocalLoginUser = "Admin";
                            uploadlog.TargetAgency = "MOHD";
                            uploadlog.TargetUrl = "path";
                            uploadlog.ResultSuccess = true;
                            uploadlog.ResultMessage = "Export Success";
                            uploadlog.ResultStatusCode = "200";
                            context_sub.ShisUploadLogs.Add(uploadlog);
                            context_sub.SaveChanges();
                        }


                    }

                    foreach (var patientinfo in patientInfos)
                    {
                        var pInfo = context.ShisCharts.Where(c => c.ChrHealthId == patientinfo.HealthId).FirstOrDefault();
                        //update registration、shischart
                        pInfo.UploadStatus = "Y";
                        pInfo.UploadTime = DateTime.Now;
                        context.ShisCharts.Update(pInfo);

                        using (var context_sub = new GatewayContext())
                        {
                            ShisUploadLog uploadlog = new ShisUploadLog();
                            uploadlog.Logid = -1;
                            uploadlog.RegDate = null;
                            uploadlog.Option = userChoice != null ? Convert.ToChar(userChoice) : ' ';
                            uploadlog.Inhospid = pInfo.ChrHealthId;
                            uploadlog.ExecDatetime = DateTime.Now;
                            uploadlog.LocalIp = GetLocalIPAddress();
                            uploadlog.LocalLoginUser = "Admin";
                            uploadlog.TargetAgency = "MOHD";
                            uploadlog.TargetUrl = "path";
                            uploadlog.ResultSuccess = true;
                            uploadlog.ResultMessage = "Export Success";
                            uploadlog.ResultStatusCode = "200";
                            context_sub.ShisUploadLogs.Add(uploadlog);
                            context_sub.SaveChanges();
                        }
                    }



                    foreach (var mergePatient in mergePatients)
                    {
                        var mInfo = context.ShisChartMergeHistories.Where(c => c.MhHealthId == mergePatient.MhHealthId).FirstOrDefault();
                        //update registration、shischart
                        mInfo.UploadStatus = "Y";
                        mInfo.UploadTime = DateTime.Now;
                        context.ShisChartMergeHistories.Update(mInfo);

                        using (var context_sub = new GatewayContext())
                        {
                            ShisUploadLog uploadlog = new ShisUploadLog();
                            uploadlog.Logid = -1;
                            uploadlog.RegDate = null;
                            uploadlog.Option = userChoice != null ? Convert.ToChar(userChoice) : ' ';
                            uploadlog.Inhospid = mInfo.MhHealthId;
                            uploadlog.ExecDatetime = DateTime.Now;
                            uploadlog.LocalIp = GetLocalIPAddress();
                            uploadlog.LocalLoginUser = "Admin";
                            uploadlog.TargetAgency = "MOHD";
                            uploadlog.TargetUrl = "path";
                            uploadlog.ResultSuccess = true;
                            uploadlog.ResultMessage = "Export Success";
                            uploadlog.ResultStatusCode = "200";
                            context_sub.ShisUploadLogs.Add(uploadlog);
                            context_sub.SaveChanges();
                        }
                    }



                    logger.Info(
                     $"export_Files_For_Offline_Use execution successful.");
                    context.SaveChanges();

                    return "00";
                }
                else
                {
                    //寫error log
                    using (var context_sub = new GatewayContext())
                    {
                        logger.Fatal($"Exception error: Export Fail");
                        ShisUploadLog uploadlog = new ShisUploadLog();
                        uploadlog.Logid = -1;
                        uploadlog.RegDate = null;
                        uploadlog.Option = userChoice != null ? Convert.ToChar(userChoice) : ' ';
                        uploadlog.Inhospid = null;
                        uploadlog.ExecDatetime = DateTime.Now;
                        uploadlog.LocalIp = GetLocalIPAddress();
                        uploadlog.LocalLoginUser = "Admin";
                        uploadlog.TargetAgency = "MOHD";
                        uploadlog.TargetUrl = null;
                        uploadlog.ResultSuccess = false;
                        uploadlog.ResultMessage = exportResult.Message;
                        uploadlog.ResultStatusCode = "999";
                        uploadlog.ResultStatusDesc = "Exception";

                        context_sub.ShisUploadLogs.Add(uploadlog);
                        context_sub.SaveChanges();
                    }
                    return "999";
                }
            }

        }
        catch (Exception ex)
        {
            using (var context_sub = new GatewayContext())
            {
                logger.Fatal($"Exception error: {ex.Message}");
                ShisUploadLog uploadlog = new ShisUploadLog();
                uploadlog.Logid = -1;
                uploadlog.RegDate = null;
                uploadlog.Option = userChoice != null ? Convert.ToChar(userChoice) : ' ';
                uploadlog.Inhospid = null;
                uploadlog.ExecDatetime = DateTime.Now;
                uploadlog.LocalIp = GetLocalIPAddress();
                uploadlog.LocalLoginUser = "Admin";
                uploadlog.TargetAgency = "MOHD";
                uploadlog.TargetUrl = null;
                uploadlog.ResultSuccess = false;
                uploadlog.ResultMessage = ex.Message.ToString();
                uploadlog.ResultStatusCode = "999";
                uploadlog.ResultStatusDesc = "Exception";

                context_sub.ShisUploadLogs.Add(uploadlog);
                context_sub.SaveChanges();
            }


            return "999";
        }
    }



    static string GetLocalIPAddress()
    {
        string ipAddress = string.Empty;


        NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

        foreach (NetworkInterface networkInterface in networkInterfaces)
        {

            if (networkInterface.OperationalStatus == OperationalStatus.Up && !networkInterface.Description.ToLower().Contains("loopback"))
            {
                IPInterfaceProperties ipProperties = networkInterface.GetIPProperties();

                foreach (UnicastIPAddressInformation ipInfo in ipProperties.UnicastAddresses)
                {
                    // 找到IPv4地址
                    if (ipInfo.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        ipAddress = ipInfo.Address.ToString();
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(ipAddress))
                {
                    break;
                }
            }
        }

        return ipAddress;
    }
    static async Task<ResultDTO> GetNewJwtToken()
    {

        ResultDTO result = new ResultDTO() { isSuccess = false };

        string apiUrl = "";
#if DEBUG
        //apiUrl = "https://172.18.8.188/api/auth/jwtlogin";
        apiUrl = _configuration.GetSection("JwtUrls")["Debug"];
#else
        //apiUrl = "https://127.0.0.1/api/auth/jwtlogin";
        apiUrl = _configuration.GetSection("JwtUrls")["Release"];
#endif


        ShisUser mohdData = new ShisUser();
        MohdUser mohdUser = new MohdUser();
        string username = "";
        string password = "";
        //string username = mohdData.UserIdno;
        //string password = mohdData.UserPassword;

        if (hosp_code != "unknown")
        {
            using (var context = new GatewayContext())
            {
                apiUrl = context.ShisCoderefs
                               .Where(c => c.RefCodetype == "MOHD_TK" && c.RefCode == "JWT").Select(c => c.RefName).FirstOrDefault();
                mohdData = context.ShisUsers.Where(c => c.Creator == "MOHDGW").FirstOrDefault();

                if (mohdData == null)
                {
                    logger.Fatal($"mohdData Essential information must not be null: JWT URL / MOHD Account");
                    return result;
                }

                username = mohdData.UserIdno;
                password = mohdData.UserPassword;


                if (string.IsNullOrWhiteSpace(apiUrl) ||
        mohdData == null ||
        string.IsNullOrWhiteSpace(mohdData.UserIdno) ||
        string.IsNullOrWhiteSpace(mohdData.UserPassword))
                {
                    logger.Fatal($"Essential information must not be null: JWT URL / MOHD Account");
                    return result;
                }
            }
        }
        else
        {
            using (var mohd_context = new MOHDGWContext())
            {
                mohdUser = mohd_context.MohdUsers.Where(c => c.UserIdno == "MOH2023").FirstOrDefault();

                if (mohdUser == null)
                {
                    logger.Fatal($"mohdUser Essential information must not be null: JWT URL / MOHD Account");
                    return result;
                }

                username = mohdUser.UserIdno;
                password = mohdUser.UserPassword;
            }
        }


        //var username = mohdData.UserIdno;
        //var password = mohdData.UserPassword;






        //Pass SSL 
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };

        using (var httpClient = new HttpClient(handler))
        {
            // 驗證請求
            var authenticationRequest = new
            {
                Account = username,
                PassWord = password
            };

            var authenticationResponse = await httpClient.PostAsJsonAsync(apiUrl, authenticationRequest);


            if (authenticationResponse.IsSuccessStatusCode)
            {
                // 提取JWT token
                var responeResult = await authenticationResponse.Content.ReadAsStringAsync();

                ResultDTO _result = JsonConvert.DeserializeObject<ResultDTO>(responeResult);

                if (_result.isSuccess == true)
                {
                    // DEMO 
                    logger.Info($"JWT Token: {_result.Message}");
                    Console.WriteLine($"JWT Token: {_result.returnValue}");
                }
                else
                {
                    logger.Fatal($"JWT Token: {_result.Message}");
                }

                return _result;
            }
            else
            {
                logger.Fatal($"Authentication failed with status code: {authenticationResponse.StatusCode}");
                result.Message = authenticationResponse.StatusCode.ToString();
                return result;
            }
        }

    }

    static Root generateRootData(Registration item, ConnectionStatus inStatus)
    {
        try
        {
            using (var context_sub = new GatewayContext())
            {
                if (item == null) return null;

                #region 必要資訊
                var pInfo = context_sub.ShisCharts.Where(c => c.ChrHealthId == item.RegHealthId).FirstOrDefault();
                if (pInfo == null)
                {
                    using (var _errorContext = new GatewayContext())
                    {
                        logger.Fatal($"shis_chart No patient found: {item.RegHealthId}");
                        ShisUploadLog _uploadlog = new ShisUploadLog();
                        _uploadlog.Logid = -1;
                        _uploadlog.RegDate = null;
                        _uploadlog.Option = userChoice != null ? Convert.ToChar(userChoice) : ' ';
                        _uploadlog.Inhospid = item.RegHealthId;
                        _uploadlog.ExecDatetime = DateTime.Now;
                        _uploadlog.LocalIp = GetLocalIPAddress();
                        _uploadlog.LocalLoginUser = "Admin";
                        _uploadlog.TargetAgency = "MOHD";
                        _uploadlog.TargetUrl = null;
                        _uploadlog.ResultSuccess = false;
                        _uploadlog.ResultMessage = "shis_chart No patient found: " + item.RegHealthId;
                        _uploadlog.ResultStatusCode = "999";
                        _uploadlog.ResultStatusDesc = "Exception";

                        _errorContext.ShisUploadLogs.Add(_uploadlog);
                        _errorContext.SaveChanges();
                    }


                    return null;
                    //continue;
                }



                var rInfo = item;
                var soapInfo = context_sub.Hisordersoas.Where(c => c.Inhospid == item.Inhospid && c.Status == 'V').ToList();
                var dxInfo = context_sub.Hisorderplans.Where(c => c.Inhospid == item.Inhospid && c.HplanType == "ICD" && c.DcDate == null).ToList();
                var medInfo = context_sub.Hisorderplans.Where(c => c.Inhospid == item.Inhospid && c.HplanType == "Med" && c.DcDate == null).ToList();
                var othersInfo = context_sub.Hisorderplans.Where(c => c.Inhospid == item.Inhospid && c.HplanType != "Med" && c.HplanType != "ICD" && c.DcDate == null).ToList();
                var orderPlanInfo = context_sub.Hisorderplans.Where(c => c.Inhospid == item.Inhospid && c.DcDate == null).ToList();
                #endregion

                var root = new Root();
                root.HospInfo = new HospInfo()
                {
                    HospCode = hosp_code,
                    HospName = "",
                    HospAddress = "",
                    HospTel = ""
                };
                root.SenderInfo = new SenderInfo()
                {
                    IP = "",
                    Name = ""
                };
                var regdptparent = context_sub.ShisDepartments.Where(d => d.DptCode == rInfo.RegDepartment).Select(d => d.DptParent).SingleOrDefault();
                root.RegInfo = new RegInfo()
                {
                    HospCode = hosp_code,
                    RegDate = rInfo.RegDate,
                    DeptCode = rInfo.RegDepartment,
                    DeptName = context_sub.ShisDepartments.Where(d => d.DptCode == rInfo.RegDepartment).Select(e => e.DptName).FirstOrDefault(),
                    Noon = rInfo.RegNoon,
                    SeqNo = rInfo.RegSeqNo,
                    HealthId = rInfo.RegHealthId,
                    Inhospid = rInfo.Inhospid,
                    Triage = rInfo.RegTriage,
                    BedNo = rInfo.RegBedNo,
                    RegAttribute = rInfo.RegAttribute,
                    AttrDesc = rInfo.RegAttrDesc,
                    DoctorId = rInfo.RegDoctorId,
                    DoctorName = context_sub.ShisUsers.Where(c => c.UserIdno == rInfo.RegDoctorId).Select(d => d.UserNameFirstname + d.UserNameMidname + d.UserNameLastname).FirstOrDefault(),
                    RoomNo = rInfo.RegRoomNo,
                    Status = rInfo.RegStatus,
                    CallTime = rInfo.RegCallTime,
                    StartTime = rInfo.RegStartTime,
                    EndTime = rInfo.RegEndTime,
                    ExamStartTime = rInfo.RegExamStartTime,
                    ExamEndTime = rInfo.RegExamEndTime,
                    CreateTime = rInfo.RegCreateTime,
                    FollowCode = rInfo.RegFollowCode,
                    FollowDesc = rInfo.RegFollowDesc,
                    dept_parent = context_sub.ShisDepartments.Where(d => d.DptCode == rInfo.RegDepartment).Select(d => d.DptParent).SingleOrDefault(),
                    dept_parent_name = context_sub.ShisDepartments.Where(d => d.DptCode == regdptparent).Select(r => r.DptName).SingleOrDefault(),


                };
                root.PatientInfo = new PatientInfo()
                {
                    FromHosp = hosp_code,
                    HealthId = pInfo.ChrHealthId,
                    NationalId = pInfo.ChrNationalId,
                    FName = pInfo.ChrPatientFirstname,
                    MName = pInfo.ChrPatientMidname,
                    LName = pInfo.ChrPatientLastname,
                    Sex = pInfo.ChrSex,
                    BirthDate = pInfo.ChrBirthDate,
                    Mobile = pInfo.ChrMobilePhone,
                    Address = pInfo.ChrAddress,
                    EmgCont = pInfo.ChrEmgContact,
                    ContRel = pInfo.ChrContactRelation,
                    ContPhone = pInfo.ChrContactPhone,
                    CombineFlag = pInfo.ChrCombineFlag,
                    Remark = pInfo.ChrRemark,
                    ModifyTime = pInfo.ModifyTime,
                    RefugeeFlag = pInfo.ChrRefugeeFlag
                };
                root.SOAP = new List<SOAP>() { };
                root.Med = new List<OrderPlan> { };
                root.Dx = new List<OrderPlan> { };
                root.Others = new List<OrderPlan> { };


                if (soapInfo != null && soapInfo.Count > 0)
                {
                    foreach (Hisordersoa _soap in soapInfo)
                    {
                        root.SOAP.Add(new SOAP()
                        {
                            Soaid = _soap.Soaid,
                            Inhospid = _soap.Inhospid,
                            HealthId = _soap.HealthId,
                            Kind = _soap.Kind,
                            Context = _soap.Context,
                            CreateTime = _soap.CreateDate,
                            ModifyTime = _soap.ModifyDate,
                            SourceType = _soap.SourceType,
                            VersionCode = _soap.VersionCode,
                            Status = _soap.Status,
                            HospCode = hosp_code
                        });
                    }
                }
                if (orderPlanInfo != null && orderPlanInfo.Count > 0)
                {
                    foreach (Hisorderplan _plan in orderPlanInfo)
                    {
                        var orderplan = new OrderPlan()
                        {
                            HospCode = hosp_code,
                            Orderplanid = _plan.Orderplanid,
                            Inhospid = _plan.Inhospid,
                            HealthId = _plan.HealthId,
                            HplanType = _plan.HplanType,
                            SeqNo = _plan.SeqNo,
                            PlanCode = _plan.PlanCode,
                            PlanDes = _plan.PlanDes,
                            FreeCharge = _plan.FreeCharge,
                            ExecDateFrom = _plan.ExecDateFrom,
                            ExecDateTo = _plan.ExecDateTo,
                            OrderDept = _plan.OrderDept,
                            OrderDr = _plan.OrderDr,
                            PlanDays = _plan.PlanDays,
                            QtyDose = _plan.QtyDose,
                            QtyDaily = _plan.QtyDaily,
                            UnitDose = _plan.UnitDose,
                            FreqCode = _plan.FreqCode,
                            DoseIndi = _plan.DoseIndication,
                            DosePath = _plan.DosePath,
                            MadeType = _plan.MadeType,
                            TotalQty = _plan.TotalQty,
                            ExamLoc = _plan.ExamLoc,
                            UrgFlag = _plan.UrgFlag,
                            PreopFlag = _plan.PreopFlag,
                            AddFlag = _plan.AddFlag,
                            KeepspecFlag = _plan.KeepspecFlag,
                            LocationCode = _plan.LocationCode,
                            TriggerTablecode = _plan.TriggerTablecode,
                            TriggerRecid = _plan.TriggerRecid,
                            Status = _plan.Status,
                            ExecStatus = _plan.ExecStatus,
                            ChargeStatus = _plan.ChargeStatus,
                            CreateDate = _plan.CreateDate,
                            ModifyDate = _plan.ModifyDate,
                            Remark = _plan.Remark,
                            MedBag = _plan.MedBag

                        };

                        switch (orderplan.HplanType)
                        {
                            case "ICD":
                                root.Dx.Add(orderplan);
                                break;
                            case "Med":
                                root.Med.Add(orderplan);
                                break;
                            default:
                                root.Others.Add(orderplan);
                                break;
                        }
                    }
                }

                return root;
            }
        }
        catch
        {
            return null;
        }
    }

}
