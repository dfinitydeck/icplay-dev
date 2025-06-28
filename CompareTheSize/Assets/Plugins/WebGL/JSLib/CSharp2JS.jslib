var jsLibPlugin = {


        login:function()
        {
            var _instance = unityInstance;
            window.gamebox.login().then(function(result){
                var buffer = _malloc(result.length + 1)
                stringToUTF8(result,buffer,result.length + 1);
                _instance.SendMessage("JsCallCharp","OnLogin",result);
            });
        },

        logout:function(){
            var _instance = unityInstance
            window.gamebox.logout().then(function(result){
                _instance.SendMessage("JsCallCharp","OnLogout",result);
            });
        },

       getPrincipal:function()
       {
            var principal = window.gamebox.getPrincipal()
            var buffer = _malloc(principal.length + 1)
            stringToUTF8(principal, buffer, principal.length + 1)
            return buffer
       },
       
       freeStringMemory:function(ptr) 
       {
            _free(ptr)
       },
       
       getBalance:function(principal)
       {
             var _instance = unityInstance;
             var priStr = UTF8ToString(principal)
            window.gamebox.getBalance(priStr)
            .then(function(result){
                _instance.SendMessage("JsCallCharp","OnBalance", result.toString())
            });
        },

        buy:function(packId)
        { 
            var _instance = unityInstance
            var packIdStr = UTF8ToString(packId)
            window.gamebox.buy(packIdStr)
            .then(function(result) {
                var resJsonStr = JSON.stringify(result)
                _instance.SendMessage("JsCallCharp", "OnSuccess", resJsonStr);
            })
            .catch(function(error) {
             });
        },

        onLoaded:function(){
            window.gamebox.onLoaded();
        },

        requestFullscreen: function() {
            var canvas = document.getElementById("canvas");
            if (canvas.requestFullscreen) {
                canvas.requestFullscreen();
            } 
            else if (canvas.webkitRequestFullscreen) {
                canvas.webkitRequestFullscreen();
            } 
            else if (canvas.msRequestFullscreen) {
                canvas.msRequestFullscreen();
            }
        },

        copy:function(str){
            var pid = UTF8ToString(str)
            window.gamebox.copy(pid)
        }
    }

mergeInto(LibraryManager.library, jsLibPlugin);