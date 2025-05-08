"use strict";

window.Interops =
{
    assemblyname: "LTres.OltApi.UI.Interops",

    deleteCookies: function () {
        let Cookies = document.cookie.split(';');
        for (let i = 0; i < Cookies.length; i++)
            document.cookie = Cookies[i] + "=;expires=" + new Date(0).toUTCString();
    }
};