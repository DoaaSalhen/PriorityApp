﻿"use strict";

    function addNotificationToMenu(notificationCount, message)
    {
        var notificationCount = parseInt(document.getElementById("notificationbg").textContent) + count;
        document.getElementById("notificationbg").value = notificationCount;
        //document.getElementById("notificationbg2").value = notificationCount;
        var heading = document.getElementById("notificationbg");
        heading.textContent = notificationCount;
        var heading2 = document.getElementById("notificationbg2");
        heading2.textContent = notificationCount;
        var ul = document.getElementById("notificaionMenu");
        var li = document.createElement("li");
        li.setAttribute("class", "notification-item");
        li.textContent = Message;
        ul.append(li);
        var hr = document.createElement("hr");
        hr.setAttribute("class", "dropdown-divider");
        ul.append(hr);



    }
