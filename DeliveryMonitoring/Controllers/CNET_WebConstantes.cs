namespace CNET_ERP_V7.WebConstants
{
    public static class CNET_WebConstantes
    {
        public static string ClaimsIssuer => "cnetERP";
        public static string CookieScheme => "cnet.erp.v6";
        public static string IdentificationCookie => "cnet.erp.v6.myId";
        public static string BranchCookie => "cnet.erp.v6.currentBranch";
        public const string CNET_ERP2016 = "CoMP000000001";

        public const int IdentificationCookieLifeTime = 10080;
        public const int IdentificationCookieDailyLifeTime = 1440;
        public const int VOUCHER_RELATION_TYPE_NES_REF = 1964;
        public const int VOUCHER_RELATION_TYPE_INT_REF = 1962;
        public const int FORMULATION = 274;
        public const string ROUTINGTIN = "0076217301";

        public const int ARTICLE_RELATION_TYPE_FAMILY = 48;
        public const int ARTICLE_RELATION_TYPE_SUBSTITIUTE = 50;
        #region 
        public const string TAX_PRIORITY_REFERENCE = "REFERENCE";
        public const string MandatoryVoucherErrorType = "Mandatory";
        public const string WarningVoucherErrorType = "Warning";
        public const string Period_Type = "Period Type";
        public const string PERIOD_TYPE_SHIFT = "Shift Period";
        public const int NormalTransactionVoucher = 1973;

        public const int ConsigneeUnit_Branch = 1719;
        #endregion

        #region PAYMENT METHODS      
        //public const string PAYMNET_METHOD_CHECK = "LKUP000000503";
        public const int PAYMENT_METHOD_TRANSFER_LETTER = 1755;
        public const int PAYMENTMETHODSCASH = 1748;

        //public const string PAYMNET_METHOD_CPO = "LKUP000000504";
        //public const string PAYMNET_METHOD_CREDITCARD = "LKUP000000505";
        //public const string PAYMNET_METHOD_DEBITCARD = "LKUP000000506";
        //public const string PAYMNET_METHOD_GIFTCARD = "LKUP000000507";
        //public const string PAYMENTMETHODS_DIRECT_BILL = "LKUP000001468";
        //public const string PAYMNET_METHOD_ENTERTAINMENT = "LKUP000006613";
        //public const string PAYMNET_METHOD_FOODALLOWANCE = "LKUP000006614";
        #endregion

        #region TaxTypeList
        //----TAxtYpeList------
        public const int VAT = 1;
        public const int TOT1 = 2;
        public const int TOT2 = 3;
        public const int NONTAXABLE = 4;
        public const int WITHOLDING = 6;
        // public const int PAYROLLTAX = 6;
        public const int INCOMETAX = 5;
        //  public const int DEVIDEND = 8;
        // public const int EXISETAX = 9;
        //  public const int SURETAX = 10;
        // public const int CUSTOMEWITHHOLDING = 11;

        #endregion

        #region "VoucherTypeList
        public const int ROW_MATERIAL_CONSUMPTION_VOUCHER = 285;
        public const int VARIATION = 275;
        public const int PACKINGLIST = 276;
        public const int BATCH = 271;
        #endregion

        #region Activity Definition
        public const int ACTIVITY_ISSUED = 1031;
        public const int ACTIVITY_SEEN = 1053;
        public const int ACTIVITY_RATE_CHANGED = 1068;
        public const int ACTIVITY_DEFINATION_HOLD = 1030;

        public const int ACTIVITY_DEFINATION_SPLIT = 1033;
        public const int ACTIVITY_DEFINATION_BREAK = 1036;
        public const int ACTIVITY_DEFINATION_REGISTRATION_DETAIL_AMENDED = 1076;
        public const int ACTIVITY_DEFINATION_PREPARED = 1001;
        public const int ACTIVITY_DEFINATION_MAINTAINED = 1000;
        public const int ACTIVITY_DEFINATION_TRANSFRED_FROM = 1057;
        public const int ACTIVITY_DEFINATION_SETTELE = 1055;
        public const int ACTIVITY_DEFINATION_REMOVED = 1049;
        public const int ACTIVITY_DEFINATION_VOID = 1005;
        public const int ACTIVITY_DEFINATION_Cancel = 1026;
        public const int ACTIVITY_DEFINATION_Dispatch = 1027;
        //public const int ACTIVITY_DEFINATION_Confirmed = "LKUP000116840";
        public const int ACTIVITY_DEFINATION_PUSHED = 1045;
        public const int ACTIVITY_DEFINATION_PAYMENDEMETHODCHANGED = 1041;

        public const int ACTIVITY_EDIT = 1022;
        public const int ACTIVITY_APPROVED = 1004;
        public const int ACTIVITY_CHECK = 1002;

        public const int Activity_SyncOut = 1009;
        public const int Activity_SyncIn = 1008;
        public const int ACTIVITY_DEFINITION_JOURNAL = 1032;



        #endregion

        #region Period Type
        public const int Accounting_Period = 1767;
        public const int Fiscal_year_Period = 1768;
        public const int Holiday_Period = 1769;
        public const int PMS_Period = 1770;
        public const int Quarter_Period = 1771;
        public const int Seasonal_Message = 1772;
        public const int Seasonal_Price = 1773;
        public const int Semi_annual = 1774;
        public const int Shift_Period = 1775;
        #endregion

        #region Accounting
        public const int BeginningTrsxTypeWithdarwal = 0;
        public const int BeginningTrsxTypeDeposit = 1;

        public static string SALES_JOURNAL = "SJ";
        public static string RECIPT_JOURNAL = "RJ";
        public static string DISBURSMENT_JOURNAL = "DJ";
        public static string PURCHASE_JOURNAL = "PJ";
        public static string GENERAL_JOURNL = "GJ";
        #endregion

        #region Bank Reconcilation
        public const int Reconciliation_NotReconciled = 0;
        public const int Reconciliation_Reconciled = 1;
        public const int Reconciliation_IncomeInterest = 2;
        public const int Reconciliation_ServiceCharge = 3;
        public const int CART = 901;//elements
        #endregion

        #region password change
        public const int LUK_Password_Change = 1663;
        public const int Security = 667;//Module
        public const int EMPLOYEE = 26;
        #endregion

        #region PMS
        //Tax Table
        //public const int VAT = 1;

        //System Constant Consignee unit type branch
        public const int ORG_UNIT_TYPE_Business_Unit = 1718;
        public const int ORG_UNIT_TYPE_BRUNCH = 1719;
        public const int ORG_UNIT_TYPE_STORE = 1727;
        public const int ORG_UNIT_TYPE_HOTEL_And_Accommodation = 1989;



        //System Constant Gsltype
        public const int COMPANY = 1;
        public const int ITEM = 11;
        public const int PRODUCT = 13;
        public const int GUEST = 27;
        public const int CUSTOMER = 28;
        public const int SERVICES = 14;
        public const int CONTACT = 30;
        public const int Employee = 26;
        public const int GROUP = 41;
        public const int AGENT = 31;
        public const int BUSINESSsOURCE = 42;
        public const int BANK = 38;

        //SystemConstant Maintype
        public const int CONSIGNEE = 4;
        public const int ARTICLE = 3;

        //SystemConstant Voucher Def
        public const int REFUND = 121;
        public const int MESSAGE = 351;
        public const int CASH_SALES = 106;
        public const int CASHRECIPT = 197;
        public const int CREDITSALES = 111;
        public const int BANKDEBITNOTE = 194;
        public const int INTERNAL_MESSAGE = 236;
        public const int PAID_OUT_VOUCHER = 353;
        public const int CREDIT_NOTE_VOUCHER = 344;
        public const int REGISTRATION_VOUCHER = 354;
        public const int LOST_AND_FOUND_VOUCHER = 350;
        public const int CHECK_OUT_BILL_VOUCHER = 343;
        public const int SERVICE_REQUEST_VOUCHER = 355;
        public const int DAILY_ROOM_CHARGE_VOUCHER = 346;
        public const int PAYMENTMETHODS_DIRECT_BILL = 1754;
        public const int STOREREQUSTION = 171;





        //System Constant Table
        public const int TABLE_PACKAGE_DETAIL = 800;
        public const int TABLE_RATE_STRATEGY = 816;
        public const int TABLE_RATE_CODE_HEADER = 810;
        public const int TABLE_ROOM_DETAIL = 817;
        public const int TABLE_RATE_CODE_DETAIL = 807;
        public const int TABLE_ROOM_TYPE = 819;



        //System Constant Specific Id
        //Space Type
        public const int Space_Blook = 1892;
        public const int Space_Floor = 1893;
        public const int Space_Room = 1896;

        //Space Category
        public const int Hotel_Rooms_Pointer = 1887;
        public const int Hotel_Halls_pointer = 1887;



        // System Constant Housekeeping Credits
        public const int STAYOVER_CREDIT = 1643;
        public const int TURNDOWN_CREDIT = 1644;
        public const int DEPARTURE_CREDIT = 1640;
        public const int PICKUP_CREDIT = 1642;
        public const int DAYSECTION_CREDIT = 1639;
        public const int EVENING_CREDIT = 1641;

        //System Constant SubSystem
        public const int PMS_Pointer = 51;







        // System Constant  Posting Rhythm
        public const int POST_EVERY_NIGHT = 1736;
        public const int POST_ON_ARRIVAL_NIGHT = 1739;
        public const int POST_ON_EVERY_X_NIGHTS_STARTING_NIGHT_Y = 1741;
        public const int POST_ON_CERTAIN_NIGHTS_OF_THE_WEEK = 1740;
        public const int POST_ON_LAST_NIGHT = 1742;
        public const int POST_EVERY_NIGHT_EXCEPT_ARRIVAL_NIGHT = 1737;
        public const int POST_EVERY_NIGHT_EXCEPT_LAST = 1738;
        public const int CUSTOM_POSTING_BASED_ON_STAY = 1734;



        //SystemConstant Frontoffice ObjectState Definition
        public const int SIX_PM_STATE = 1218;
        public const int NO_SHOW_STATE = 1224;
        public const int GAURANTED_STATE = 1220;
        public const int CHECKED_IN_STATE = 1223;
        public const int OSD_CANCEL_STATE = 1219;
        public const int CHECKED_OUT_STATE = 1221;
        public const int OSD_PENDING_STATE = 1303;
        public const int OSD_WAITLIST_STATE = 1217;
        public const int ONLINE_CHECKED_OUT_STATE = 1216;





        public const int Activity_Updated = 1058;


        public const int OSD_UNREAD_STATE = 1329;

        public const int OSD_PREPARED_STATE = 1305;


        //PMS Setting Attribute Value
        public const string PMS_SETTING_CheckInTime = "CheckInTime";
        public const string PMS_SETTING_CheckOutTime = "CheckOutTime";
        public const string PMS_SETTING_LateCheckoutTime = "LateCheckoutTime";
        public const string PMS_SETTING_UndoCheckinMin = "UndoCheckinMin";
        public const string PMS_SETTING_NightAuditTime = "NightAuditTime";
        public const string PMS_SETTING_MinRateAdjustment = "MinRateAdjustment";
        public const string PMS_SETTING_MattressAmount = "MattressAmount";
        public const string PMS_SETTING_CustHighBalance = "CustHighBalance";
        public const string PMS_SETTING_ArchivePath = "ArchivePath";
        public const string PMS_SETTING_ArchivePrint = "ArchivePrint";
        public const string PMS_SETTING_LateCheckoutRequiredPayment = "LateCheckoutRequiredPayment";
        public const string PMS_SETTING_LateCheckoutAdditionPayment = "LateCheckoutAdditionPayment";
        public const string PMS_SETTING_UseLateCheckout = "UseLateCheckout";
        public const string PMS_SETTING_EnforceCheckoutCardReturn = "EnforceCheckoutCardReturn";
        public const string PMS_SETTING_LostCardFeeArticle = "LostCardFeeArticle";
        public const string PMS_SETTING_LabelPrinter = "Label Printer";
        public const string PMS_SETTING_LabelDesignFile = "Label Design File";
        public const string PMS_SETTING_PostineRoutine = "NightAuditPostingRoutine";
        public const string PMS_SETTING_DefaultFiscalBillType = "DefaultFiscalBillType";
        public const string PMS_SETTING_EnableJournalization = "EnableJournalization";
        public const string PMS_SETTING_ValidateReference = "ValidateExternalReference";
        public const string PMS_SETTING_ChargeAtCheckIn = "ChargeAtCheckin";
        public const string PMS_SETTING_EarlyCheckIn = "Early CheckIn";
        public const string PMS_SETTING_EarlyCheckInChargeMandatory = "Early CheckIn Charge Mandatory";
        public const string PMS_SETTING_EarlyCheckInUntilTime = "Early CheckIn Until Time";
        public const string PMS_SETTING_EarlyCheckInArticle = "Early CheckIn Article";
        public const string PMS_SETTING_FTPFolderName = "FTP Folder Name";
        public const string PMS_SETTING_EnableCloudReporting = "Enable Cloud Reporting";
        public const string PMS_SETTING_GenerateReportForNullData = "Generate Report For Null Data";
        public const string PMS_SETTING_EnforceClosing = "Enforce Closing";
        public const string PMS_SETTING_ClosingPeriod = "Closing Period";
        public const string PMS_SETTING_ClosingFrequency = "Closing Frequency";
        public const string PMS_SETTING_SummaryVoucher = "Summary Voucher";
        public const string PMS_SETTING_ROOMServicePOS = "Room Service POS";
        public const string PMS_SETTING_MessageCheckFrequency = "Message Check Frequency";
        public const string PMS_SETTING_ShowOnlineBusinessRate = "Show Online Business Rate";


        public const int RATE_STRATEGY_RESTRICTION_OPEN = 1816;
        public const int RATE_STRATEGY_RESTRICTION_CLOSED = 1815;
        public const int RATE_STRATEGY_CONDITION_OPEN = 1814;
        public const int RATE_STRATEGY_CONDITION_OCCUPIED = 1813;

        public const int MAINTAINED = 1000;



        //public const string SECURITYRegistrationDocument = 667;
        //Table
        public const int VOUCHER_COMPONENET = 767;


        //System Constant Activity lookup
        public const int LU_ACTIVITY_SEEN = 1053;
        public const int LU_ACTIVITY_DEFINATION_RATE_ADJUSTED = 1067;
        public const int LU_ACTIVITY_DEFINATION_CheckIN = 1065;
        public const int LU_ACTIVITY_DEFINATION_Waitinglist = 1060;
        public const int LU_ACTIVITY_DEFINATION_Guaranteed = 1062;
        public const int LU_ACTIVITY_DEFINATION_MESSAGEMADE = 1092;
        public const int LU_ACTIVITY_DEFINATION_ACCOMPYINGGUESTADDED = 1077;
        public const int LU_ACTIVITY_DEFINATION_DATE_AMENDED = 1081;
        public const int LU_ACTIVITY_ISSUED = 1031;
        public const int LU_ACTIVITY_DEFINATION_6PM = 1061;
        public const int LU_ACTIVITY_DEFINATION_PREPARED = 1001;
        public const int LU_ACTIVITY_DEFINATION_SPLIT = 1033;
        public const int LU_ACTIVITY_DEFINATION_ROOM_POS_CHARGE_TRANSFERED = 1075;
        public const int LU_ACTIVITY_SERVICE_REQUESTED = 1089;
        public const int LU_ACTIVITY_DEFINATION_Cancel = 1026;
        public const int LU_ACTIVITY_DEFINATION_ROOM_POS_CHARGE_MADE = 1073;
        public const int LU_ACTIVITY_DEFINATION_ROOM_MOVED = 1072;
        public const int LU_ACTIVITY_RATE_CHANGED = 1068;
        public const int LU_ACTIVITY_DEFINATION_REINSTATE = 1048;
        public const int LU_ACTIVITY_DEFINATION_VOID = 1005;
        public const int LU_ACTIVITY_DEFINATION_PREVILAGED = 1070;
        public const int LU_ACTIVITY_DEFINATION_Pending = 1042;
        public const int LU_ACTIVITY_DEFINATION_NOSHOW = 1064;
        public const int LU_ACTIVITY_DEFINATION_ADD_ON_MADE = 1083;
        public const int LU_ACTIVITY_DEFINATION_REGISTRATION_DETAIL_AMENDED = 1076;
        public const int LU_ACTIVITY_DEFINATION_SHAREMADE = 1084;
        public const int LU_ACTIVITY_DEFINATION_TRANSFRED = 1057;
        public const int LU_ACTIVITY_DEFINATION_ROOMCHARGE = 1071;
        public const int LU_ACTIVITY_BILL_RETURNED = 1052;
        public const int LK_ROOM_SHARE = 1084;
        public const int LU_ACTIVITY_DEFINATION_CheckOut = 1066;
        public const int LU_ACTIVITY_DEFINATION_ProfileAmended = 1069;
        public const int LU_ACTIVITY_DEFINATION_BREAK = 1036;



        //System Constant Category
        public const string PAYMENT_METHODS = "Payment Methods";
        public const string SPLIT_WINDOWS = "Split Windows";
        public const string PACKAGE_POSTING_RHYTHM = "Package Posting Rhythm";
        public const string PACKAGE_CALCULATION_RATE = "Package Calculation Rate";
        public const string EXCHANGE_RULE = "ExchangeRule";
        public const string RATE_STRATEGY_RESTRICTION_TYPE = "Rate Strategy Restriction Type";
        public const string RATE_STRATEGY_CONDITION = "Rate Strategy Condition";
        public const string Hotel_Rating = "Hotel Rating";
        public const string OSD_Category_Transaction = "Transaction";
        public const string RESERVATION_TYPE = "Reservation Type";
        public const string CREDIT_CARD_TYPES = "Credit Card";
        public const string GENDER = "Gender";
        public const string BUSINESSTYPE = "Business Type";
        public const string MESSAGE_TYPE = "Message Type";


        //System Constant Passport Scanner Type
        public const string WINTONE_PASSPORT_SCANNER = "WINTONE-PSPR1000";
        public const string ARH_PASSPORT_SCANNER = "ARH-PRMC";
        public const string CNET_FLATBED_SCANNER = "CNET Standard Scanner";


        //system constnant Device
        public const int DEVICE_DOOR_LOCK = 584;
        public const int DEVICE_SIGNATURE = 587;
        public const int PASSPORT_SCANNER = 577;
        public const int ORDERPRINTER = 550;



        //System Constant Split window
        public const int DEFAULT_WINDOW = 1902;


        //System Constant Paymnet Method
        //public const int PAYMENTMETHODSCASH = 1748;
        public const int PAYMNET_METHOD_CHECK = 1751;
        public const int PAYMENTMETHODSMobileMoney = 1753;
        public const int PAYMNET_METHOD_CREDITCARD = 1750;

        //Reservation Type
        public const int RESERVATIONTYPECHECKIN = 1836;

        //Roomcharge ExchangeRule
        public const int ArrivalDayRate = 1613;
        public const int ReservationDayRate = 1615;
        //Message Type
        public const int GUEST_MESSAGE = 1667;
        public const int WELCOME_MESSAGE = 1671;

        //System Constant  UOM
        public const int UNITOFMEASURMENTPCS = 1400;
        //System Constant VoucherRelation Type
        public const int VOUCHER_RELATION_TYPE_VOUCHER_EXT = 1965;

        //System Constant Schedule Category
        public const int WAKEUP_SCHEDULE_Category = 1839;



        //System Constant Housekeeping Activities
        public const int CLEAN = 1085;
        public const int INSPECTED = 1003;
        public const int Dirty = 1086;
        public const int OOO = 1087;
        public const int OOS = 1088;
        public const int PICKUP = 1105;

        //System Constant Housekeeping ObjectState
        public const int OS_CLEAN = 1225;
        public const int OS_INSPECTED = 1227;
        public const int OS_Dirty = 1226;
        public const int OS_OOO = 1228;
        public const int OS_OOS = 1229;
        public const int OS_PICKUP = 1230;

        //Gender
        public const int FEMALE = 1631;
        public const int MALE = 1632;





        //System Constant Value Factor Type
        public const int ADDTIONAL_CHARGE = 1956;
        public const int DISCOUNT = 1959;



















        //lookup Calculation Rule
        public const int Flat_Rate = 90;
        public const int Per_Person = 94;
        public const int Per_Adult = 91;
        public const int Per_Child = 93;
        public const int Per_Room = 95;



        //lookup
        public const int PERSONAL_IDENTIFICATIONTYPE_PASPORT = 576;
        public const int PERSONAL_IDENTIFICATIONTYPE_NationalID = 578;
        public const int ORGANIZATION_IDENTIFICATION_TYPEVAT = 541;
        public const int TD_PICK_UP = 14;
        public const int TD_DROP_OFF = 12;
        public const int WAKEUP_SCHEDULE_TYPE = 854;

        //lookup Travel Detail Type
        public const int TravelDetail_Arrival = 108;
        public const int TravelDetail_Departure = 109;

        //Lookup Type 
        public const string PACKAGE_GROUP = "Package Group";
        public const string PACKAGE_TYPE = "Package Type";
        public const string PACKAGE_RATE_APPEARANCE = "Package Rate Appearance";
        public const string RATE_CATEGORY = "Rate Category";
        public const string RATE_CATEGORY_DESCRIPTION = "Rate Categoty Description";
        public const string RATE_COMPONENT = "Rate Component";
        public const string BUSSINESS_SOURCE = "Business Source";
        public const string MARKET = "Markets";
        public const string ROOM_CLASS = "Room Class";
        public const string SPACE_ARRANGEMENT = "Space Arrangement";
        public const string HOTEL_FEATURE = "Hotel Feature";
        public const string ROOM_FEATURE = "Room Feature";
        public const string SERVICE_REQUEST = "Service Request";
        public const string STATION = "Station";
        public const string CARRIER = "Carrier";
        public const string ACTION_REQUIRED = "ActionRequired";
        public const string POST_MASTER_REASON = "PM Reason";
        public const string MEMBER_TYPE = "Member Type";
        public const string DISCOUNT_REASONS = "Discount Reason";
        public const string LIST_OF_ORGIN = "List of Origin";
        public const string SPECIALS = "Specials";
        public const string TravelPurpose = "Travel Purpose";
        public const string Room_Move_Reason = "Room Move Reason";
        public const string PERSONALTITLE = "Personal Title";
        public const string PersonalIdentificationType = "Personal Identification Type";
        public const string PRIVACYRULE = "Privacy Rule";
        public const string RELIGION = "Religion";
        public const string TRANSPORTATION_TYPE = "Transportation Type";
        public const string RATE_ADJUSTMENT_REASON = "Rate Adjustment Reason";

        //lookup transaction type
        public const int TRANSACTIONTYPENORMALTXN = 1973;

        //lookup Voucher Extension Definition
        public const int VOUCHER_EXTENTION_DEFINITION_DOOR_LOCK = 1113;

        public const int PERSONAL_IDENTIFICATIONTYPE_VISSA = 589;




        //public const int VOUCHER_RELATION_TYPE_NES_REF = 1964;
        //public const int VOUCHER_RELATION_TYPE_INT_REF = 1962;
        public const int VOUCHER_RELATION_TYPE_EXT_REF = 1965;






        //night audit reports
        public const string TRIAL_BALANCE = "Trial Balance";
        public const string RATE_ADJUSTMENT = "Rate Adjustment Report";
        public const string DAILY_RESIDENNT_SUMMARY = "Daily Resident Summary";
        public const string CANCELATION_OF_THE_DAY = "Cancellation of the Day";
        public const string CASHIER_SUMMARY = "Cashier Summary";
        public const string CHECK_REPORT_OF_THE_DAY = "Check Report of the Day";
        public const string CHECK_OUT_REPORT = "Checkout Report";
        public const string CITY_LEDGER = "City Ledger";
        public const string CREDIT_CARDS_OF_THE_DAY = "Credit Cards of the Day";
        public const string DAILY_BUSINESS_REPORT = "Daily Business Report";
        public const string DEPOSIT_LEDGER = "Deposit Ledger";
        public const string NO_SHOW_REPORT = "No Show Report";
        public const string PACKAGE_REPORT = "Package Report";
        public const string RATE_CHECK_REPORT = "Rate Check Report";
        public const string DAILY_SALES_SUMMARY = "Daily Sales Summary";
        public const string CASH_DROPPED_REPORT = "Cash Dropped Report";
        public const string ROOM_INCOME_REPORT = "Room Income Report";
        public const string MANAGERIAL_FLASH = "Managerial Flash";
        public const string FOREIGN_EXCHANGE_DETAIL = "Foreign Exchange Detail";
        public const string FOREIGN_EXCHANGE_SUMMARY = "Foreign Exchange Summary";
        public const string NATIONALITY_REPORT = "Nationality Report";

        //Transaction Reports
        public const string REBATE_REPORT = "Rebate Report";
        public const string CASH_RECEIPT_REPORT = "Cash Receipt Report";
        public const string PAID_OUT_REPORT = "Paid Out Report";
        public const string CREDIT_SALES_REPORT = "Credit Sales Report";
        public const string CASH_SALES_REPORT = "Cash Sales Report";
        public const string DEBIT_NOTE_REPORT = "Debit Note Report";
        public const string REFUND_REPORT = "Refund Report";
        public const string DAILY_ROOM_CHARGE_REPORT = "Daily Room Charge Report";
        public const string ROOM_POS_CHARGES = "Room POS Charges Report";
        public const string PMS_Fiscal_Reconciliation = "Fiscal Reconciliation";

        //house keeping reports
        public const string DISCRIPANCY_REPORT = "Discripancy Report";
        public const string HK_ACTIVITY_REPORT = "HK Activity Report";
        public const string HK_ROOMSTATUS_REPORT = "HK Room Status Report";
        //public const string HK_ATTENDANTS_REPORTS = "Rp000005002";
        public const string STATUS_REPORT = "Status Report";
        public const string TASK_ASSIGNMENT_REPORT = "Task Assignemnt Report";


        //other reports
        public const string ARRIVAL_LIST = "Arrival List";
        public const string DEPARTED_LIST = "Departed List";
        public const string DETAIL_DAILY_SALES_TRANSACTION = "Detail Daily Sales Transaction";
        public const string DROP_OFF_REPORT = "Drop Off Report";
        // public const string GUEST_INHOUSE_LIST = "Rp000005028";
        public const string PICK_UP_REPORT = "Pickup Report";
        public const string POLICE_REPORT = "Police Report";
        public const string POST_MASTER_INHOUSE_LIST = "Postmaster In-House List";
        public const string ROOM_MOVE = "Room Move";
        public const string ARRIVED_LIST = "Arrived List";
        public const string DUE_OUTS = "Due Outs";
        public const string STAY_OVERS = "Stay Overs";
        public const string SUMMARY_OF_SUMMARY = "Summary Of Summary Report";
        public const string UNASSIGNED_RESERVATION = "Unassigned Reservations";
        public const string BILL_TRANSFER_REPORT = "Bill Transfer Report";

        // HouseKeeping
        public const string status_clean = "Clean";
        public const string status_dirty = "Dirty";
        public const string status_ooo = "Out Of Order";
        public const string status_oos = "Out Of Service";
        public const string status_inspected = "Inspected";
        public const string status_pickup = "Pickup";


        public static int CASHSALESSUMMRY = 105;
        public static int LU_ACTIVITY_DEFINATION_BUCKETCHECKED = 1078;
        public static int PERIOD_TYPE_PMS = 1770;
        public static int PERIOD_TYPE_Accounting = 1767;
        public static int ORDER = 102;
        public static int OSD_HELD_STATE = 1294;
        public static int Daily_Resident_Summary_Voucher = 345;
        public static int LU_ACTIVITY_DEFINATION_PUSHED = 1045;

        public const int PMS_Report = 3017;
        public const int Report_Category_Night_Audit = 3037;


        public static int HouseKeeping_Mgt = 954;

        public static int EVENT_REQUIREMENT_VOUCHER = 348;
        public static int PACKAGE_CONSUMPTION_VOUCHER = 352;
        public static int LU_ACTIVITY_DEFINATION_ROOM_POS_CHARGE_REMOVED = 1074;

        public static int HK_MAINTAINED = 1000;
        public static int ORGUNITTYPE_DEPARTMENT = 1721;

        public const string HOUSE_KEEPING_ATTENDANT = "Housekeeping Attendant";

        public const string TurnDown = "Turndown Status";
        public const string TD_Required = "Required";
        public const string TD_NotRequired = "Not Required";

        public static int Voucher = 767;
        #endregion
        #region Payment Units
        public static int Telebirr_OTP = 4; 
        public static int Telebirr_USSD_Push = 5;  
        public static int Telebirr_QR_Code = 6;//no
        public static int M_PESA_Customer_initated = 7;
        public static int M_PESA_USSD_Push = 8;
        public static int M_PESA_QR_Code = 9; //no    
        public static int CBE_Birr_USSD_Push = 10;//no
        public static int CBE_Birr_QRCode = 11;//no
        public static int Apollo_USSD_Push = 12;//no
        public static int Cyber_source = 13;//no
        public static int Apollo_QR_Code = 14;  
        public static int Amole_OTP = 15;
        public static int Amole_Gift_card = 16;
        public static int Amole_QR_code = 17;//no
        public static int ET_Switch_card = 18;//no
        public static int E_Birr_USSD_Push = 19;
        public static int E_Birr_QR_Code = 20; 
        public static int hulubeje_giftcard_otp = 20;//no
        #endregion
    }
}
