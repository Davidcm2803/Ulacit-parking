using System;
using System.Web;
using System.Web.Mvc;

public class AuthorizeRoleAttribute : AuthorizeAttribute
{
    private readonly int[] allowedRoles;

    public AuthorizeRoleAttribute(params int[] roles)
    {
        this.allowedRoles = roles;
    }

    protected override bool AuthorizeCore(HttpContextBase httpContext)
    {
        var sessionUser = httpContext.Session["UserLogged"] as Ulacit_parking.Models.User;
        var sessionAdmin = httpContext.Session["AdminLogged"] as Ulacit_parking.Models.User;

        Ulacit_parking.Models.User user = sessionUser ?? sessionAdmin;

        if (user == null)
            return false;

        System.Diagnostics.Debug.WriteLine("Rol del usuario: " + user.RoleId);

        return Array.Exists(allowedRoles, r => r == user.RoleId);
    }


    protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
    {
        filterContext.Result = new RedirectResult("/Admin/Login"); 
    }
}
