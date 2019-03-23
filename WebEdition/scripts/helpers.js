// useful helper methods

function trace(msg)
{
    if (typeof console != 'undefined' && typeof console.log != 'undefined')
    {
        console.log(msg);
    }
}

var MouseButton = {
    Left: 1,
    Middle: 2,
    Right: 3
};

var Convert = {
    bool: function(str) { return str == "1" },
    int: function(str) { var n = parseInt(str); return isNaN(n) ? 0 : n; }
};

function Rest(method, url, data, success) {
    //TODO: add error handler
    return $.ajax({
        url: url,
        type: method,
        dataType: "json",
        data: data,
        success: success
    });
}

if (typeof Array.prototype.last != 'function') {
    Array.prototype.last = function () {
        return this[this.length - 1];
    };
}

if (typeof Array.prototype.remove != 'function') {
    Array.prototype.remove = function (element) {
        var i = this.indexOf(element);
        if (i == -1) {
            return this;
        }
        this.splice(i, 1);
        return this;
    };
}

if (typeof Array.prototype.contains != 'function') {
    Array.prototype.contains = function(obj) {
        var i = this.length;
        while (i--) {
            if (this[i] === obj) {
                return true;
            }
        }
        return false;
    };
}

if (typeof String.prototype.startsWith != 'function') {
    String.prototype.startsWith = function (str){
        return this.slice(0, str.length) == str;
    };
}

if (typeof String.prototype.endsWith != 'function') {
    String.prototype.endsWith = function (str){
        return this.slice(-str.length) == str;
    };
}
