(function ($) {
    "use strict";
    $.fn.sketchPenCode_blockframe = function (method) {
        if (methods[method]) {
            return methods[method].apply(this, Array.prototype.slice.call(arguments, 1));
        }
        else if (typeof method === 'object' || !method) {
            return methods.init.apply(this, arguments);
        }
        else {
            $.error('Method ' + method + ' does not exist on jQuery.sketchPenCode_blockframe');
        }
    };
    var defaults = {
        onShow: null
    };
    var methods = {
        init: function (options) {
            var settings = $.extend({}, defaults, options);
            return this.each(function () {
                new initUI(this, settings);
            });
        },
        close: function (options) {
            $(this).children('.sketchpen-code-blockframe-blocker').remove();
        }
    };
    var initUI = function (parent, options) {
        var $parent = $(parent);
        
        var $blocker = $("<div>")
            .addClass("sketchpen-code-blockframe-blocker")
            .appendTo($parent);

        $("<div>")
            .addClass("sketchpen-code-blockframe-close")
            .appendTo($blocker)
            .click(function (e) {
                e.stopPropagation();
                $blocker.remove();
            });

        var $content = $("<div>")
            .addClass("sketchpen-code-blockframe-content")
            .appendTo($blocker);

        if (options.onShow) {
            options.onShow($content);
        }
    };
})(jQuery);
