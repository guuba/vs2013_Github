@ModelType SendCodeViewModel
@Code
    ViewBag.Title = "傳送"
End Code

<h2>@ViewBag.Title.</h2>

@Using Html.BeginForm("SendCode", "Account", New With { .ReturnUrl = Model.ReturnUrl }, FormMethod.Post, New With { .class = "form-horizontal", .role = "form" })
    @Html.AntiForgeryToken()
    @Html.Hidden("rememberMe", Model.RememberMe)
    @<text>
    <h4>傳送驗證碼</h4>
    <hr />
    <div class="row">
        <div class="col-md-8">
            選取雙因素驗證提供者:
            @Html.DropDownListFor(Function(model) model.SelectedProvider, Model.Providers)
            <input type="submit" value="提交" class="btn btn-default" />
        </div>
    </div>
    </text>
End Using

@Section Scripts
    @Scripts.Render("~/bundles/jqueryval")
End Section
