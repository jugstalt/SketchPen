(function ($) {
    "use strict";
    $.fn.sketchPenCode_globals = function (method) {
        if (methods[method]) {
            return methods[method].apply(this, Array.prototype.slice.call(arguments, 1));
        }
        else if (typeof method === 'object' || !method) {
            return methods.init.apply(this, arguments);
        }
        else {
            $.error('Method ' + method + ' does not exist on jQuery.sketchPenCode_globals');
        }
    };
    var defaults = {
        $toolbar: null,
    };
    var methods = {
        init: function (options) {
            var settings = $.extend({}, defaults, options);
            return this.each(function () {
                new initUI(this, settings);
            });
        },
        refresh: function (options) {
            refresh($(this));
        }
    };
    var initUI = function (parent, options) {
        
    };

    var refresh = function ($parent) {
        
    }

})(jQuery);
