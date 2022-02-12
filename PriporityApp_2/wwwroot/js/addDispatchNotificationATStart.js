"use strict";

//alert("PPP");
//var divNotfy = document.getElementById("notificaionMenu");
//alert("ccc");
//var divider = document.createElement("div");
//alert("ccc");

//divider.setAttribute("class", "dropdown-divider");
//alert("ccc");

//divNotfy.prepend(divider);
//alert("ccc");

    function addNotificationToMenu(notificationCount, message)
    {
        //document.getElementById("bgId").value = notificationCount;
        //alert(message);
        //    var heading = document.getElementById("bgId");
        //    heading.textContent = notificationCount;
        //    var divNotfy = document.getElementById("notificaionMenu");
        //    var divider = document.createElement("div");
        //    divider.setAttribute("class", "dropdown-divider");
        //divNotfy.prepend(divider);
        //alert(message);
        //var container = document.createElement("div");

        //    var span = document.createElement("span");
        //    span.textContent = message;
        //    span.setAttribute("class", "dropdown-item");
        //    container.prepend(span);
        //     divNotfy.prepend(container);
        //     alert(message);
        alert("test");
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
