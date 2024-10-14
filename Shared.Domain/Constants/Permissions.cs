using System.ComponentModel;

namespace Shared.Domain.Constants;

public enum Permissions
{
    #region
    [Description("Transaction History View")]
    Transaction_History_View = 1,
    
    [Description("Transaction Tagging")]
    Transaction_Tagging = 2,
    
    
    //
    [Description("Bills View")]
    Bills_Open_View = 11,
    
    [Description("Bills Fully Paid View")]
    Bills_Fully_Paid_View = 12,
    
    
    [Description("Bills Closed View")]
    Bills_Closed_View = 13,
    
    [Description("Bills Payment")]
    Bills_Payment = 14,
    
    [Description("Bills Upload Proof")]
    Bills_Upload_Proof = 15,
    
    [Description("Bills Download Invoice")]
    Bills_Download_Invoice = 16,
    
    //
    [Description("Reports View")]
    Reports_View = 21,
    
    [Description("Reports Create")]
    Reports_Create = 22,
    
    [Description("Reports Edit")]
    Reports_Edit = 23,
    
    [Description("Reports Delete")]
    Reports_Delete = 24,
    
    //
    
    [Description("Users View")]
    Users_View = 31,
    
    [Description("Users Create")]
    Users_Create = 32,
    
    [Description("Users Edit")]
    Users_Edit = 33,
    
    [Description("Users Delete")]
    Users_Delete = 34,
    
    //
    
    [Description("Solutions Module View")]
    Solutions_View = 41,
    
    [Description("Solutions Create")]
    Solutions_Create = 42,
    
    [Description("Solutions Edit")]
    Solutions_Edit = 43,
    
    [Description("Solutions Delete")]
    Solutions_Delete = 44,
    
    
    #endregion
}