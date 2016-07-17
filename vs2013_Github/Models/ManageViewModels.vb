Imports System.ComponentModel.DataAnnotations
Imports Microsoft.AspNet.Identity
Imports Microsoft.Owin.Security

Public Class IndexViewModel
    Public Property HasPassword As Boolean
    Public Property Logins As IList(Of UserLoginInfo)
    Public Property PhoneNumber As String
    Public Property TwoFactor As Boolean
    Public Property BrowserRemembered As Boolean
End Class

Public Class ManageLoginsViewModel
    Public Property CurrentLogins As IList(Of UserLoginInfo)
    Public Property OtherLogins As IList(Of AuthenticationDescription)
End Class

Public Class FactorViewModel
    Public Property Purpose As String
End Class

Public Class SetPasswordViewModel
    <Required>
    <StringLength(100, ErrorMessage:="{0} 的長度至少必須為 {2} 個字元。", MinimumLength:=6)>
    <DataType(DataType.Password)>
    <Display(Name:="新密碼")>
    Public Property NewPassword As String

    <DataType(DataType.Password)>
    <Display(Name:="確認新密碼")>
    <Compare("NewPassword", ErrorMessage:="新密碼與確認密碼不相符。")>
    Public Property ConfirmPassword As String
End Class

Public Class ChangePasswordViewModel
    <Required>
    <DataType(DataType.Password)>
    <Display(Name:="目前密碼")>
    Public Property OldPassword As String

    <Required>
    <StringLength(100, ErrorMessage:="{0} 的長度至少必須為 {2} 個字元。", MinimumLength:=6)>
    <DataType(DataType.Password)>
    <Display(Name:="新密碼")>
    Public Property NewPassword As String

    <DataType(DataType.Password)>
    <Display(Name:="確認新密碼")>
    <Compare("NewPassword", ErrorMessage:="新密碼與確認密碼不相符。")>
    Public Property ConfirmPassword As String
End Class

Public Class AddPhoneNumberViewModel
    <Required>
    <Phone>
    <Display(Name:="電話號碼")>
    Public Property Number As String
End Class

Public Class VerifyPhoneNumberViewModel
    <Required>
    <Display(Name:="代碼")>
    Public Property Code As String

    <Required>
    <Phone>
    <Display(Name:="電話號碼")>
    Public Property PhoneNumber As String
End Class

Public Class ConfigureTwoFactorViewModel
    Public Property SelectedProvider As String
    Public Property Providers As ICollection(Of System.Web.Mvc.SelectListItem)
End Class