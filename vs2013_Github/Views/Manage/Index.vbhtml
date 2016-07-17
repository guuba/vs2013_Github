@ModelType IndexViewModel
@Code
    ViewBag.Title = "管理"
End Code

<h2>@ViewBag.Title.</h2>

<p class="text-success">@ViewBag.StatusMessage</p>
<div>
    <h4>變更帳戶設定</h4>
    <hr />
    <dl class="dl-horizontal">
        <dt>密碼:</dt>
        <dd>
            [
            @If Model.HasPassword Then
                @Html.ActionLink("變更密碼", "ChangePassword")
            Else
                @Html.ActionLink("建立", "SetPassword")
            End If
            ]
        </dd>
        <dt>外部登入:</dt>
        <dd>
            @Model.Logins.Count [
            @Html.ActionLink("管理", "ManageLogins") ]
        </dd>
        @*
            電話號碼可以當做雙因素驗證系統中的第二個驗證因素。
             
             請參閱<a href="http://go.microsoft.com/fwlink/?LinkId=403804">此文章</a>
                以了解設定此 ASP.NET 應用程式以支援使用 SMS 進行雙因素驗證的詳細資料。
             
            雙因素驗證設定完成之後，請取消註解以下區塊
        *@
        @* 
            <dt>電話號碼:</dt>
            <dd>
                @(If(Model.PhoneNumber, "None")) [
                @If (Model.PhoneNumber <> Nothing) Then
                    @Html.ActionLink("變更", "AddPhoneNumber")
                    @: &nbsp;|&nbsp;
                    @Html.ActionLink("移除", "RemovePhoneNumber")
                Else
                    @Html.ActionLink("新增", "AddPhoneNumber")
                End If
                ]
            </dd>
        *@
        <dt>雙因素驗證:</dt>
        <dd>
            <p>
                並未設定雙因素驗證提供者。請參閱<a href="http://go.microsoft.com/fwlink/?LinkId=403804">此文章</a>
                以了解設定此 ASP.NET 應用程式以支援雙因素驗證的詳細資料。
            </p>
            @*
                @If Model.TwoFactor Then
                    @Using Html.BeginForm("DisableTwoFactorAuthentication", "Manage", FormMethod.Post, New With { .class = "form-horizontal", .role = "form" })
                      @Html.AntiForgeryToken()
                      @<text>
                      已啟用
                      <input type="submit" value="停用" class="btn btn-link" />
                      </text>
                    End Using
                Else
                    @Using Html.BeginForm("EnableTwoFactorAuthentication", "Manage", FormMethod.Post, New With { .class = "form-horizontal", .role = "form" })
                      @Html.AntiForgeryToken()
                      @<text>
                      已停用
                      <input type="submit" value="啟用" class="btn btn-link" />
                      </text>
                    End Using
                End If
	     *@
        </dd>
    </dl>
</div>
