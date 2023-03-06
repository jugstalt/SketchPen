(function ($) {
    "use strict";
    $.fn.sketchPenCode_download_package = function (method) {
        if (methods[method]) {
            return methods[method].apply(this, Array.prototype.slice.call(arguments, 1));
        }
        else if (typeof method === 'object' || !method) {
            return methods.init.apply(this, arguments);
        }
        else {
            $.error('Method ' + method + ' does not exist on jQuery.sketchPenCode_download_package');
        }
    };

    let defaults = {
        
    };

    let methods = {
        init: function (options) {
            let settings = $.extend({}, defaults, options);
            return this.each(function () {
                new initUI(this, settings);
            });
        }
    };

    let initUI = function (parent, options) {
        let $parent = $(parent);

        let $composers = $("<select>").appendTo($parent);
        sketchPenCode.api.getComposers(function (result) {
            for (var composer of result) {
                $("<option>")
                    .attr('value', composer)
                    .text(composer)
                    .addpendTo($composers);
            }
        });
    };
})(jQuery);