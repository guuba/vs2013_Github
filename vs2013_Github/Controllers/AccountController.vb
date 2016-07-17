Imports System.Globalization
Imports System.Security.Claims
Imports System.Threading.Tasks
Imports Microsoft.AspNet.Identity
Imports Microsoft.AspNet.Identity.Owin
Imports Microsoft.Owin.Security
Imports Owin

<Authorize>
Public Class AccountController
    Inherits Controller
    Private _signInManager As ApplicationSignInManager
    Private _userManager As ApplicationUserManager

    Public Sub New()
    End Sub

    Public Sub New(appUserMan As ApplicationUserManager, signInMan As ApplicationSignInManager)
        UserManager = appUserMan
        SignInManager = signInMan
    End Sub

    Public Property SignInManager() As ApplicationSignInManager
        Get
            Return If(_signInManager, HttpContext.GetOwinContext().[Get](Of ApplicationSignInManager)())
        End Get
        Private Set
            _signInManager = value
        End Set
    End Property

    Public Property UserManager() As ApplicationUserManager
        Get
            Return If(_userManager, HttpContext.GetOwinContext().GetUserManager(Of ApplicationUserManager)())
        End Get
        Private Set
            _userManager = value
        End Set
    End Property

    '
    ' GET: /Account/Login
    <AllowAnonymous>
    Public Function Login(returnUrl As String) As ActionResult
        ViewData!ReturnUrl = returnUrl
        Return View()
    End Function

    '
    ' POST: /Account/Login
    <HttpPost>
    <AllowAnonymous>
    <ValidateAntiForgeryToken>
    Public Async Function Login(model As LoginViewModel, returnUrl As String) As Task(Of ActionResult)
        If Not ModelState.IsValid Then
            Return View(model)
        End If

        ' 這不會計算為帳戶鎖定的登入失敗
        ' 若要啟用密碼失敗來觸發帳戶鎖定，請變更為 shouldLockout := True
        Dim result = Await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout := False)
        Select Case result
            Case SignInStatus.Success
                Return RedirectToLocal(returnUrl)
            Case SignInStatus.LockedOut
                Return View("Lockout")
            Case SignInStatus.RequiresVerification
                Return RedirectToAction("SendCode", New With {
                    .ReturnUrl = returnUrl,
                    .RememberMe = model.RememberMe
                })
            Case Else
                ModelState.AddModelError("", "登入嘗試失試。")
                Return View(model)
        End Select
    End Function

    '
    ' GET: /Account/VerifyCode
    <AllowAnonymous>
    Public Async Function VerifyCode(provider As String, returnUrl As String, rememberMe As Boolean) As Task(Of ActionResult)
        ' 需要使用者已透過使用者名稱/密碼或外部登入進行登入
        If Not Await SignInManager.HasBeenVerifiedAsync() Then
            Return View("Error")
        End If
        Return View(New VerifyCodeViewModel() With {
            .Provider = provider,
            .ReturnUrl = returnUrl,
            .RememberMe = rememberMe
        })
    End Function

    '
    ' POST: /Account/VerifyCode
    <HttpPost>
    <AllowAnonymous>
    <ValidateAntiForgeryToken>
    Public Async Function VerifyCode(model As VerifyCodeViewModel) As Task(Of ActionResult)
        If Not ModelState.IsValid Then
            Return View(model)
        End If

        ' 下列程式碼保護兩個因素碼不受暴力密碼破解攻擊。 
        ' 如果使用者輸入不正確的代碼來表示一段指定的時間，則使用者帳戶 
        ' 會有一段指定的時間遭到鎖定。 
        ' 您可以在 IdentityConfig 中設定帳戶鎖定設定
        Dim result = Await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent := model.RememberMe, rememberBrowser := model.RememberBrowser)
        Select Case result
            Case SignInStatus.Success
                Return RedirectToLocal(model.ReturnUrl)
            Case SignInStatus.LockedOut
                Return View("Lockout")
            Case Else
                ModelState.AddModelError("", "代碼無效。")
                Return View(model)
        End Select
    End Function

    '
    ' GET: /Account/Register
    <AllowAnonymous>
    Public Function Register() As ActionResult
        Return View()
    End Function

    '
    ' POST: /Account/Register
    <HttpPost>
    <AllowAnonymous>
    <ValidateAntiForgeryToken>
    Public Async Function Register(model As RegisterViewModel) As Task(Of ActionResult)
        If ModelState.IsValid Then
            Dim user = New ApplicationUser() With {
                .UserName = model.Email,
                .Email = model.Email
            }
            Dim result = Await UserManager.CreateAsync(user, model.Password)
            If result.Succeeded Then
                Await SignInManager.SignInAsync(user, isPersistent := False, rememberBrowser := False)

                ' 如需如何啟用帳戶確認和密碼重設的詳細資訊，請造訪 http://go.microsoft.com/fwlink/?LinkID=320771
                ' 傳送包含此連結的電子郵件
                ' Dim code = Await UserManager.GenerateEmailConfirmationTokenAsync(user.Id)
                ' Dim callbackUrl = Url.Action("ConfirmEmail", "Account", New With { .userId = user.Id, .code = code }, protocol := Request.Url.Scheme)
                ' Await UserManager.SendEmailAsync(user.Id, "確認您的帳戶", "請按一下此連結確認您的帳戶 <a href=""" & callbackUrl & """>這裏</a>")

                Return RedirectToAction("Index", "Home")
            End If
            AddErrors(result)
        End If

        ' 如果執行到這裡，發生某項失敗，則重新顯示表單
        Return View(model)
    End Function

    '
    ' GET: /Account/ConfirmEmail
    <AllowAnonymous>
    Public Async Function ConfirmEmail(userId As String, code As String) As Task(Of ActionResult)
        If userId Is Nothing OrElse code Is Nothing Then
            Return View("Error")
        End If
        Dim result = Await UserManager.ConfirmEmailAsync(userId, code)
        Return View(If(result.Succeeded, "ConfirmEmail", "Error"))
    End Function

    '
    ' GET: /Account/ForgotPassword
    <AllowAnonymous>
    Public Function ForgotPassword() As ActionResult
        Return View()
    End Function

    '
    ' POST: /Account/ForgotPassword
    <HttpPost>
    <AllowAnonymous>
    <ValidateAntiForgeryToken>
    Public Async Function ForgotPassword(model As ForgotPasswordViewModel) As Task(Of ActionResult)
        If ModelState.IsValid Then
            Dim user = Await UserManager.FindByNameAsync(model.Email)
            If user Is Nothing OrElse Not (Await UserManager.IsEmailConfirmedAsync(user.Id)) Then
                ' 不顯示使用者不存在或未受確認
                Return View("ForgotPasswordConfirmation")
            End If
            ' 如需如何啟用帳戶確認和密碼重設的詳細資訊，請造訪 http://go.microsoft.com/fwlink/?LinkID=320771
            ' 傳送包含此連結的電子郵件
            ' Dim code = Await UserManager.GeneratePasswordResetTokenAsync(user.Id)
            ' Dim callbackUrl = Url.Action("ResetPassword", "Account", New With { .userId = user.Id, .code = code }, protocol := Request.Url.Scheme)
            ' Await UserManager.SendEmailAsync(user.Id, "重設密碼", "請按 <a href=""" & callbackUrl & """>這裡</a>")
            ' Return RedirectToAction("ForgotPasswordConfirmation", "Account")
        End If

        ' 如果執行到這裡，發生某項失敗，則重新顯示表單
        Return View(model)
    End Function

    '
    ' GET: /Account/ForgotPasswordConfirmation
    <AllowAnonymous>
    Public Function ForgotPasswordConfirmation() As ActionResult
        Return View()
    End Function

    '
    ' GET: /Account/ResetPassword
    <AllowAnonymous>
    Public Function ResetPassword(code As String) As ActionResult
        Return If(code Is Nothing, View("Error"), View())
    End Function

    '
    ' POST: /Account/ResetPassword
    <HttpPost>
    <AllowAnonymous>
    <ValidateAntiForgeryToken>
    Public Async Function ResetPassword(model As ResetPasswordViewModel) As Task(Of ActionResult)
        If Not ModelState.IsValid Then
            Return View(model)
        End If
        Dim user = Await UserManager.FindByNameAsync(model.Email)
        If user Is Nothing Then
            ' 不顯示使用者不存在
            Return RedirectToAction("ResetPasswordConfirmation", "Account")
        End If
        Dim result = Await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password)
        If result.Succeeded Then
            Return RedirectToAction("ResetPasswordConfirmation", "Account")
        End If
        AddErrors(result)
        Return View()
    End Function

    '
    ' GET: /Account/ResetPasswordConfirmation
    <AllowAnonymous>
    Public Function ResetPasswordConfirmation() As ActionResult
        Return View()
    End Function

    '
    ' POST: /Account/ExternalLogin
    <HttpPost>
    <AllowAnonymous>
    <ValidateAntiForgeryToken>
    Public Function ExternalLogin(provider As String, returnUrl As String) As ActionResult
        ' 要求重新導向至外部登入提供者
        Return New ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", New With {
            .ReturnUrl = returnUrl
        }))
    End Function

    '
    ' GET: /Account/SendCode
    <AllowAnonymous>
    Public Async Function SendCode(returnUrl As String, rememberMe As Boolean) As Task(Of ActionResult)
        Dim userId = Await SignInManager.GetVerifiedUserIdAsync()
        If userId Is Nothing Then
            Return View("Error")
        End If
        Dim userFactors = Await UserManager.GetValidTwoFactorProvidersAsync(userId)
        Dim factorOptions = userFactors.[Select](Function(purpose) New SelectListItem() With {
            .Text = purpose,
            .Value = purpose
        }).ToList()
        Return View(New SendCodeViewModel() With {
            .Providers = factorOptions,
            .ReturnUrl = returnUrl,
            .RememberMe = rememberMe
        })
    End Function

    '
    ' POST: /Account/SendCode
    <HttpPost>
    <AllowAnonymous>
    <ValidateAntiForgeryToken>
    Public Async Function SendCode(model As SendCodeViewModel) As Task(Of ActionResult)
        If Not ModelState.IsValid Then
            Return View()
        End If

        ' 產生並傳送 Token
        If Not Await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider) Then
            Return View("Error")
        End If
        Return RedirectToAction("VerifyCode", New With { _
            .Provider = model.SelectedProvider,
            .ReturnUrl = model.ReturnUrl,
            .RememberMe = model.RememberMe
        })
    End Function

    '
    ' GET: /Account/ExternalLoginCallback
    <AllowAnonymous>
    Public Async Function ExternalLoginCallback(returnUrl As String) As Task(Of ActionResult)
        Dim loginInfo = Await AuthenticationManager.GetExternalLoginInfoAsync()
        If loginInfo Is Nothing Then
            Return RedirectToAction("Login")
        End If

        ' 若使用者已經有登入資料，請使用此外部登入提供者登入使用者
        Dim result = Await SignInManager.ExternalSignInAsync(loginInfo, isPersistent := False)
        Select Case result
            Case SignInStatus.Success
                Return RedirectToLocal(returnUrl)
            Case SignInStatus.LockedOut
                Return View("Lockout")
            Case SignInStatus.RequiresVerification
                Return RedirectToAction("SendCode", New With {
                    .ReturnUrl = returnUrl,
                    .RememberMe = False
                })
            Case Else
                ' 若使用者沒有帳戶，請提示使用者建立帳戶
                ViewData!ReturnUrl = returnUrl
                ViewData!LoginProvider = loginInfo.Login.LoginProvider
                Return View("ExternalLoginConfirmation", New ExternalLoginConfirmationViewModel() With {
                    .Email = loginInfo.Email
                })
        End Select
    End Function

    '
    ' POST: /Account/ExternalLoginConfirmation
    <HttpPost>
    <AllowAnonymous>
    <ValidateAntiForgeryToken>
    Public Async Function ExternalLoginConfirmation(model As ExternalLoginConfirmationViewModel, returnUrl As String) As Task(Of ActionResult)
        If User.Identity.IsAuthenticated Then
          Return RedirectToAction("Index", "Manage")
        End If

        If ModelState.IsValid Then
          ' 從外部登入提供者處取得使用者資訊
          Dim info = Await AuthenticationManager.GetExternalLoginInfoAsync()
          If info Is Nothing Then
              Return View("ExternalLoginFailure")
          End If
          Dim userInfo = New ApplicationUser() With {
              .UserName = model.Email,
              .Email = model.Email
          }
          Dim result = Await UserManager.CreateAsync(userInfo)
          If result.Succeeded Then
            result = Await UserManager.AddLoginAsync(userInfo.Id, info.Login)
            If result.Succeeded Then
                Await SignInManager.SignInAsync(userInfo, isPersistent := False, rememberBrowser := False)
                Return RedirectToLocal(returnUrl)
            End If
          End If
          AddErrors(result)
        End If

        ViewData!ReturnUrl = returnUrl
        Return View(model)
    End Function

    '
    ' POST: /Account/LogOff
    <HttpPost>
    <ValidateAntiForgeryToken>
    Public Function LogOff() As ActionResult
        AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie)
        Return RedirectToAction("Index", "Home")
    End Function

    '
    ' GET: /Account/ExternalLoginFailure
    <AllowAnonymous>
    Public Function ExternalLoginFailure() As ActionResult
        Return View()
    End Function

    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing Then
            If _userManager IsNot Nothing Then
                _userManager.Dispose()
                _userManager = Nothing
            End If
            If _signInManager IsNot Nothing Then
                _signInManager.Dispose()
                _signInManager = Nothing
            End If
        End If

        MyBase.Dispose(disposing)
    End Sub

    #Region "Helper"
    ' 新增外部登入時用來當做 XSRF 保護
    Private Const XsrfKey As String = "XsrfId"

    Private ReadOnly Property AuthenticationManager() As IAuthenticationManager
        Get
            Return HttpContext.GetOwinContext().Authentication
        End Get
    End Property

    Private Sub AddErrors(result As IdentityResult)
        For Each [error] In result.Errors
            ModelState.AddModelError("", [error])
        Next
    End Sub

    Private Function RedirectToLocal(returnUrl As String) As ActionResult
        If Url.IsLocalUrl(returnUrl) Then
            Return Redirect(returnUrl)
        End If
        Return RedirectToAction("Index", "Home")
    End Function

    Friend Class ChallengeResult
        Inherits HttpUnauthorizedResult
        Public Sub New(provider As String, redirectUri As String)
            Me.New(provider, redirectUri, Nothing)
        End Sub

        Public Sub New(provider As String, redirect As String, user As String)
            LoginProvider = provider
            RedirectUri = redirect
            UserId = user
        End Sub

        Public Property LoginProvider As String
        Public Property RedirectUri As String
        Public Property UserId As String

        Public Overrides Sub ExecuteResult(context As ControllerContext)
            Dim properties = New AuthenticationProperties() With {
                .RedirectUri = RedirectUri
            }
            If UserId IsNot Nothing Then
              properties.Dictionary(XsrfKey) = UserId
            End If
            context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider)
        End Sub
    End Class
    #End Region
End Class
