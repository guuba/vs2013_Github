@ModelType ManageLoginsViewModel
@Imports Microsoft.Owin.Security
@Imports Microsoft.AspNet.Identity
@Code
    ViewBag.Title = "管理您的外部登入"
End Code

<h2>@ViewBag.Title.</h2>

<p class="text-success">@ViewBag.StatusMessage</p>
@Code
    Dim loginProviders = Context.GetOwinContext().Authentication.GetExternalAuthenticationTypes()
    If loginProviders.Count = 0 Then
        @<div>
            <p>
                並未設定雙因素驗證提供者。請參閱<a href="http://go.microsoft.com/fwlink/?LinkId=313242">此文章</a>
                以了解設定此 ASP.NET 應用程式以支援雙因素驗證的詳細資料。
            </p>
        </div>
    Else
        If Model.CurrentLogins.Count > 0  Then
            @<h4>已註冊的登入</h4>
            @<table class="table">
                <tbody>
                    @For Each account As UserLoginInfo In Model.CurrentLogins
                        @<tr>
                            <td>@account.LoginProvider</td>
                            <td>
                                @If ViewBag.ShowRemoveButton
                                    @Using Html.BeginForm("RemoveLogin", "Manage")
                                        @Html.AntiForgeryToken()
                                        @<div>
                                            @Html.Hidden("loginProvider", account.LoginProvider)
                                            @Html.Hidden("providerKey", account.ProviderKey)
                                            <input type="submit" class="btn btn-default" value="移除" title="移除您帳戶的這個 @account.LoginProvider 登入" />
                                        </div>
                                    End Using
                                Else
                                    @: &nbsp;
                                End If
                            </td>
                        </tr>
                    Next
                </tbody>
            </table>
        End If
        If Model.OtherLogins.Count > 0
            @<text>
            <h4>新增其他服務以登入。</h4>
            <hr />
            </text>
            @Using Html.BeginForm("LinkLogin", "Manage")
                @Html.AntiForgeryToken()
                @<div id="socialLoginList">
                <p>
                    @For Each p As AuthenticationDescription In Model.OtherLogins
                        @<button type="submit" class="btn btn-default" id="@p.AuthenticationType" name="provider" value="@p.AuthenticationType" title="使用 @p.Caption 帳戶登入">@p.AuthenticationType</button>
                    Next
                </p>
                </div>
            End Using
        End If
    End If
End Code
